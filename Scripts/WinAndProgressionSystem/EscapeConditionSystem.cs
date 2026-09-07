using UnityEngine;
using System.Collections.Generic;

// EscapeConditionSystem — ระบบหลักตัดสินว่าใครชนะ / หนีรอดได้
// เช็คหลัง End Turn ทุกครั้ง และเมื่อ Objective สำเร็จ
//
// Escape Conditions:
//   1. ผู้เล่นทำ Objective สำเร็จ         → Win (Escape)
//   2. ผู้เล่นถูก Eliminate (hp = 0)       → Lose
//   3. Deck หมด + ทุกคนยัง Active          → Sudden Death Round
//   4. มีผู้เล่นที่ Active เหลือ 1 คน      → Win by Survival

public class EscapeConditionSystem : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private ObjectiveConditionChecker conditionChecker;
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private EndGameResult endGameResult;

    [Header("Settings")]
    [SerializeField] private int maxRounds = 10;  // จำนวน Round สูงสุด (0 = ไม่จำกัด)

    // ==========================================
    // EVENTS
    // ==========================================
    public event System.Action<PlayerData> OnPlayerEscape;    // ชนะด้วย Objective
    public event System.Action<PlayerData> OnPlayerLose;      // แพ้ (ถูก Eliminate)
    public event System.Action<List<PlayerData>> OnGameEnd;         // จบเกม — ส่ง List ผู้ชนะ

    // ==========================================
    // LIFECYCLE
    // ==========================================
    void Awake()
    {
        // เช็คหลัง End Turn ทุกครั้ง
        turnManager.OnTurnEnd += HandleTurnEnd;
        turnManager.OnPlayerEliminated += HandlePlayerEliminated;
    }

    void OnDestroy()
    {
        turnManager.OnTurnEnd -= HandleTurnEnd;
        turnManager.OnPlayerEliminated -= HandlePlayerEliminated;
    }

    // ==========================================
    // EVENT HANDLERS
    // ==========================================
    private void HandleTurnEnd(PlayerData player)
    {
        // 1. เช็ค Objective ของผู้เล่นคนนี้
        CheckPlayerObjective(player);

        // 2. เช็ค Deck หมด
        CheckDeckEmpty();

        // 3. เช็ค Max Rounds
        if (maxRounds > 0 && turnManager.CurrentRound > maxRounds)
            TriggerMaxRoundEnd();

        // 4. เช็คว่าเหลือคนเดียว
        CheckLastSurvivor();
    }

    private void HandlePlayerEliminated(PlayerData player)
    {
        Debug.Log($"[EscapeCondition] {player.playerName} ถูก Eliminate");
        OnPlayerLose?.Invoke(player);
        CheckLastSurvivor();
    }

    // ==========================================
    // CHECK METHODS
    // ==========================================
    private void CheckPlayerObjective(PlayerData player)
    {
        if (conditionChecker.CheckObjective(player))
        {
            Debug.Log($"[EscapeCondition] {player.playerName} สำเร็จ Objective! — ESCAPE");
            OnPlayerEscape?.Invoke(player);

            // ถ้า Escape แล้วจบเกม
            TriggerGameEnd(new List<PlayerData> { player });
        }
    }

    // เช็คว่ามีผู้เล่นหลายคน Escape พร้อมกันหรือไม่
    public void CheckAllObjectives()
    {
        var winners = conditionChecker.CheckAll();
        if (winners.Count > 0)
            TriggerGameEnd(winners);
    }

    private void CheckDeckEmpty()
    {
        if (deckManager.DeckCount == 0)
        {
            Debug.Log("[EscapeCondition] Deck หมดแล้ว — Sudden Death: ตรวจสอบ Objective ทุกคน");
            CheckAllObjectives();
        }
    }

    private void CheckLastSurvivor()
    {
        var active = turnManager.GetActivePlayers();
        if (active.Count == 1)
        {
            Debug.Log($"[EscapeCondition] {active[0].playerName} รอดคนสุดท้าย — Win by Survival");
            TriggerGameEnd(active);
        }
        else if (active.Count == 0)
        {
            Debug.Log("[EscapeCondition] ไม่มีผู้เล่นที่ Active — Draw");
            TriggerGameEnd(new List<PlayerData>());
        }
    }

    private void TriggerMaxRoundEnd()
    {
        Debug.Log($"[EscapeCondition] ครบ {maxRounds} Round — เช็ค Objective ทุกคน");
        CheckAllObjectives();

        // ถ้ายังไม่มีใครชนะ ให้ผู้เล่นที่ hp มากสุดชนะ
        var active = turnManager.GetActivePlayers();
        if (active.Count > 0)
        {
            active.Sort((a, b) => b.hp.CompareTo(a.hp));
            TriggerGameEnd(new List<PlayerData> { active[0] });
        }
    }

    // ==========================================
    // TRIGGER END
    // ==========================================
    private void TriggerGameEnd(List<PlayerData> winners)
    {
        OnGameEnd?.Invoke(winners);
        endGameResult.ShowResult(winners, turnManager.Players, turnManager.CurrentRound);
    }
}
