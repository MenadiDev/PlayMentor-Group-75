using UnityEngine;

[System.Serializable]
public class QuizQuestion
{
    [Header("Question")]
    public string questionText;

    [Header("Answers")]
    public string answerA;
    public string answerB;
    public string answerC;
    public string answerD;

    [Header("Correct Answer")]
    [Tooltip("Enter A, B, C, or D")]
    public string correctAnswer;

    [Header("Topic")]
    public string topic; 

    // Method to check if answer is correct
    public bool IsCorrect(string playerAnswer)
    {
        return playerAnswer.ToUpper() == correctAnswer.ToUpper();
    }
}
