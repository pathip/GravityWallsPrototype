using UnityEngine;
using UnityEngine.UI;
using TMPro;

// PlayerResultDisplay — แสดงผลของผู้เล่นแต่ละคนใน End Game Panel
// แขวนบน PlayerResultPrefab

public class PlayerResultDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI labelPlayerName;
    [SerializeField] private TextMeshProUGUI labelObjective;
    [SerializeField] private TextMeshProUGUI labelProgress;
    [SerializeField] private TextMeshProUGUI labelStatus;    // Win / Lose / Eliminated
    [SerializeField] private TextMeshProUGUI labelHP;
    [SerializeField] private Image bgImage;        // สีพื้นหลัง

    public void Setup(EndGameResult.PlayerEndResult result)
    {
        if (labelPlayerName != null)
            labelPlayerName.text = result.playerName;

        if (labelObjective != null)
            labelObjective.text = result.objectiveName;

        if (labelProgress != null)
            labelProgress.text = result.progressText;

        if (labelHP != null)
            labelHP.text = $"HP: {result.hpRemaining}";

        if (labelStatus != null)
        {
            if (result.isWinner)
            {
                labelStatus.text = "ESCAPED";
                labelStatus.color = new Color(0.2f, 0.8f, 0.2f); // เขียว
            }
            else if (result.isEliminated)
            {
                labelStatus.text = "ELIMINATED";
                labelStatus.color = new Color(0.8f, 0.2f, 0.2f); // แดง
            }
            else
            {
                labelStatus.text = "SURVIVED";
                labelStatus.color = new Color(0.8f, 0.8f, 0.2f); // เหลือง
            }
        }

        if (bgImage != null)
        {
            bgImage.color = result.isWinner
                ? new Color(0.1f, 0.3f, 0.1f, 0.8f)    // เขียวเข้ม
                : result.isEliminated
                    ? new Color(0.3f, 0.1f, 0.1f, 0.8f) // แดงเข้ม
                    : new Color(0.1f, 0.1f, 0.1f, 0.8f); // ดำ
        }
    }
}
