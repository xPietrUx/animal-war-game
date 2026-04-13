using UnityEngine;

public class FloatingEffect : MonoBehaviour
{
    public float floatSpeed = 50f; // Dla UI warto?ci musz? by? rz?du 50-100
    public float duration = 0.8f;

    private RectTransform rectTransform;

    void Start()
    {
        // Pobieramy "wersj? UI" transformacji
        rectTransform = GetComponent<RectTransform>();
        Destroy(gameObject, duration);
    }

    void Update()
    {
        // Zabezpieczenie: Je?li to nie jest UI, przesuwaj starym sposobem
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition += Vector2.up * floatSpeed * Time.deltaTime;
        }
        else
        {
            transform.position += Vector3.up * floatSpeed * Time.deltaTime;
        }
    }
}