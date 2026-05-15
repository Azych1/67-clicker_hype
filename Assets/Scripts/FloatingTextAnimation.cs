using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FloatingTextAnimation : MonoBehaviour
{
    [Header("Text Settings")]
    [SerializeField] private TextMeshProUGUI floatingText;
    [SerializeField] private string textToDisplay = "Ваш текст здесь";
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private int fontSize = 24;

    [Header("Animation Settings")]
    [SerializeField] private AnimationType animationType = AnimationType.SineWave;
    [SerializeField] private float movementSpeed = 1f;
    [SerializeField] private float amplitude = 50f; // Амплитуда колебаний
    [SerializeField] private float frequency = 1f; // Частота колебаний

    [Header("Path Settings (для Path Animation)")]
    [SerializeField] private Vector2[] pathPoints;
    [SerializeField] private bool loopPath = true;

    private RectTransform rectTransform;
    private Vector2 startPosition;
    private float animationTime = 0f;
    private int currentPathIndex = 0;
    private float pathProgress = 0f;

    public enum AnimationType
    {
        SineWave,       // Синусоида
        Circle,         // Круговая траектория
        InfinitySymbol, // Знак бесконечности
        Path            // Пользовательский путь
    }

    void Awake()
    {
        // Создаем текст, если он не назначен
        if (floatingText == null)
        {
            CreateTextObject();
        }

        rectTransform = floatingText.GetComponent<RectTransform>();

        // Сохраняем начальную позицию
        startPosition = rectTransform.anchoredPosition;

        // Настраиваем текст
        SetupText();
    }

    void Update()
    {
        animationTime += Time.deltaTime * movementSpeed;

        Vector2 newPosition = CalculateNewPosition();
        rectTransform.anchoredPosition = newPosition;
    }

    private void CreateTextObject()
    {
        // Создаем новый объект для текста
        GameObject textObject = new GameObject("FloatingText");
        textObject.transform.SetParent(transform, false);

        // Добавляем компоненты
        floatingText = textObject.AddComponent<TextMeshProUGUI>();
        rectTransform = textObject.GetComponent<RectTransform>();

        // Настройки RectTransform
        rectTransform.sizeDelta = new Vector2(300, 50);
        rectTransform.anchoredPosition = Vector2.zero;
    }

    private void SetupText()
    {
        if (floatingText != null)
        {
            floatingText.text = textToDisplay;
            floatingText.color = textColor;
            floatingText.fontSize = fontSize;
            floatingText.alignment = TextAlignmentOptions.Center;
        }
    }

    private Vector2 CalculateNewPosition()
    {
        Vector2 basePosition = startPosition;

        switch (animationType)
        {
            case AnimationType.SineWave:
                return CalculateSineWavePosition(basePosition);

            case AnimationType.Circle:
                return CalculateCirclePosition(basePosition);

            case AnimationType.InfinitySymbol:
                return CalculateInfinityPosition(basePosition);

            case AnimationType.Path:
                return CalculatePathPosition(basePosition);

            default:
                return basePosition;
        }
    }

    private Vector2 CalculateSineWavePosition(Vector2 basePos)
    {
        // Движение по синусоиде: горизонтальное движение + вертикальные колебания
        float x = basePos.x + animationTime * 50f; // Движение вправо
        float y = basePos.y + Mathf.Sin(animationTime * frequency) * amplitude;

        // Если текст уходит за экран, возвращаем его в начало
        if (x > Screen.width / 2 + 200)
        {
            animationTime = 0f;
            x = -Screen.width / 2 - 200;
        }

        return new Vector2(x, y);
    }

    private Vector2 CalculateCirclePosition(Vector2 basePos)
    {
        // Круговая траектория
        float radius = amplitude;
        float angle = animationTime * 2f * Mathf.PI;

        float x = basePos.x + Mathf.Cos(angle) * radius;
        float y = basePos.y + Mathf.Sin(angle) * radius;

        return new Vector2(x, y);
    }

    private Vector2 CalculateInfinityPosition(Vector2 basePos)
    {
        // Траектория в виде знака бесконечности (лемниската Бернулли)
        float scale = amplitude / 2f;
        float t = animationTime * 2f;

        float x = basePos.x + (scale * Mathf.Sqrt(2) * Mathf.Cos(t)) / (Mathf.Pow(Mathf.Sin(t), 2) + 1);
        float y = basePos.y + (scale * Mathf.Sqrt(2) * Mathf.Cos(t) * Mathf.Sin(t)) / (Mathf.Pow(Mathf.Sin(t), 2) + 1);

        return new Vector2(x, y);
    }

    private Vector2 CalculatePathPosition(Vector2 basePos)
    {
        if (pathPoints == null || pathPoints.Length < 2)
        {
            Debug.LogWarning("Path points not set or insufficient points. Using default position.");
            return basePos;
        }

        pathProgress += Time.deltaTime * movementSpeed * 0.5f;

        if (pathProgress >= 1f)
        {
            pathProgress = 0f;
            currentPathIndex++;

            if (currentPathIndex >= pathPoints.Length - 1)
            {
                if (loopPath)
                {
                    currentPathIndex = 0;
                }
                else
                {
                    currentPathIndex = pathPoints.Length - 2;
                    pathProgress = 1f;
                }
            }
        }

        // Интерполяция между текущей и следующей точкой
        Vector2 currentPoint = pathPoints[currentPathIndex];
        Vector2 nextPoint = pathPoints[(currentPathIndex + 1) % pathPoints.Length];

        return Vector2.Lerp(currentPoint, nextPoint, pathProgress) + basePos;
    }

    // Методы для изменения настроек во время выполнения

    public void SetText(string newText)
    {
        textToDisplay = newText;
        if (floatingText != null)
        {
            floatingText.text = newText;
        }
    }

    public void SetColor(Color newColor)
    {
        textColor = newColor;
        if (floatingText != null)
        {
            floatingText.color = newColor;
        }
    }

    public void SetAnimationType(AnimationType type)
    {
        animationType = type;
        animationTime = 0f;
        currentPathIndex = 0;
        pathProgress = 0f;
    }

    public void SetMovementSpeed(float speed)
    {
        movementSpeed = Mathf.Max(0.1f, speed);
    }

    public void SetAmplitude(float newAmplitude)
    {
        amplitude = newAmplitude;
    }

    public void SetPathPoints(Vector2[] points)
    {
        pathPoints = points;
        currentPathIndex = 0;
        pathProgress = 0f;
    }

    // Метод для сброса анимации
    public void ResetAnimation()
    {
        animationTime = 0f;
        currentPathIndex = 0;
        pathProgress = 0f;

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = startPosition;
        }
    }
}