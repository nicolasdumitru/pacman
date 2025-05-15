#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <ctype.h>
#include <time.h>

// cjson library
#include "../C_Libraries/cJSON-1.7.18/cJSON.h"

#define MAX_QUESTIONS 100
#define MAX_LINE_LEN 256

#define INPUT "questions.txt"
#define JSON_OUTPUT "questions.json"

typedef struct
{
    char question[MAX_LINE_LEN];
    char options[4][MAX_LINE_LEN];
    char correct_option;
} MCQ;

MCQ questions[MAX_QUESTIONS];
int question_count = 0;

void trim_newline(char *str)
{
    char *pos;
    if ((pos = strchr(str, '\n')) != NULL)
        *pos = '\0';
}

void trim(char *str)
{
    char *start = str;
    char *end;

    while (isspace((unsigned char)*start))
        start++;
    if (*start == 0)
    {
        *str = '\0';
        return;
    }

    end = start + strlen(start) - 1;
    while (end > start && isspace((unsigned char)*end))
        end--;

    *(end + 1) = '\0';
    memmove(str, start, (size_t)(end - start + 2));
}

int is_valid_option_letter(char ch)
{
    ch = (char)toupper(ch);
    return ch >= 'A' && ch <= 'D';
}

void loadQuestions(const char *filename)
{
    FILE *file = fopen(filename, "r");
    if (!file)
    {
        perror("Error opening file");
        return;
    }

    char line[MAX_LINE_LEN];
    while (fgets(line, sizeof(line), file))
    {
        if (strncmp(line, "QUESTION:", 9) == 0)
        {
            trim_newline(line);
            strncpy(questions[question_count].question, line + 9, MAX_LINE_LEN);
            trim(questions[question_count].question);

            int valid_options = 1;
            for (int i = 0; i < 4; i++)
            {
                if (fgets(line, sizeof(line), file))
                {
                    trim_newline(line);
                    trim(line);
                    if (strlen(line) > 2 && line[1] == ':' && is_valid_option_letter(line[0]))
                    {
                        strncpy(questions[question_count].options[line[0] - 'A'], line + 2, MAX_LINE_LEN);
                        trim(questions[question_count].options[line[0] - 'A']);
                    }
                    else
                    {
                        valid_options = 0;
                        break;
                    }
                }
            }

            if (!valid_options)
                continue;

            if (fgets(line, sizeof(line), file) && strncmp(line, "ANSWER:", 7) == 0)
            {
                trim_newline(line);
                char *ans = line + 7;
                trim(ans);
                char ans_letter = (char)toupper(ans[0]);

                if (is_valid_option_letter(ans_letter))
                {
                    questions[question_count].correct_option = ans_letter;
                    question_count++;
                    if (question_count >= MAX_QUESTIONS)
                        break;
                }
                else
                {
                    printf("Warning: Skipping question with invalid ANSWER: %s\n", ans);
                }
            }
        }
    }

    fclose(file);
}

void shuffle_indices(MCQ *array, int size)
{
    for (int i = 0; i < size; i++)
    {
        int correct_index = array[i].correct_option - 'A';

        // Create an array of indices and shuffle it
        int indices[4] = {0, 1, 2, 3};
        for (int j = 3; j > 0; j--)
        {
            int rand_idx = rand() % (j + 1);
            int temp = indices[j];
            indices[j] = indices[rand_idx];
            indices[rand_idx] = temp;
        }

        char new_options[4][MAX_LINE_LEN];
        int new_correct_index = -1;
        for (int j = 0; j < 4; j++)
        {
            strcpy(new_options[j], array[i].options[indices[j]]);
            if (indices[j] == correct_index)
                new_correct_index = j;
        }

        for (int j = 0; j < 4; j++)
        {
            strcpy(array[i].options[j], new_options[j]);
        }

        array[i].correct_option = 'A' + new_correct_index;
    }
}


void shuffle_questions(MCQ *array, int size)
{
    for (int i = 0; i < size; i++)
    {
        int random_index = rand() % size;
        MCQ temp = array[i];
        array[i] = array[random_index];
        array[random_index] = temp;
    }
}

// JSON FUNCTION
void exportQuestionsToJSON(const char *filename)
{
    cJSON *json_array = cJSON_CreateArray();

    // shuffle the questions
    shuffle_indices(questions,question_count);
    shuffle_questions(questions, question_count);

    for (int i = 0; i < question_count; i++)
    {
        cJSON *q = cJSON_CreateObject();

        cJSON_AddStringToObject(q, "question", questions[i].question);

        cJSON *options = cJSON_CreateArray();
        for (int j = 0; j < 4; j++)
        {
            cJSON_AddItemToArray(options, cJSON_CreateString(questions[i].options[j]));
        }
        cJSON_AddItemToObject(q, "options", options);

        char correct_option[2] = {questions[i].correct_option, '\0'};
        cJSON_AddStringToObject(q, "correct_option", correct_option);

        cJSON_AddItemToArray(json_array, q);
    }

    char *json_string = cJSON_Print(json_array);

    FILE *fp = fopen(filename, "w");
    if (fp)
    {
        fputs(json_string, fp);
        fclose(fp);
    }
    else
    {
        perror("Failed to open output file");
    }

    cJSON_Delete(json_array);
    free(json_string);
}

int main()
{
    srand((unsigned int)time(NULL)); // reboot the srand for each run
    loadQuestions(INPUT);
    exportQuestionsToJSON(JSON_OUTPUT);
    return 0;
}
