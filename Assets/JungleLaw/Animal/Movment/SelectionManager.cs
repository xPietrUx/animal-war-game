using UnityEngine;
using UnityEngine.Tilemaps;

public class SelectionManager : MonoBehaviour
{
    public Grid grid;
    public Animal selectedAnimal;
    public Tilemap highlightMap;
    public Tile highlightTile;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            Vector3Int clickedCell = grid.WorldToCell(mouseWorldPos);

            Animal clickedAnimal = FindAnimalAtCell(clickedCell);

            // 1. Wybór nowej jednostki
            if (clickedAnimal != null)
            {
                selectedAnimal = clickedAnimal;
                ShowMoveRange(selectedAnimal);
                Debug.Log("Wybrano: " + selectedAnimal.data.speciesName);
            }
            // 2. Ruch wybraną jednostką
            else if (selectedAnimal != null)
            {
                // Sprawdzamy czy kliknięte pole jest podświetlone (czyli czy jest w zasięgu)
                if (highlightMap.HasTile(clickedCell))
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
                    // NOWY WARUNEK: Pole jest OK, jeśli:
                    // 1. Jest to pole startowe (animal tu stoi, więc może tu zostać)
                    // 2. LUB GridManager mówi, że jest wolne
                    bool isStartTile = (tilePos == startPos);

                    if (isStartTile || GridManager.Instance.IsTileWalkable(tilePos))
                    {
                        // Sprawdzamy czy nie stoi tam INNA jednostka
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