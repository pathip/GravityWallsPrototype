using UnityEngine;

// Runtime state ของการ์ดที่ถูก instantiate
// แยกจาก ScriptableObject เพื่อไม่ให้ data ถาวรเปลี่ยน
public class CardInstance
{
    public TarotCard data;
    public bool isReversed;

    public CardInstance(TarotCard data, bool isReversed = false)
    {
        this.data = data;
        this.isReversed = isReversed;
    }

    public string GetMeaning()
    {
        return isReversed
            ? data.reversedMeaning
            : data.uprightMeaning;
    }
}
