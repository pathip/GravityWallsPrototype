using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// EndGameResult — สรุปผลเกม
// เรียกจาก EscapeConditionSystem เมื่อเกมจบ
// แสดงผลใน Panel พร้อม: ผู้ชนะ, สถิติ, Objective ของทุกคน

public class EndGameResult : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private ObjectiveAssignment assignment;
    [SerializeField] private ObjectiveProgressTracker tracker;
    [SerializeField] private ObjectiveHintSystem hintSystem;

    [Header("UI — Panel")]
    [SerializeField] private GameObject panelResult;
    [SerializeField] private TextMeshProUGUI labelTitle;         // "Game Over" หรือ "Escaped!"
    [SerializeField] private TextMeshProUGUI labelWinnerNames;   // ชื่อผู้ชนะ
    [SerializeField] private TextMeshProUGUI labelRoundPlayed;   // "จบใน Round X"
    [SerializeField] private Transform playerResultParent; // Parent ของ PlayerResultEntry
    [SerializeField] private GameObject playerResultPrefab; // Prefab แสดงผลทีละคน
    [SerializeField] private Button btnRestart;
    [SerializeField] private Button btnQuit;

    // ==========================================
    // DATA
    // ==========================================
    [System.Serializable]
    public class PlayerEndResult
    {
        public string playerName;
        public bool isWinner;
        public bool isEliminated;
        public string objectiveName;
        public string progressText;   // "3 / 5"
        public bool objectiveComplete;
        public int hpRemaining;
    }

    private List<PlayerEndResult> lastResults = new List<PlayerEndResult>();

    // ==========================================
    // LIFECYCLE
    // ==========================================
    void Awake()
    {
        if (panelResult != null) panelResult.SetActive(false);

        if (btnRestart != null)
            btnRestart.onClick.AddListener(RestartGame);
        if (btnQuit != null)
            btnQuit.onClick.AddListener(QuitGame);
    }

    // ==========================================
    // PUBLIC API — เรียกจาก EscapeConditionSystem
    // ==========================================
    public void ShowResult(List<PlayerData> winners,
                           List<PlayerData> allPlayers,
                           int roundsPlayed)
    {
        lastResults.Clear();

        // เก็บผลของทุกคน
        foreach (var p in allPlayers)
        {
            bool isWinner = winners.Contains(p);
            var obj = assignment.GetObjective(p);
            bool complete = tracker.IsComplete(p);

            var entry = new PlayerEndResult
            {
                playerName = p.playerName,
                isWinner = isWinner,
                isEliminated = p.isEliminated,
                objectiveName = obj?.objectiveName ?? "ไม่มี",
                progressText = hintSystem.GetProgressHint(p),
                objectiveComplete = complete,
                hpRemaining = p.hp,
            };
            lastResults.Add(entry);

            Debug.Log($"[EndGame] {p.playerName} — " +
                      $"Win: {isWinner} | Objective: {obj?.objectiveName} | " +
                      $"Progress: {entry.progressText} | HP: {p.hp}");
        }

        // อัปเดต UI
        UpdateUI(winners, roundsPlayed);

        // แสดง Panel
        if (panelResult != null) panelResult.SetActive(true);

        // หยุดเกม
        Time.timeScale = 0f;
    }

    // ==========================================
    // UI
    // ==========================================
    private void UpdateUI(List<PlayerData> winners, int roundsPlayed)
    {
        // Title
        if (labelTitle != null)
        {
            labelTitle.text = winners.Count == 0
                ? "Game Over — No Winner"
                : winners.Count == 1
                    ? $"{winners[0].playerName} Escaped!"
                    : "Multiple Escape!";
        }

        // Winner names
        if (labelWinnerNames != null)
        {
            if (winners.Count == 0)
                labelWinnerNames.text = "Draw";
            else
            {
                var names = new List<string>();
                foreach (var w in winners) names.Add(w.playerName);
                labelWinnerNames.text = string.Join(", ", names);
            }
        }

        // Round
        if (labelRoundPlayed != null)
            labelRoundPlayed.text = $"Round {roundsPlayed}";

        // Player result entries
        if (playerResultParent != null && playerResultPrefab != null)
        {
            foreach (Transform child in playerResultParent)
                Destroy(child.gameObject);

            foreach (var result in lastResults)
            {
                var go = Instantiate(playerResultPrefab, playerResultParent);
                var display = go.GetComponent<PlayerResultDisplay>();
                if (display != null)
                    display.Setup(result);
            }
        }
    }

    // ==========================================
    // BUTTONS
    // ==========================================
    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
        Debug.Log("[EndGame] Quit");
    }

    // ==========================================
    // QUERY — ใช้ตอน Debug หรือ Save
    // ==========================================
    public List<PlayerEndResult> GetLastResults() => lastResults;
}
