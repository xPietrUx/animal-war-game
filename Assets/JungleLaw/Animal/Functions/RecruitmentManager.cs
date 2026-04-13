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
        int cost = 0;
        GameObject prefabToSpawn = null;

        // 1. Sprawdzamy co gracz chce kupić
        if (unitID == 1) { cost = 40; prefabToSpawn = unit1Prefab; }
        else if (unitID == 2) { cost = 60; prefabToSpawn = unit2Prefab; }
        else if (unitID == 3) { cost = 100; prefabToSpawn = unit3Prefab; }
        else if (unitID == 4) { cost = 50; prefabToSpawn = unit4Prefab; }
        else if (unitID == 5) { cost = 70; prefabToSpawn = unit5Prefab; }
        else if (unitID == 6) { cost = 80; prefabToSpawn = unit6Prefab; }

        // 2. KTO KUPUJE? Player 1
        if (TurnManager.Instance.currentTurn == TurnManager.TurnState.Player1)
        {
            // Czy stać go ORAZ czy punkt spawnu jest pusty?
            if (TurnManager.Instance.p1_Gold >= cost && GridManager.Instance.IsTileWalkable(p1SpawnPoint))
            {
                TurnManager.Instance.p1_Gold -= cost; // Pobieramy opłatę
                SpawnUnit(prefabToSpawn, p1SpawnPoint, 1);
                UIManager.Instance.UpdateTurnInfo(TurnManager.Instance.currentTurn.ToString(), TurnManager.Instance.p1_Gold);
            }
            else Debug.LogWarning("Za mało złota lub punkt spawnu jest zablokowany!");
        }
        // 3. KTO KUPUJE? Player 2
        else if (TurnManager.Instance.currentTurn == TurnManager.TurnState.Player2)
        {
            if (TurnManager.Instance.p2_Gold >= cost && GridManager.Instance.IsTileWalkable(p2SpawnPoint))
            {
                TurnManager.Instance.p2_Gold -= cost;
                SpawnUnit(prefabToSpawn, p2SpawnPoint, 2);
                UIManager.Instance.UpdateTurnInfo(TurnManager.Instance.currentTurn.ToString(), TurnManager.Instance.p2_Gold);
            }
            else Debug.LogWarning("Za mało złota lub punkt spawnu jest zablokowany!");
        }
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