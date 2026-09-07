using UnityEngine;
using System.Collections.Generic;

// ChangeObjectiveSystem — เปลี่ยน Objective กลางเกม
// เงื่อนไขการเปลี่ยน:
//   1. ผู้เล่นขอเปลี่ยนเอง (ใช้ชาร์จที่มี)
//   2. Event พิเศษบังคับเปลี่ยน (ไม่หักชาร์จ)
//   3. Admin/GM บังคับเปลี่ยนให้

public class ChangeObjectiveSystem : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private ObjectiveAssignment assignment;
    [SerializeField] private ObjectiveProgressTracker tracker;
    [SerializeField] private List<PersonalObjectiveData> objectivePool; // pool สำหรับสุ่มใหม่

    // playerName → จำนวนครั้งที่เปลี่ยนได้
    private Dictionary<string, int> remainingChanges = new Dictionary<string, int>();

    // ==========================================
    // EVENTS
    // ==========================================
    public event System.Action<PlayerData, PersonalObjectiveData, PersonalObjectiveData>
        OnObjectiveChanged; // (player, oldObj, newObj)

    // ==========================================
    // LIFECYCLE
    // ==========================================
    void Awake()
    {
        assignment.OnObjectiveAssigned += (player, obj) =>
        {
            // ตั้งค่าจำนวนครั้งเปลี่ยนจาก ScriptableObject
            remainingChanges[player.playerName] = obj.changeCount;
        };
    }

    // ==========================================
    // PUBLIC API
    // ==========================================

    // ผู้เล่นขอเปลี่ยน Objective เอง — หักชาร์จ
    public bool RequestChange(PlayerData player)
    {
        if (!CanChange(player))
        {
            Debug.LogWarning($"[ChangeObjective] {player.playerName} เปลี่ยน Objective ไม่ได้แล้ว");
            return false;
        }

        var currentObj = assignment.GetObjective(player);
        if (currentObj == null || !currentObj.canChange)
        {
            Debug.LogWarning($"[ChangeObjective] Objective นี้ไม่อนุญาตให้เปลี่ยน");
            return false;
        }

        var newObj = GetRandomObjective(player, currentObj);
        if (newObj == null)
        {
            Debug.LogWarning("[ChangeObjective] ไม่มี Objective ใหม่ให้เลือก");
            return false;
        }

        ExecuteChange(player, currentObj, newObj, useCharge: true);
        return true;
    }

    // Event พิเศษบังคับเปลี่ยน — ไม่หักชาร์จ
    public void ForceChange(PlayerData player, PersonalObjectiveData newObjective = null)
    {
        var currentObj = assignment.GetObjective(player);
        var target = newObjective ?? GetRandomObjective(player, currentObj);

        if (target == null) return;

        ExecuteChange(player, currentObj, target, useCharge: false);
    }

    // GM/Admin กำหนดให้ตรง ๆ
    public void SetObjective(PlayerData player, PersonalObjectiveData newObjective)
    {
        var currentObj = assignment.GetObjective(player);
        ExecuteChange(player, currentObj, newObjective, useCharge: false);
    }

    // ==========================================
    // QUERY
    // ==========================================
    public bool CanChange(PlayerData player)
    {
        if (!remainingChanges.ContainsKey(player.playerName)) return false;
        return remainingChanges[player.playerName] > 0;
    }

    public int GetRemainingChanges(PlayerData player)
    {
        remainingChanges.TryGetValue(player.playerName, out int val);
        return val;
    }

    // ==========================================
    // HELPERS
    // ==========================================
    private void ExecuteChange(PlayerData player, PersonalObjectiveData oldObj,
                               PersonalObjectiveData newObj, bool useCharge)
    {
        // เปลี่ยน Objective
        assignment.ReplaceObjective(player, newObj);

        // Reset progress เมื่อเปลี่ยน
        tracker.ResetProgress(player);

        // หักชาร์จถ้าผู้เล่นขอเอง
        if (useCharge && remainingChanges.ContainsKey(player.playerName))
            remainingChanges[player.playerName]--;

        Debug.Log($"[ChangeObjective] {player.playerName}: {oldObj?.objectiveName} → {newObj.objectiveName}" +
                  $" (เหลือ {GetRemainingChanges(player)} ครั้ง)");

        OnObjectiveChanged?.Invoke(player, oldObj, newObj);
    }

    private PersonalObjectiveData GetRandomObjective(PlayerData player,
                                                      PersonalObjectiveData exclude)
    {
        var available = objectivePool.FindAll(o => o != exclude);
        if (available.Count == 0) return null;
        return available[Random.Range(0, available.Count)];
    }
}
