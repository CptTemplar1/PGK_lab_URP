using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    // Okre�la czu�o�� myszki
    public float mouseSensitivity;

    // Dowi�zanie do obiektu przechowuj�cego rotacj� kamery w osi Y
    public Transform orientation;

    // Zmienne okre�laj�ce rotacj� kamery 
    float rotationX;
    float rotationY;

    void Start()
    {
        // Blokada widoczno�ci kursora
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Pobieranie danych o zmianie po�o�enia myszki
        float mouseMovementX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime * 2;
        float mouseMovementY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime * 2;

        // Aktualizacja zmiennych przechowuj�cych rotacj� kamery o pobrane warto�ci
        rotationY += mouseMovementX;
        rotationX -= mouseMovementY;

        // Ograniczenie maksymalnego k�ta spojrzenia w g�r� i w d� do 90 stopni
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);

        // Zmiana rotacji kamery na ustalon� przez zmienne
        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
        // Zmiana rotacji obiektu przechowuj�cego orientacj� gracza
        orientation.rotation = Quaternion.Euler(0, rotationY, 0);
    }
}
