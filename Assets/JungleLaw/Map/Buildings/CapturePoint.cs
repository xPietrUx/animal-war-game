using UnityEngine;
using System.Collections.Generic;

public class CapturePoint : MonoBehaviour
{
    // PRZYWRÓCONO: Główna pozycja, by Mgła Wojny znowu działała!
    public Vector3Int gridPosition;
    public List<Vector3Int> areaPositions = new List<Vector3Int>();

    public int ownerTeam = 0;
    public bool isContested = false;

    [Header("Typ Punktu")]
    public bool isLargeArea = false;

    [Header("Ekonomia")]
    public int goldPerTurn = 20;
    public int manaPerTurn = 2;
    public bool isAttackPoint = true;
    public int damagePerTurn = 5;

    // NOWOŚĆ: Pola na Twoje własne grafiki!
    [Header("Grafiki Bazy")]
    public Sprite neutralSprite;
    public Sprite team1Sprite;
    public Sprite team2Sprite;
    public Sprite contestedSprite; // Grafika gdy punkt jest sporny (opcjonalnie)

    private SpriteRenderer spriteRenderer;
    private Grid mainGrid;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainGrid = FindFirstObjectByType<Grid>();
        areaPositions.Clear();

        if (isLargeArea)
        {
            // WYRÓWNANIE: Zanim obliczymy kafelki, "snapujemy" bazę do przecięcia kratek
            Vector3 currentPos = transform.position;
            // Zaokrąglamy do pełnych kratek, aby środek bazy 2x2 był zawsze na linii styku
            transform.position = new Vector3(Mathf.Round(currentPos.x), Mathf.Round(currentPos.y), 0);

            Vector3 center = transform.position;

            areaPositions.Add(mainGrid.WorldToCell(center + new Vector3(-0.5f, -0.5f, 0)));
            areaPositions.Add(mainGrid.WorldToCell(center + new Vector3(0.5f, -0.5f, 0)));
            areaPositions.Add(mainGrid.WorldToCell(center + new Vector3(-0.5f, 0.5f, 0)));
            areaPositions.Add(mainGrid.WorldToCell(center + new Vector3(0.5f, 0.5f, 0)));

            gridPosition = areaPositions[0];
        }
        else
        {
            gridPosition = mainGrid.WorldToCell(transform.position);
            transform.position = mainGrid.GetCellCenterWorld(gridPosition);
            areaPositions.Add(gridPosition);
        }

        UpdateVisuals();
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying && areaPositions.Count > 0)
        {
            Grid grid = FindFirstObjectByType<Grid>();
            if (grid == null) return;

            Gizmos.color = Color.magenta;

            foreach (Vector3Int pos in areaPositions)
            {
                Vector3 cellCenter = grid.GetCellCenterWorld(pos);
                Gizmos.DrawSphere(cellCenter, 0.2f);
            }
        }
    }

    public void EvaluateControl()
    {
        int p1Units = 0;
        int p2Units = 0;

        Animal[] allAnimals = Object.FindObjectsByType<Animal>(FindObjectsSortMode.None);
        foreach (Animal a in allAnimals)
        {
            if (!a.enabled) continue;

            if (areaPositions.Contains(a.gridPosition))
            {
                if (a.team == 1) p1Units++;
                else if (a.team == 2) p2Units++;
            }
        }

        if (p1Units > 0 && p2Units > 0)
        {
            isContested = true;
            Debug.Log("PUNKT JEST SPORNY!");
        }
        else if (p1Units > 0 && p2Units == 0)
        {
            isContested = false;
            if (ownerTeam != 1) Capture(1);
        }
        else if (p2Units > 0 && p1Units == 0)
        {
            isContested = false;
            if (ownerTeam != 2) Capture(2);
        }
        else
        {
            // Nikt nie stoi na punkcie
            isContested = false;

            // DODAJ TO: Resetujemy właściciela na 0 (neutralny)
            ownerTeam = 0;

            Debug.Log("Punkt jest teraz pusty i neutralny.");
        }

        UpdateVisuals();
    }

    private void Capture(int newOwner)
    {
        ownerTeam = newOwner;
        Debug.Log($"Punkt przejęty przez Gracza {ownerTeam}!");
        if (UIManager.Instance != null && UIManager.Instance.coinSound != null)
        {
            UIManager.Instance.PlaySFX(UIManager.Instance.coinSound);
        }
    }

    // ZMIENIONO: Teraz podmienia Sprite'y zamiast tylko kolorować kwadrat
    private void UpdateVisuals()
    {
        // Upewniamy się, że kolor to czysty biały, aby Twoje grafiki miały swoje naturalne kolory
        spriteRenderer.color = Color.white;

        if (isContested)
        {
            if (contestedSprite != null) spriteRenderer.sprite = contestedSprite;
            else spriteRenderer.color = Color.yellow; // Zapasowe zachowanie, gdyby brakło grafiki
            return;
        }

        if (ownerTeam == 0)
        {
            if (neutralSprite != null) spriteRenderer.sprite = neutralSprite;
        }
        else if (ownerTeam == 1)
        {
            if (team1Sprite != null) spriteRenderer.sprite = team1Sprite;
        }
        else if (ownerTeam == 2)
        {
            if (team2Sprite != null) spriteRenderer.sprite = team2Sprite;
        }
    }
}