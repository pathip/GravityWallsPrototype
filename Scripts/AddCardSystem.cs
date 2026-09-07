using UnityEngine;
using System.Collections.Generic;

// AddCardSystem — ระบบเพิ่มการ์ดเข้า Deck
// เชื่อมกับ DeckManager, ShuffleSystem, CardInstance, TarotCard
//
// 3 Mode หลัก:
//   AddToTop    — เพิ่มบนสุด (จั่วได้ทันทีครั้งถัดไป)
//   AddToBottom — เพิ่มล่างสุด (คิวสุดท้าย)
//   AddAtRandom — แทรกตำแหน่งสุ่มกลาง Deck
//
// Optional: reshuffle หลังเพิ่ม เพื่อกระจายการ์ดใหม่

public class AddCardSystem : MonoBehaviour
{
    public enum AddMode { Top, Bottom, Random }

    [SerializeField] private DeckManager deckManager;
    [SerializeField] private ShuffleSystem shuffleSystem;

    // ถ้าเปิด จะ Shuffle ทันทีหลังเพิ่มการ์ดทุกครั้ง
    [SerializeField] private bool reshuffleAfterAdd = false;

    // ==========================================
    // PUBLIC API — เพิ่มการ์ด 1 ใบ
    // ==========================================

    // เพิ่มจาก TarotCard (ScriptableObject) โดยตรง
    // Unity สร้าง CardInstance ให้อัตโนมัติ
    public void AddCard(TarotCard cardData, AddMode mode = AddMode.Top, bool isReversed = false)
    {
        if (cardData == null)
        {
            Debug.LogWarning("AddCardSystem: cardData is null");
            return;
        }

        var instance = new CardInstance(cardData, isReversed);
        AddInstance(instance, mode);
    }

    // เพิ่มจาก CardInstance ที่มีอยู่แล้ว
    // เช่น การ์ดที่ดึงจาก Discard Pile กลับเข้า Deck
    public void AddInstance(CardInstance instance, AddMode mode = AddMode.Top)
    {
        if (instance == null)
        {
            Debug.LogWarning("AddCardSystem: instance is null");
            return;
        }

        int targetIndex = GetTargetIndex(mode);
        deckManager.InsertAt(instance, targetIndex);

        Debug.Log($"[AddCard] {instance.data.cardName} " +
                  $"(Reversed: {instance.isReversed}) " +
                  $"→ {mode} (index {targetIndex}) | " +
                  $"Deck: {deckManager.DeckCount} ใบ");

        if (reshuffleAfterAdd)
            shuffleSystem.FisherYates(GetDeckRef());
    }

    // ==========================================
    // PUBLIC API — เพิ่มหลายใบพร้อมกัน
    // ==========================================
    public void AddCards(List<TarotCard> cardDataList, AddMode mode = AddMode.Top, bool isReversed = false)
    {
        if (cardDataList == null || cardDataList.Count == 0) return;

        foreach (var cardData in cardDataList)
            AddCard(cardData, mode, isReversed);
    }

    public void AddInstances(List<CardInstance> instances, AddMode mode = AddMode.Top)
    {
        if (instances == null || instances.Count == 0) return;

        foreach (var instance in instances)
            AddInstance(instance, mode);
    }

    // ==========================================
    // เพิ่มการ์ดจาก Discard Pile กลับเข้า Deck
    // ใช้กรณี Recycle การ์ดที่ถูก Discard ไปแล้ว
    // ==========================================
    public void RecycleFromDiscard(List<CardInstance> discardPile, AddMode mode = AddMode.Random)
    {
        if (discardPile == null || discardPile.Count == 0)
        {
            Debug.LogWarning("AddCardSystem: discardPile ว่างเปล่า");
            return;
        }

        AddInstances(discardPile, mode);
        discardPile.Clear();

        Debug.Log($"[RecycleDiscard] เพิ่มกลับเข้า Deck แล้ว | Deck: {deckManager.DeckCount} ใบ");
    }

    // ==========================================
    // PRIVATE HELPERS
    // ==========================================

    // คำนวณ index จาก AddMode
    // index 0            = ล่างสุดของ List (จั่วได้คนสุดท้าย)
    // index DeckCount    = บนสุดของ List  (จั่วได้ครั้งถัดไป)
    private int GetTargetIndex(AddMode mode)
    {
        switch (mode)
        {
            case AddMode.Top:
                return deckManager.DeckCount;          // บนสุด

            case AddMode.Bottom:
                return 0;                               // ล่างสุด

            case AddMode.Random:
                // แทรกระหว่าง index 1 ถึง DeckCount-1
                // เพื่อไม่ให้ออกมาทันทีหรืออยู่ล่างสุดเลย
                return Random.Range(1, Mathf.Max(2, deckManager.DeckCount));

            default:
                return deckManager.DeckCount;
        }
    }

    // ดึง reference ของ deck จาก DeckManager เพื่อส่งให้ ShuffleSystem
    // ใช้เฉพาะกรณี reshuffleAfterAdd = true
    private List<CardInstance> GetDeckRef()
    {
        // เข้าถึง deck โดยใช้ DrawCards แบบ peek ไม่ได้
        // วิธีที่ถูกต้องคือ expose method ใน DeckManager แทน
        // ดู DeckManager.GetDeckRef() ด้านล่าง
        return deckManager.GetDeckRef();
    }
}
