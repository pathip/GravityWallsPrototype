using UnityEngine;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
    [SerializeField] private List<TarotCard> allCards;
    [SerializeField] private ShuffleSystem shuffleSystem;

    // Mode เลือกได้จาก Inspector
    [SerializeField]
    private ShuffleSystem.ShuffleMode shuffleMode
        = ShuffleSystem.ShuffleMode.FisherYates;

    private List<CardInstance> deck = new List<CardInstance>();
    private List<CardInstance> discard = new List<CardInstance>();

    void Start()
    {
        BuildDeck();
        Shuffle();
    }

    // ==========================================
    // BUILD
    // ==========================================
    public void BuildDeck()
    {
        deck.Clear();
        foreach (var card in allCards)
            deck.Add(new CardInstance(card));
    }

    // ==========================================
    // SHUFFLE
    // ==========================================
    public void Shuffle()
    {
        shuffleSystem.Shuffle(deck, shuffleMode);
    }

    public void ShuffleAndCut()
    {
        shuffleSystem.FisherYates(deck);
        shuffleSystem.CutDeck(deck);
    }

    // ==========================================
    // DRAW
    // ==========================================
    public List<CardInstance> DrawCards(int n = 1)
    {
        if (deck.Count == 0) return null;
        n = Mathf.Min(n, deck.Count);
        var drawn = deck.GetRange(deck.Count - n, n);
        deck.RemoveRange(deck.Count - n, n);
        return drawn;
    }

    // Peek การ์ดบนสุดโดยไม่ดึงออก
    public CardInstance PeekTop()
    {
        if (deck.Count == 0) return null;
        return deck[deck.Count - 1];
    }

    // ==========================================
    // ADD CARD — ใช้โดย AddCardSystem
    // index 0          = ล่างสุด (จั่วได้คนสุดท้าย)
    // index DeckCount  = บนสุด  (จั่วได้ครั้งถัดไป)
    // ==========================================
    public void InsertAt(CardInstance instance, int index)
    {
        index = Mathf.Clamp(index, 0, deck.Count);
        deck.Insert(index, instance);
    }

    // ให้ AddCardSystem เข้าถึง deck reference
    // เพื่อส่งต่อให้ ShuffleSystem ในกรณี reshuffleAfterAdd
    public List<CardInstance> GetDeckRef()
    {
        return deck;
    }

    // ==========================================
    // DISCARD
    // ==========================================
    public void AddToDiscard(List<CardInstance> cards)
    {
        discard.AddRange(cards);
    }

    public List<CardInstance> GetDiscardRef()
    {
        return discard;
    }

    public void ReshuffleDiscard()
    {
        deck.AddRange(discard);
        discard.Clear();
        Shuffle();
    }

    // ==========================================
    // PROPERTIES
    // ==========================================
    public int DeckCount => deck.Count;
    public int DiscardCount => discard.Count;
}