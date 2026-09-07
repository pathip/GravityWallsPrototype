using UnityEngine;
using System.Collections.Generic;

public class DrawSystem : MonoBehaviour
{
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private Transform handParent;  // Parent ของ Card GameObjects
    [SerializeField] private GameObject cardPrefab;  // Prefab มี CardDisplay component

    private List<CardInstance> hand = new List<CardInstance>();

    void Start()
    {
        deckManager.BuildDeck();
        deckManager.Shuffle();
    }

    // จั่ว 1 ใบ
    public void DrawOne()
    {
        var cards = deckManager.DrawCards(1);
        if (cards == null)
        {
            Debug.LogWarning("Deck is empty!");
            return;
        }
        hand.AddRange(cards);
        SpawnCardObjects(cards);
    }

    // จั่ว 3 ใบ (Past / Present / Future Spread)
    public void DrawSpread()
    {
        var cards = deckManager.DrawCards(3);
        if (cards == null) return;

        hand.AddRange(cards);
        SpawnCardObjects(cards);
    }

    // สร้าง GameObject สำหรับแต่ละการ์ด
    private void SpawnCardObjects(List<CardInstance> cards)
    {
        foreach (var card in cards)
        {
            var go = Instantiate(cardPrefab, handParent);
            //go.GetComponent<CardDisplay>().Setup(card);
        }
    }

    // Discard Hand ทั้งหมด
    public void DiscardHand()
    {
        deckManager.AddToDiscard(hand);
        hand.Clear();

        foreach (Transform child in handParent)
            Destroy(child.gameObject);
    }
}
