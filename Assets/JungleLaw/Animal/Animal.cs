using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Animal : MonoBehaviour
{
    public AnimalData data;
    public Vector3Int gridPosition;
    public int currentHP;

    [Header("Efekty")]
    public GameObject hpEffectPrefab; // Przeciągnij tu prefab -hp
    [Header("UI")]
    public Image healthBarFill;
    [Header("Aktualne Statystyki (Zmienne)")]
    public int currentMoveRange;
    public int currentVisionRange;
    public int currentAttack; // NOWOŚĆ: Zmienny atak

    // Zmienna, w której przechowamy referencję do warstwy wzgórz
    private UnityEngine.Tilemaps.Tilemap hillsMap;

    private Grid mainGrid;
    private SpriteRenderer spriteRenderer;
    private bool isDead = false; // Zabezpieczenie przed "miganiem" animacji
    public int team; // 1 dla Player 1, 2 dla Player 2
    public bool hasMoved = false; // Czy jednostka już wykonała akcję?

    // Wywołaj to w MoveTo() po udanym ruchu
    public void FinishAction()
    {
        hasMoved = true;
        // Opcjonalnie: "Poszarzenie" grafiki, by gracz widział, że jednostka odpoczywa
        GetComponent<SpriteRenderer>().color = Color.gray;
    }

    public void ResetTurn()
    {
        hasMoved = false;
        // Przywracamy kolor, aby gracz widział, że jednostka znów "żyje"
        GetComponent<SpriteRenderer>().color = Color.white;
        Debug.Log($"{data.speciesName} jest gotowy do nowej tury.");
    }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        mainGrid = FindFirstObjectByType<Grid>();
        currentHP = data.maxHP;

        // Szukamy obiektu o nazwie "HillsMap" na scenie
        GameObject hillsObj = GameObject.Find("HillsMap");
        if (hillsObj != null) hillsMap = hillsObj.GetComponent<UnityEngine.Tilemaps.Tilemap>();

        // Zamiast ustawiać parametry ręcznie, odpalamy nasz nowy silnik przeliczający!
        RecalculateStats();

        SnapToGrid(mainGrid);
        // Przepisujemy bazowe dane z matrycy do tymczasowych zmiennych
        currentMoveRange = data.moveRange;
        currentVisionRange = data.visionRange;

        if (data.idleSprite != null)
            spriteRenderer.sprite = data.idleSprite;

        SnapToGrid(mainGrid);
        GridManager.Instance.OccupyTile(gridPosition, this);
        hasMoved = false;
        UpdateHealthBar();
        UpdateAllegianceColor();
    }

    public void SnapToGrid(Grid grid)
    {
        gridPosition = grid.WorldToCell(transform.position);
        transform.position = grid.GetCellCenterWorld(gridPosition);
    }

    // --- LOGIKA WALKI I EMOCJI ---

    public void PlayAttackAnimation()
    {
        if (isDead) return;

        StopAllCoroutines();
        StartCoroutine(ChangeExpressionRoutine(data.angrySprite, 0.8f));
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return; // Jeśli już umiera, nie przyjmuj więcej obrażeń

        currentHP -= amount;
        UpdateHealthBar();
        Debug.Log($"{data.speciesName} oberwał za {amount}. Zostało HP: {currentHP}");

        // 1. Spawnowanie ikonki -hp nad głową
        if (hpEffectPrefab != null)
        {
            // Spawnuje ikonkę lekko nad głową postaci
            Instantiate(hpEffectPrefab, transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);
        }

        if (currentHP <= 0)
        {
            isDead = true;

            // NOWOŚĆ: Skoro jednostka padła, aktualizujemy układ sił na mapie
            CapturePoint[] allPoints = Object.FindObjectsByType<CapturePoint>(FindObjectsSortMode.None);
            foreach (CapturePoint point in allPoints) point.EvaluateControl();

            Die();
        }
        else
        {
            // 2. Zmień minę na Surprise tylko jeśli postać przeżyła
            StopAllCoroutines();
            StartCoroutine(ChangeExpressionRoutine(data.surpriseSprite, 0.8f));
        }
    }

    private IEnumerator ChangeExpressionRoutine(Sprite newSprite, float duration)
    {
        if (newSprite == null || isDead) yield break;

        spriteRenderer.sprite = newSprite;
        yield return new WaitForSeconds(duration);

        if (!isDead) // Wracamy do idle tylko jeśli w międzyczasie nie umarliśmy
            spriteRenderer.sprite = data.idleSprite;
    }

    private void Die()
    {
        // 1. ZATRZYMUJEMY inne procesy (np. zmianę miny na Surprise)
        StopAllCoroutines();
        isDead = true;

        StartCoroutine(DeathSequenceRoutine());
    }

    // --- RUCH ---

    public void MoveTo(Vector3Int targetCell)
    {
        if (isDead) return;

        int dx = Mathf.Abs(targetCell.x - gridPosition.x);
        int dy = Mathf.Abs(targetCell.y - gridPosition.y);
        int dist = dx + dy;

        // Sprawdzamy czy cel jest w zasięgu i czy GridManager pozwala tam wejść
        if (dist <= currentMoveRange && GridManager.Instance.IsTileWalkable(targetCell))
        {
            // 1. SPRZĄTANIE: Zwalniamy stare pole, żeby nie robiły się "dziury" w zasięgu
            GridManager.Instance.LeaveTile(gridPosition);

            // 2. RUCH: Aktualizujemy pozycję
            gridPosition = targetCell;
            transform.position = mainGrid.GetCellCenterWorld(gridPosition);

            // 3. REZERWACJA: Zajmujemy nowe pole w GridManagerze
            GridManager.Instance.OccupyTile(gridPosition, this);

            // 4. STATYSTYKI: Przeliczamy buffy (np. ze wzgórz)
            RecalculateStats();

            // 5. MGŁA WOJNY: To, o co pytałeś – odsłaniamy teren w nowym miejscu
            if (FogOfWarManager.Instance != null)
            {
                FogOfWarManager.Instance.UpdateFog(this.team);
            }
        }

        // 6. BAZY: Niezależnie od tego czy ruch się udał, sprawdzamy kto kontroluje punkty
        CapturePoint[] allPoints = Object.FindObjectsByType<CapturePoint>(FindObjectsSortMode.None);
        foreach (CapturePoint point in allPoints)
        {
            point.EvaluateControl();
        }
    }

    // --- ŚMIERĆ (ANIMOWANA) ---

    private IEnumerator DeathSequenceRoutine()
    {
        Debug.Log("Rozpoczynam animację śmierci...");

        // 1. Animacja upadku
        if (data.deathFrames != null && data.deathFrames.Length > 0)
        {
            foreach (Sprite frame in data.deathFrames)
            {
                spriteRenderer.sprite = frame;
                yield return new WaitForSeconds(0.12f);
            }
        }

        yield return new WaitForSeconds(0.5f);

        // 2. Zamiana w nagrobek
        if (data.graveSprite != null)
        {
            spriteRenderer.sprite = data.graveSprite;
        }

        // 3. Wyłączamy skrypt Animal
        // To sprawi, że nagrobek nie będzie reagował na kliknięcia i nie będzie mógł atakować
        this.enabled = false;

        // 4. Nagrobek stoi i blokuje przejście przez 10 sekund
        yield return new WaitForSeconds(10.0f);

        // 5. DOPIERO TERAZ zwalniamy pole w GridManagerze
        // Robimy to tuż przed zniknięciem, żeby inni mogli wejść na to miejsce
        GridManager.Instance.LeaveTile(gridPosition);

        // 6. Usunięcie obiektu
        Destroy(gameObject);
        Debug.Log("Nagrobek zniknął, pole jest wolne.");
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            // Rzutujemy (float), żeby wynik nie zaokrąglił się do 0 (np. 50/100 to 0.5)
            healthBarFill.fillAmount = (float)currentHP / data.maxHP;
        }
    }

    // Funkcja zmieniająca kolor paska w zależności od tury
    public void UpdateAllegianceColor()
    {
        if (healthBarFill != null)
        {
            // Przypomnienie: TurnState.Player1 to 0 (więc +1 daje nam Team 1)
            int currentTurnTeam = (int)TurnManager.Instance.currentTurn + 1;

            if (this.team == currentTurnTeam)
            {
                // To jest moja tura - pasek jest zielony
                healthBarFill.color = Color.green;
            }
            else
            {
                // To tura przeciwnika - pasek jest czerwony
                healthBarFill.color = Color.red;
            }
        }
    }
    public void ApplyWeather(WeatherManager.WeatherCondition weather)
    {
        if (weather == WeatherManager.WeatherCondition.Rain)
        {
            // Deszcz obcina ruch i wizję o 1 (Mathf.Max zapobiega spadkowi poniżej 1)
            currentMoveRange = Mathf.Max(1, data.moveRange - 1);
            currentVisionRange = Mathf.Max(1, data.visionRange - 1);
            Debug.Log($"{data.speciesName} moknie! Ruch: {currentMoveRange}, Zasięg widzenia: {currentVisionRange}");
        }
        else if (weather == WeatherManager.WeatherCondition.Clear)
        {
            // Słońce przywraca normalne statystyki
            currentMoveRange = data.moveRange;
            currentVisionRange = data.visionRange;
        }
    }

    // MÓZG OBLICZENIOWY JEDNOSTKI
    public void RecalculateStats()
    {
        // KROK 1: Reset do ustawień fabrycznych (Złota zasada z plików Data)
        currentMoveRange = data.moveRange;
        currentVisionRange = data.visionRange;
        currentAttack = data.maxAttack;

        // KROK 2: Debuff z Pogody (Np. Deszcz ucina 1 pole widzenia)
        if (WeatherManager.Instance != null && WeatherManager.Instance.currentWeather == WeatherManager.WeatherCondition.Rain)
        {
            currentMoveRange = Mathf.Max(1, currentMoveRange - 1);
            currentVisionRange = Mathf.Max(1, currentVisionRange - 1);
        }

        // KROK 3: Buff z Terenu (Wzgórze)
        // Jeśli mapa wzgórz istnieje i na naszych koordynatach znajduje się jakiś kafelek...
        if (hillsMap != null && hillsMap.HasTile(gridPosition))
        {
            currentVisionRange += 2; // Radar widzi o 2 pola dalej!
            currentAttack += 10;     // Przewaga wysokości daje +10 do obrażeń!

            // Opcjonalnie: Zmiana koloru na lekko pomarańczowy by gracz wiedział, że jednostka jest zbuffowana
            // GetComponent<SpriteRenderer>().color = new Color(1f, 0.8f, 0.6f); 
        }
        else
        {
            // Resetujemy kolor, gdy schodzimy ze wzgórza
            if (this.hasMoved) GetComponent<SpriteRenderer>().color = Color.gray;
            else GetComponent<SpriteRenderer>().color = Color.white;
        }

        Debug.Log($"{data.speciesName} na polu {gridPosition}. Atak: {currentAttack}, Wizja: {currentVisionRange}");
    }
}