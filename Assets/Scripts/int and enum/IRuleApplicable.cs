public interface IRuleApplicable
{
    bool CanApplyRule(RuleType rule);
    void ApplyRule(RuleType rule);
    void ClearRules();
    RuleType GetActiveRule();
}