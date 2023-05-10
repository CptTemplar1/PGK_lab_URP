using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    // Okre�la czu�o�� myszki
    public float mouseSensitivity = 4;

    // Dowi�zanie do obiektu przechowuj�cego rotacj� kamery w osi Y
    public Transform orientation;

    // Zmienne okre�laj�ce rotacj� kamery 
    public float rotationX;
    public float rotationY;

    // Zmienna okre�laj�ca, czy aktywna jest mo�liwo�� poruszania kamer�
    public bool isActive = true;

    void Start()
    {
        // Blokada widoczno�ci kursora
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SetActiveState(bool newState)
    {
        isActive = newState;
    }

    void Update()
    {
        // Pobieranie danych o zmianie po�o�enia myszki
        float mouseMovementX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseMovementY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Aktualizacja zmiennych przechowuj�cych rotacj� kamery o pobrane warto�ci
        rotationY += mouseMovementX;
        rotationX -= mouseMovementY;

        // Ograniczenie maksymalnego k�ta spojrzenia w g�r� i w d� do 60 stopni
        rotationX = Mathf.Clamp(rotationX, -60f, 60f);

        // Zmiana rotacji kamery na ustalon� przez zmienne
        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
        // Zmiana rotacji obiektu przechowuj�cego orientacj� gracza
        orientation.rotation = Quaternion.Euler(0, rotationY, 0);
    }
}
