using UnityEngine;

// Base class ของ Status Effect
// ขยายได้โดย inherit แล้ว override Apply()
[System.Serializable]
public abstract class StatusEffect
{
    public string effectName;
    public int duration;    // กี่ Turn ถึงหมด

    public StatusEffect(string name, int duration)
    {
        this.effectName = name;
        this.duration = duration;
    }

    // Logic ของ effect — override ใน subclass
    public abstract void Apply(PlayerData player);
}

// ==========================================
// ตัวอย่าง Effect สำเร็จรูป
// ==========================================

// Poison — ลด HP ทุก Turn
public class PoisonEffect : StatusEffect
{
    private int damagePerTurn;

    public PoisonEffect(int damage, int duration)
        : base("Poison", duration)
    {
        damagePerTurn = damage;
    }

    public override void Apply(PlayerData player)
    {
        player.TakeDamage(damagePerTurn);
        Debug.Log($"[Poison] {player.playerName} -HP {damagePerTurn}");
    }
}

// Regen — ฟื้น HP ทุก Turn
public class RegenEffect : StatusEffect
{
    private int healPerTurn;

    public RegenEffect(int heal, int duration)
        : base("Regen", duration)
    {
        healPerTurn = heal;
    }

    public override void Apply(PlayerData player)
    {
        player.Heal(healPerTurn);
        Debug.Log($"[Regen] {player.playerName} +HP {healPerTurn}");
    }
}

// Stun — ทำให้ถูก Skip Turn ในรอบถัดไป
public class StunEffect : StatusEffect
{
    public StunEffect() : base("Stun", 1) { }

    public override void Apply(PlayerData player)
    {
        player.skipTurns++;
        Debug.Log($"[Stun] {player.playerName} ถูก Skip Turn");
    }
}
