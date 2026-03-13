using TMPro;
using UnityEngine;

public class UiTest : MonoBehaviour
{
    private RuleInventory ri;
    private TextMeshProUGUI textField;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ri = FindFirstObjectByType<RuleInventory>();
        textField = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        textField.text = ri.GetCurrentRuleName;
    }
}
