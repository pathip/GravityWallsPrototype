using UnityEngine;
using System.Collections.Generic;

// CoinFlipSystem — ระบบทอยเหรียญ
// รองรับ 2 Mode:
//   1. Solo   — ทอยคนเดียว ได้ Head หรือ Tail
//   2. Group  — ทอยหลายคน นับ majority (ส่วนมาก) และ minority (ส่วนน้อย)
//
// เชื่อมกับ PlayerData และ TurnManager ผ่าน Events

public class CoinFlipSystem : MonoBehaviour
{
    // ==========================================
    // DATA
    // ==========================================
    public enum CoinSide { Head, Tail }

    // ผลการทอยเหรียญ 1 ครั้งของผู้เล่น 1 คน
    [System.Serializable]
    public class CoinFlipResult
    {
        public string playerName;
        public CoinSide result;
        public bool isHead => result == CoinSide.Head;

        public CoinFlipResult(string playerName, CoinSide result)
        {
            this.playerName = playerName;
            this.result = result;
        }

        public override string ToString() =>
            $"{playerName}: {result}";
    }

    // ผลรวมของการทอยแบบกลุ่ม
    [System.Serializable]
    public class GroupFlipResult
    {
        public List<CoinFlipResult> allResults = new List<CoinFlipResult>();
        public List<string> majorityPlayers = new List<string>(); // ผลส่วนมาก
        public List<string> minorityPlayers = new List<string>(); // ผลส่วนน้อย
        public CoinSide majoritySide;
        public CoinSide minoritySide;
        public int headCount;
        public int tailCount;
        public bool isTie;        // หัว = ก้อย (เสมอ)
    }

    // ==========================================
    // EVENTS
    // ==========================================
    public event System.Action<CoinFlipResult> OnSoloFlip;   // ทอยเดี่ยวเสร็จ
    public event System.Action<GroupFlipResult> OnGroupFlip;  // ทอยกลุ่มเสร็จ

    // ==========================================
    // MODE 1 — Solo Flip
    // ทอยเหรียญ 1 ครั้ง คืนผล Head หรือ Tail
    // ==========================================
    public CoinFlipResult FlipSolo(PlayerData player)
    {
        string name = player != null ? player.playerName : "Unknown";
        CoinSide side = Random.value < 0.5f ? CoinSide.Head : CoinSide.Tail;

        var result = new CoinFlipResult(name, side);

        Debug.Log($"[CoinFlip] Solo — {result}");
        OnSoloFlip?.Invoke(result);

        return result;
    }

    // ทอยเดี่ยวโดยไม่ผูกกับ PlayerData (เรียกจาก event กลางได้)
    public CoinFlipResult FlipAnonymous(string label = "Flip")
    {
        CoinSide side = Random.value < 0.5f ? CoinSide.Head : CoinSide.Tail;
        var result = new CoinFlipResult(label, side);

        Debug.Log($"[CoinFlip] Anonymous — {result}");
        OnSoloFlip?.Invoke(result);

        return result;
    }

    // ==========================================
    // MODE 2 — Group Flip
    // ทอยทุกคนใน List พร้อมกัน
    // นับว่าใครได้ผลส่วนมาก (majority) และส่วนน้อย (minority)
    // ==========================================
    public GroupFlipResult FlipGroup(List<PlayerData> players)
    {
        if (players == null || players.Count == 0)
        {
            Debug.LogWarning("[CoinFlip] FlipGroup — ไม่มีผู้เล่น");
            return null;
        }

        var groupResult = new GroupFlipResult();

        // ทอยทีละคน
        foreach (var player in players)
        {
            CoinSide side = Random.value < 0.5f ? CoinSide.Head : CoinSide.Tail;
            var flip = new CoinFlipResult(player.playerName, side);
            groupResult.allResults.Add(flip);

            if (side == CoinSide.Head) groupResult.headCount++;
            else groupResult.tailCount++;
        }

        // คำนวณ Majority / Minority
        groupResult.isTie = groupResult.headCount == groupResult.tailCount;

        if (!groupResult.isTie)
        {
            // Majority = ฝั่งที่มีคนมากกว่า
            groupResult.majoritySide = groupResult.headCount > groupResult.tailCount
                ? CoinSide.Head
                : CoinSide.Tail;

            groupResult.minoritySide = groupResult.majoritySide == CoinSide.Head
                ? CoinSide.Tail
                : CoinSide.Head;
        }
        else
        {
            // เสมอ — ตั้ง majority เป็น Head ตามธรรมเนียม แต่ isTie = true
            groupResult.majoritySide = CoinSide.Head;
            groupResult.minoritySide = CoinSide.Tail;
        }

        // แยก List ผู้เล่นตาม majority/minority
        foreach (var flip in groupResult.allResults)
        {
            if (flip.result == groupResult.majoritySide)
                groupResult.majorityPlayers.Add(flip.playerName);
            else
                groupResult.minorityPlayers.Add(flip.playerName);
        }

        // Log ผลสรุป
        LogGroupResult(groupResult);
        OnGroupFlip?.Invoke(groupResult);

        return groupResult;
    }

    // ==========================================
    // HELPER
    // ==========================================
    private void LogGroupResult(GroupFlipResult r)
    {
        string allFlips = "";
        foreach (var f in r.allResults)
            allFlips += $"\n  {f}";

        if (r.isTie)
        {
            Debug.Log($"[CoinFlip] Group — เสมอ! Head: {r.headCount} | Tail: {r.tailCount}{allFlips}");
        }
        else
        {
            string maj = string.Join(", ", r.majorityPlayers);
            string min = string.Join(", ", r.minorityPlayers);
            Debug.Log($"[CoinFlip] Group — Majority: {r.majoritySide} ({maj}) | Minority: {r.minoritySide} ({min}){allFlips}");
        }
    }
}
