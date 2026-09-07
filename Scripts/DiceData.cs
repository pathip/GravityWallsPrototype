using UnityEngine;
using System.Collections.Generic;

// DiceData — เก็บผลการทอยลูกเต๋า 1 ครั้ง
// ใช้ส่งต่อระหว่าง DiceRoller, TurnManager, DiceUIHandler
[System.Serializable]
public class DiceResult
{
    public int sides;          // หน้าลูกเต๋า (d4, d6, d8, d10, d12, d20)
    public int diceCount;      // จำนวนลูกที่ทอย
    public List<int> rolls;          // ผลทีละลูก เช่น [3, 5, 2]
    public int modifier;       // bonus/penalty เพิ่มลบจากผลรวม
    public int total;          // rolls.Sum() + modifier
    public bool isCritical;     // total == sides * diceCount (สูงสุด)
    public bool isFumble;       // total - modifier == diceCount (ต่ำสุด)
    public RollMode mode;           // Normal / Advantage / Disadvantage
    public string rollerName;     // ชื่อผู้เล่นที่ทอย

    public DiceResult(int sides, int diceCount, int modifier,
                      List<int> rolls, RollMode mode, string rollerName)
    {
        this.sides = sides;
        this.diceCount = diceCount;
        this.modifier = modifier;
        this.rolls = rolls;
        this.mode = mode;
        this.rollerName = rollerName;

        // คำนวณ total
        int sum = 0;
        foreach (var r in rolls) sum += r;
        this.total = sum + modifier;

        // Critical: ทุกลูกออกหน้าสูงสุด
        this.isCritical = (sum == sides * diceCount);

        // Fumble: ทุกลูกออก 1
        this.isFumble = (sum == diceCount);
    }

    public override string ToString()
    {
        string rollStr = string.Join(", ", rolls);
        string modStr = modifier != 0
            ? (modifier > 0 ? $" +{modifier}" : $" {modifier}")
            : "";
        string tag = isCritical ? " [CRITICAL!]" : isFumble ? " [FUMBLE]" : "";
        return $"{rollerName} ทอย {diceCount}d{sides}{modStr} → [{rollStr}] = {total}{tag}";
    }
}

// Mode การทอย
public enum RollMode
{
    Normal,       // ทอยปกติ
    Advantage,    // ทอย 2 ชุด เอาชุดที่สูงกว่า  (D&D Advantage)
    Disadvantage  // ทอย 2 ชุด เอาชุดที่ต่ำกว่า  (D&D Disadvantage)
}
