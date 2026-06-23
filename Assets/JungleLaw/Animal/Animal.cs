using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Animal : MonoBehaviour
{
    public AnimalData data;
    public Vector3Int gridPosition;
    public int currentHP;

    [Header("Efekty")]
    public GameObject hpEffectPrefab;
    [Header("UI")]
    public Image healthBarFill;
    [Header("Aktualne Statystyki (Zmienne)")]
    public int currentMoveRange;
    public int currentVisionRange;
    public int currentAttack;

    private UnityEngine.Tilemaps.Tilemap hillsMap;
    private Grid mainGrid;
    private SpriteRenderer spriteRenderer;
    private bool isDead = false;
    public int team;
    public bool hasMoved = false;

    public void FinishAction()
    {
        hasMoved = true;
        GetComponent<SpriteRenderer>().color = Color.gray;
    }

    public void ResetTurn()
    {
        hasMoved = false;
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

        // Szukamy warstwy wzgórz
        GameObject hillsObj = GameObject.Find("HillsMap");
        if (hillsObj != null) hillsMap = hillsObj.GetComponent<UnityEngine.Tilemaps.Tilemap>();

        SnapToGrid(mainGrid);

        // POPRAWKA 1: Najpierw rezerwujemy pole w managerze siatki
        GridManager.Instance.OccupyTile(gridPosition, this);

        // POPRAWKA 2: Wywołujemy MÓZG OBLICZENIOWY jako ostatni. 
        // Usunąłem stąd linijki hardkodujące "data.moveRange", dzięki czemu 
        // przeliczenie wzgórz i pogody nie zostanie skasowane na starcie!
        RecalculateStats();

        if (data.idleSprite != null)
            spriteRenderer.sprite = data.idleSprite;

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
        if (isDead) return;

        currentHP -= amount;
        if (UIManager.Instance != null) UIManager.Instance.PlaySFX(UIManager.Instance.hurtSound);
        UpdateHealthBar();
        Debug.Log($"{data.speciesName} oberwał za {amount}. Zostało HP: {currentHP}");

        if (hpEffectPrefab != null)
        {
            Instantiate(hpEffectPrefab, transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);
        }

        if (currentHP <= 0)
        {
            isDead = true;
            Die();
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(ChangeExpressionRoutine(data.surpriseSprite, 0.8f));
        }
    }

    private IEnumerator ChangeExpressionRoutine(Sprite newSprite, float duration)
    {
        if (newSprite == null || isDead) yield break;

        spriteRenderer.sprite = newSprite;
        yield return new WaitForSeconds(duration);

        if (!isDead)
            spriteRenderer.sprite = data.idleSprite;
    }

    private void Die()
    {
        if (UIManager.Instance != null) UIManager.Instance.PlaySFX(UIManager.Instance.deadSound);
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

        if (dist <= currentMoveRange && GridManager.Instance.IsTileWalkable(targetCell))
        {
            // 1. SPRZĄTANIE: Zwalniamy stare pole
            GridManager.Instance.LeaveTile(gridPosition);

            // 2. RUCH: Aktualizujemy pozycję
            gridPosition = targetCell;
            transform.position = mainGrid.GetCellCenterWorld(gridPosition);

            // 3. REZERWACJA: Zajmujemy nowe pole w GridManagerze
            GridManager.Instance.OccupyTile(gridPosition, this);
            if (UIManager.Instance != null) UIManager.Instance.PlaySFX(UIManager.Instance.jumpSound);

            // 4. STATYSTYKI: Przeliczamy buffy (np. ze wzgórz)
            RecalculateStats();

            // 5. MGŁA WOJNY: Odsłaniamy teren w nowym miejscu
            if (FogOfWarManager.Instance != null)
            {
                FogOfWarManager.Instance.UpdateFog(this.team);
            }

            // POPRAWKA 3: PRZESUNIĘCIE LOGIKI PRZEJĘĆ
            // Zamiast bezwarunkowo resetować wszystkie punkty na mapie, szukamy 
            // czy na kafelku, na którym WŁAŚNIE STANĘLIŚMY, znajduje się punkt.
            // Jeśli Twój GridManager posiada taką funkcję lub punkty mają Collidery typu Trigger, 
            // punkt wywoła swoją wewnętrzną funkcję Capture(this.team) i zapisze stan na stałe.
            CapturePoint[] allPoints = Object.FindObjectsByType<CapturePoint>(FindObjectsSortMode.None);
            foreach (CapturePoint point in allPoints)
            {
                // Jeśli koordynaty punktu zgadzają się z naszą nową pozycją -> przejmujemy go!
                // (Zakładam, że CapturePoint ma zmienną gridPosition lub pobiera ją w Start)
                if (point.gridPosition == this.gridPosition)
                {
                    point.Capture(this.team);
                }
            }
        }
    }

    // --- ŚMIERĆ (ANIMOWANA) ---

    private IEnumerator DeathSequenceRoutine()
    {
        Debug.Log("Rozpoczynam animację śmierci...");

        if (data.deathFrames != null && data.deathFrames.Length > 0)
        {
            foreach (Sprite frame in data.deathFrames)
            {
                spriteRenderer.sprite = frame;
                yield return new WaitForSeconds(0.12f);
            }
        }

        yield return new WaitForSeconds(0.5f);

        if (data.graveSprite != null)
        {
            spriteRenderer.sprite = data.graveSprite;
        }

        this.enabled = false;

        yield return new WaitForSeconds(10.0f);

        GridManager.Instance.LeaveTile(gridPosition);
        Destroy(gameObject);
        Debug.Log("Nagrobek zniknął, pole jest wolne.");
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)currentHP / data.maxHP;
        }
    }

    public void UpdateAllegianceColor()
    {
        if (healthBarFill != null)
        {
            int currentTurnTeam = (int)TurnManager.Instance.currentTurn + 1;

            if (this.team == currentTurnTeam)
            {
                healthBarFill.color = Color.green;
            }
            else
            {
                healthBarFill.color = Color.red;
            }
        }
    }

    // Starsza metoda zastąpiona przez bezpieczniejszy RecalculateStats()
    public void ApplyWeather(WeatherManager.WeatherCondition weather)
    {
        // Ta funkcja nie jest już używana, ponieważ RecalculateStats pobiera stan bezpośrednio z managera pogody.
    }

    public void RecalculateStats()
    {
        currentMoveRange = data.moveRange;
        currentVisionRange = data.visionRange;
        currentAttack = data.maxAttack;

        if (WeatherManager.Instance != null && WeatherManager.Instance.currentWeather == WeatherManager.WeatherCondition.Rain)
        {
            currentMoveRange = Mathf.Max(1, currentMoveRange - 1);
            currentVisionRange = Mathf.Max(1, currentVisionRange - 1);
        }

        if (hillsMap != null && hillsMap.HasTile(gridPosition))
        {
            currentVisionRange += 2;
            currentAttack += 10;
        }
        else
        {
            if (this.hasMoved) GetComponent<SpriteRenderer>().color = Color.gray;
            else GetComponent<SpriteRenderer>().color = Color.white;
        }

        Debug.Log($"{data.speciesName} na polu {gridPosition}. Atak: {currentAttack}, Wizja: {currentVisionRange}");
    }
}