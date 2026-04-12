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
        // PRAWY PRZYCISK MYSZY (PPM) - Przełącz na Tryb Ataku
        // --------------------------------------------------------
        if (Input.GetMouseButtonDown(1) && selectedAnimal != null)
        {
            isAttackMode = true;
            ShowAttackTargets(selectedAnimal);
            Debug.Log("Tryb Ataku! Wybierz cel.");
        }

        // --------------------------------------------------------
        // LEWY PRZYCISK MYSZY (LPM) - Wybór / Ruch / Wykonanie Ataku
        // --------------------------------------------------------
        if (Input.GetMouseButtonDown(0))
        {
            // POPRAWKA BŁĘDU "OUT OF VIEW FRUSTUM":
            // Pobieramy pozycję myszki i ustawiamy Z na odległość kamery od świata (zwykle 10f w 2D)
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10f;

            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
            mouseWorldPos.z = 0; // Zerujemy Z, aby idealnie trafić w siatkę 2D

            Vector3Int clickedCell = grid.WorldToCell(mouseWorldPos);
            Animal clickedAnimal = FindAnimalAtCell(clickedCell);

            if (clickedAnimal != null)
            {
                // Logika ataku lub wyboru (to masz dobrze)
                if (isAttackMode && selectedAnimal != null && clickedAnimal != selectedAnimal)
                {
                    TryAttack(selectedAnimal, clickedAnimal);
                }
                else
                {
                    selectedAnimal = clickedAnimal;
                    isAttackMode = false;
                    ShowMoveRange(selectedAnimal);
                    Debug.Log("Wybrano: " + selectedAnimal.data.speciesName);
                }
            }
            else
            {
                // 2. Kliknięto w POLE BEZ ŻYWEGO ZWIERZĘCIA (pusta trawa LUB nagrobek)

                // Sprawdzamy, czy pod myszką jest nagrobek (martwe zwierzę)
                // Musimy stworzyć pomocniczą funkcję FindAnyObjectAtCell, 
                // która widzi nawet te z wyłączonym skryptem.
                bool isGraveHere = CheckIfGraveAtCell(clickedCell);

                if (selectedAnimal != null)
                {
                    if (isAttackMode)
                    {
                        // Jeśli kliknęliśmy w nagrobek lub trawę w trybie ataku -> anuluj
                        isAttackMode = false;
                        ShowMoveRange(selectedAnimal);
                        Debug.Log("Anulowano atak.");
                    }
                    else if (!isGraveHere && highlightMap.HasTile(clickedCell))
                    {
                        // MOŻEMY IŚĆ tylko jeśli NIE MA tam nagrobka i kafel jest podświetlony
                        selectedAnimal.MoveTo(clickedCell);
                        highlightMap.ClearAllTiles();
                        selectedAnimal = null;
                    }
                    else if (isGraveHere)
                    {
                        Debug.Log("Tu stoi nagrobek, nie przejdziesz!");
                        // Opcjonalnie: highlightMap.ClearAllTiles(); selectedAnimal = null;
                    }
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
        if (target == null || !target.enabled) return;

        int dx = Mathf.Abs(target.gridPosition.x - attacker.gridPosition.x);
        int dy = Mathf.Abs(target.gridPosition.y - attacker.gridPosition.y);
        int dist = dx + dy;

        if (dist <= attacker.data.attackRange)
        {
            // 1. Atakujący zmienia minę na złą
            attacker.PlayAttackAnimation();

            // 2. POBIERAMY OBRAŻENIA Z DANYCH ATAKUJĄCEGO
            // Zamiast wpisywać "10", bierzemy to, co wpisałaś w AnimalData (maxAttack)
            int damage = attacker.data.maxAttack;

            // 3. Zadajemy te konkretne obrażenia ofierze
            target.TakeDamage(damage);

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