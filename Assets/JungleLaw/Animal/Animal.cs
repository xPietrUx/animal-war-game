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
            isDead = true; // Blokujemy inne animacje
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

        if (dist <= data.moveRange && GridManager.Instance.IsTileWalkable(targetCell))
        {
            GridManager.Instance.LeaveTile(gridPosition);
            gridPosition = targetCell;
            transform.position = mainGrid.GetCellCenterWorld(gridPosition);
            GridManager.Instance.OccupyTile(gridPosition, this);
        }
        CapturePoint[] allPoints = Object.FindObjectsByType<CapturePoint>(FindObjectsSortMode.None);
        foreach (CapturePoint point in allPoints)
        {
            if (point.gridPosition == this.gridPosition)
            {
                point.Capture(this.team); // Wbijamy flagę naszej drużyny!
                break;
            }
        }

        if (FogOfWarManager.Instance != null)
        {
            FogOfWarManager.Instance.UpdateFog(this.team);
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
}