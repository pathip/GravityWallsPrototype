using UnityEngine;

// ObjectiveConditionChecker — ตรวจสอบเงื่อนไขพิเศษที่เช็คแบบ snapshot
// ต่างจาก ObjectiveProgressTracker ที่นับสะสม
// ตัวนี้เช็ค "ณ ขณะนี้" เช่น CollectCards เช็คว่ามีการ์ดครบหรือเปล่า
//
// เรียกจาก EscapeConditionSystem ทุกต้น/ปลาย Turn

public class ObjectiveConditionChecker : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private ObjectiveAssignment assignment;
    [SerializeField] private ObjectiveProgressTracker tracker;

    // ==========================================
    // CHECK — เรียกจาก EscapeConditionSystem
    // คืน true ถ้า Objective สำเร็จแล้ว
    // ==========================================
    public bool CheckObjective(PlayerData player)
    {
        var obj = assignment.GetObjective(player);
        if (obj == null) return false;

        // Progress-based conditions — delegate ให้ tracker
        switch (obj.conditionType)
        {
            case PersonalObjectiveData.ConditionType.DrawSpecificCard:
            case PersonalObjectiveData.ConditionType.DrawAnyCard:
            case PersonalObjectiveData.ConditionType.RollDiceTotal:
            case PersonalObjectiveData.ConditionType.RollDiceCount:
            case PersonalObjectiveData.ConditionType.FlipCoinHead:
            case PersonalObjectiveData.ConditionType.FlipCoinTail:
            case PersonalObjectiveData.ConditionType.WinGroupFlipMajority:
            case PersonalObjectiveData.ConditionType.EndTurnCount:
                return tracker.IsComplete(player);

            // Snapshot conditions — เช็ค ณ ขณะนี้
            case PersonalObjectiveData.ConditionType.SurviveUntilRound:
                return turnManager.CurrentRound >= obj.targetRound
                       && player.IsActive;

            case PersonalObjectiveData.ConditionType.CollectCards:
                return player.hand.Count >= obj.targetCount;

            case PersonalObjectiveData.ConditionType.Custom:
                return CheckCustomCondition(player, obj);

            default:
                return false;
        }
    }

    // CheckAll — เช็คทุกคนพร้อมกัน
    // คืน List ของผู้เล่นที่สำเร็จ Objective
    public System.Collections.Generic.List<PlayerData> CheckAll()
    {
        var completed = new System.Collections.Generic.List<PlayerData>();
        foreach (var p in turnManager.Players)
        {
            if (!p.isEliminated && CheckObjective(p))
                completed.Add(p);
        }
        return completed;
    }

    // ==========================================
    // CUSTOM CONDITION — override เมื่อต้องการ
    // ==========================================
    protected virtual bool CheckCustomCondition(PlayerData player, PersonalObjectiveData obj)
    {
        // Override method นี้ใน subclass เพื่อเพิ่ม condition พิเศษ
        // เช่น:
        //   return player.hand.Count > 0
        //       && player.hand[0].data.arcana == TarotCard.Arcana.Major;
        Debug.LogWarning($"[ConditionChecker] Custom condition ไม่ได้ implement สำหรับ {obj.objectiveName}");
        return false;
    }
}