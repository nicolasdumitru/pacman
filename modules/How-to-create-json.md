# Documentation regarding the questions for the modules from moodle
All questions from modules 11 and 12 are included inside the pacman/modules/questions.txt

## Question format for serialization
```
QUESTION: {question text}
A: {1st option}
B: {2nd option}
C: {3rd option}
D: {4th option}
ANSWER: {letter matching right answer}
```
## How to create a new JSON file
You can build the modules.c file with ```gcc modules.c -o generateJson.exe``` and this will
create a new executable. After that, you can rerun this executable as many times as you want
to generate a different order for both questions and their respective answer options.