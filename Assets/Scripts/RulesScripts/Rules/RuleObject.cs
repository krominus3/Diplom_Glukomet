using UnityEngine;
using System.Collections.Generic;

public abstract class RuleObject : MonoBehaviour, IRuleApplicable
{
    [Header("Rule Settings")]
    public bool isRuleApplicable = true;

    [Tooltip("Теги, на которые можно применять правила (оставьте пустым для всех)")]
    public string[] applicableTags;

    [Tooltip("Конкретные правила, разрешенные для этого объекта")]
    public RuleType[] allowedRules;

    [Tooltip("Правила, запрещенные для этого объекта")]
    public RuleType[] forbiddenRules;

    [Tooltip("Тип объекта (для логики в игре)")]
    public string objectType = "Default";

    protected RuleType activeRule = RuleType.None;
    protected Dictionary<RuleType, float> ruleTimers = new Dictionary<RuleType, float>();
    protected Dictionary<RuleType, System.Action> ruleActions = new Dictionary<RuleType, System.Action>();

    protected virtual void Start()
    {
        InitializeRuleActions();
    }

    //protected virtual void Update()
    //{
    //    UpdateRuleTimers();
    //}

    protected abstract void InitializeRuleActions();

    //protected virtual void UpdateRuleTimers()
    //{
    //    List<RuleType> rulesToRemove = new List<RuleType>();

    //    foreach (var timer in ruleTimers)
    //    {
    //        ruleTimers[timer.Key] -= Time.deltaTime;
    //        if (ruleTimers[timer.Key] <= 0)
    //        {
    //            rulesToRemove.Add(timer.Key);
    //        }
    //    }

    //    foreach (var rule in rulesToRemove)
    //    {
    //        RemoveRule(rule);
    //    }
    //}

    public virtual bool CanApplyRule(RuleType rule)
    {
        if (!isRuleApplicable) return false;
        if (rule == RuleType.RuleClearer) return true;

        // Проверка по тегам
        if (applicableTags != null && applicableTags.Length > 0)
        {
            bool hasValidTag = false;
            foreach (string tag in applicableTags)
            {
                if (CompareTag(tag))
                {
                    hasValidTag = true;
                    break;
                }
            }
            if (!hasValidTag) return false;
        }

        // Проверка запрещенных правил
        if (forbiddenRules != null)
        {
            foreach (RuleType forbidden in forbiddenRules)
            {
                if (forbidden == rule) return false;
            }
        }

        // Проверка разрешенных правил (если список не пуст)
        if (allowedRules != null && allowedRules.Length > 0)
        {
            foreach (RuleType allowed in allowedRules)
            {
                if (allowed == rule) return true;
            }
            return false; // Правило не в списке разрешенных
        }

        return true; // Если ничего не указано, правило разрешено
    }

    public virtual void ApplyRule(RuleType rule)
    {
        if (rule == RuleType.RuleClearer)
        {
            ClearRules();
            return;
        }

        if (!CanApplyRule(rule))
        {
            Debug.Log($"Правило {rule} нельзя применить к {gameObject.name}");
            return;
        }

        if (ruleActions.ContainsKey(rule))
        {
            // Если это правило уже активно, сначала сбросим его
            if (activeRule == rule)
            {
                RemoveRule(rule);
            }

            ruleActions[rule]?.Invoke();
            activeRule = rule;

            OnRuleApplied(rule);
        }
        else
        {
            Debug.LogWarning($"Нет реализации для правила {rule} на объекте {gameObject.name}");
        }
    }

    public virtual void ClearRules()
    {
        activeRule = RuleType.None;
        ruleTimers.Clear();

        ResetToDefault();
        OnRulesCleared();
    }

    protected virtual void RemoveRule(RuleType rule)
    {
        if (ruleTimers.ContainsKey(rule))
            ruleTimers.Remove(rule);

        if (activeRule == rule)
        {
            activeRule = RuleType.None;
            ResetToDefault();
        }
    }

    protected abstract void ResetToDefault();

    protected virtual void OnRuleApplied(RuleType rule)
    {
        Debug.Log($"Правило {rule} применено к {gameObject.name}");
    }

    protected virtual void OnRulesCleared()
    {
        Debug.Log($"Правила очищены с {gameObject.name}");
    }

    public RuleType GetActiveRule() => activeRule;
}