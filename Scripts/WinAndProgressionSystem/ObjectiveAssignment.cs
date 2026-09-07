using UnityEngine;
using System.Collections.Generic;

// ObjectiveAssignment — กำหนด Objective ให้ผู้เล่นแต่ละคน
// เชื่อมกับ TurnManager (รอ StartGame แล้วแจก)
// และ PersonalObjectiveData (pool ของ Objective ทั้งหมด)
//
// 2 Mode:
//   Random  — สุ่มจาก pool ให้แต่ละคน (ไม่ซ้ำกัน)
//   Manual  — กำหนดล่วงหน้าใน Inspector

public class ObjectiveAssignment : MonoBehaviour
{
    public enum AssignMode { Random, Manual }

    [Header("Dependencies")]
    [SerializeField] private TurnManager turnManager;

    [Header("Objective Pool")]
    [SerializeField] private List<PersonalObjectiveData> objectivePool;  // ใส่ทุก Objective ที่มี

    [Header("Settings")]
    [SerializeField] private AssignMode assignMode = AssignMode.Random;

    // Manual mode: ระบุตรง ๆ ว่าใครได้อะไร
    [SerializeField] private List<ManualAssignment> manualAssignments;

    [System.Serializable]
    public class ManualAssignment
    {
        public string playerName;
        public PersonalObjectiveData objective;
    }

    // playerName → Objective ที่ได้รับ
    private Dictionary<string, PersonalObjectiveData> assignments
        = new Dictionary<string, PersonalObjectiveData>();

    // ==========================================
    // EVENTS
    // ==========================================
    public event System.Action<PlayerData, PersonalObjectiveData> OnObjectiveAssigned;

    // ==========================================
    // LIFECYCLE
    // ==========================================
    void Awake()
    {
        // รอ TurnManager StartGame แล้วค่อยแจก Objective
        turnManager.OnTurnStart += (player, round) =>
        {
            if (round == 1 && !assignments.ContainsKey(player.playerName))
                AssignAll();
        };
    }

    // ==========================================
    // ASSIGN
    // ==========================================
    public void AssignAll()
    {
        var players = turnManager.Players;

        if (assignMode == AssignMode.Random)
            AssignRandom(players);
        else
            AssignManual(players);
    }

    private void AssignRandom(List<PlayerData> players)
    {
        // สับ pool ก่อนแจก
        var pool = new List<PersonalObjectiveData>(objectivePool);
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        for (int i = 0; i < players.Count; i++)
        {
            if (i >= pool.Count)
            {
                Debug.LogWarning($"[ObjectiveAssignment] Objective Pool ไม่พอสำหรับผู้เล่น {players[i].playerName}");
                break;
            }

            Assign(players[i], pool[i]);
        }
    }

    private void AssignManual(List<PlayerData> players)
    {
        foreach (var m in manualAssignments)
        {
            var player = players.Find(p => p.playerName == m.playerName);
            if (player != null && m.objective != null)
                Assign(player, m.objective);
        }
    }

    public void Assign(PlayerData player, PersonalObjectiveData objective)
    {
        assignments[player.playerName] = objective;
        Debug.Log($"[ObjectiveAssignment] {player.playerName} ได้รับ Objective: {objective.objectiveName}");
        OnObjectiveAssigned?.Invoke(player, objective);
    }

    // ==========================================
    // QUERY
    // ==========================================
    public PersonalObjectiveData GetObjective(PlayerData player)
    {
        assignments.TryGetValue(player.playerName, out var obj);
        return obj;
    }

    public PersonalObjectiveData GetObjective(string playerName)
    {
        assignments.TryGetValue(playerName, out var obj);
        return obj;
    }

    public bool HasObjective(PlayerData player) =>
        assignments.ContainsKey(player.playerName);

    // ใช้โดย ChangeObjectiveSystem
    public void ReplaceObjective(PlayerData player, PersonalObjectiveData newObjective)
    {
        assignments[player.playerName] = newObjective;
        Debug.Log($"[ObjectiveAssignment] {player.playerName} เปลี่ยน Objective → {newObjective.objectiveName}");
        OnObjectiveAssigned?.Invoke(player, newObjective);
    }
}
