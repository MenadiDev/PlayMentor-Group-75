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

    [Header("Adaptive Learning")]
    public string difficulty; // "easy" , "medium" , "hard"  

    public bool IsCorrect(string playerAnswer)
    {
        return playerAnswer.ToUpper() == correctAnswer.ToUpper();
    }
}
