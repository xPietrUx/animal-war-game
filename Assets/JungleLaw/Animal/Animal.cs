using UnityEngine;

public class Animal : MonoBehaviour
{
    public AnimalData data; // Tu wrzucisz plik z punktu 1
    public Vector3Int gridPosition; // Gdzie kwiatek jest na siatce

    void Start()
    {
        // Znajduje grid na scenie i ustawia postać idealnie w kratce na starcie
        Grid mainGrid = FindFirstObjectByType<Grid>();
        SnapToGrid(mainGrid);
    }

    // Funkcja przyciągająca do środka kafelka
    public void SnapToGrid(Grid grid)
    {
        // Zamieniamy pozycję w świecie na współrzędne siatki
        gridPosition = grid.WorldToCell(transform.position);
        // Ustawiamy kwiatek idealnie w centrum tej komórki
        transform.position = grid.GetCellCenterWorld(gridPosition);
    }

    public void MoveTo(Vector3Int targetCell, Grid grid)
    {
        int dx = Mathf.Abs(targetCell.x - gridPosition.x);
        int dy = Mathf.Abs(targetCell.y - gridPosition.y);

        // Odległość Czebyszewa: wybiera większą z dwóch różnic
        int dist = Mathf.Max(dx, dy);

        if (dist <= data.moveRange)
        {
            gridPosition = targetCell;
            transform.position = grid.GetCellCenterWorld(gridPosition);
        }
    }
}