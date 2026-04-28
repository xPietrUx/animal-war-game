using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class FogOfWarManager : MonoBehaviour
{
    public static FogOfWarManager Instance;

    public Tilemap fogMap;
    public Tilemap obstaclesMap; // NOWOŚĆ: Mapa z drzewami/górami
    public TileBase fogTile; // Twój czarny kwadrat

    [Header("Granice Mapy (Rozmiar Koca)")]
    public Vector3Int minBounds = new Vector3Int(-20, -15, 0);
    public Vector3Int maxBounds = new Vector3Int(20, 15, 0);

    private void Awake() => Instance = this;

    // Główna funkcja odświeżająca widoczność
    public void UpdateFog(int currentTeam)
    {
        // 1. ZACIĄGNIJ KOC NA CAŁĄ MAPĘ
        for (int x = minBounds.x; x <= maxBounds.x; x++)
        {
            for (int y = minBounds.y; y <= maxBounds.y; y++)
            {
                fogMap.SetTile(new Vector3Int(x, y, 0), fogTile);
            }
        }

        // 2. WYPALAJ DZIURY LATARKAMI (Dla aktualnego gracza)
        Animal[] allAnimals = Object.FindObjectsByType<Animal>(FindObjectsSortMode.None);

        foreach (Animal a in allAnimals)
        {
            // Zdejmujemy mgłę tylko wokół żywych jednostek NASZEJ drużyny
            if (a.team == currentTeam && a.enabled)
            {
                RevealArea(a.gridPosition, a.currentVisionRange);
            }
        }

        // OPCJONALNIE: Przejęte punkty też mogą świecić!
        CapturePoint[] allPoints = Object.FindObjectsByType<CapturePoint>(FindObjectsSortMode.None);
        foreach (CapturePoint cp in allPoints)
        {
            if (cp.ownerTeam == currentTeam)
            {
                RevealArea(cp.gridPosition, 2); // Wioska widzi na 2 kafelki wokół
            }
        }

        // 3. UKRYWANIE WROGÓW
        HideEnemiesInFog(currentTeam);
    }

    // Funkcja wycinająca dziurę we mgle
    private void RevealArea(Vector3Int center, int range)
    {
        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                // Odległość Manhattan (kształt rombu)
                if (Mathf.Abs(x) + Mathf.Abs(y) <= range)
                {
                    Vector3Int targetTile = new Vector3Int(center.x + x, center.y + y, 0);

                    // NOWOŚĆ: Pytamy "Lasera", czy droga od środka do celu jest czysta
                    if (HasLineOfSight(center, targetTile))
                    {
                        fogMap.SetTile(targetTile, null); // Kasujemy mgłę
                    }
                }
            }
        }
    }

    // Jeśli wróg stoi w czarnej mgle, musimy schować jego "Sprite'a" i Pasek HP
    private void HideEnemiesInFog(int currentTeam)
    {
        Animal[] allAnimals = Object.FindObjectsByType<Animal>(FindObjectsSortMode.None);
        foreach (Animal a in allAnimals)
        {
            if (a.team != currentTeam && a.enabled)
            {
                // Czy na polu wroga jest kafelek mgły?
                bool isHidden = fogMap.HasTile(a.gridPosition);

                // Wyłączamy/włączamy renderowanie (grafikę) jednostki
                a.GetComponent<SpriteRenderer>().enabled = !isHidden;

                // Ukrywamy też pasek HP! (Wymaga małej zmiany w Animal.cs)
                Transform healthBar = a.transform.Find("HealthBar"); // Szukamy Twojego Canvasa po nazwie
                if (healthBar != null) healthBar.gameObject.SetActive(!isHidden);
            }
            else if (a.team == currentTeam)
            {
                // Upewnijmy się, że nasze jednostki są zawsze widoczne
                a.GetComponent<SpriteRenderer>().enabled = true;
                Transform healthBar = a.transform.Find("HealthBar");
                if (healthBar != null) healthBar.gameObject.SetActive(true);
            }
        }
    }

    // Algorytm śledzący linię prostą na kwadratowej siatce
    private bool HasLineOfSight(Vector3Int start, Vector3Int end)
    {
        // Pole na którym stoimy jest zawsze widoczne
        if (start == end) return true;

        int x = start.x;
        int y = start.y;
        int w = end.x - x;
        int h = end.y - y;
        int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;

        if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
        if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
        if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;

        int longest = Mathf.Abs(w);
        int shortest = Mathf.Abs(h);

        if (!(longest > shortest))
        {
            longest = Mathf.Abs(h);
            shortest = Mathf.Abs(w);
            if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
            dx2 = 0;
        }

        int numerator = longest >> 1;

        // "Rysujemy" linię kratka po kratce
        for (int i = 0; i <= longest; i++)
        {
            Vector3Int currentPos = new Vector3Int(x, y, 0);

            // Sprawdzamy przeszkody TYLKO na drodze (ignorujemy start i sam cel)
            // Dzięki temu widzimy samo drzewo, ale nie to, co za nim!
            if (currentPos != start && currentPos != end)
            {
                if (obstaclesMap != null && obstaclesMap.HasTile(currentPos))
                {
                    return false; // Laser trafił w przeszkodę! Odcinamy wizję.
                }
            }

            numerator += shortest;
            if (!(numerator < longest))
            {
                numerator -= longest;
                x += dx1;
                y += dy1;
            }
            else
            {
                x += dx2;
                y += dy2;
            }
        }
        return true; // Droga wolna
    }
}