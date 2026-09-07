using UnityEngine;
using System.Collections.Generic;

// DiceRoller — ระบบทอยลูกเต๋าหลัก
// เชื่อมกับ TurnManager ผ่าน Event OnDiceRolled
// เชื่อมกับ PlayerData สำหรับระบุผู้ทอย
//
// ใช้งาน:
//   diceRoller.Roll(player, 2, 6)           → ทอย 2d6 ปกติ
//   diceRoller.Roll(player, 1, 20, 3)        → ทอย 1d20 +3
//   diceRoller.RollAdvantage(player, 1, 20)  → ทอย Advantage
//   diceRoller.RollDisadvantage(player,1,20) → ทอย Disadvantage

public class DiceRoller : MonoBehaviour
{
    // ==========================================
    // EVENTS — TurnManager / UI subscribe ได้
    // ==========================================
    public event System.Action<DiceResult> OnDiceRolled;   // ทุกครั้งที่ทอย
    public event System.Action<DiceResult> OnCritical;     // เมื่อ Critical
    public event System.Action<DiceResult> OnFumble;       // เมื่อ Fumble

    // ==========================================
    // VALIDATION — หน้าที่รองรับ
    // ==========================================
    private static readonly int[] validSides = { 2, 4, 6, 8, 10, 12, 20, 100 };

    private bool IsValidSides(int sides)
    {
        foreach (var s in validSides)
            if (s == sides) return true;
        return false;
    }

    // ==========================================
    // PUBLIC API
    // ==========================================

    // ทอยปกติ — nDice ลูก, sides หน้า, modifier บวกลบ
    public DiceResult Roll(PlayerData player, int nDice, int sides, int modifier = 0)
    {
        return ExecuteRoll(player, nDice, sides, modifier, RollMode.Normal);
    }

    // Advantage — ทอย 2 ชุด เอาชุด total สูงกว่า
    public DiceResult RollAdvantage(PlayerData player, int nDice, int sides, int modifier = 0)
    {
        return ExecuteRoll(player, nDice, sides, modifier, RollMode.Advantage);
    }

    // Disadvantage — ทอย 2 ชุด เอาชุด total ต่ำกว่า
    public DiceResult RollDisadvantage(PlayerData player, int nDice, int sides, int modifier = 0)
    {
        return ExecuteRoll(player, nDice, sides, modifier, RollMode.Disadvantage);
    }

    // ==========================================
    // CORE ROLL LOGIC
    // ==========================================
    private DiceResult ExecuteRoll(PlayerData player, int nDice, int sides,
                                   int modifier, RollMode mode)
    {
        // Validate
        if (nDice <= 0)
        {
            Debug.LogWarning($"[DiceRoller] nDice ต้องมากกว่า 0");
            nDice = 1;
        }
        if (!IsValidSides(sides))
        {
            Debug.LogWarning($"[DiceRoller] sides {sides} ไม่ถูกต้อง — ใช้ d6 แทน");
            sides = 6;
        }

        string rollerName = player != null ? player.playerName : "Unknown";
        DiceResult result;

        if (mode == RollMode.Normal)
        {
            var rolls = RollDice(nDice, sides);
            result = new DiceResult(sides, nDice, modifier, rolls, mode, rollerName);
        }
        else
        {
            // ทอย 2 ชุด แล้วเลือกตาม mode
            var rollsA = RollDice(nDice, sides);
            var rollsB = RollDice(nDice, sides);

            int sumA = Sum(rollsA);
            int sumB = Sum(rollsB);

            List<int> chosen;
            if (mode == RollMode.Advantage)
                chosen = sumA >= sumB ? rollsA : rollsB;
            else
                chosen = sumA <= sumB ? rollsA : rollsB;

            result = new DiceResult(sides, nDice, modifier, chosen, mode, rollerName);
        }

        // Log
        Debug.Log($"[DiceRoller] {result}");

        // Fire Events
        OnDiceRolled?.Invoke(result);
        if (result.isCritical) OnCritical?.Invoke(result);
        if (result.isFumble) OnFumble?.Invoke(result);

        return result;
    }

    // ==========================================
    // HELPERS
    // ==========================================

    // ทอย nDice ลูก sides หน้า — คืน List ผลแต่ละลูก
    private List<int> RollDice(int nDice, int sides)
    {
        var results = new List<int>();
        for (int i = 0; i < nDice; i++)
            results.Add(Random.Range(1, sides + 1));
        return results;
    }

    private int Sum(List<int> list)
    {
        int s = 0;
        foreach (var v in list) s += v;
        return s;
    }

    // ==========================================
    // SHORTCUT — ทอยลูกเต๋าทั่วไปโดยไม่ต้องมี player
    // ใช้ในกรณีทอยเพื่อ event กลาง ไม่ใช่ของผู้เล่น
    // ==========================================
    public DiceResult RollAnonymous(int nDice, int sides, int modifier = 0)
    {
        return Roll(null, nDice, sides, modifier);
    }
}
