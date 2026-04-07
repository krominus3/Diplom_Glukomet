using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelLoader : MonoBehaviour
{
    [Header("Trigger Settings")]
    public string targetSceneName = "NextLevel";
    public string triggerTag = "Player";
    public float loadDelay = 0.5f;
    public bool allowMultipleLoads = false;

    [Header("Time & Player Control")]
    public bool stopTimeOnTrigger = true;
    public float timeScaleOnTrigger = 0f;
    public bool disablePlayerScripts = true;

    [Header("Save Settings")]
    public bool savePlayerPosition = true;
    public bool savePlayerRotation = true;

    [Header("UI References")]
    public GameObject loadingPanel;
    public UnityEngine.UI.Slider loadingProgressBar;
    public TMPro.TextMeshProUGUI loadingText;

    [Header("Effects")]
    public AudioClip triggerSound;
    public ParticleSystem triggerEffect;

    private bool isLoading = false;
    private bool hasTriggered = false;

    private PlayerController playerController;
    private RuleWeapon ruleWeapon;
    private GameObject player;
    private float originalTimeScale;

    void Start()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        FindPlayerScripts();
    }

    void FindPlayerScripts()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
            ruleWeapon = player.GetComponent<RuleWeapon>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isLoading && !hasTriggered && other.CompareTag(triggerTag))
        {
            if (!allowMultipleLoads && hasTriggered)
                return;

            hasTriggered = true;

            // Сохраняем данные игрока ПЕРЕД загрузкой
            if (savePlayerPosition || savePlayerRotation)
            {
                SavePlayerData();
            }

            StartCoroutine(LoadLevelWithDelay());
        }
    }

    void SavePlayerData()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            string currentScene = SceneManager.GetActiveScene().name;

            PlayerData.SavePlayerPosition(playerObj.transform);
            PlayerData.currentLevel = currentScene; // Сохраняем ТЕКУЩИЙ уровень, а не следующий
            PlayerData.SaveLevel();

            Debug.Log($"LevelLoader: Сохранена позиция {playerObj.transform.position} для уровня {currentScene}");
        }
    }

    IEnumerator LoadLevelWithDelay()
    {
        isLoading = true;

        originalTimeScale = Time.timeScale;

        // Останавливаем время
        if (stopTimeOnTrigger)
        {
            Time.timeScale = timeScaleOnTrigger;
        }

        // Отключаем скрипты игрока
        if (disablePlayerScripts)
        {
            if (playerController != null) playerController.enabled = false;
            if (ruleWeapon != null) ruleWeapon.enabled = false;
        }

        // Эффекты
        if (triggerSound != null)
        {
            AudioSource.PlayClipAtPoint(triggerSound, transform.position);
        }

        if (triggerEffect != null)
        {
            Instantiate(triggerEffect, transform.position, Quaternion.identity);
        }

        // Показываем плашку загрузки
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);

            CanvasGroup canvasGroup = loadingPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
                float fadeTime = 0.3f;
                float elapsed = 0;

                while (elapsed < fadeTime)
                {
                    elapsed += Time.unscaledDeltaTime;
                    canvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / fadeTime);
                    yield return null;
                }
                canvasGroup.alpha = 1;
            }
        }

        if (loadingText != null)
        {
            loadingText.text = "ЗАГРУЗКА...";
        }

        // Задержка
        float waitTime = 0;
        while (waitTime < loadDelay)
        {
            waitTime += Time.unscaledDeltaTime;
            yield return null;
        }

        // Загружаем уровень
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            if (loadingProgressBar != null)
            {
                loadingProgressBar.value = progress;
            }

            if (loadingText != null)
            {
                loadingText.text = $"ЗАГРУЗКА... {(progress * 100):F0}%";
            }

            if (asyncLoad.progress >= 0.9f)
            {
                if (loadingText != null)
                {
                    loadingText.text = "НАЖМИТЕ ЛЮБУЮ КЛАВИШУ";
                }

                if (Input.anyKeyDown)
                {
                    asyncLoad.allowSceneActivation = true;
                }
            }

            yield return null;
        }
    }

    public void LoadLevelNow()
    {
        if (!isLoading)
        {
            SavePlayerData();
            StartCoroutine(LoadLevelWithDelay());
        }
    }

    public void LoadSpecificScene(string sceneName)
    {
        targetSceneName = sceneName;
        LoadLevelNow();
    }

    void OnDestroy()
    {
        if (stopTimeOnTrigger)
        {
            Time.timeScale = 1f;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Collider col = GetComponent<Collider>();
        if (col != null && col.isTrigger)
        {
            if (col is BoxCollider)
                Gizmos.DrawWireCube(transform.position + (col as BoxCollider).center, (col as BoxCollider).size);
            else if (col is SphereCollider)
                Gizmos.DrawWireSphere(transform.position + (col as SphereCollider).center, (col as SphereCollider).radius);
            else
                Gizmos.DrawWireCube(transform.position, Vector3.one);
        }
        else
        {
            Gizmos.DrawWireCube(transform.position, Vector3.one);
        }
    }
}