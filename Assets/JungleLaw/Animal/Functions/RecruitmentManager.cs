using UnityEngine;

public class RecruitmentManager : MonoBehaviour
{
    public static RecruitmentManager Instance;

    [Header("Matryce Jednostek (Prefabs)")]
    public GameObject unit1Prefab; // np. Kapibara (Koszt 40)
    public GameObject unit2Prefab; // np. Żaba (Koszt 60)
    public GameObject unit3Prefab;
    public GameObject unit4Prefab;
    public GameObject unit5Prefab;
    public GameObject unit6Prefab;

    [Header("Gdzie mają się pojawiać?")]
    public Vector3Int p1SpawnPoint; // Kordynaty przy niebieskiej bazie
    public Vector3Int p2SpawnPoint; // Kordynaty przy czerwonej bazie

    private void Awake() => Instance = this;

    // Funkcja odpalana przez szare przyciski w UI
    public void BuyUnit(int unitID)
    {
        GameObject prefabToSpawn = null;

        // 1. Sprawdzamy co gracz chce kupić (TYLKO przypisujemy prefab, bez cen!)
        if (unitID == 1) { prefabToSpawn = unit1Prefab; }
        else if (unitID == 2) { prefabToSpawn = unit2Prefab; }
        else if (unitID == 3) { prefabToSpawn = unit3Prefab; }
        else if (unitID == 4) { prefabToSpawn = unit4Prefab; }
        else if (unitID == 5) { prefabToSpawn = unit5Prefab; }
        else if (unitID == 6) { prefabToSpawn = unit6Prefab; }

        // Pobieramy koszty bezpośrednio z wybranego Prefabu
        int goldCost = prefabToSpawn.GetComponent<Animal>().data.cost;
        int manaCost = prefabToSpawn.GetComponent<Animal>().data.manaCost;

        // KTO KUPUJE? Player 1
        if (TurnManager.Instance.currentTurn == TurnManager.TurnState.Player1)
        {
            // Sprawdzamy czy mamy wystarczająco OBU zasobów
            if (TurnManager.Instance.p1_Gold >= goldCost && TurnManager.Instance.p1_Mana >= manaCost && GridManager.Instance.IsTileWalkable(p1SpawnPoint))
            {
                TurnManager.Instance.p1_Gold -= goldCost;
                TurnManager.Instance.p1_Mana -= manaCost;

                SpawnUnit(prefabToSpawn, p1SpawnPoint, 1);
                UIManager.Instance.UpdateTurnInfo("PLAYER 1", TurnManager.Instance.p1_Gold, TurnManager.Instance.currentGoldIncome, TurnManager.Instance.p1_Mana, TurnManager.Instance.currentManaIncome);
            }
            else
            {
                Debug.LogWarning("Za mało złota/many lub punkt spawnu jest zajęty!");
                UIManager.Instance.ShowWarningMessage(); // Wywołujemy naszą nową funkcję UI
            }
        }
        // KTO KUPUJE? Player 2
        else if (TurnManager.Instance.currentTurn == TurnManager.TurnState.Player2)
        {
            if (TurnManager.Instance.p2_Gold >= goldCost && TurnManager.Instance.p2_Mana >= manaCost && GridManager.Instance.IsTileWalkable(p2SpawnPoint))
            {
                TurnManager.Instance.p2_Gold -= goldCost;
                TurnManager.Instance.p2_Mana -= manaCost;

                SpawnUnit(prefabToSpawn, p2SpawnPoint, 2);
                UIManager.Instance.UpdateTurnInfo("PLAYER 2", TurnManager.Instance.p2_Gold, TurnManager.Instance.currentGoldIncome, TurnManager.Instance.p2_Mana, TurnManager.Instance.currentManaIncome);
            }
            else
            {
                Debug.LogWarning("Za mało złota/many lub punkt spawnu jest zajęty!");
                UIManager.Instance.ShowWarningMessage(); // Wywołujemy naszą nową funkcję UI
            }
        }
        if (UIManager.Instance != null && UIManager.Instance.clickSound != null)
        {
            UIManager.Instance.PlaySFX(UIManager.Instance.clickSound);
        }
    }

    // Funkcja wywoływana przy NAJECHANIU (Pointer Enter)
    public void HoverUnit(int unitID)
    {
        GameObject hoveredPrefab = null;

        // Szukamy, o którym prefabie mowa
        if (unitID == 1) hoveredPrefab = unit1Prefab;
        else if (unitID == 2) hoveredPrefab = unit2Prefab;
        else if (unitID == 3) hoveredPrefab = unit3Prefab;
        else if (unitID == 4) hoveredPrefab = unit4Prefab;
        else if (unitID == 5) hoveredPrefab = unit5Prefab;
        else if (unitID == 6) hoveredPrefab = unit6Prefab;

        if (hoveredPrefab != null)
        {
            // Pobieramy prawdziwe ceny prosto z karty postaci (AnimalData)
            int gCost = hoveredPrefab.GetComponent<Animal>().data.cost;
            int mCost = hoveredPrefab.GetComponent<Animal>().data.manaCost;

            // Wysyłamy do UI!
            UIManager.Instance.UpdateHoverCosts(gCost.ToString(), mCost.ToString());
        }
    }

    // Funkcja wywoływana przy ZJECHANIU MYSZKĄ (Pointer Exit)
    public void ClearHover()
    {
        // Zmieniamy teksty na zera lub puste kreski
        UIManager.Instance.UpdateHoverCosts("0", "0");
    }

    // Funkcja budująca fizyczny obiekt na mapie
    private void SpawnUnit(GameObject prefab, Vector3Int spawnPos, int teamID)
    {
        // Instantiate = Unity kopiuje prefab
        GameObject newObj = Instantiate(prefab);
        Animal newAnimal = newObj.GetComponent<Animal>();

        newAnimal.team = teamID;
        newAnimal.gridPosition = spawnPos;

        // Ustawiamy na siatce
        Grid grid = FindFirstObjectByType<Grid>();
        newObj.transform.position = grid.GetCellCenterWorld(spawnPos);

        // ZAJMUJEMY POLE (żeby nikt inny tu nie wszedł)
        GridManager.Instance.OccupyTile(spawnPos, newAnimal);

        // Nowo kupiona jednostka jest "szara" - ruszy się dopiero w następnej turze
        newAnimal.hasMoved = true;
        newObj.GetComponent<SpriteRenderer>().color = Color.gray;

        Debug.Log($"Gracz {teamID} zrekrutował {newAnimal.data.speciesName}!");
    }
}