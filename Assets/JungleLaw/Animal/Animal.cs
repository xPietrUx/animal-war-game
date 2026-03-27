using UnityEngine;

public class Animal : MonoBehaviour
{
    public AnimalData data; // Tu wrzucisz plik z punktu 1
    public Vector3Int gridPosition; // Gdzie kwiatek jest na siatce

    private Grid mainGrid;
    void Start()
    {
        // Teraz przypisujemy wynik prosto do naszej głównej zmiennej
        mainGrid = FindFirstObjectByType<Grid>();
        SnapToGrid(mainGrid);

        // Postać pojawia się na planszy i od razu rezerwuje swoje pole
        GridManager.Instance.OccupyTile(gridPosition, this);
    }

    // Funkcja przyciągająca do środka kafelka
    public void SnapToGrid(Grid grid)
    {
        // Zamieniamy pozycję w świecie na współrzędne siatki
        gridPosition = grid.WorldToCell(transform.position);
        // Ustawiamy kwiatek idealnie w centrum tej komórki
        transform.position = grid.GetCellCenterWorld(gridPosition);
    }

    // Usunęliśmy argument "Grid grid", bo zwierzę już go pamięta z funkcji Start()
    public void MoveTo(Vector3Int targetCell)
    {
        int dx = Mathf.Abs(targetCell.x - gridPosition.x);
        int dy = Mathf.Abs(targetCell.y - gridPosition.y);

        // Odległość Czebyszewa: wybiera większą z dwóch różnic
        int dist = Mathf.Max(dx, dy);

        // Pytamy GridManager, czy docelowe pole jest puste (IsTileWalkable)
        if (dist <= data.moveRange && GridManager.Instance.IsTileWalkable(targetCell))
        {
            GridManager.Instance.LeaveTile(gridPosition); // Zwalniamy stare pole

            gridPosition = targetCell;
            transform.position = mainGrid.GetCellCenterWorld(gridPosition); // Przesuwamy się

            GridManager.Instance.OccupyTile(gridPosition, this); // Zajmujemy nowe pole
        }
    }
}