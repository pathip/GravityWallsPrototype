using UnityEngine;
using System.Collections.Generic;

public class ShuffleSystem : MonoBehaviour
{
    public enum ShuffleMode { FisherYates, Riffle, Cut }

    // โอกาสการ์ดจะ Reversed หลัง Shuffle (0 - 1)
    [SerializeField] private float reversedChance = 0.3f;

    // ===== Fisher-Yates =====
    // ทุก permutation มีโอกาสเกิดเท่ากัน (1/n!)
    // ใช้เป็น default เสมอ
    public void FisherYates(List<CardInstance> deck)
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (deck[i], deck[j]) = (deck[j], deck[i]);
            deck[i].isReversed = Random.value < reversedChance;
        }
    }

    // ===== Riffle Shuffle =====
    // ตัดกองครึ่ง แล้วสอดสลับทีละใบ
    // ได้ผลดีขึ้นถ้าทำซ้ำ 7 รอบ (Bayer-Diaconis theorem)
    public void RiffleShuffle(List<CardInstance> deck)
    {
        int mid = deck.Count / 2;
        var left = deck.GetRange(0, mid);
        var right = deck.GetRange(mid, deck.Count - mid);

        deck.Clear();
        int l = 0, r = 0;

        while (l < left.Count || r < right.Count)
        {
            bool takeLeft = (r >= right.Count) ||
                            (l < left.Count && Random.value < 0.5f);

            var card = takeLeft ? left[l++] : right[r++];
            card.isReversed = Random.value < reversedChance;
            deck.Add(card);
        }
    }

    // ===== Cut Deck =====
    // ตัดกองที่ตำแหน่งสุ่ม (30%-70%) แล้วสลับส่วนบน/ล่าง
    // ไม่เปลี่ยน order — มักใช้ต่อจาก FisherYates
    public void CutDeck(List<CardInstance> deck)
    {
        int cut = Mathf.RoundToInt(
            deck.Count * (0.3f + Random.value * 0.4f)
        );
        var bottom = deck.GetRange(0, cut);
        var top = deck.GetRange(cut, deck.Count - cut);

        deck.Clear();
        deck.AddRange(top);
        deck.AddRange(bottom);
    }

    // ===== Shuffle ตาม Mode =====
    public void Shuffle(List<CardInstance> deck, ShuffleMode mode)
    {
        switch (mode)
        {
            case ShuffleMode.FisherYates:
                FisherYates(deck); break;
            case ShuffleMode.Riffle:
                RiffleShuffle(deck); break;
            case ShuffleMode.Cut:
                CutDeck(deck); break;
        }
    }
}
