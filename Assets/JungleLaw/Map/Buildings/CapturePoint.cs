using UnityEngine;

public class CapturePoint : MonoBehaviour
{
    public Vector3Int gridPosition;
    public int ownerTeam = 0; // 0 = Neutralny, 1 = Player 1, 2 = Player 2
    [Header("Ekonomia")]
    public int goldPerTurn = 20;
    public int manaPerTurn = 1;
    public bool isAttackPoint = true; // Czy ten punkt bije bazę?
    public int damagePerTurn = 5;

    private SpriteRenderer spriteRenderer;
    private Grid mainGrid;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainGrid = FindFirstObjectByType<Grid>();

        // Automatyczne ustawianie na siatce (jak u zwierząt)
        gridPosition = mainGrid.WorldToCell(transform.position);
        transform.position = mainGrid.GetCellCenterWorld(gridPosition);

        UpdateColor();
    }

    // Tę funkcję wywoła zwierzę, gdy na ten punkt wejdzie
    public void Capture(int newOwner)
    {
        if (ownerTeam != newOwner)
        {
            ownerTeam = newOwner;
            UpdateColor();
            Debug.Log($"Punkt przejęty przez Gracza {ownerTeam}!");
        }
    }

    private void UpdateColor()
    {
        if (ownerTeam == 0) spriteRenderer.color = Color.white; // Neutralny
        else if (ownerTeam == 1) spriteRenderer.color = Color.blue; // Player 1
        else if (ownerTeam == 2) spriteRenderer.color = Color.red; // Player 2
    }
}