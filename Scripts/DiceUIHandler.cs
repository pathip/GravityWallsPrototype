using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// DiceUIHandler — รับ Event จาก DiceRoller แล้วอัปเดต UI
// แยกออกจาก DiceRoller เพื่อให้ Logic กับ UI ไม่ปนกัน
//
// ผูก DiceRoller.OnDiceRolled / OnCritical / OnFumble
// แล้ว:
//   - แสดงผลลัพธ์ใน Text
//   - เล่น Animation เขย่าลูกเต๋า
//   - แสดง Panel Critical / Fumble

public class DiceUIHandler : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private DiceRoller diceRoller;
    [SerializeField] private TurnManager turnManager;

    [Header("UI — Result")]
    [SerializeField] private TextMeshProUGUI labelResult;     // ผลรวม เช่น "15"
    [SerializeField] private TextMeshProUGUI labelDetail;     // รายละเอียด เช่น "[3,5,7] +0"
    [SerializeField] private TextMeshProUGUI labelRollerName; // ชื่อผู้ทอย

    [Header("UI — Buttons")]
    [SerializeField] private Button btnRollD6;
    [SerializeField] private Button btnRollD20;
    [SerializeField] private Button btnRollAdvantage;
    //[SerializeField] private Button btnRollDisadvantage;

    [Header("UI — Special Panels")]
    [SerializeField] private GameObject panelCritical;  // แสดงตอน Critical
    [SerializeField] private GameObject panelFumble;    // แสดงตอน Fumble

    [Header("Animation")]
    [SerializeField] private RectTransform diceIcon;    // Icon ลูกเต๋าที่จะ animate
    [SerializeField] private float shakeDuration = 0.4f;

    // ==========================================
    // LIFECYCLE
    // ==========================================
    void Awake()
    {
        diceRoller.OnDiceRolled += HandleRolled;
        diceRoller.OnCritical += HandleCritical;
        diceRoller.OnFumble += HandleFumble;

        // ผูกปุ่มกับ TurnManager เพื่อดึง CurrentPlayer
        btnRollD6.onClick.AddListener(() =>
            diceRoller.Roll(turnManager.CurrentPlayer, 2, 6));

        btnRollD20.onClick.AddListener(() =>
            diceRoller.Roll(turnManager.CurrentPlayer, 1, 20));

        btnRollAdvantage.onClick.AddListener(() =>
            diceRoller.RollAdvantage(turnManager.CurrentPlayer, 1, 20));

        /*btnRollDisadvantage.onClick.AddListener(() =>
            diceRoller.RollDisadvantage(turnManager.CurrentPlayer, 1, 20));*/

        HideSpecialPanels();
    }

    void OnDestroy()
    {
        diceRoller.OnDiceRolled -= HandleRolled;
        diceRoller.OnCritical -= HandleCritical;
        diceRoller.OnFumble -= HandleFumble;
    }

    // ==========================================
    // EVENT HANDLERS
    // ==========================================
    private void HandleRolled(DiceResult result)
    {
        HideSpecialPanels();

        if (labelResult != null)
            labelResult.text = result.total.ToString();

        if (labelDetail != null)
        {
            string rolls = string.Join(", ", result.rolls);
            string modStr = result.modifier != 0
                ? (result.modifier > 0 ? $" +{result.modifier}" : $" {result.modifier}")
                : "";
            labelDetail.text = $"[{rolls}]{modStr}  ({result.diceCount}d{result.sides})";
        }

        if (labelRollerName != null)
            labelRollerName.text = result.rollerName;

        // เล่น Animation เขย่า
        if (diceIcon != null)
            StartCoroutine(ShakeDice());
    }

    private void HandleCritical(DiceResult result)
    {
        if (panelCritical != null)
        {
            panelCritical.SetActive(true);
            StartCoroutine(HideAfter(panelCritical, 2f));
        }
    }

    private void HandleFumble(DiceResult result)
    {
        if (panelFumble != null)
        {
            panelFumble.SetActive(true);
            StartCoroutine(HideAfter(panelFumble, 2f));
        }
    }

    // ==========================================
    // HELPERS
    // ==========================================
    private void HideSpecialPanels()
    {
        if (panelCritical != null) panelCritical.SetActive(false);
        if (panelFumble != null) panelFumble.SetActive(false);
    }

    private IEnumerator HideAfter(GameObject panel, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (panel != null) panel.SetActive(false);
    }

    // เขย่า diceIcon แบบง่าย
    private IEnumerator ShakeDice()
    {
        Vector3 origin = diceIcon.anchoredPosition;
        float t = 0f;

        while (t < shakeDuration)
        {
            float x = Random.Range(-6f, 6f);
            float y = Random.Range(-6f, 6f);
            diceIcon.anchoredPosition = origin + new Vector3(x, y, 0);
            t += Time.deltaTime;
            yield return null;
        }

        diceIcon.anchoredPosition = origin;
    }
}
