using UnityEngine;

public class LevelRestarter : MonoBehaviour
{
    [Header("Restart Settings")]
    public KeyCode restartKey = KeyCode.R;
    public KeyCode hardRestartKey = KeyCode.H;

    [Header("UI")]
    public GameObject restartNotificationPanel;
    public TMPro.TextMeshProUGUI restartNotificationText;

    void Update()
    {
        if (Input.GetKeyDown(restartKey))
        {
            RestartLevel();
        }

        if (Input.GetKeyDown(hardRestartKey))
        {
            HardRestartLevel();
        }
    }

    void RestartLevel()
    {
        if (SaveManager.Instance != null)
        {
            if (restartNotificationPanel != null)
            {
                restartNotificationPanel.SetActive(true);
                if (restartNotificationText != null)
                    restartNotificationText.text = "ПЕРЕЗАПУСК...";
            }

            SaveManager.Instance.RestartLevel();
        }
        else
        {
            Debug.LogError("SaveManager не найден");
        }
    }

    void HardRestartLevel()
    {
        if (SaveManager.Instance != null)
        {
            if (restartNotificationPanel != null)
            {
                restartNotificationPanel.SetActive(true);
                if (restartNotificationText != null)
                    restartNotificationText.text = "ЖЕСТКИЙ ПЕРЕЗАПУСК...";
            }

            SaveManager.Instance.HardRestart();
        }
        else
        {
            Debug.LogError("SaveManager не найден");
        }
    }
}