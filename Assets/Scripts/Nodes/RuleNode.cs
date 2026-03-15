using UnityEngine;
using System.Collections.Generic;

public class RuleNode : MonoBehaviour
{
    [Header("Node Settings")]
    public string nodeID = "Node_1";
    public Color nodeColor = Color.cyan;
    public float connectionRadius = 5f;

    [Header("Rule Transfer Settings")]
    public bool autoTransferOnContact = true; // Передавать правило при контакте
    public bool requireLineOfSight = true; // Требуется ли прямая видимость
    public float transferDelay = 0.5f; // Задержка между передачами

    [Header("Transfer Behavior")]
    public bool transferToConnectedNodes = true; // Передавать ли правила дальше
    public bool applyToObjectsInRadius = true; // Применять ли к объектам в радиусе
    public float applyRadius = 2f; // Радиус применения правил

    [Header("Connected Nodes")]
    public List<RuleNode> connectedNodes = new List<RuleNode>(); // Ручное соединение

    // Храним правила, которые уже прошли через эту ноду (для избежания зацикливания)
    private HashSet<RuleBase> processedRules = new HashSet<RuleBase>();

    // Компоненты
    private LineRenderer lineRenderer;
    private float lastTransferTime;

    void Start()
    {
        SetupLineRenderer();
    }

    void SetupLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = nodeColor;
        lineRenderer.endColor = nodeColor;
        lineRenderer.enabled = false;
    }

    void Update()
    {
        // Визуализация соединений
        if (connectedNodes.Count > 0)
        {
            DrawConnections();
        }

        // Автоматический поиск соединений по нажатию F
        if (Input.GetKeyDown(KeyCode.F))
        {
            FindNearbyNodes();
        }

        // Показать статус по нажатию L
        if (Input.GetKeyDown(KeyCode.L))
        {
            ShowStatus();
        }
    }

    void ShowStatus()
    {
        // Этот метод можно оставить для отладки, но без Debug.Log
        // Если не нужен - удали целиком
    }

    // Найти ближайшие ноды
    public void FindNearbyNodes()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, connectionRadius);

        foreach (var col in colliders)
        {
            RuleNode node = col.GetComponent<RuleNode>();
            if (node != null && node != this && !connectedNodes.Contains(node))
            {
                // Проверяем прямую видимость если нужно
                if (requireLineOfSight)
                {
                    RaycastHit hit;
                    Vector3 direction = node.transform.position - transform.position;
                    if (Physics.Raycast(transform.position, direction, out hit, connectionRadius))
                    {
                        if (hit.collider.GetComponent<RuleNode>() == node)
                        {
                            connectedNodes.Add(node);
                        }
                    }
                }
                else
                {
                    connectedNodes.Add(node);
                }
            }
        }
    }

    // Передать правило на все соединенные ноды
    public void TransferRule(RuleBase rule, GameObject sourceObject)
    {
        if (!transferToConnectedNodes || rule == null)
            return;

        if (Time.time - lastTransferTime < transferDelay)
            return;

        // Проверяем, не передавали ли мы это правило уже
        if (processedRules.Contains(rule))
            return;

        // Отмечаем правило как обработанное
        processedRules.Add(rule);
        lastTransferTime = Time.time;

        if (connectedNodes.Count == 0)
            return;

        foreach (var node in connectedNodes)
        {
            if (node != null)
            {
                node.ReceiveRule(rule, sourceObject, this);
            }
        }
    }

    // Получить правило от другой ноды
    public void ReceiveRule(RuleBase rule, GameObject sourceObject, RuleNode sourceNode)
    {
        if (rule == null)
            return;

        // Проверяем, не обрабатывали ли мы это правило уже
        if (processedRules.Contains(rule))
            return;

        // Отмечаем как обработанное сразу при получении
        processedRules.Add(rule);

        // 1. СНАЧАЛА применяем правило к объектам рядом с этой нодой
        if (applyToObjectsInRadius)
        {
            ApplyRuleToNearbyObjects(rule, sourceObject);
        }

        // Визуальный эффект (опционально)
        //StartCoroutine(FlashNode(Color.yellow));

        // 2. ПОТОМ передаем правило дальше по соединениям
        TransferRule(rule, sourceObject);
    }

    // Применить правило к объектам в радиусе
    void ApplyRuleToNearbyObjects(RuleBase rule, GameObject sourceObject)
    {
        // Проверяем, есть ли объекты в радиусе
        Collider[] colliders = Physics.OverlapSphere(transform.position, applyRadius);

        if (colliders.Length == 0)
            return;

        foreach (var col in colliders)
        {
            // Игнорируем саму ноду и источник
            if (col.gameObject == gameObject || col.gameObject == sourceObject)
                continue;

            RuleContainer container = col.GetComponent<RuleContainer>();
            if (container != null)
            {
                // Применяем правило к объекту
                container.ApplyRule(rule);
            }
        }
    }

    // Применить правило к конкретному объекту (для выстрела)
    public void ApplyRuleToObject(RuleBase rule, GameObject targetObject, GameObject sourceObject)
    {
        if (targetObject == null || rule == null)
            return;

        // Применяем правило напрямую
        RuleContainer container = targetObject.GetComponent<RuleContainer>();
        if (container != null)
        {
            container.ApplyRule(rule);
        }

        // После применения, передаем правило через сеть
        TransferRule(rule, sourceObject);
    }

    // Обработка столкновений
    void OnTriggerEnter(Collider other)
    {
        if (autoTransferOnContact)
        {
            // Проверяем, есть ли у объекта правило
            RuleBase[] rules = other.GetComponents<RuleBase>();

            foreach (var rule in rules)
            {
                if (rule != null && rule.isActive)
                {
                    // Передаем правило через сеть нод
                    TransferRule(rule, other.gameObject);
                }
            }
        }
    }

    // Визуальные эффекты
    void DrawConnections()
    {
        if (lineRenderer == null) return;

        lineRenderer.enabled = true;
        lineRenderer.positionCount = connectedNodes.Count * 2;

        int index = 0;
        foreach (var node in connectedNodes)
        {
            if (node != null)
            {
                lineRenderer.SetPosition(index++, transform.position);
                lineRenderer.SetPosition(index++, node.transform.position);
            }
        }
    }

    System.Collections.IEnumerator FlashNode(Color color)
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Color originalColor = renderer.material.color;
            renderer.material.color = color;
            yield return new WaitForSeconds(0.2f);
            renderer.material.color = originalColor;
        }
    }

    System.Collections.IEnumerator FlashObject(GameObject obj, Color color)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Color originalColor = renderer.material.color;
            renderer.material.color = color;
            yield return new WaitForSeconds(0.3f);
            renderer.material.color = originalColor;
        }
    }

    // Очистить историю переданных правил
    public void ClearProcessedRules()
    {
        processedRules.Clear();
    }

    void OnDrawGizmos()
    {
        // Рисуем радиус соединения
        Gizmos.color = nodeColor;
        Gizmos.DrawWireSphere(transform.position, connectionRadius);

        // Рисуем радиус применения правил
        if (applyToObjectsInRadius)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, applyRadius);
        }

        // Рисуем линии к соединенным нодам
        if (connectedNodes != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var node in connectedNodes)
            {
                if (node != null)
                {
                    Gizmos.DrawLine(transform.position, node.transform.position);
                }
            }
        }
    }
}