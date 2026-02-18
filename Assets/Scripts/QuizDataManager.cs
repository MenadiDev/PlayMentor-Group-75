using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class QuizDataManager : MonoBehaviour
{
    public static QuizDataManager Instance;

    [Header("Quiz Questions")]
    public List<QuizQuestion> allQuestions = new List<QuizQuestion>();

    [Header("Current Quiz Settings")]
    public int questionsPerQuiz = 10;
    public string currentTopic = "All";

    private List<QuizQuestion> currentQuizQuestions = new List<QuizQuestion>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (allQuestions.Count == 0)
        {
            LoadSampleQuestions();
        }
    }

    public List<QuizQuestion> GetQuizQuestions()
    {
        currentQuizQuestions.Clear();

        List<QuizQuestion> availableQuestions = new List<QuizQuestion>(allQuestions);

        for (int i = 0; i < questionsPerQuiz && availableQuestions.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, availableQuestions.Count);
            currentQuizQuestions.Add(availableQuestions[randomIndex]);
            availableQuestions.RemoveAt(randomIndex);
        }

        return currentQuizQuestions;
    }

    void LoadSampleQuestions()
    {
        allQuestions = new List<QuizQuestion>
        {
            new QuizQuestion
            {
                questionText = "What is the largest organ in the human body?",
                answerA = "Heart",
                answerB = "Skin",
                answerC = "Liver",
                answerD = "Brain",
                correctAnswer = "B",
                topic = "Human Body"
            },
            new QuizQuestion
            {
                questionText = "How many bones are in the adult human body?",
                answerA = "186",
                answerB = "206",
                answerC = "226",
                answerD = "246",
                correctAnswer = "B",
                topic = "Human Body"
            },
            new QuizQuestion
            {
                questionText = "What is the powerhouse of the cell?",
                answerA = "Nucleus",
                answerB = "Ribosome",
                answerC = "Mitochondria",
                answerD = "Chloroplast",
                correctAnswer = "C",
                topic = "Cell Biology"
            },
            new QuizQuestion
            {
                questionText = "What process do plants use to make food?",
                answerA = "Respiration",
                answerB = "Photosynthesis",
                answerC = "Digestion",
                answerD = "Fermentation",
                correctAnswer = "B",
                topic = "Plant Biology"
            },
            new QuizQuestion
            {
                questionText = "What is the basic unit of life?",
                answerA = "Atom",
                answerB = "Molecule",
                answerC = "Cell",
                answerD = "Tissue",
                correctAnswer = "C",
                topic = "Cell Biology"
            },
            new QuizQuestion
            {
                questionText = "Which blood type is the universal donor?",
                answerA = "A",
                answerB = "B",
                answerC = "AB",
                answerD = "O",
                correctAnswer = "D",
                topic = "Human Body"
            },
            new QuizQuestion
            {
                questionText = "What gas do plants absorb from the atmosphere?",
                answerA = "Oxygen",
                answerB = "Nitrogen",
                answerC = "Carbon Dioxide",
                answerD = "Hydrogen",
                correctAnswer = "C",
                topic = "Plant Biology"
            },
            new QuizQuestion
            {
                questionText = "What is the study of microorganisms called?",
                answerA = "Biology",
                answerB = "Microbiology",
                answerC = "Zoology",
                answerD = "Botany",
                correctAnswer = "B",
                topic = "Microbiology"
            },
            new QuizQuestion
            {
                questionText = "How many chambers does the human heart have?",
                answerA = "Two",
                answerB = "Three",
                answerC = "Four",
                answerD = "Five",
                correctAnswer = "C",
                topic = "Human Body"
            },
            new QuizQuestion
            {
                questionText = "What part of the cell contains genetic material?",
                answerA = "Cytoplasm",
                answerB = "Nucleus",
                answerC = "Cell membrane",
                answerD = "Vacuole",
                correctAnswer = "B",
                topic = "Cell Biology"
            }
        };
    }
}
