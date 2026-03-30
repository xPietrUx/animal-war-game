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
        // 1. CZY TU W OGÓLE JEST ZIEMIA? (Whitelisting)
        // Je?li na mapie 'Ground' nie ma ?adnego kafelka, nie pozwól wej??.
        if (!groundTilemap.HasTile(cellPos)) return false;

        // 2. CZY JEST TU PRZESZKODA? (Blacklisting)
        if (obstacleTilemap.HasTile(cellPos)) return false;

        // 3. CZY STOI TU INNE ZWIERZÊ?
        if (occupiedTiles.ContainsKey(cellPos) && occupiedTiles[cellPos] != null) return false;

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