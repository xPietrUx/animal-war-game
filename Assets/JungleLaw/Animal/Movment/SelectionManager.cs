using UnityEngine;
using UnityEngine.Tilemaps;

public class SelectionManager : MonoBehaviour
{
    public Grid grid;               // Przeciągnij tu swój Grid z Hierarchy
    public Animal selectedAnimal;   // Tu gra zapamięta, kogo wybrałeś

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int clickedCell = grid.WorldToCell(mouseWorldPos);

            Animal clickedAnimal = FindAnimalAtCell(clickedCell);

            // 1. Jeśli kliknąłeś w zwierzę -> WYBIERZ I POKAŻ ZASIĘG
            if (clickedAnimal != null)
            {
                selectedAnimal = clickedAnimal;
                ShowMoveRange(selectedAnimal);
                Debug.Log("Wybrano: " + selectedAnimal.data.speciesName);
            }
            // 2. Jeśli nie kliknąłeś w zwierzę, ale masz kogoś wybranego -> RUSZ SIĘ
            else if (selectedAnimal != null)
            {
                selectedAnimal.MoveTo(clickedCell, grid);
                highlightMap.ClearAllTiles(); // Czyścimy po ruchu
                selectedAnimal = null;        // Opcjonalnie: odznaczamy jednostkę
            }
        }
    }

    // Prosta funkcja pomocnicza
    Animal FindAnimalAtCell(Vector3Int cell)
    {
        Animal[] allAnimals = Object.FindObjectsByType<Animal>(FindObjectsSortMode.None);
        foreach (Animal a in allAnimals)
        {
            if (a.gridPosition == cell) return a;
        }
        return null;
    }

    public Tilemap highlightMap; // Przeciągnij tu nową mapę w Inspektorze
    public Tile highlightTile;   // Wybierz dowolny kafel (np. zwykły biały kwadrat)

    public void ShowMoveRange(Animal animal)
    {
        highlightMap.ClearAllTiles(); // Czyścimy poprzednie podświetlenie

        int range = animal.data.moveRange;
        Vector3Int startPos = animal.gridPosition;

        // Pętla sprawdzająca kwadrat wokół jednostki
        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                Vector3Int tilePos = new Vector3Int(startPos.x + x, startPos.y + y, 0);
                highlightMap.SetTile(tilePos, highlightTile);
            }
        }
    }
}