using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

// CoinFlipUIHandler — รับ Event จาก CoinFlipSystem แล้วอัปเดต UI
// แยกออกจาก CoinFlipSystem เพื่อให้ Logic กับ UI ไม่ปนกัน
//
// วาง Component นี้ไว้บน UICanvas พร้อมกับ TurnUIHandler, DiceUIHandler

public class CoinFlipUIHandler : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private CoinFlipSystem coinFlipSystem;
    [SerializeField] private TurnManager turnManager;

    [Header("UI — Solo Panel")]
    [SerializeField] private GameObject panelSolo;
    [SerializeField] private TextMeshProUGUI labelSoloResult;    // "Head" หรือ "Tail"
    [SerializeField] private TextMeshProUGUI labelSoloPlayerName;
    [SerializeField] private Button btnFlipSolo;

    [Header("UI — Group Panel")]
    [SerializeField] private GameObject panelGroup;
    [SerializeField] private TextMeshProUGUI labelMajority;      // "Majority: Head — Player1, Player2"
    [SerializeField] private TextMeshProUGUI labelMinority;      // "Minority: Tail — Player3"
    [SerializeField] private TextMeshProUGUI labelGroupDetail;   // รายละเอียดทีละคน
    [SerializeField] private TextMeshProUGUI labelTie;           // แสดงตอนเสมอ
    [SerializeField] private Button btnFlipGroup;

    [Header("Animation")]
    [SerializeField] private RectTransform coinIcon;
    [SerializeField] private float flipDuration = 0.5f;

    // ==========================================
    // LIFECYCLE
    // ==========================================
    void Awake()
    {
        coinFlipSystem.OnSoloFlip += HandleSoloFlip;
        coinFlipSystem.OnGroupFlip += HandleGroupFlip;

        // ปุ่ม Solo — ทอยเหรียญของ CurrentPlayer
        btnFlipSolo.onClick.AddListener(() =>
            coinFlipSystem.FlipSolo(turnManager.CurrentPlayer));

        // ปุ่ม Group — ทอยทุก Active Players
        btnFlipGroup.onClick.AddListener(() =>
            coinFlipSystem.FlipGroup(turnManager.GetActivePlayers()));

        HideAllPanels();
    }

    void OnDestroy()
    {
        coinFlipSystem.OnSoloFlip -= HandleSoloFlip;
        coinFlipSystem.OnGroupFlip -= HandleGroupFlip;
    }

    // ==========================================
    // EVENT HANDLERS
    // ==========================================
    private void HandleSoloFlip(CoinFlipSystem.CoinFlipResult result)
    {
        panelSolo.SetActive(true);
        panelGroup.SetActive(false);

        if (labelSoloResult != null)
        {
            labelSoloResult.text = result.result.ToString(); // "Head" หรือ "Tail"
            labelSoloResult.color = result.isHead
                ? new Color(0.9f, 0.7f, 0.1f)  // สีทอง = Head
                : new Color(0.6f, 0.6f, 0.6f); // สีเทา  = Tail
        }

        if (labelSoloPlayerName != null)
            labelSoloPlayerName.text = result.playerName;

        if (coinIcon != null)
            StartCoroutine(AnimateCoinFlip());
    }

    private void HandleGroupFlip(CoinFlipSystem.GroupFlipResult result)
    {
        panelSolo.SetActive(false);
        panelGroup.SetActive(true);

        // กรณีเสมอ
        if (result.isTie)
        {
            if (labelTie != null)
            {
                labelTie.gameObject.SetActive(true);
                labelTie.text = $"Tie! Head: {result.headCount} | Tail: {result.tailCount}";
            }
            if (labelMajority != null) labelMajority.gameObject.SetActive(false);
            if (labelMinority != null) labelMinority.gameObject.SetActive(false);
        }
        else
        {
            if (labelTie != null) labelTie.gameObject.SetActive(false);

            if (labelMajority != null)
            {
                labelMajority.gameObject.SetActive(true);
                string majNames = string.Join(", ", result.majorityPlayers);
                labelMajority.text = $"Majority: {result.majoritySide}\n{majNames}";
            }

            if (labelMinority != null)
            {
                labelMinority.gameObject.SetActive(true);
                string minNames = string.Join(", ", result.minorityPlayers);
                labelMinority.text = $"Minority: {result.minoritySide}\n{minNames}";
            }
        }

        // รายละเอียดทีละคน
        if (labelGroupDetail != null)
        {
            string detail = "";
            foreach (var f in result.allResults)
                detail += $"{f.playerName}: {f.result}\n";
            labelGroupDetail.text = detail.TrimEnd();
        }

        if (coinIcon != null)
            StartCoroutine(AnimateCoinFlip());
    }

    // ==========================================
    // ANIMATION — หมุนเหรียญ
    // ==========================================
    private IEnumerator AnimateCoinFlip()
    {
        float t = 0f;
        Vector3 origin = coinIcon.localScale;

        while (t < flipDuration)
        {
            // Scale X จาก 1 → 0 → 1 เหมือนเหรียญหมุน
            float scaleX = Mathf.Abs(Mathf.Cos(t / flipDuration * Mathf.PI * 3f));
            coinIcon.localScale = new Vector3(scaleX, 1f, 1f);
            t += Time.deltaTime;
            yield return null;
        }

        coinIcon.localScale = origin;
    }

    // ==========================================
    // HELPER
    // ==========================================
    private void HideAllPanels()
    {
        if (panelSolo != null) panelSolo.SetActive(false);
        if (panelGroup != null) panelGroup.SetActive(false);
    }
}
