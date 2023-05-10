using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    // Okreœla czu³oœæ myszki
    public float mouseSensitivity;

    // Dowi¹zanie do obiektu przechowuj¹cego rotacjê kamery w osi Y
    public Transform orientation;

    // Zmienne okreœlaj¹ce rotacjê kamery 
    float rotationX;
    float rotationY;

    void Start()
    {
        // Blokada widocznoœci kursora
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Pobieranie danych o zmianie po³o¿enia myszki
        float mouseMovementX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime * 2;
        float mouseMovementY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime * 2;

        // Aktualizacja zmiennych przechowuj¹cych rotacjê kamery o pobrane wartoœci
        rotationY += mouseMovementX;
        rotationX -= mouseMovementY;

        // Ograniczenie maksymalnego k¹ta spojrzenia w górê i w dó³ do 90 stopni
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);

        // Zmiana rotacji kamery na ustalon¹ przez zmienne
        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
        // Zmiana rotacji obiektu przechowuj¹cego orientacjê gracza
        orientation.rotation = Quaternion.Euler(0, rotationY, 0);
    }
}
