using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RuleSegment : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public Image backgroundImage;
    public Button button;
    public Image selectionIndicator;

    [Header("Colors")]
    public Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    public Color highlightedColor = new Color(0.4f, 0.6f, 1f, 0.9f);
    public Color selectedColor = new Color(0.2f, 0.8f, 0.2f, 0.9f);

    private RuleType ruleType;
    private RuleWeaponSystem weaponSystem;
    private RuleWheelUI wheelUI;
    private float minAngle;
    private float maxAngle;
    private bool isCurrentRule;

    public RuleType RuleType => ruleType;

    public void Initialize(RuleType rule, RuleWeaponSystem system, RuleWheelUI wheel)
    {
        ruleType = rule;
        weaponSystem = system;
        wheelUI = wheel;

        // Получаем данные о правиле
        var ruleData = system.GetRuleData(rule);
        if (ruleData != null)
        {
            if (iconImage && ruleData.ruleIcon)
                iconImage.sprite = ruleData.ruleIcon;

            if (nameText)
                nameText.text = ruleData.ruleName;
        }
        else
        {
            // Для специальных правил (очиститель)
            if (rule == RuleType.RuleClearer)
            {
                if (nameText)
                    nameText.text = "Очистка правил";
            }
        }

        // Настраиваем кнопку
        if (button)
        {
            button.onClick.AddListener(OnSegmentClicked);
        }

        // Проверяем, является ли это правило текущим
        UpdateCurrentRuleStatus();
    }

    void Update()
    {
        // Обновляем статус текущего правила каждый кадр
        UpdateCurrentRuleStatus();
    }

    void UpdateCurrentRuleStatus()
    {
        if (weaponSystem != null)
        {
            bool isCurrent = (weaponSystem.GetCurrentRule() == ruleType);
            if (isCurrent != isCurrentRule)
            {
                isCurrentRule = isCurrent;
                UpdateSelectionIndicator();
            }
        }
    }

    void UpdateSelectionIndicator()
    {
        if (selectionIndicator)
        {
            selectionIndicator.gameObject.SetActive(isCurrentRule);
        }
    }

    public void SetAngleRange(float min, float max)
    {
        minAngle = min;
        maxAngle = max;
    }

    public bool IsAngleInRange(float angle)
    {
        if (minAngle <= maxAngle)
        {
            return angle >= minAngle && angle <= maxAngle;
        }
        else // Для случаев, когда диапазон переходит через 0
        {
            return angle >= minAngle || angle <= maxAngle;
        }
    }

    public void SetHighlighted(bool highlighted)
    {
        if (backgroundImage)
        {
            if (highlighted)
            {
                backgroundImage.color = highlightedColor;
            }
            else if (isCurrentRule)
            {
                backgroundImage.color = selectedColor;
            }
            else
            {
                backgroundImage.color = normalColor;
            }
        }
    }

    void OnSegmentClicked()
    {
        if (weaponSystem != null)
        {
            weaponSystem.SetCurrentRule(ruleType);

            // Закрываем колесо
            if (wheelUI != null)
            {
                wheelUI.gameObject.SetActive(false);
                if (weaponSystem != null)
                {
                    weaponSystem.CloseRuleWheelManually();
                }
            }
        }
    }
}