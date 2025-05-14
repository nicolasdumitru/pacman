// QuestionMenu.cs
//
// GODOT EDITOR SETUP:
// 1. Create a Control node (or Panel, Container, etc.) as the root of your question menu
// 2. Attach this script to that node
// 3. Add the following child nodes:
//    - A Label node for the question text
//    - Four Button nodes for the answer options
// 4. In the Inspector panel, set the NodePath properties to point to your nodes:
//    - questionLabelPath -> path to your question Label
//    - buttonAPath -> path to Button A
//    - buttonBPath -> path to Button B
//    - buttonCPath -> path to Button C
//    - buttonDPath -> path to Button D
// 5. Set the externalProgramPath property to the path of your external program
// 6. Connect to the AnswerSelected signal in your game logic to handle user choices
// 7. Connect to the CorrectAnswerSelected and WrongAnswerSelected signals for specific feedback

using Godot;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

public partial class QuestionMenu : Control
{
    [Export]
    private NodePath questionLabelPath;

    [Export]
    private NodePath buttonAPath;

    [Export]
    private NodePath buttonBPath;

    [Export]
    private NodePath buttonCPath;

    [Export]
    private NodePath buttonDPath;

    [Export]
    private string externalProgramPath = "";

    [Export]
    private string[] programArguments = null;

    private Label questionLabel;
    private Button buttonA;
    private Button buttonB;
    private Button buttonC;
    private Button buttonD;
    private string correctAnswer;

    [Signal]
    public delegate void AnswerSelectedEventHandler(string answer);

    [Signal]
    public delegate void CorrectAnswerSelectedEventHandler();

    [Signal]
    public delegate void WrongAnswerSelectedEventHandler(string selectedAnswer, string correctAnswer);

    // Class to hold the question data from JSON
    private class QuestionData
    {
        public string Question { get; set; }
        public string AnswerA { get; set; }
        public string AnswerB { get; set; }
        public string AnswerC { get; set; }
        public string AnswerD { get; set; }
        public string CorrectAnswer { get; set; } // Should be "A", "B", "C", or "D"
    }

    public override void _Ready()
    {
        // Get references to nodes
        questionLabel = GetNode<Label>(questionLabelPath);
        buttonA = GetNode<Button>(buttonAPath);
        buttonB = GetNode<Button>(buttonBPath);
        buttonC = GetNode<Button>(buttonCPath);
        buttonD = GetNode<Button>(buttonDPath);

        // Make sure all required nodes were found
        if (questionLabel == null || buttonA == null || buttonB == null ||
            buttonC == null || buttonD == null)
        {
            GD.PrintErr("QuestionMenu: Not all required nodes were assigned in the Inspector!");
            return;
        }

        // Connect button signals
        buttonA.Pressed += () => OnAnswerButtonPressed("A");
        buttonB.Pressed += () => OnAnswerButtonPressed("B");
        buttonC.Pressed += () => OnAnswerButtonPressed("C");
        buttonD.Pressed += () => OnAnswerButtonPressed("D");

        // Initialize with default values
        Reset();
    }

    private void OnAnswerButtonPressed(string answer)
    {
        EmitSignal(nameof(AnswerSelected), answer);

        // Check if the answer is correct
        if (answer == correctAnswer)
        {
            EmitSignal(nameof(CorrectAnswerSelected));
        }
        else
        {
            EmitSignal(nameof(WrongAnswerSelected), answer, correctAnswer);
        }
    }

    // Set the question text
    public void SetQuestion(string question)
    {
        questionLabel.Text = question;
    }

    // Set all four answers at once
    public void SetAnswers(string answerA, string answerB, string answerC, string answerD)
    {
        buttonA.Text = $"A: {answerA}";
        buttonB.Text = $"B: {answerB}";
        buttonC.Text = $"C: {answerC}";
        buttonD.Text = $"D: {answerD}";
    }

    // Set the question and all answers in one method call
    public void SetQuestionAndAnswers(string question, string answerA, string answerB,
                                     string answerC, string answerD, string correct)
    {
        SetQuestion(question);
        SetAnswers(answerA, answerB, answerC, answerD);
        correctAnswer = correct;
    }

    // Reset the question and answers to default values
    public void Reset()
    {
        questionLabel.Text = "";
        buttonA.Text = "A:";
        buttonB.Text = "B:";
        buttonC.Text = "C:";
        buttonD.Text = "D:";
        correctAnswer = "";
    }

    // Hide the question menu
    public void Hide()
    {
        Visible = false;
    }

    // Show the question menu
    public void Show()
    {
        Visible = true;
    }

    // Run the external program and load a question
    public async void LoadQuestionFromExternalProgram()
    {
        if (string.IsNullOrEmpty(externalProgramPath))
        {
            GD.PrintErr("External program path is not set!");
            return;
        }

        try
        {
            string jsonOutput = await RunExternalProgramAsync();
            ParseQuestionJson(jsonOutput);
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error loading question from external program: {e.Message}");
            Reset();
        }
    }

    // Run the external program and return its stdout output
    private async Task<string> RunExternalProgramAsync()
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = externalProgramPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        // Add arguments if specified
        if (programArguments != null)
        {
            foreach (var arg in programArguments)
            {
                processStartInfo.ArgumentList.Add(arg);
            }
        }

        using (var process = new Process { StartInfo = processStartInfo })
        {
            process.Start();
            // Read the stdout asynchronously
            string output = await process.StandardOutput.ReadToEndAsync();
            await Task.Run(() => process.WaitForExit());
            return output;
        }
    }

    // Parse the JSON output and set up the question
    private void ParseQuestionJson(string json)
    {
        try
        {
            var questionData = JsonSerializer.Deserialize<QuestionData>(json);

            if (questionData == null)
            {
                GD.PrintErr("Failed to parse question JSON.");
                return;
            }

            // Validate the correct answer
            if (questionData.CorrectAnswer != "A" &&
                questionData.CorrectAnswer != "B" &&
                questionData.CorrectAnswer != "C" &&
                questionData.CorrectAnswer != "D")
            {
                GD.PrintErr($"Invalid correct answer: {questionData.CorrectAnswer}. Must be A, B, C, or D.");
                return;
            }

            // Set the question, answers and correct answer
            SetQuestionAndAnswers(
                questionData.Question,
                questionData.AnswerA,
                questionData.AnswerB,
                questionData.AnswerC,
                questionData.AnswerD,
                questionData.CorrectAnswer
            );
        }
        catch (JsonException e)
        {
            GD.PrintErr($"JSON parsing error: {e.Message}");
        }
    }
}
