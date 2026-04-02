using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f; // Prêdkoœæ poruszania
    public float zoomSpeed = 5f;  // Prêdkoœæ przybli¿ania

    void Update()
    {
        // 1. POBIERANIE WEJŒCIA (Input)
        //GetAxis pobiera wartoœæ od -1 do 1 (np. 'A' to -1, 'D' to 1)
        float moveX = Input.GetAxis("Horizontal"); 
        float moveY = Input.GetAxis("Vertical");

        // 2. OBLICZANIE RUCHU
        Vector3 moveDirection = new Vector3(moveX, moveY, 0);
        
        // 3. PRZESUNIÊCIE
        // Time.deltaTime sprawia, ¿e ruch jest niezale¿ny od FPS-ów
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        // 4. OPCJONALNIE: ZOOM (Dla kamer ortograficznych)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Camera.main.orthographicSize -= scroll * zoomSpeed;
    }
}