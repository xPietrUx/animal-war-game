using UnityEngine;
using UnityEngine.Tilemaps;

public class SelectionManager : MonoBehaviour
{
    public Grid grid;
    public Animal selectedAnimal;
    public Tilemap highlightMap;
    public Tile highlightTile;
    public Tile attackHighlightTile; // Czerwony kafel (Atak)

    private bool isAttackMode = false; // Zapamiętuje, czy planujemy atak

    void Update()
    {
        // --------------------------------------------------------
        // PPM - Przełącz na Tryb Ataku (Tylko jeśli to Twoja tura!)
        // --------------------------------------------------------
        if (Input.GetMouseButtonDown(1) && selectedAnimal != null)
        {
            // Sprawdzamy, czy jednostka nie wykonała już akcji
            if (!selectedAnimal.hasMoved)
            {
                isAttackMode = true;
                ShowAttackTargets(selectedAnimal);
                Debug.Log("Tryb Ataku! Wybierz cel.");
            }
        }

        // --------------------------------------------------------
        // LPM - Wybór / Ruch / Atak
        // --------------------------------------------------------
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            Vector3Int clickedCell = grid.WorldToCell(mouseWorldPos);

            Animal clickedAnimal = FindAnimalAtCell(clickedCell);

            // 1. Kliknięto w zwierzę
            if (clickedAnimal != null)
            {
                // ATAK: Jeśli tryb ataku aktywny i klikamy wroga
                if (isAttackMode && clickedAnimal.team != selectedAnimal.team)
                {
                    TryAttack(selectedAnimal, clickedAnimal);
                }
                // WYBÓR: Sprawdzamy czy to nasza jednostka i czy ma jeszcze ruch
                else
                {
                    int currentTurnTeam = (int)TurnManager.Instance.currentTurn + 1;

                    if (clickedAnimal.team == currentTurnTeam && !clickedAnimal.hasMoved)
                    {
                        selectedAnimal = clickedAnimal;
                        isAttackMode = false;
                        ShowMoveRange(selectedAnimal);
                    }
                    else
                    {
                        Debug.Log("To nie Twoja tura lub jednostka już się ruszyła!");
                    }
                }
            }
            // 2. Kliknięto w puste pole
            else if (selectedAnimal != null)
            {
                if (isAttackMode)
                {
                    isAttackMode = false;
                    ShowMoveRange(selectedAnimal);
                }
                else if (highlightMap.HasTile(clickedCell))
                {
                    selectedAnimal.MoveTo(clickedCell);
                    // NOWOŚĆ: Po ruchu jednostka kończy swoją aktywność w tej turze
                    selectedAnimal.FinishAction();

                    highlightMap.ClearAllTiles();
                    selectedAnimal = null;
                }
            }
        }
    }

    public void ShowMoveRange(Animal animal)
    {
        highlightMap.ClearAllTiles();
        int range = animal.data.moveRange;
        Vector3Int startPos = animal.gridPosition;

        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                int dist = Mathf.Abs(x) + Mathf.Abs(y); // Manhattan
                Vector3Int tilePos = new Vector3Int(startPos.x + x, startPos.y + y, 0);

                if (dist <= range)
                {
                    bool isStartTile = (tilePos == startPos);

                    if (isStartTile || GridManager.Instance.IsTileWalkable(tilePos))
                    {
                        Animal occupant = FindAnimalAtCell(tilePos);

                        if (occupant == null || occupant == animal)
                        {
                            highlightMap.SetTile(tilePos, highlightTile);
                        }
                    }
                }
            }
        }
    }

    // -------------------------------------------------------------------
    // Funkcja podświetlająca tylko wrogów w zasięgu
    // -------------------------------------------------------------------

    public void ShowAttackTargets(Animal attacker)
    {
        highlightMap.ClearAllTiles();
        int range = attacker.data.attackRange; // Wymaga pola attackRange w AnimalData

        Animal[] allAnimals = Object.FindObjectsByType<Animal>(FindObjectsSortMode.None);
        foreach (Animal target in allAnimals)
        {
            if (target == attacker || !target.enabled) continue; // Ignorujemy samego siebie i nagrobek

            int dx = Mathf.Abs(target.gridPosition.x - attacker.gridPosition.x);
            int dy = Mathf.Abs(target.gridPosition.y - attacker.gridPosition.y);
            int dist = dx + dy; // Manhattan (taki sam kształt jak ruch)

            // Jeśli cel jest w zasięgu, podświetlamy płytkę pod nim na czerwono
            if (dist <= range)
            {
                highlightMap.SetTile(target.gridPosition, attackHighlightTile);
            }
        }
    }

    public void TryAttack(Animal attacker, Animal target)
    {
        int dx = Mathf.Abs(target.gridPosition.x - attacker.gridPosition.x);
        int dy = Mathf.Abs(target.gridPosition.y - attacker.gridPosition.y);
        int dist = dx + dy;

        if (dist <= attacker.data.attackRange)
        {
            Debug.Log($"BAM! {attacker.data.speciesName} atakuje {target.data.speciesName}!");

            // FAKTYCZNE ZADAWANIE OBRAŻEŃ
            // Bierzemy maxAttack atakującego i przekazujemy do TakeDamage celu
            target.TakeDamage(attacker.data.maxAttack);

            // Po ataku jednostka również kończy turę
            attacker.FinishAction();

            highlightMap.ClearAllTiles();
            selectedAnimal = null;
            isAttackMode = false;
        }
    }

    Animal FindAnimalAtCell(Vector3Int cell)
    {
        // Szukamy wszystkich zwierząt na scenie
        Animal[] allAnimals = Object.FindObjectsByType<Animal>(FindObjectsSortMode.None);
        foreach (Animal a in allAnimals)
        {
            if (a.gridPosition == cell && a.enabled)
            {
                return a;
            }
        }
        return null;
    }

    bool CheckIfGraveAtCell(Vector3Int cell)
    {
        Animal[] allObjects = Object.FindObjectsByType<Animal>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Animal a in allObjects)
        {
            // Jeśli pozycja się zgadza, ale skrypt jest WYŁĄCZONY, to znaczy że to grób
            if (a.gridPosition == cell && !a.enabled) return true;
        }
        return false;
    }
}