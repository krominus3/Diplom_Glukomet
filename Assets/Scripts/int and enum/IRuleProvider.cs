public interface IRuleProvider
{
    bool HasRule(RuleType rule);
    void AddRule(RuleType rule);
    void RemoveRule(RuleType rule);
    RuleType GetCurrentRule();
    void SetCurrentRule(RuleType rule);
}