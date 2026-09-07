using UnityEngine;
using UnityEngine.UI;
using TMPro;

// TurnUIHandler — รับ Event จาก TurnManager แล้วอัปเดต UI
// แยกออกจาก TurnManager เพื่อให้ Logic กับ UI ไม่ปนกัน
//
// ผูก TurnManager.OnTurnStart / OnTurnEnd / OnGameOver
// แล้วอัปเดต Text, Button, Panel ตามที่ต้องการ

public class TurnUIHandler : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private TurnManager turnManager;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI labelCurrentPlayer;
    [SerializeField] private TextMeshProUGUI labelRound;
    [SerializeField] private TextMeshProUGUI labelDeckCount;
    [SerializeField] private Button btnEndTurn;
    [SerializeField] private GameObject panelGameOver;
    [SerializeField] private TextMeshProUGUI labelWinner;

    void Awake()
    {
        // Subscribe Events จาก TurnManager
        turnManager.OnTurnStart += HandleTurnStart;
        turnManager.OnTurnEnd += HandleTurnEnd;
        turnManager.OnPlayerEliminated += HandleEliminated;
        turnManager.OnGameOver += HandleGameOver;

        // ปุ่ม End Turn เรียก TurnManager.EndTurn()
        btnEndTurn.onClick.AddListener(turnManager.EndTurn);
        btnEndTurn.interactable = false;

        if (panelGameOver != null)
            panelGameOver.SetActive(false);
    }

    void OnDestroy()
    {
        // Unsubscribe เมื่อ object ถูกทำลาย
        turnManager.OnTurnStart -= HandleTurnStart;
        turnManager.OnTurnEnd -= HandleTurnEnd;
        turnManager.OnPlayerEliminated -= HandleEliminated;
        turnManager.OnGameOver -= HandleGameOver;
    }

    // ==========================================
    // EVENT HANDLERS
    // ==========================================
    private void HandleTurnStart(PlayerData player, int round)
    {
        if (labelCurrentPlayer != null)
            labelCurrentPlayer.text = $"Turn: {player.playerName}";

        if (labelRound != null)
            labelRound.text = $"Round {round}";

        if (labelDeckCount != null)
            labelDeckCount.text = $"Deck: {GetDeckCount()} Card";

        // เปิดปุ่ม End Turn เมื่อ Turn เริ่ม
        btnEndTurn.interactable = true;
    }

    private void HandleTurnEnd(PlayerData player)
    {
        // ปิดปุ่ม End Turn ระหว่างรอ Turn ถัดไป
        btnEndTurn.interactable = false;
    }

    private void HandleEliminated(PlayerData player)
    {
        Debug.Log($"[UI] {player.playerName} ถูกตัดออก");
    }

    private void HandleGameOver(PlayerData winner)
    {
        btnEndTurn.interactable = false;

        if (panelGameOver != null)
            panelGameOver.SetActive(true);

        if (labelWinner != null)
        {
            labelWinner.text = winner != null
                ? $"Winner!: {winner.playerName}"
                : "Draw!";
        }
    }

    // ==========================================
    // HELPER
    // ==========================================
    private int GetDeckCount()
    {
        var dm = FindAnyObjectByType<DeckManager>();
        return dm != null ? dm.DeckCount : 0;
    }

    public void EndTurn()
    {
        turnManager.EndTurn();
    }
}
