using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    public static WeatherManager Instance;

    public enum WeatherCondition { Clear, Rain }
    public WeatherCondition currentWeather = WeatherCondition.Clear;

    // Możesz tu podpiąć system cząsteczek deszczu (Particle System) z Hierarchy
    public GameObject rainParticles;

    private void Awake() => Instance = this;

    public void ChangeWeather(WeatherCondition newWeather)
    {
        currentWeather = newWeather;
        Debug.Log($"ZMIANA POGODY: {currentWeather}");

        // Włącz/wyłącz wizualny deszcz na ekranie
        if (rainParticles != null)
        {
            rainParticles.SetActive(currentWeather == WeatherCondition.Rain);
        }

        // Powiadom wszystkie zwierzęta na mapie, że pogoda się zmieniła!
        Animal[] allAnimals = Object.FindObjectsByType<Animal>(FindObjectsSortMode.None);
        foreach (Animal a in allAnimals)
        {
            a.ApplyWeather(currentWeather);
        }

        // Odśwież mgłę wojny od razu po zmianie wizji
        int currentTurnTeam = (TurnManager.Instance.currentTurn == TurnManager.TurnState.Player1) ? 1 : 2;
        if (FogOfWarManager.Instance != null) FogOfWarManager.Instance.UpdateFog(currentTurnTeam);
    }
}