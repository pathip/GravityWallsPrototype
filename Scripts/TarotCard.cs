using UnityEngine;

// ScriptableObject เก็บข้อมูลการ์ดแต่ละใบ
// สร้างได้จาก Assets > Create > Tarot > Card
[CreateAssetMenu(fileName = "NewCard", menuName = "Tarot/Card")]
public class TarotCard : ScriptableObject
{
    public enum Arcana { Major, Minor }
    public enum Suit { None, Wands, Cups, Swords, Pentacles }

    [SerializeField] public string cardName;
    [SerializeField] public Arcana arcana;
    [SerializeField] public Suit suit;           // Minor เท่านั้น
    [SerializeField] public int number;         // Major: 0-21
    [SerializeField] public string uprightMeaning;
    [SerializeField] public string reversedMeaning;
    [SerializeField] public Sprite artwork;
}
