using UnityEngine;

public class FloatingEffect : MonoBehaviour
{
    public float floatSpeed = 1.0f; // Szybkoœæ lotu w górê
    public float duration = 0.8f;   // Po ilu sekundach ma znikn¹æ

    void Start()
    {
        // Polecenie: Skasuj ten obiekt po up³ywie 'duration'
        Destroy(gameObject, duration);
    }

    void Update()
    {
        // Ruch w górê co klatkê
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
    }
}