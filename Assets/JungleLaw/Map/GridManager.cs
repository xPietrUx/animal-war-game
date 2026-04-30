using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance; // Singleton, ¿eby inne skrypty mia³y tu ³atwy dostêp

    [Header("Tilemaps - Pod³o¿e")]
    public Tilemap groundTilemap;   // Ziemia
    public Tilemap bridgeTilemap;   // Most

    [Header("Tilemaps - Przeszkody")]
    public Tilemap obstacleTilemap; // Drzewa i obwódka
    public Tilemap riverTilemap;    // Rzeka

    // S³ownik pamiêtaj¹cy, kto stoi na danej kratce
    private Dictionary<Vector3Int, Animal> occupiedTiles = new Dictionary<Vector3Int, Animal>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public bool IsTileWalkable(Vector3Int cellPos)
    {
        // Sprawdzamy, czy na tym polu jest most (bêdzie potrzebny do ominiêcia blokady rzeki)
        bool hasBridge = bridgeTilemap != null && bridgeTilemap.HasTile(cellPos);

        // 1. CZY JEST TU PRZESZKODA? (Drzewa, ramka)
        if (obstacleTilemap != null && obstacleTilemap.HasTile(cellPos))
            return false;

        // 2. CZY JEST TU RZEKA? (Jeœli jest rzeka, ale NIE MA mostu -> blokada)
        if (riverTilemap != null && riverTilemap.HasTile(cellPos) && !hasBridge)
            return false;

        // 3. CZY JEST PO CZYM CHODZIÆ? (Ziemia lub Most)
        bool hasGround = groundTilemap != null && groundTilemap.HasTile(cellPos);
        if (!hasGround && !hasBridge)
            return false;

        // 4. CZY STOI TU INNE ZWIERZÊ?
        if (occupiedTiles.ContainsKey(cellPos) && occupiedTiles[cellPos] != null)
            return false;

        return true; // Jeœli przesz³o wszystkie testy, mo¿na wejœæ!
    }

    // Funkcja do Twojego systemu widocznoœci (Mg³a wojny / Line of Sight)
    public bool DoesTileBlockVision(Vector3Int cellPos)
    {
        // Drzewa i przeszkody blokuj¹ widok
        if (obstacleTilemap != null && obstacleTilemap.HasTile(cellPos))
            return true;

        return false;
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