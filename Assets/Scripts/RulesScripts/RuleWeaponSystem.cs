using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RuleWeaponSystem : MonoBehaviour, IRuleProvider
{
    [System.Serializable]
    public class RuleData
    {
        public RuleType ruleType;
        public string ruleName;
        public Sprite ruleIcon;
        public Color ruleColor = Color.white;
        public GameObject projectilePrefab;
        public float cooldown = 0.5f;
        public int maxUses = -1; // -1 = бесконечно
    }

    [Header("Rule Settings")]
    public List<RuleData> availableRules = new List<RuleData>();
    public RuleData defaultRule;
    public Transform shootPoint;
    public float shootForce = 20f;

    [Header("UI References")]
    public GameObject ruleWheelUI;
    public UnityEngine.UI.Image currentRuleIcon;
    public TMPro.TextMeshProUGUI currentRuleName;

    private Dictionary<RuleType, RuleData> rulesDictionary = new Dictionary<RuleType, RuleData>();
    private List<RuleType> unlockedRules = new List<RuleType>();
    private RuleType currentRule;
    private float lastShootTime;
    private bool isSelectingRule;
    private float originalTimeScale;

    // Публичное свойство для доступа к разблокированным правилам
    public List<RuleType> UnlockedRules => unlockedRules;
    public System.Action<RuleType> OnRuleChanged;

    public void CloseRuleWheelManually()
    {
        CloseRuleWheel();
    }

    void Start()
    {
        // Заполняем словарь правил
        foreach (var rule in availableRules)
        {
            if (!rulesDictionary.ContainsKey(rule.ruleType))
                rulesDictionary.Add(rule.ruleType, rule);
        }

        // Устанавливаем правило по умолчанию
        if (defaultRule != null)
        {
            AddRule(defaultRule.ruleType);
            SetCurrentRule(defaultRule.ruleType);
        }

        if (ruleWheelUI)
            ruleWheelUI.SetActive(false);

        originalTimeScale = 1f;
    }

    void Update()
    {
        HandleRuleSelection();
        HandleShooting();
        HandleQuickSelect();
    }

    void HandleShooting()
    {
        if (Input.GetButtonDown("Fire1") && Time.time > lastShootTime + GetCurrentCooldown())
        {
            Shoot();
            lastShootTime = Time.time;
        }
    }

    void HandleRuleSelection()
    {
        // Открыть колесо выбора при удержании R
        if (Input.GetKey(KeyCode.R) && !isSelectingRule)
        {
            OpenRuleWheel();
        }
        else if (Input.GetKeyUp(KeyCode.R) && isSelectingRule)
        {
            CloseRuleWheel();
        }

        // Выбор правила в колесе
        if (isSelectingRule && ruleWheelUI)
        {
            // Здесь должна быть логика выбора по направлению мыши
            // Для примера - простой выбор по цифрам
            HandleWheelSelection();
        }
    }

    void HandleQuickSelect()
    {
        // Быстрый выбор правил цифрами 1-4
        for (int i = 0; i < unlockedRules.Count && i < 4; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SetCurrentRule(unlockedRules[i]);
                print(i);
                UpdateUIRuleInfo();
            }
        }

        // Колесо мыши для переключения
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0 && unlockedRules.Count > 0)
        {
            int currentIndex = unlockedRules.IndexOf(currentRule);
            int newIndex = scroll > 0 ?
                (currentIndex + 1) % unlockedRules.Count :
                (currentIndex - 1 + unlockedRules.Count) % unlockedRules.Count;

            SetCurrentRule(unlockedRules[newIndex]);
            print(newIndex);
            UpdateUIRuleInfo();
        }
    }

    void HandleWheelSelection()
    {
        // Пример простого выбора по нажатию цифр во время колеса
        for (int i = 0; i < unlockedRules.Count && i < 8; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SetCurrentRule(unlockedRules[i]);
                UpdateUIRuleInfo();
                CloseRuleWheel();
            }
        }
    }

    void OpenRuleWheel()
    {
        isSelectingRule = true;
        if (ruleWheelUI)
        {
            ruleWheelUI.SetActive(true);
            originalTimeScale = Time.timeScale;
            Time.timeScale = 0.2f; // Замедление времени
        }
    }

    void CloseRuleWheel()
    {
        isSelectingRule = false;
        if (ruleWheelUI)
        {
            ruleWheelUI.SetActive(false);
            Time.timeScale = originalTimeScale;
        }
    }

    void Shoot()
    {
        if (currentRule == RuleType.None || !HasRule(currentRule))
            return;

        RuleData ruleData = GetRuleData(currentRule);
        if (ruleData == null) return;

        // Создаем снаряд с правилом
        if (ruleData.projectilePrefab)
        {
            GameObject projectile = Instantiate(ruleData.projectilePrefab,
                shootPoint.position, shootPoint.rotation);

            RuleProjectile ruleProjectile = projectile.GetComponent<RuleProjectile>();
            if (ruleProjectile)
            {
                ruleProjectile.Initialize(currentRule, shootForce);
            }
            else
            {
                // Если нет компонента снаряда, добавляем физику
                Rigidbody rb = projectile.GetComponent<Rigidbody>();
                if (rb)
                    rb.AddForce(shootPoint.forward * shootForce, ForceMode.Impulse);
            }
        }
        else
        {
            // Рейкаст для мгновенного применения правила
            RaycastHit hit;
            if (Physics.Raycast(shootPoint.position, shootPoint.forward, out hit, 100f))
            {
                IRuleApplicable ruleApplicable = hit.collider.GetComponent<IRuleApplicable>();
                if (ruleApplicable != null && ruleApplicable.CanApplyRule(currentRule))
                {
                    ruleApplicable.ApplyRule(currentRule);
                }
            }
        }

        // Визуальный/звуковой эффект
        OnRuleApplied(currentRule);
    }

    void OnRuleApplied(RuleType rule)
    {
        // Здесь можно добавить эффекты выстрела
        Debug.Log($"Applied rule: {rule}");
    }

    float GetCurrentCooldown()
    {
        RuleData data = GetRuleData(currentRule);
        return data != null ? data.cooldown : 0.5f;
    }

    public RuleData GetRuleData(RuleType rule)
    {
        rulesDictionary.TryGetValue(rule, out RuleData data);
        return data;
    }

    void UpdateUIRuleInfo()
    {
        if (currentRuleIcon)
        {
            RuleData data = GetRuleData(currentRule);
            if (data != null && data.ruleIcon != null)
                currentRuleIcon.sprite = data.ruleIcon;
        }

        if (currentRuleName)
        {
            RuleData data = GetRuleData(currentRule);
            currentRuleName.text = data != null ? data.ruleName : "None";
        }
    }

    // IRuleProvider implementation
    public bool HasRule(RuleType rule) => unlockedRules.Contains(rule);

    public void AddRule(RuleType rule)
    {
        if (!unlockedRules.Contains(rule) && rule != RuleType.None)
        {
            unlockedRules.Add(rule);
            Debug.Log($"Rule unlocked: {rule}");
        }
    }

    public void RemoveRule(RuleType rule)
    {
        unlockedRules.Remove(rule);
        if (currentRule == rule)
        {
            currentRule = unlockedRules.Count > 0 ? unlockedRules[0] : RuleType.None;
            UpdateUIRuleInfo();
        }
    }

    public RuleType GetCurrentRule() => currentRule;

    public void SetCurrentRule(RuleType rule)
    {
        if (HasRule(rule) || rule == RuleType.RuleClearer)
        {
            currentRule = rule;
            UpdateUIRuleInfo();

            // Вызываем событие
            OnRuleChanged?.Invoke(rule);
        }
    }

}