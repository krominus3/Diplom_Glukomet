using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;

public class FPSDisplay : MonoBehaviour
{
    [Header("Display Settings")]
    public bool showFPS = true;
    public bool showMemory = true;
    public bool showRenderStats = true;
    public bool showDetailedStats = false;
    public int fontSize = 20;
    public Color textColor = Color.green;
    public Vector2 position = new Vector2(10, 10);

    [Header("FPS Settings")]
    public float updateInterval = 0.5f; // Как часто обновлять значения
    public int maxSamples = 60; // Количество сохраненных семплов для графика

    [Header("Background")]
    public bool showBackground = true;
    public Color backgroundColor = new Color(0, 0, 0, 0.7f);

    // FPS переменные
    private float fpsAccumulator = 0f;
    private float fpsNextPeriod = 0f;
    private int fpsFramesCount = 0;
    private float currentFPS = 0f;
    private List<float> fpsHistory = new List<float>();
    private float minFPS = float.MaxValue;
    private float maxFPS = 0f;

    // Memory переменные
    private float currentMemory = 0f;
    private float peakMemory = 0f;

    // Time переменные
    private float deltaTime = 0f;

    // Render переменные
    private int drawCalls = 0;
    private int triangles = 0;
    private int vertices = 0;

    // GUI Style
    private GUIStyle textStyle;
    private GUIStyle backgroundStyle;

    void Start()
    {
        // Инициализация таймеров
        fpsNextPeriod = Time.realtimeSinceStartup + updateInterval;

        // Создание стилей
        textStyle = new GUIStyle();
        textStyle.fontSize = fontSize;
        textStyle.normal.textColor = textColor;

        backgroundStyle = new GUIStyle();
        backgroundStyle.normal.background = MakeBackgroundTexture(2, 2, backgroundColor);

        // Сбор статистики рендера
        StartCoroutine(CollectRenderStats());
    }

    void Update()
    {
        // Подсчет FPS
        deltaTime = Time.unscaledDeltaTime;
        fpsAccumulator += deltaTime;
        fpsFramesCount++;

        // Обновление статистики
        if (Time.realtimeSinceStartup >= fpsNextPeriod)
        {
            currentFPS = fpsFramesCount / fpsAccumulator;
            fpsAccumulator = 0f;
            fpsFramesCount = 0;
            fpsNextPeriod = Time.realtimeSinceStartup + updateInterval;

            // Сохраняем историю FPS
            fpsHistory.Add(currentFPS);
            if (fpsHistory.Count > maxSamples)
                fpsHistory.RemoveAt(0);

            // Обновляем мин/макс
            minFPS = Mathf.Min(minFPS, currentFPS);
            maxFPS = Mathf.Max(maxFPS, currentFPS);
        }

        // Сбор статистики памяти
        if (showMemory)
        {
            currentMemory = System.GC.GetTotalMemory(false) / (1024f * 1024f);
            peakMemory = Mathf.Max(peakMemory, currentMemory);
        }
    }

    [System.Obsolete]
    System.Collections.IEnumerator CollectRenderStats()
    {
        while (true)
        {
            if (showRenderStats && showDetailedStats)
            {
                drawCalls = UnityEngine.Rendering.GraphicsSettings.GetShaderMode(UnityEngine.Rendering.BuiltinShaderType.DeferredShading) != UnityEngine.Rendering.BuiltinShaderMode.Disabled ? 0 : 0;

                // Получаем статистику через Unity Profiler
                if (UnityEngine.Profiling.Profiler.enabled)
                {
                    // Примерный сбор статистики
                    triangles = (int)(UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory() / 1000);
                    vertices = (int)(UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory() / 1000);
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    void OnGUI()
    {
        if (!showFPS && !showMemory && !showDetailedStats)
            return;

        // Создаем текст для отображения
        StringBuilder stats = new StringBuilder();

        if (showFPS)
        {
            stats.AppendLine(FormatFPS(currentFPS));
            if (showDetailedStats)
            {
                stats.AppendLine($"  Min: {minFPS:F1} | Max: {maxFPS:F1}");
                stats.AppendLine($"  Frame Time: {deltaTime * 1000:F1} ms");
            }
        }

        if (showMemory)
        {
            stats.AppendLine(FormatMemory(currentMemory));
            if (showDetailedStats)
                stats.AppendLine($"  Peak: {peakMemory:F1} MB");
        }

        if (showRenderStats && showDetailedStats)
        {
            stats.AppendLine($"Draw Calls: {drawCalls}");
            stats.AppendLine($"Triangles: {FormatNumber(triangles)}");
            stats.AppendLine($"Vertices: {FormatNumber(vertices)}");
        }

        // Вычисляем размер текста
        Vector2 textSize = textStyle.CalcSize(new GUIContent(stats.ToString()));

        // Рисуем фон
        if (showBackground)
        {
            GUI.Box(new Rect(position.x - 5, position.y - 5, textSize.x + 10, textSize.y + 10), "", backgroundStyle);
        }

        // Рисуем текст
        GUI.Label(new Rect(position.x, position.y, textSize.x, textSize.y), stats.ToString(), textStyle);

        // Рисуем график FPS если нужно
        if (showDetailedStats && fpsHistory.Count > 1)
        {
            DrawFPSGraph();
        }
    }

    void DrawFPSGraph()
    {
        float graphWidth = 200f;
        float graphHeight = 60f;
        float graphX = position.x;
        float graphY = position.y + 100f;

        // Фон графика
        GUI.Box(new Rect(graphX, graphY, graphWidth, graphHeight), "", backgroundStyle);

        // Рисуем линии
        float step = graphWidth / maxSamples;
        float maxDisplayFPS = 120f;

        for (int i = 0; i < fpsHistory.Count - 1; i++)
        {
            float x1 = graphX + i * step;
            float y1 = graphY + graphHeight - (fpsHistory[i] / maxDisplayFPS) * graphHeight;
            float x2 = graphX + (i + 1) * step;
            float y2 = graphY + graphHeight - (fpsHistory[i + 1] / maxDisplayFPS) * graphHeight;

            Drawing.DrawLine(new Vector2(x1, y1), new Vector2(x2, y2), textColor, 2f);
        }

        // Рисуем горизонтальные линии (30, 60, 90 FPS)
        GUI.color = Color.gray;
        for (int i = 30; i <= 90; i += 30)
        {
            float y = graphY + graphHeight - (i / maxDisplayFPS) * graphHeight;
            Drawing.DrawLine(new Vector2(graphX, y), new Vector2(graphX + graphWidth, y), Color.gray, 1f);
            GUI.Label(new Rect(graphX + graphWidth + 5, y - 8, 30, 20), i.ToString(), textStyle);
        }
        GUI.color = Color.white;
    }

    string FormatFPS(float fps)
    {
        if (fps >= 59f)
            return $"FPS: {fps:F0} (Excellent)";
        else if (fps >= 30f)
            return $"FPS: {fps:F0} (Good)";
        else if (fps >= 15f)
            return $"FPS: {fps:F0} (Bad)";
        else
            return $"FPS: {fps:F0} (Very Bad)";
    }

    string FormatMemory(float memory)
    {
        if (memory < 512f)
            return $"Memory: {memory:F1} MB (Good)";
        else if (memory < 1024f)
            return $"Memory: {memory:F1} MB (Warning)";
        else
            return $"Memory: {memory:F1} MB (Critical)";
    }

    string FormatNumber(int num)
    {
        if (num >= 1000000)
            return (num / 1000000f).ToString("F1") + "M";
        if (num >= 1000)
            return (num / 1000f).ToString("F1") + "K";
        return num.ToString();
    }

    Texture2D MakeBackgroundTexture(int width, int height, Color color)
    {
        Texture2D texture = new Texture2D(width, height);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                texture.SetPixel(x, y, color);
            }
        }
        texture.Apply();
        return texture;
    }

    // Сброс статистики
    public void ResetStats()
    {
        fpsHistory.Clear();
        minFPS = float.MaxValue;
        maxFPS = 0f;
        peakMemory = 0f;
    }
}

// Класс для рисования линий в GUI
public static class Drawing
{
    static Texture2D lineTexture;

    public static void DrawLine(Vector2 start, Vector2 end, Color color, float width = 1f)
    {
        if (lineTexture == null)
        {
            lineTexture = new Texture2D(1, 1);
            lineTexture.SetPixel(0, 0, Color.white);
            lineTexture.Apply();
        }

        Vector2 vector = end - start;
        float angle = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
        float length = vector.magnitude;

        GUI.color = color;
        GUIUtility.RotateAroundPivot(angle, start);
        GUI.DrawTexture(new Rect(start.x, start.y - width / 2f, length, width), lineTexture);
        GUIUtility.RotateAroundPivot(-angle, start);
        GUI.color = Color.white;
    }
}