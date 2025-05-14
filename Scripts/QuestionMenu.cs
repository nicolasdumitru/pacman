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
// 5. Connect to the AnswerSelected signal in your game logic to handle user choices

using Godot;
using System;

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

    private Label questionLabel;
    private Button buttonA;
    private Button buttonB;
    private Button buttonC;
    private Button buttonD;

    [Signal]
    public delegate void AnswerSelectedEventHandler(string answer);

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
        buttonA.Pressed += () => EmitSignal(nameof(AnswerSelected), "A");
        buttonB.Pressed += () => EmitSignal(nameof(AnswerSelected), "B");
        buttonC.Pressed += () => EmitSignal(nameof(AnswerSelected), "C");
        buttonD.Pressed += () => EmitSignal(nameof(AnswerSelected), "D");
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
                                     string answerC, string answerD)
    {
        SetQuestion(question);
        SetAnswers(answerA, answerB, answerC, answerD);
    }

    // Reset the question and answers to default values
    public void Reset()
    {
        questionLabel.Text = "";
        buttonA.Text = "A:";
        buttonB.Text = "B:";
        buttonC.Text = "C:";
        buttonD.Text = "D:";
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
}
