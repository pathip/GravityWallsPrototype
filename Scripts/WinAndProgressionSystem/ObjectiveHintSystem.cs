using UnityEngine;
using System.Collections.Generic;

// ObjectiveHintSystem — ระบบ Hint บอกใบ้ผู้เล่น
// 3 ประเภท Hint:
//   1. TextHint    — ข้อความใบ้จาก ScriptableObject
//   2. ProgressHint — บอก progress ปัจจุบัน เช่น "2/5"
//   3. TimeHint    — บอก Round ที่เหลือ (สำหรับ SurviveUntilRound)

public class ObjectiveHintSystem : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private ObjectiveAssignment assignment;
    [SerializeField] private ObjectiveProgressTracker tracker;

    [Header("Settings")]
    [SerializeField] private bool hideOtherPlayersObjective = true; // ซ่อน Objective ของคนอื่น

    // ==========================================
    // PUBLIC API — เรียกจาก UI
    // ==========================================

    // Hint ข้อความจาก ScriptableObject
    public string GetTextHint(PlayerData player)
    {
        var obj = assignment.GetObjective(player);
        if (obj == null) return "ไม่มี Objective";
        return obj.isSecret && IsOtherPlayer(player)
            ? "???"
            : obj.hintText;
    }

    // บอก Objective Name (ซ่อนถ้าเป็นของคนอื่นและ isSecret)
    public string GetObjectiveName(PlayerData player)
    {
        var obj = assignment.GetObjective(player);
        if (obj == null) return "ไม่มี Objective";
        return obj.isSecret && IsOtherPlayer(player)
            ? "Secret Objective"
            : obj.objectiveName;
    }

    // บอก Description เต็ม
    public string GetDescription(PlayerData player)
    {
        var obj = assignment.GetObjective(player);
        if (obj == null) return "";
        return obj.isSecret && IsOtherPlayer(player)
            ? "???"
            : obj.description;
    }

    // Progress Hint เช่น "2 / 5"
    public string GetProgressHint(PlayerData player)
    {
        var obj = assignment.GetObjective(player);
        if (obj == null) return "0 / 0";
        if (obj.isSecret && IsOtherPlayer(player)) return "? / ?";

        int current = tracker.GetProgress(player);
        int target = GetTarget(obj);
        return $"{current} / {target}";
    }

    // Progress 0.0 - 1.0 สำหรับ Progress Bar
    public float GetProgressRatio(PlayerData player)
    {
        var obj = assignment.GetObjective(player);
        if (obj == null) return 0f;

        int target = GetTarget(obj);
        if (target <= 0) return 0f;

        int current = tracker.GetProgress(player);
        return Mathf.Clamp01((float)current / target);
    }

    // Time Hint — Round ที่เหลือ (เฉพาะ SurviveUntilRound)
    public string GetTimeHint(PlayerData player)
    {
        var obj = assignment.GetObjective(player);
        if (obj == null || obj.conditionType !=
            PersonalObjectiveData.ConditionType.SurviveUntilRound)
            return "";

        int remaining = obj.targetRound - turnManager.CurrentRound;
        return remaining > 0
            ? $"เหลืออีก {remaining} Round"
            : "ถึง Round เป้าหมายแล้ว!";
    }

    // จำนวนครั้งที่เปลี่ยน Objective ได้
    public string GetChangeHint(PlayerData player)
    {
        var obj = assignment.GetObjective(player);
        if (obj == null) return "";
        return obj.canChange
            ? $"เปลี่ยน Objective ได้อีก {obj.changeCount} ครั้ง"
            : "ไม่สามารถเปลี่ยน Objective ได้";
    }

    // Hint รวมทุกอย่าง — ใช้กับ Tooltip หรือ Panel Objective
    public ObjectiveHintPackage GetFullHint(PlayerData player)
    {
        return new ObjectiveHintPackage
        {
            objectiveName = GetObjectiveName(player),
            description = GetDescription(player),
            hintText = GetTextHint(player),
            progressText = GetProgressHint(player),
            progressRatio = GetProgressRatio(player),
            timeHint = GetTimeHint(player),
            changeHint = GetChangeHint(player),
        };
    }

    // ==========================================
    // HELPERS
    // ==========================================
    private bool IsOtherPlayer(PlayerData player)
    {
        if (!hideOtherPlayersObjective) return false;
        return turnManager.CurrentPlayer?.playerName != player.playerName;
    }

    private int GetTarget(PersonalObjectiveData obj)
    {
        return obj.conditionType == PersonalObjectiveData.ConditionType.SurviveUntilRound
            ? obj.targetRound
            : obj.targetCount;
    }

    // ==========================================
    // DATA CLASS
    // ==========================================
    [System.Serializable]
    public class ObjectiveHintPackage
    {
        public string objectiveName;
        public string description;
        public string hintText;
        public string progressText;   // เช่น "2 / 5"
        public float progressRatio;  // 0.0 - 1.0
        public string timeHint;
        public string changeHint;
    }
}
