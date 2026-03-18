using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Debug = UnityEngine.Debug;
using Image = UnityEngine.UI.Image;

/// <summary>
/// Manages the Topic Selection scene.
/// Spawns topic cards, handles selection, and starts the quiz.
/// </summary>
public class TopicSelectionManager : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // Inspector References
    // ─────────────────────────────────────────────

    [Header("Card Prefab & Containers")]
    [SerializeField] private GameObject topicCardPrefab;
    [SerializeField] private Transform grade10Grid;    // Grid Layout Group parent for Grade 10
    [SerializeField] private Transform grade11Grid;    // Grid Layout Group parent for Grade 11

    [Header("Start Bar")]
    [SerializeField] private GameObject startBar;          // the bottom start bar
    [SerializeField] private TMP_Text selectedTopicText; // shows selected topic name
    [SerializeField] private Button startButton;       // the Start! button

    [Header("Loading")]
    [SerializeField] private GameObject loadingOverlay;    // full screen loading panel

    [Header("Topic Icons — Grade 10 (drag in order)")]
    [SerializeField] private Sprite iconChemical;
    [SerializeField] private Sprite iconCharacteristics;
    [SerializeField] private Sprite iconCellStructure;
    [SerializeField] private Sprite iconClassification;
    [SerializeField] private Sprite iconReproduction;
    [SerializeField] private Sprite iconInheritance;

    [Header("Topic Icons — Grade 11 (drag in order)")]
    [SerializeField] private Sprite iconCells;
    [SerializeField] private Sprite iconPhotosynthesis;
    [SerializeField] private Sprite iconBiologicalProcesses;
    [SerializeField] private Sprite iconBiosphere;

    // ─────────────────────────────────────────────
    // Topic Data
    // ─────────────────────────────────────────────
    private class TopicData
    {
        public string topicId;
        public string displayName;
        public int grade;
        public Sprite icon;
    }

    // ─────────────────────────────────────────────
    // Private State
    // ─────────────────────────────────────────────
    private List<TopicCard> allCards = new List<TopicCard>();
    private TopicCard selectedCard = null;

    // ─────────────────────────────────────────────
    // Unity Lifecycle
    // ─────────────────────────────────────────────
    void Start()
    {
        // Hide start bar until a topic is selected
        if (startBar != null) startBar.SetActive(false);
        if (loadingOverlay != null) loadingOverlay.SetActive(false);

        // Wire start button
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);

        SpawnTopicCards();
    }

    // ─────────────────────────────────────────────
    // Spawn all topic cards into the grid
    // ─────────────────────────────────────────────
    private void SpawnTopicCards()
    {
        // Grade 10 topics
        List<TopicData> grade10Topics = new List<TopicData>
        {
            new TopicData { topicId = "chemical_basis_of_life",      displayName = "Chemical Basis of Life",         grade = 10, icon = iconChemical          },
            new TopicData { topicId = "characteristics_of_organisms", displayName = "Characteristics of Organisms",  grade = 10, icon = iconCharacteristics    },
            new TopicData { topicId = "cell_structure",               displayName = "Cell Structure & Functions",    grade = 10, icon = iconCellStructure      },
            new TopicData { topicId = "classification",               displayName = "Classification of Organisms",   grade = 10, icon = iconClassification     },
            new TopicData { topicId = "reproduction",                 displayName = "Reproduction",                  grade = 10, icon = iconReproduction       },
            new TopicData { topicId = "inheritance",                  displayName = "Inheritance",                   grade = 10, icon = iconInheritance        },
        };

        // Grade 11 topics
        List<TopicData> grade11Topics = new List<TopicData>
        {
            new TopicData { topicId = "cells_gr11",           displayName = "Cells",                          grade = 11, icon = iconCells                  },
            new TopicData { topicId = "photosynthesis",       displayName = "Photosynthesis",                 grade = 11, icon = iconPhotosynthesis          },
            new TopicData { topicId = "biological_processes", displayName = "Biological Processes in Humans", grade = 11, icon = iconBiologicalProcesses    },
            new TopicData { topicId = "biosphere",            displayName = "Biosphere",                      grade = 11, icon = iconBiosphere              },
        };

        SpawnCards(grade10Topics, grade10Grid);
        SpawnCards(grade11Topics, grade11Grid);
    }

    private void SpawnCards(List<TopicData> topics, Transform parent)
    {
        if (parent == null)
        {
            Debug.LogError("TopicSelectionManager: grid parent is null!");
            return;
        }

        foreach (var topic in topics)
        {
            GameObject cardObj = Instantiate(topicCardPrefab, parent);
            TopicCard cardScript = cardObj.GetComponent<TopicCard>();

            if (cardScript == null)
            {
                Debug.LogError("TopicSelectionManager: topicCardPrefab missing TopicCard component!");
                continue;
            }

            cardScript.Setup(topic.topicId, topic.displayName, topic.grade, topic.icon, this);
            allCards.Add(cardScript);
        }
    }

    // ─────────────────────────────────────────────
    // Called by TopicCard when tapped
    // ─────────────────────────────────────────────
    public void OnTopicSelected(TopicCard card)
    {
        // Deselect previous card
        if (selectedCard != null)
            selectedCard.SetSelected(false);

        // Select new card
        selectedCard = card;
        card.SetSelected(true);

        // Update start bar
        if (selectedTopicText != null)
            selectedTopicText.text = card.topicDisplayName;

        // Show start bar if hidden
        if (startBar != null)
            startBar.SetActive(true);
    }

    // ─────────────────────────────────────────────
    // Start Button — load questions then go to quiz
    // ─────────────────────────────────────────────
    private async void OnStartClicked()
    {
        if (selectedCard == null)
        {
            Debug.LogWarning("TopicSelectionManager: no topic selected.");
            return;
        }

        // Show loading overlay while fetching questions from Firestore
        if (loadingOverlay != null) loadingOverlay.SetActive(true);
        if (startButton != null) startButton.interactable = false;

        bool success = await QuizDataManager.Instance.LoadQuestionsForTopic(selectedCard.topicId);

        if (loadingOverlay != null) loadingOverlay.SetActive(false);
        if (startButton != null) startButton.interactable = true;

        if (!success)
        {
            Debug.LogError($"TopicSelectionManager: failed to load questions for '{selectedCard.topicId}'");
            // Optionally show an error message to the player here
            return;
        }

        // Transition to quiz scene
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene("QuizScene");
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("QuizScene");
    }
}
