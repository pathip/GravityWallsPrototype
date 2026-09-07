using UnityEngine;
using System.Collections.Generic;

// ข้อมูลของผู้เล่นแต่ละคน
// แยกออกจาก TurnManager เพื่อให้ขยายง่าย
[System.Serializable]
public class PlayerData
{
    public string playerName;
    public int hp;
    public int maxHp;
    public bool isEliminated;

    // จำนวน Turn ที่ถูก Skip (เช่น ถูก stun)
    public int skipTurns;

    // Hand ของผู้เล่นคนนี้ — เชื่อมกับ CardInstance
    public List<CardInstance> hand = new List<CardInstance>();

    // Status Effects ที่ติดอยู่ (poison, burn, regen ฯลฯ)
    public List<StatusEffect> statusEffects = new List<StatusEffect>();

    public PlayerData(string name, int hp)
    {
        this.playerName = name;
        this.hp = hp;
        this.maxHp = hp;
        this.isEliminated = false;
        this.skipTurns = 0;
    }

    // ==========================================
    // HP
    // ==========================================
    public void TakeDamage(int amount)
    {
        hp = Mathf.Max(0, hp - amount);
        if (hp <= 0) isEliminated = true;
    }

    public void Heal(int amount)
    {
        hp = Mathf.Min(maxHp, hp + amount);
    }

    // ==========================================
    // STATUS EFFECTS
    // ==========================================
    public void AddEffect(StatusEffect effect)
    {
        statusEffects.Add(effect);
    }

    // Apply effects ทั้งหมด และลด duration
    // เรียกตอนต้น Turn ของผู้เล่นคนนี้
    public void TickEffects()
    {
        for (int i = statusEffects.Count - 1; i >= 0; i--)
        {
            statusEffects[i].Apply(this);
            statusEffects[i].duration--;
            if (statusEffects[i].duration <= 0)
                statusEffects.Remove(statusEffects[i]);
        }
    }

    public bool IsActive => !isEliminated && hp > 0;
}
