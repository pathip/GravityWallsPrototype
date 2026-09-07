using UnityEngine;
using System.Collections.Generic;

// ObjectiveProgressTracker — ติดตาม Progress ของ Objective แต่ละคน
// Subscribe Events จาก DiceRoller, CoinFlipSystem, DrawSystem, TurnManager
// แล้วเพิ่ม progress ให้ถูก Objective อัตโนมัติ

public class ObjectiveProgressTracker : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private ObjectiveAssignment assignment;
    [SerializeField] private DiceRoller diceRoller;
    [SerializeField] private CoinFlipSystem coinFlipSystem;
    [SerializeField] private DeckManager deckManager;

    // playerName → current progress count
    private Dictionary<string, int> progressMap = new Dictionary<string, int>();

    // ==========================================
    // EVENTS
    // ==========================================
    public event System.Action<PlayerData, int, int> OnProgressUpdated;  // (player, current, target)
    public event System.Action<PlayerData> OnObjectiveComplete; // (player)

    // ==========================================
    // LIFECYCLE
    // ==========================================
    void Awake()
    {
        // Subscribe ทุก event ที่เกี่ยวกับ progress
        turnManager.OnTurnStart += HandleTurnStart;
        turnManager.OnTurnEnd += HandleTurnEnd;
        diceRoller.OnDiceRolled += HandleDiceRolled;
        coinFlipSystem.OnSoloFlip += HandleCoinFlip;
        coinFlipSystem.OnGroupFlip += HandleGroupFlip;
    }

    void OnDestroy()
    {
        turnManager.OnTurnStart -= HandleTurnStart;
        turnManager.OnTurnEnd -= HandleTurnEnd;
        diceRoller.OnDiceRolled -= HandleDiceRolled;
        coinFlipSystem.OnSoloFlip -= HandleCoinFlip;
        coinFlipSystem.OnGroupFlip -= HandleGroupFlip;
    }

    // ==========================================
    // EVENT HANDLERS
    // ==========================================
    private void HandleTurnStart(PlayerData player, int round)
    {
        var obj = assignment.GetObjective(player);
        if (obj == null) return;

        // SurviveUntilRound — นับ round ที่เริ่ม Turn ได้
        if (obj.conditionType == PersonalObjectiveData.ConditionType.SurviveUntilRound)
            AddProgress(player, 1);
    }

    private void HandleTurnEnd(PlayerData player)
    {
        var obj = assignment.GetObjective(player);
        if (obj == null) return;

        // EndTurnCount — นับทุกครั้งที่กด End Turn
        if (obj.conditionType == PersonalObjectiveData.ConditionType.EndTurnCount)
            AddProgress(player, 1);

        // CollectCards — เช็คจำนวนการ์ดใน Hand ณ End Turn
        if (obj.conditionType == PersonalObjectiveData.ConditionType.CollectCards)
        {
            int handSize = player.hand.Count;
            SetProgress(player, handSize);
        }

        // DrawAnyCard — นับจาก Hand ที่จั่วมาใน Turn นี้
        if (obj.conditionType == PersonalObjectiveData.ConditionType.DrawAnyCard)
            AddProgress(player, player.hand.Count > 0 ? 1 : 0);
    }

    private void HandleDiceRolled(DiceResult result)
    {
        var player = FindPlayerByName(result.rollerName);
        if (player == null) return;

        var obj = assignment.GetObjective(player);
        if (obj == null) return;

        if (obj.conditionType == PersonalObjectiveData.ConditionType.RollDiceCount)
            AddProgress(player, 1);

        if (obj.conditionType == PersonalObjectiveData.ConditionType.RollDiceTotal
            && result.total >= obj.targetDiceTotal)
            AddProgress(player, 1);
    }

    private void HandleCoinFlip(CoinFlipSystem.CoinFlipResult result)
    {
        var player = FindPlayerByName(result.playerName);
        if (player == null) return;

        var obj = assignment.GetObjective(player);
        if (obj == null) return;

        if (obj.conditionType == PersonalObjectiveData.ConditionType.FlipCoinHead
            && result.result == CoinFlipSystem.CoinSide.Head)
            AddProgress(player, 1);

        if (obj.conditionType == PersonalObjectiveData.ConditionType.FlipCoinTail
            && result.result == CoinFlipSystem.CoinSide.Tail)
            AddProgress(player, 1);
    }

    private void HandleGroupFlip(CoinFlipSystem.GroupFlipResult result)
    {
        foreach (var p in turnManager.Players)
        {
            var obj = assignment.GetObjective(p);
            if (obj == null) continue;

            if (obj.conditionType == PersonalObjectiveData.ConditionType.WinGroupFlipMajority
                && result.majorityPlayers.Contains(p.playerName))
                AddProgress(p, 1);
        }
    }

    // เรียกจาก DrawSystem หลังจั่วการ์ด
    public void NotifyCardDrawn(PlayerData player, CardInstance card)
    {
        var obj = assignment.GetObjective(player);
        if (obj == null) return;

        if (obj.conditionType == PersonalObjectiveData.ConditionType.DrawSpecificCard
            && obj.targetCard != null
            && card.data == obj.targetCard)
            AddProgress(player, 1);
    }

    // ==========================================
    // PROGRESS MANAGEMENT
    // ==========================================
    private void AddProgress(PlayerData player, int amount)
    {
        if (!progressMap.ContainsKey(player.playerName))
            progressMap[player.playerName] = 0;

        progressMap[player.playerName] += amount;
        CheckCompletion(player);

        var obj = assignment.GetObjective(player);
        int target = GetTarget(obj);
        OnProgressUpdated?.Invoke(player, progressMap[player.playerName], target);

        Debug.Log($"[Progress] {player.playerName}: {progressMap[player.playerName]}/{target} ({obj?.objectiveName})");
    }

    private void SetProgress(PlayerData player, int value)
    {
        progressMap[player.playerName] = value;
        CheckCompletion(player);

        var obj = assignment.GetObjective(player);
        int target = GetTarget(obj);
        OnProgressUpdated?.Invoke(player, value, target);
    }

    private void CheckCompletion(PlayerData player)
    {
        var obj = assignment.GetObjective(player);
        if (obj == null) return;

        int current = GetProgress(player);
        int target = GetTarget(obj);

        if (current >= target)
        {
            Debug.Log($"[Progress] {player.playerName} สำเร็จ Objective: {obj.objectiveName}!");
            OnObjectiveComplete?.Invoke(player);
        }
    }

    // ==========================================
    // QUERY
    // ==========================================
    public int GetProgress(PlayerData player)
    {
        progressMap.TryGetValue(player.playerName, out int val);
        return val;
    }

    public int GetProgress(string playerName)
    {
        progressMap.TryGetValue(playerName, out int val);
        return val;
    }

    public bool IsComplete(PlayerData player)
    {
        var obj = assignment.GetObjective(player);
        if (obj == null) return false;
        return GetProgress(player) >= GetTarget(obj);
    }

    public void ResetProgress(PlayerData player)
    {
        progressMap[player.playerName] = 0;
    }

    // ==========================================
    // HELPERS
    // ==========================================
    private int GetTarget(PersonalObjectiveData obj)
    {
        if (obj == null) return 0;
        return obj.conditionType == PersonalObjectiveData.ConditionType.SurviveUntilRound
            ? obj.targetRound
            : obj.targetCount;
    }

    private PlayerData FindPlayerByName(string name)
    {
        return turnManager.Players.Find(p => p.playerName == name);
    }
}
