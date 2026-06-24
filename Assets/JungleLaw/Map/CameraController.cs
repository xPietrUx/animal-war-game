using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    [Header("Ruch i Zoom")]
    public float moveSpeed = 10f;
    public float zoomSpeed = 5f;

    [Header("Limity Zoomu")]
    public float minZoom = 3f;
    public float maxZoom = 10f;

    [Header("Granice Mapy")]
    public Vector2 minBounds;
    public Vector2 maxBounds;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        // POPRAWKA: Blokujemy kamerę TYLKO gdy gra jest spauzowana (czas wstrzymany).
        // Usunęliśmy stąd warunek 'IsPointerOverGameObject()'. 
        // Dzięki temu możesz swobodnie latać kamerą po mapie, nawet gdy myszka celuje w przyciski UI!
        if (Time.timeScale == 0f)
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
            cam.orthographicSize = Mathf.Clamp(newZoom, minZoom, maxZoom);
        }

        // --------------------------------------------------------
        // 2. OBLICZANIE RUCHU
        // --------------------------------------------------------
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        Vector3 moveDirection = new Vector3(moveX, moveY, 0);

        Vector3 targetPosition = transform.position + moveDirection * moveSpeed * Time.deltaTime;

        // --------------------------------------------------------
        // 3. BLOKADA GRANIC (Dynamiczna)
        // --------------------------------------------------------
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        float clampedX = Mathf.Clamp(targetPosition.x, minBounds.x + camWidth, maxBounds.x - camWidth);
        float clampedY = Mathf.Clamp(targetPosition.y, minBounds.y + camHeight, maxBounds.y - camHeight);

        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }
}