using UnityEngine;
using System.Collections;

public class Animal : MonoBehaviour
{
    public AnimalData data;
    public Vector3Int gridPosition;
    public int currentHP;

    [Header("Efekty")]
    public GameObject hpEffectPrefab; // Przeciągnij tu prefab -hp

    private Grid mainGrid;
    private SpriteRenderer spriteRenderer;
    private bool isDead = false; // Zabezpieczenie przed "miganiem" animacji

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
        GridManager.Instance.LeaveTile(gridPosition);
        StopAllCoroutines(); // Zatrzymuje powrót do "Idle" z TakeDamage
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
    }

    // --- ŚMIERĆ (ANIMOWANA) ---

    private IEnumerator DeathSequenceRoutine()
    {
        Debug.Log("Rozpoczynam animację śmierci...");

        // 1. Pętla przez klatki animacji
        if (data.deathFrames != null && data.deathFrames.Length > 0)
        {
            foreach (Sprite frame in data.deathFrames)
            {
                spriteRenderer.sprite = frame;
                yield return new WaitForSeconds(0.12f);
            }
        }

        // 2. Pauza na ostatniej klatce (postać leży)
        yield return new WaitForSeconds(1.0f);

        // 3. Zamiana w nagrobek
        if (data.graveSprite != null)
        {
            spriteRenderer.sprite = data.graveSprite;
            spriteRenderer.sortingOrder = -1; // Nagrobek pod spodem innych jednostek
        }

        // 4. Wyłączenie skryptu
        this.enabled = false;
        Debug.Log("Nagrobek ustawiony.");
    }
}