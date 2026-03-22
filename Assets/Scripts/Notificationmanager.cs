using System;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Image = UnityEngine.UI.Image;


public class NotificationManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform notifContent;
    [SerializeField] private GameObject notifRowPrefab;
    [SerializeField] private GameObject emptyStatePanel;
    [SerializeField] private GameObject loadingOverlay;
    [SerializeField] private Button markAllReadButton;
    [SerializeField] private TMP_Text unreadCountText;

    
    [Header("Notification Icons")]
    [SerializeField] private Sprite iconBadge;       
    [SerializeField] private Sprite iconStreak;       
    [SerializeField] private Sprite iconLeaderboard;  
    [SerializeField] private Sprite iconChallenge;    
    [SerializeField] private Sprite iconQuiz;         
    [SerializeField] private Sprite iconDefault;      

    void Start()
    {
        string uid = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
        if (string.IsNullOrEmpty(uid))
        {
            ShowEmpty(true);
            return;
        }

        if (markAllReadButton != null)
            markAllReadButton.onClick.AddListener(() => MarkAllRead(uid));

        if (loadingOverlay != null) loadingOverlay.SetActive(true);

        LoadNotifications(uid);
    }

    
    // Load notifications from Firestore
    
    void LoadNotifications(string uid)
    {
        FirebaseFirestore.DefaultInstance
            .Collection("users").Document(uid)
            .Collection("notifications")
            .OrderByDescending("Timestamp")
            .Limit(30)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (loadingOverlay != null) loadingOverlay.SetActive(false);

                if (task.IsFaulted)
                {
                    Debug.LogWarning("NotificationManager: load failed.");
                    ShowEmpty(true);
                    return;
                }

                var docList = new List<DocumentSnapshot>(task.Result.Documents);

                if (docList.Count == 0)
                {
                    ShowEmpty(true);
                    return;
                }

                ShowEmpty(false);

                int unread = 0;

                foreach (var doc in docList)
                {
                    string message = doc.TryGetValue("Message", out string m) ? m : "";
                    bool isRead = doc.TryGetValue("IsRead", out bool r) && r;
                    string type = doc.TryGetValue("Type", out string t) ? t : "quiz";

                    if (!isRead) unread++;

                    // Spawn row
                    var row = Instantiate(notifRowPrefab, notifContent);

                   
                   
                    var iconImg = row.transform.Find("IconBg/NIcon")?.GetComponent<Image>();
                    if (iconImg != null)
                    {
                        iconImg.sprite = GetSprite(type);
                        iconImg.color = Color.white;
                    }

                   
                    var msgTmp = row.transform.Find("NContent/NMessage")?.GetComponent<TMP_Text>();
                    if (msgTmp != null)
                    {
                        msgTmp.text = message;
                        msgTmp.color = isRead
                            ? new Color(0.5f, 0.5f, 0.5f)
                            : new Color(0.1f, 0.1f, 0.25f);
                    }

                    
                    var timeTmp = row.transform.Find("NContent/NTime")?.GetComponent<TMP_Text>();
                    if (timeTmp != null && doc.TryGetValue("Timestamp", out Timestamp ts))
                    {
                        var dt = ts.ToDateTime().ToLocalTime();
                        timeTmp.text = dt.Date == DateTime.Today
                            ? $"Today, {dt:h:mm tt}"
                            : dt.Date == DateTime.Today.AddDays(-1)
                                ? $"Yesterday, {dt:h:mm tt}"
                                : dt.ToString("MMM d");
                    }

                   
                    var rowBg = row.GetComponent<Image>();
                    if (rowBg != null)
                        rowBg.color = isRead ? Color.white : new Color(0.97f, 0.96f, 1f);

                    
                    var unreadDot = row.transform.Find("UnreadDot")?.gameObject;
                    if (unreadDot != null) unreadDot.SetActive(!isRead);

                    
                    string docId = doc.Id;
                    var btn = row.GetComponent<Button>();
                    if (btn != null && !isRead)
                        btn.onClick.AddListener(() => MarkRead(uid, docId));
                }

                // Unread badge on nav icon
                if (unreadCountText != null)
                {
                    unreadCountText.gameObject.SetActive(unread > 0);
                    unreadCountText.text = unread > 9 ? "9+" : unread.ToString();
                }
            });
    }

   
    // Mark single notification read
    
    void MarkRead(string uid, string docId)
    {
        FirebaseFirestore.DefaultInstance
            .Collection("users").Document(uid)
            .Collection("notifications").Document(docId)
            .UpdateAsync("IsRead", true);
    }

    
    // Mark all read
   
    void MarkAllRead(string uid)
    {
        FirebaseFirestore.DefaultInstance
            .Collection("users").Document(uid)
            .Collection("notifications")
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted) return;
                var batch = FirebaseFirestore.DefaultInstance.StartBatch();
                foreach (var doc in task.Result.Documents)
                    batch.Update(doc.Reference, "IsRead", true);
                batch.CommitAsync();
            });
    }

   
    // Static helper — call from other scripts
   
    public static void SendNotification(string uid, string type, string message)
    {
        FirebaseFirestore.DefaultInstance
            .Collection("users").Document(uid)
            .Collection("notifications")
            .AddAsync(new Dictionary<string, object>
            {
                { "Type",      type    },
                { "Message",   message },
                { "IsRead",    false   },
                { "Timestamp", FieldValue.ServerTimestamp }
            });
    }

    
    // Helpers
    
    void ShowEmpty(bool show)
    {
        if (emptyStatePanel != null) emptyStatePanel.SetActive(show);
        if (notifContent != null) notifContent.gameObject.SetActive(!show);
    }

    Sprite GetSprite(string type)
    {
        switch (type)
        {
            case "badge": return iconBadge != null ? iconBadge : iconDefault;
            case "streak": return iconStreak != null ? iconStreak : iconDefault;
            case "leaderboard": return iconLeaderboard != null ? iconLeaderboard : iconDefault;
            case "challenge": return iconChallenge != null ? iconChallenge : iconDefault;
            case "quiz": return iconQuiz != null ? iconQuiz : iconDefault;
            default: return iconDefault;
        }
    }
}