using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RuleWheelUI : MonoBehaviour
{
    [Header("References")]
    public RuleWeaponSystem weaponSystem;
    public GameObject ruleSegmentPrefab;
    public Transform segmentsParent;

    [Header("Wheel Settings")]
    public float radius = 150f;
    public Color selectedColor = Color.yellow;
    public Color normalColor = Color.white;

    private List<RuleSegment> segments = new List<RuleSegment>();
    private RuleType currentSelectedRule;

    void Start()
    {
        if (!weaponSystem)
            weaponSystem = FindObjectOfType<RuleWeaponSystem>();

        if (!segmentsParent)
            segmentsParent = transform;
    }

    void OnEnable()
    {
        UpdateWheel();
    }

    void OnDisable()
    {
        ClearWheel();
    }

    void Update()
    {
        UpdateSelectionFromMouse();

        // Выбор правила по клику мыши
        if (Input.GetMouseButtonDown(0) && currentSelectedRule != RuleType.None)
        {
            SelectCurrentRule();
        }
    }

    /// <summary>
    /// Получает доступные правила из системы оружия
    /// </summary>
    private List<RuleType> GetAvailableRules()
    {
        List<RuleType> rules = new List<RuleType>();

        if (weaponSystem == null)
        {
            Debug.LogError("RuleWeaponSystem not assigned to RuleWheelUI!");
            return rules;
        }

        // Получаем разблокированные правила через публичное свойство
        // (которое мы добавили в RuleWeaponSystem)
        rules = weaponSystem.UnlockedRules;

        // Всегда добавляем очиститель правил, если его нет
        if (!rules.Contains(RuleType.RuleClearer))
        {
            // Можно добавить принудительно, если нужно
            // rules.Add(RuleType.RuleClearer);
        }

        return rules;
    }

    void UpdateWheel()
    {
        ClearWheel();

        List<RuleType> availableRules = GetAvailableRules();
        if (availableRules.Count == 0) return;

        float angleStep = 360f / availableRules.Count;
        float startAngle = -90f; // Начинаем с верха (12 часов)

        for (int i = 0; i < availableRules.Count; i++)
        {
            RuleType rule = availableRules[i];

            // Создаем сегмент
            GameObject segmentObj = Instantiate(ruleSegmentPrefab, segmentsParent);

            // Вычисляем угол для текущего сегмента
            float angle = (startAngle + i * angleStep) * Mathf.Deg2Rad;

            // Позиционируем по кругу
            Vector3 position = new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0
            );

            RectTransform rectTransform = segmentObj.GetComponent<RectTransform>();
            if (rectTransform)
            {
                rectTransform.anchoredPosition = position;

                // Поворачиваем сегмент чтобы смотреть наружу
                float rotationAngle = (startAngle + i * angleStep) + 90f;
                rectTransform.rotation = Quaternion.Euler(0, 0, rotationAngle);
            }

            // Настраиваем сегмент
            RuleSegment segment = segmentObj.GetComponent<RuleSegment>();
            if (segment == null)
                segment = segmentObj.AddComponent<RuleSegment>();

            // Вычисляем диапазон углов для этого сегмента
            float minAngle = (startAngle + i * angleStep - angleStep / 2f) % 360f;
            float maxAngle = (startAngle + i * angleStep + angleStep / 2f) % 360f;

            segment.Initialize(rule, weaponSystem, this);
            segment.SetAngleRange(minAngle, maxAngle);

            segments.Add(segment);
        }
    }

    void ClearWheel()
    {
        foreach (var segment in segments)
        {
            if (segment != null && segment.gameObject != null)
                Destroy(segment.gameObject);
        }
        segments.Clear();
        currentSelectedRule = RuleType.None;
    }

    void UpdateSelectionFromMouse()
    {
        if (segments.Count == 0) return;

        // Получаем угол мыши относительно центра колеса
        Vector2 mousePos = Input.mousePosition;
        Vector2 centerPos = RectTransformUtility.WorldToScreenPoint(Camera.main, transform.position);
        Vector2 direction = mousePos - centerPos;

        bool anySelected = false;
        RuleType newSelectedRule = RuleType.None;

        if (direction.magnitude > 20f) // Минимальное расстояние от центра
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            angle = (angle + 360f) % 360f;

            // Находим сегмент под мышью
            foreach (var segment in segments)
            {
                bool isSelected = segment.IsAngleInRange(angle);
                segment.SetHighlighted(isSelected);

                if (isSelected)
                {
                    anySelected = true;
                    newSelectedRule = segment.RuleType;
                }
            }
        }

        if (!anySelected)
        {
            // Сбрасываем выделение
            foreach (var segment in segments)
            {
                segment.SetHighlighted(false);
            }
            newSelectedRule = RuleType.None;
        }

        currentSelectedRule = newSelectedRule;
    }

    void SelectCurrentRule()
    {
        if (currentSelectedRule != RuleType.None && weaponSystem != null)
        {
            weaponSystem.SetCurrentRule(currentSelectedRule);

            // Закрываем колесо
            if (weaponSystem != null)
            {
                weaponSystem.CloseRuleWheelManually();
            }
        }
    }

    public RuleType GetSelectedRule()
    {
        return currentSelectedRule;
    }
}