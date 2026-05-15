using UnityEngine;

public class ButtonSpinAnimation : MonoBehaviour
{
    [Tooltip("Угол поворота в градусах (например, 10)")]
    [SerializeField] private float spinAngle = 10f;

    [Tooltip("Время одного полного цикла качания (в секундах)")]
    [SerializeField] private float spinDuration = 1f;

    private RectTransform rectTransform;
    private float startRotation;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        startRotation = rectTransform.rotation.eulerAngles.z;
    }

    void Update()
    {
        // Плавное колебание по синусоиде
        float sineValue = Mathf.Sin(Time.time * (2 * Mathf.PI / spinDuration));
        float currentRotation = startRotation + (spinAngle * sineValue);
        rectTransform.rotation = Quaternion.Euler(0, 0, currentRotation);
    }
}
