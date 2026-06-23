using UnityEngine;
using System.Collections.Generic;

public class CapturePoint : MonoBehaviour
{
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

    [Header("Grafiki Bazy")]
    public Sprite neutralSprite;
    public Sprite team1Sprite;
    public Sprite team2Sprite;
    public Sprite contestedSprite;

    private SpriteRenderer spriteRenderer;
    private Grid mainGrid;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainGrid = FindFirstObjectByType<Grid>();
        areaPositions.Clear();

        if (isLargeArea)
        {
            Vector3 currentPos = transform.position;
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
            // POPRAWKA 1: Nikt nie stoi na punkcie
            isContested = false;

            // USUNIĘTO: ownerTeam = 0; 
            // Dzięki temu punkt stabilnie pamięta swojego ostatniego właściciela!

            Debug.Log($"Punkt jest pusty, ale zostaje pod kontrolą Gracza: {ownerTeam}");
        }

        UpdateVisuals();
    }

    // POPRAWKA 2: Zmieniono z 'private' na 'public', aby zapobiec błędowi CS0122 w Animal.cs
    public void Capture(int newOwner)
    {
        ownerTeam = newOwner;
        Debug.Log($"Punkt przejęty przez Gracza {ownerTeam}!");

        if (UIManager.Instance != null && UIManager.Instance.coinSound != null)
        {
            UIManager.Instance.PlaySFX(UIManager.Instance.coinSound);
        }

        // DODAJ TO TUTAJ: Zabezpieczenie wizualne przy każdym udanym przejęciu
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        // Usunęliśmy bezwarunkowe 'spriteRenderer.color = Color.white' z samej góry!

        // STAN 1: PUNKT SPORNY (WALKA)
        if (isContested)
        {
            spriteRenderer.color = Color.white; // Czyścimy pod sprajt
            if (contestedSprite != null) spriteRenderer.sprite = contestedSprite;
            else spriteRenderer.color = Color.yellow; // Zapasowy żółty, gdy brak grafiki
            return;
        }

        // STAN 2: PUNKT NEUTRALNY
        if (ownerTeam == 0)
        {
            spriteRenderer.color = Color.white;
            if (neutralSprite != null) spriteRenderer.sprite = neutralSprite;
            else spriteRenderer.color = Color.white; // Zapasowy biały
        }
        // STAN 3: PRZEJĘTY PRZEZ GRACZA 1 (SOJUSZNIK / NIEBIESKI)
        else if (ownerTeam == 1)
        {
            if (team1Sprite != null)
            {
                spriteRenderer.color = Color.white; // Jeśli jest sprajt, resetujemy barwę filtru na białą
                spriteRenderer.sprite = team1Sprite;
            }
            else
            {
                // KOD ZAPASOWY: Jeśli nie wrzuciłeś grafiki w Inspektorze, 
                // system pomaluje kwadrat na ładny niebieski kolor!
                spriteRenderer.color = new Color(0.2f, 0.6f, 1f);
            }
        }
        // STAN 4: PRZEJĘTY PRZEZ GRACZA 2 (PRZECIWNIK / CZERWONY)
        else if (ownerTeam == 2)
        {
            if (team2Sprite != null)
            {
                spriteRenderer.color = Color.white;
                spriteRenderer.sprite = team2Sprite;
            }
            else
            {
                // KOD ZAPASOWY: Zastępczy jasnoczerwony dla przeciwnika
                spriteRenderer.color = new Color(1f, 0.3f, 0.3f);
            }
        }
    }
}