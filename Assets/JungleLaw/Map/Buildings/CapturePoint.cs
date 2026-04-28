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
    // NOWOŚĆ: Przełącznik w Inspektorze!
    public bool isLargeArea = false;

    [Header("Ekonomia")]
    public int goldPerTurn = 20;
    public int manaPerTurn = 2;
    public bool isAttackPoint = true;
    public int damagePerTurn = 5;

    private SpriteRenderer spriteRenderer;
    private Grid mainGrid;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainGrid = FindFirstObjectByType<Grid>();
        areaPositions.Clear();

        if (isLargeArea)
        {
            // PANCERNA LOGIKA:
            // Szukamy środków 4 kafelków fizycznie znajdujących się pod budynkiem.
            // Współrzędne +0.5 i -0.5 celują idealnie w środki kratek!
            Vector3 center = transform.position;

            areaPositions.Add(mainGrid.WorldToCell(center + new Vector3(-0.5f, -0.5f, 0))); // Lewy Dół
            areaPositions.Add(mainGrid.WorldToCell(center + new Vector3(0.5f, -0.5f, 0)));  // Prawy Dół
            areaPositions.Add(mainGrid.WorldToCell(center + new Vector3(-0.5f, 0.5f, 0)));  // Lewy Góra
            areaPositions.Add(mainGrid.WorldToCell(center + new Vector3(0.5f, 0.5f, 0)));   // Prawy Góra

            gridPosition = areaPositions[0]; // Baza dla Mgły Wojny
        }
        else
        {
            gridPosition = mainGrid.WorldToCell(transform.position);
            transform.position = mainGrid.GetCellCenterWorld(gridPosition);
            areaPositions.Add(gridPosition);
        }

        UpdateColor();
    }

    // ==========================================
    // NOWOŚĆ: MAGIA DLA DEVELOPERA (Wizualizacja)
    // ==========================================
    private void OnDrawGizmosSelected()
    {
        // Ta funkcja rysuje kształty w edytorze TYLKO gdy klikniesz budynek
        if (Application.isPlaying && areaPositions.Count > 0)
        {
            Grid grid = FindFirstObjectByType<Grid>();
            if (grid == null) return;

            // Ustawiamy "farbę" na rzucający się w oczy fioletowy kolor
            Gizmos.color = Color.magenta;

            // Rysujemy kuleczki dokładnie tam, gdzie skanuje nasz Radar!
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

        // Szukamy kto stoi na naszym wyznaczonym terenie
        Animal[] allAnimals = Object.FindObjectsByType<Animal>(FindObjectsSortMode.None);
        foreach (Animal a in allAnimals)
        {
            if (!a.enabled) continue;

            // Kod działa identycznie dla 1 jak i 4 kafelków!
            if (areaPositions.Contains(a.gridPosition))
            {
                if (a.team == 1) p1Units++;
                else if (a.team == 2) p2Units++;
            }
        }

        // LOGIKA KONTROLI TERYTORIUM
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
            // Nikogo tu nie ma, zachowujemy status quo
            isContested = false;
        }

        UpdateColor();
    }

    private void Capture(int newOwner)
    {
        ownerTeam = newOwner;
        Debug.Log($"Punkt przejęty przez Gracza {ownerTeam}!");
    }

    private void UpdateColor()
    {
        if (isContested)
        {
            spriteRenderer.color = Color.yellow;
            return;
        }

        if (ownerTeam == 0) spriteRenderer.color = Color.white;
        else if (ownerTeam == 1) spriteRenderer.color = Color.blue;
        else if (ownerTeam == 2) spriteRenderer.color = Color.red;
    }
}