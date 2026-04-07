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
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            Vector3Int clickedCell = grid.WorldToCell(mouseWorldPos);

            Animal clickedAnimal = FindAnimalAtCell(clickedCell);

            // 1. Kliknięto w jakieś zwierzę
            if (clickedAnimal != null)
            {
                // Jeśli planujemy atak i klikamy INNE zwierzę -> Uderzamy!
                if (isAttackMode && selectedAnimal != null && clickedAnimal != selectedAnimal)
                {
                    TryAttack(selectedAnimal, clickedAnimal);
                }
                // Jeśli nie planujemy ataku -> Zwykły wybór postaci
                else
                {
                    selectedAnimal = clickedAnimal;
                    isAttackMode = false;
                    ShowMoveRange(selectedAnimal);
                    Debug.Log("Wybrano: " + selectedAnimal.data.speciesName);
                }
            }
            // 2. Kliknięto w puste pole (bez zwierzęcia)
            else if (selectedAnimal != null)
            {
                // Jeśli planowaliśmy atak, ale kliknęliśmy w trawę -> Anulujemy atak i wracamy do ruchu
                if (isAttackMode)
                {
                    isAttackMode = false;
                    ShowMoveRange(selectedAnimal);
                    Debug.Log("Anulowano atak. Powrót do ruchu.");
                }
                // W przeciwnym razie sprawdzamy, czy można tam pójść
                else if (highlightMap.HasTile(clickedCell))
                {
                    selectedAnimal.MoveTo(clickedCell);
                    highlightMap.ClearAllTiles();
                    selectedAnimal = null;
                }
                else
                {
                    Debug.Log("To pole jest nieosiągalne!");
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
            if (target == attacker) continue; // Ignorujemy samego siebie

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

        // Ostateczne sprawdzenie przed uderzeniem
        if (dist <= attacker.data.attackRange)
        {
            Debug.Log($"BAM! {attacker.data.speciesName} atakuje {target.data.speciesName}!");

            highlightMap.ClearAllTiles();
            selectedAnimal = null;
            isAttackMode = false;
        }
        else
        {
            Debug.Log("Cel uciekł z zasięgu!");
        }
    }

    Animal FindAnimalAtCell(Vector3Int cell)
    {
        Animal[] allAnimals = Object.FindObjectsByType<Animal>(FindObjectsSortMode.None);
        foreach (Animal a in allAnimals)
        {
            if (a.gridPosition == cell) return a;
        }
        return null;
    }
}