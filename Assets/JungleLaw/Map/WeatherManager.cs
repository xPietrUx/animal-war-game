using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    public static WeatherManager Instance;

    public enum WeatherCondition { Clear, Rain }
    public WeatherCondition currentWeather = WeatherCondition.Clear;

    [Header("Wizualia i Audio")]
    public GameObject rainParticles;

    // TUTAJ BYŁ BRAK: Deklarujemy zmienną globalną dla głośnika deszczu
    public AudioSource rainAudioSource;

    private void Awake() => Instance = this;

    public void ChangeWeather(WeatherCondition newWeather)
    {
        currentWeather = newWeather;
        Debug.Log($"ZMIANA POGODY: {currentWeather}");

        // Włączanie/wyłączanie cząsteczek deszczu
        if (rainParticles != null)
        {
            rainParticles.SetActive(currentWeather == WeatherCondition.Rain);
        }

        // Kontrola dźwięku deszczu (Linie 26-31, które zgłaszały błąd)
        if (rainAudioSource != null)
        {
            if (currentWeather == WeatherCondition.Rain)
                rainAudioSource.Play(); // Start odtwarzania w pętli
            else
                rainAudioSource.Stop(); // Wyciszenie ulewy
        }

        // Powiadomienie jednostek na mapie o zmianie statystyk
        Animal[] allAnimals = Object.FindObjectsByType<Animal>(FindObjectsSortMode.None);
        foreach (Animal a in allAnimals)
        {
            a.RecalculateStats();
        }
    }
}