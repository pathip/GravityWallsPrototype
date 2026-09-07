using UnityEngine;
using System.Collections.Generic;

// PersonalObjectiveData — ScriptableObject เก็บข้อมูล Objective แต่ละใบ
// สร้างได้จาก Assets > Create > Objective > Personal Objective
//
// ตัวอย่าง Objective:
//   "จั่วการ์ด The Fool 3 ครั้ง"
//   "ทอยลูกเต๋าได้ค่ารวม >= 15"
//   "อยู่รอดจนถึง Round 5"
//   "ทอยเหรียญได้ Head 3 ครั้ง"

[CreateAssetMenu(fileName = "NewObjective", menuName = "Objective/Personal Objective")]
public class PersonalObjectiveData : ScriptableObject
{
    // ==========================================
    // ประเภท Condition ที่รองรับ
    // ==========================================
    public enum ConditionType
    {
        DrawSpecificCard,       // จั่วการ์ดที่กำหนด N ครั้ง
        DrawAnyCard,            // จั่วการ์ดรวม N ใบ
        SurviveUntilRound,      // อยู่รอดถึง Round N
        RollDiceTotal,          // ทอยลูกเต๋าได้ค่ารวม >= N ใน 1 ครั้ง
        RollDiceCount,          // ทอยลูกเต๋า N ครั้ง
        FlipCoinHead,           // ทอยเหรียญได้ Head N ครั้ง
        FlipCoinTail,           // ทอยเหรียญได้ Tail N ครั้ง
        WinGroupFlipMajority,   // ชนะ Majority ใน Group Flip N ครั้ง
        CollectCards,           // มีการ์ดใน Hand >= N ใบพร้อมกัน
        EndTurnCount,           // กด End Turn ครบ N ครั้ง
        Custom,                 // กำหนดเองผ่าน Code
    }

    [Header("ข้อมูลพื้นฐาน")]
    public string objectiveID;          // ID ไม่ซ้ำกัน เช่น "OBJ_001"
    public string objectiveName;        // ชื่อสั้น เช่น "The Fool's Journey"
    [TextArea(2, 4)]
    public string description;          // คำอธิบายเต็ม
    [TextArea(2, 3)]
    public string hintText;             // ข้อความ Hint บอกใบ้

    [Header("เงื่อนไข")]
    public ConditionType conditionType;
    public int targetCount;       // จำนวนที่ต้องทำให้ครบ
    public TarotCard targetCard;        // สำหรับ DrawSpecificCard
    public int targetRound;       // สำหรับ SurviveUntilRound
    public int targetDiceTotal;   // สำหรับ RollDiceTotal

    [Header("การตั้งค่า")]
    public bool canChange = true;     // เปลี่ยน Objective ได้ระหว่างเกมไหม
    public bool isSecret = true;     // ซ่อนจากผู้เล่นคนอื่นไหม
    public int changeCount = 1;        // เปลี่ยนได้กี่ครั้ง
}
