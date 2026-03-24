using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using Debug = UnityEngine.Debug;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class LeaderboardManager : MonoBehaviour
{
    
    // Inspector References

    [Header("Row Spawning")]
    [SerializeField] private GameObject rowPrefab;
    [SerializeField] private Transform contentParent;

    [Header("Podium — Slot 1st")]
    [SerializeField] private TMP_Text podium1Name;
    [SerializeField] private TMP_Text podium1Points;
    [SerializeField] private TMP_Text podium1Avatar;

    [Header("Podium — Slot 2nd")]
    [SerializeField] private TMP_Text podium2Name;
    [SerializeField] private TMP_Text podium2Points;
    [SerializeField] private TMP_Text podium2Avatar;

    [Header("Podium — Slot 3rd")]
    [SerializeField] private TMP_Text podium3Name;
    [SerializeField] private TMP_Text podium3Points;
    [SerializeField] private TMP_Text podium3Avatar;

    [Header("Filter Buttons")]
    [SerializeField] private Image weeklyButtonBg;
    [SerializeField] private Image monthlyButtonBg;
    [SerializeField] private Image allTimeButtonBg;

    [SerializeField] private PodiumSlot podiumSlot1;
    [SerializeField] private PodiumSlot podiumSlot2;
    [SerializeField] private PodiumSlot podiumSlot3;

    [Header("Loading")]
    [SerializeField] private GameObject loadingSpinner;

    
    // Private State
    private FirebaseFirestore db;
    private string currentUserId;
    private string activeFilter = "weekly";

    private static readonly Color ActiveTabColor = new Color(0.49f, 0.23f, 0.93f, 1f);
    private static readonly Color InactiveTabColor = new Color(0.16f, 0.19f, 0.29f, 1f);

    
    // Unity Lifecycle
   
    async void Start()
    {
        db = FirebaseFirestore.DefaultInstance;

        var auth = FirebaseAuth.DefaultInstance;
        currentUserId = auth.CurrentUser != null ? auth.CurrentUser.UserId : "";

        if (loadingSpinner != null) loadingSpinner.SetActive(true);

        await LoadLeaderboard(activeFilter);

        if (loadingSpinner != null) loadingSpinner.SetActive(false);
    }

    
    // Filter Button Callbacks
    
    public async void OnWeeklyClicked()
    {
        if (activeFilter == "weekly") return;
        activeFilter = "weekly";
        UpdateFilterButtonVisuals();
        await LoadLeaderboard("weekly");
    }

    public async void OnMonthlyClicked()
    {
        if (activeFilter == "monthly") return;
        activeFilter = "monthly";
        UpdateFilterButtonVisuals();
        await LoadLeaderboard("monthly");
    }

    public async void OnAllTimeClicked()
    {
        if (activeFilter == "alltime") return;
        activeFilter = "alltime";
        UpdateFilterButtonVisuals();
        await LoadLeaderboard("alltime");
    }
    
    // Core Data Loading

    private async Task LoadLeaderboard(string filter)
    {
        // Clear existing rows
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        // All filters use TotalPoints for now.
        // Add WeeklyPoints / MonthlyPoints fields in Firestore later if needed.
        string sortField = filter switch
        {
            "weekly" => "TotalPoints",
            "monthly" => "TotalPoints",
            _ => "TotalPoints"
        };

        Query query = db.Collection("users")
                        .OrderByDescending(sortField)
                        .Limit(10);

        QuerySnapshot snapshot = await query.GetSnapshotAsync();

        
        Debug.Log($"Leaderboard query returned {snapshot.Count} documents");

        List<PlayerData> players = new List<PlayerData>();

        foreach (DocumentSnapshot doc in snapshot.Documents)
        {
            try
            {
                PlayerData p = new PlayerData
                {
                    userId = doc.Id,
                    name = doc.GetValue<string>("Username"),        
                    points = (int)doc.GetValue<long>("TotalPoints"),  
                    rankChange = 0
                };
                players.Add(p);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"LeaderboardManager: skipping malformed doc {doc.Id}: {e.Message}");
            }
        }

        PopulatePodium(players);
        PopulateScrollList(players);
    }

    
    // Populate Top 3 Podium
   
    private void PopulatePodium(List<PlayerData> players)
    {
        if (players.Count < 3)
        {
            Debug.LogWarning($"LeaderboardManager: only {players.Count} players found — need at least 3 for podium.");
            return;
        }

        SetPodiumSlot(podium1Name, podium1Points, podium1Avatar, players[0]);
        SetPodiumSlot(podium2Name, podium2Points, podium2Avatar, players[1]);
        SetPodiumSlot(podium3Name, podium3Points, podium3Avatar, players[2]);

        if (podiumSlot1 != null) podiumSlot1.AnimateIn();
        if (podiumSlot2 != null) podiumSlot2.AnimateIn();
        if (podiumSlot3 != null) podiumSlot3.AnimateIn();
    }

    private void SetPodiumSlot(TMP_Text nameText, TMP_Text pointsText,
                                TMP_Text avatarText, PlayerData player)
    {
        string[] parts = player.name.Split(' ');
        nameText.text = parts[0];
        pointsText.text = player.points.ToString("N0");
        avatarText.text = GetInitials(player.name);
    }

   
    // Populate Scroll List (Ranks 1–10)
   
    private void PopulateScrollList(List<PlayerData> players)
    {
        for (int i = 0; i < players.Count; i++)
        {
            GameObject rowObj = Instantiate(rowPrefab, contentParent);

            LeaderboardRow rowScript = rowObj.GetComponent<LeaderboardRow>();

            if (rowScript == null)
            {
                Debug.LogError("LeaderboardManager: rowPrefab is missing LeaderboardRow component!");
                continue;
            }

            bool isCurrentUser = players[i].userId == currentUserId;
            rowScript.Setup(rank: i + 1, data: players[i], isMe: isCurrentUser);
        }
    }

    
    // Filter Button Visuals
   
    private void UpdateFilterButtonVisuals()
    {
        if (weeklyButtonBg != null) weeklyButtonBg.color = InactiveTabColor;
        if (monthlyButtonBg != null) monthlyButtonBg.color = InactiveTabColor;
        if (allTimeButtonBg != null) allTimeButtonBg.color = InactiveTabColor;

        Image activeButton = activeFilter switch
        {
            "weekly" => weeklyButtonBg,
            "monthly" => monthlyButtonBg,
            _ => allTimeButtonBg
        };

        if (activeButton != null) activeButton.color = ActiveTabColor;
    }

  
    // Helper
    private string GetInitials(string fullName)
    {
        if (string.IsNullOrEmpty(fullName)) return "??";
        string[] parts = fullName.Trim().Split(' ');
        if (parts.Length >= 2)
            return $"{parts[0][0]}{parts[1][0]}".ToUpper();
        return fullName.Substring(0, Mathf.Min(2, fullName.Length)).ToUpper();
    }
}


// Shared Data Model

[System.Serializable]
public class PlayerData
{
    public string userId;
    public string name;
    public int points;
    public int rankChange;
}
