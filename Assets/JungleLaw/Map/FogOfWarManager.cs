using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class FogOfWarManager : MonoBehaviour
{
    public static FogOfWarManager Instance;

    public Tilemap fogMap;
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
                RevealArea(a.gridPosition, a.data.visionRange);
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
                // Używamy odległości Manhattan (romby), tak jak przy chodzeniu
                if (Mathf.Abs(x) + Mathf.Abs(y) <= range)
                {
                    Vector3Int tilePos = new Vector3Int(center.x + x, center.y + y, 0);
                    fogMap.SetTile(tilePos, null); // Kasujemy czarny kafelek = widać świat pod spodem
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
}