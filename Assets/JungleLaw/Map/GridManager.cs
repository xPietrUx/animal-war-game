using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance; // Singleton, ¿eby inne skrypty mia³y tu ³atwy dostêp

    [Header("Tilemaps")]
    public Tilemap obstacleTilemap; // Mapa z kamieniami/drzewami

    // S³ownik pamiêtaj¹cy, kto stoi na danej kratce
    private Dictionary<Vector3Int, Animal> occupiedTiles = new Dictionary<Vector3Int, Animal>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Funkcja sprawdzaj¹ca, czy mo¿na wejœæ na dane pole
    public bool IsTileWalkable(Vector3Int cellPos)
    {
        // 1. Czy jest tu narysowany kamieñ/drzewo?
        if (obstacleTilemap.HasTile(cellPos)) return false;

        // 2. Czy stoi tu inne zwierzê?
        if (occupiedTiles.ContainsKey(cellPos) && occupiedTiles[cellPos] != null) return false;

        return true; // Pole jest puste i wolne
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