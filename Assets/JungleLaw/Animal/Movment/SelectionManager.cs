using System.Collections.Generic;
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
        // --- TYMCZASOWY KOD DO DEBUGOWANIA ---
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            Vector3Int cellPos = grid.WorldToCell(mouseWorldPos);

            bool hasGround = GridManager.Instance.groundTilemap.HasTile(cellPos);
            bool hasRiver = GridManager.Instance.riverTilemap.HasTile(cellPos);
            bool hasBridge = GridManager.Instance.bridgeTilemap.HasTile(cellPos);
            bool hasObstacle = GridManager.Instance.obstacleTilemap.HasTile(cellPos);

            Debug.Log($"Sprawdzam pole {cellPos} | Ziemia: {hasGround} | Rzeka: {hasRiver} | Most: {hasBridge} | Przeszkody: {hasObstacle}");
        }
        // -------------------------------------
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
        int range = animal.currentMoveRange;
        Vector3Int startPos = animal.gridPosition;

        // Używamy algorytmu BFS (Breadth-First Search) do "rozlewania" zasięgu ruchu
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        Dictionary<Vector3Int, int> distances = new Dictionary<Vector3Int, int>();

        queue.Enqueue(startPos);
        distances[startPos] = 0;

        // Zawsze podświetlamy pole, na którym stoi jednostka
        highlightMap.SetTile(startPos, highlightTile);

        // 4 kierunki ruchu (bez skosów)
        Vector3Int[] directions = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

        while (queue.Count > 0)
        {
            Vector3Int currentPos = queue.Dequeue();
            int currentDist = distances[currentPos];

            // Jeśli osiągnęliśmy limit ruchu, nie szukamy dalej z tego pola
            if (currentDist >= range) continue;

            foreach (Vector3Int dir in directions)
            {
                Vector3Int neighborPos = currentPos + dir;

                // Jeśli jeszcze tu nie byliśmy
                if (!distances.ContainsKey(neighborPos))
                {
                    // Sprawdzamy czy na to sąsiednie pole można wejść
                    if (GridManager.Instance.IsTileWalkable(neighborPos))
                    {
                        // Sprawdzamy kto tam stoi
                        Animal occupant = FindAnimalAtCell(neighborPos);

                        // Pozwalamy wejść tylko na puste pola (lub pole na którym już stoimy)
                        // (Opcjonalnie możesz dodać logikę pozwalającą przechodzić przez sojuszników)
                        if (occupant == null || occupant == animal)
                        {
                            distances[neighborPos] = currentDist + 1;
                            queue.Enqueue(neighborPos);
                            highlightMap.SetTile(neighborPos, highlightTile);
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