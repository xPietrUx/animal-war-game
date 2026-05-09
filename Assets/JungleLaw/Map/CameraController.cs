using UnityEngine;
using UnityEngine.EventSystems; // Dodano dostęp do mechanizmów wykrywania UI

public class CameraController : MonoBehaviour
{
    [Header("Ruch i Zoom")]
    public float moveSpeed = 10f;
    public float zoomSpeed = 5f;

    [Header("Limity Zoomu")]
    public float minZoom = 3f;  // Maksymalne przybliżenie (niska wartość = blisko)
    public float maxZoom = 10f; // Maksymalne oddalenie (wysoka wartość = daleko)

    [Header("Granice Mapy")]
    public Vector2 minBounds; // Np. lewy dolny róg mapy (np. X: -15, Y: -10)
    public Vector2 maxBounds; // Np. prawy górny róg mapy (np. X: 15, Y: 10)

    private Camera cam;

    void Start()
    {
        // Dobra praktyka: zapisujemy referencję do kamery na starcie (jest to wydajniejsze)
        cam = Camera.main;
    }

    void Update()
    {
        // Blokujemy kontrolę nad kamerą, gdy gra jest spauzowana (czas wstrzymany)
        // lub gdy kursor myszy znajduje się nad elementem UI (menu/panel)
        if (Time.timeScale == 0f || EventSystem.current.IsPointerOverGameObject())
        {
            return; 
        }

        // --------------------------------------------------------
        // 1. ZOOM
        // --------------------------------------------------------
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            float newZoom = cam.orthographicSize - scroll * zoomSpeed;
            // Używamy klamry, aby zoom nigdy nie zszedł poniżej minZoom ani powyżej maxZoom
            cam.orthographicSize = Mathf.Clamp(newZoom, minZoom, maxZoom);
        }

        // --------------------------------------------------------
        // 2. OBLICZANIE RUCHU
        // --------------------------------------------------------
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        Vector3 moveDirection = new Vector3(moveX, moveY, 0);

        // Obliczamy, gdzie kamera CHCE polecieć w tej klatce
        Vector3 targetPosition = transform.position + moveDirection * moveSpeed * Time.deltaTime;

        // --------------------------------------------------------
        // 3. BLOKADA GRANIC (Dynamiczna)
        // --------------------------------------------------------
        // Obliczamy fizyczny rozmiar tego, co kamera aktualnie widzi (w zależności od zooma!)
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        // Zamykamy docelową pozycję w klamrze, odejmując od granic "margines" widzenia kamery
        float clampedX = Mathf.Clamp(targetPosition.x, minBounds.x + camWidth, maxBounds.x - camWidth);
        float clampedY = Mathf.Clamp(targetPosition.y, minBounds.y + camHeight, maxBounds.y - camHeight);

        // Nakładamy wyliczoną, uciętą pozycję. Zostawiamy oryginalne 'Z', żeby kamera się nie zepsuła
        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }
}