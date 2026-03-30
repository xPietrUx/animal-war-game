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
        int dist = dx + dy;

        if (dist <= data.moveRange)
        {
            // Upewnij siê, ¿e nazwa zgadza siê z GridManager (LeaveTile)
            GridManager.Instance.LeaveTile(gridPosition);

            gridPosition = targetCell;
            transform.position = mainGrid.GetCellCenterWorld(gridPosition);

            GridManager.Instance.OccupyTile(gridPosition, this);
        }
    }
}