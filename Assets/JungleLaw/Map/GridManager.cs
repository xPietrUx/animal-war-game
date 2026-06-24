using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance; // Singleton, ¿eby inne skrypty mia³y tu ³atwy dostêp

    // Funkcja sprawdzaj¹ca, czy mo¿na wejœæ na dane pole
    [Header("Tilemaps")]
    public Tilemap groundTilemap;   // TWOJA NOWOŒÆ: G?ówna mapa z traw?/ziemi?
    public Tilemap obstacleTilemap; // To ju? masz

    // S³ownik pamiêtaj¹cy, kto stoi na danej kratce
    private Dictionary<Vector3Int, Animal> occupiedTiles = new Dictionary<Vector3Int, Animal>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public bool IsTileWalkable(Vector3Int cellPos)
    {
        // 1. Czy jest ziemia?
        if (!groundTilemap.HasTile(cellPos)) return false;

        // 2. Czy jest przeszkoda (œciana/woda)?
        if (obstacleTilemap.HasTile(cellPos)) return false;

        // 3. Czy stoi tu inne zwierzê? 
        // SPRAWDZAMY: Jeœli w s³owniku coœ jest, to czy to na pewno ¿ywe zwierzê?
        if (occupiedTiles.ContainsKey(cellPos))
        {
            Animal animalOnTile = occupiedTiles[cellPos];
            // Jeœli na polu stoi zwierzê i jest ono w³¹czone (nie jest nagrobkiem) -> blokujemy
            if (animalOnTile != null && animalOnTile.enabled)
                return false;
        }

        return true;
    }

    // Funkcja zajmuj¹ca pole (u¿ywana, gdy postaæ na nie wchodzi)
    public void OccupyTile(Vector3Int cellPos, Animal animal)
    {
        occupiedTiles[cellPos] = animal;
    }

    // Funkcja zwalniaj¹ca pole (u¿ywana, gdy postaæ z niego schodzi)
    public void LeaveTile(Vector3Int cellPos)
    {
        if (occupiedTiles.ContainsKey(cellPos)) occupiedTiles.Remove(cellPos);
    }
}