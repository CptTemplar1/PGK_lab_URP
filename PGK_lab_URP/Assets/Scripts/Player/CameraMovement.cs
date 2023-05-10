using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    // Okreœla czu³oœæ myszki
    public float mouseSensitivity = 4;

    // Dowi¹zanie do obiektu przechowuj¹cego rotacjê kamery w osi Y
    public Transform orientation;

    // Zmienne okreœlaj¹ce rotacjê kamery 
    public float rotationX;
    public float rotationY;

    // Zmienna okreœlaj¹ca, czy aktywna jest mo¿liwoœæ poruszania kamer¹
    public bool isActive = true;

    void Start()
    {
        // Blokada widocznoœci kursora
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SetActiveState(bool newState)
    {
        isActive = newState;
    }

    void Update()
    {
        // Pobieranie danych o zmianie po³o¿enia myszki
        float mouseMovementX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseMovementY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Aktualizacja zmiennych przechowuj¹cych rotacjê kamery o pobrane wartoœci
        rotationY += mouseMovementX;
        rotationX -= mouseMovementY;

        // Ograniczenie maksymalnego k¹ta spojrzenia w górê i w dó³ do 60 stopni
        rotationX = Mathf.Clamp(rotationX, -60f, 60f);

        // Zmiana rotacji kamery na ustalon¹ przez zmienne
        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
        // Zmiana rotacji obiektu przechowuj¹cego orientacjê gracza
        orientation.rotation = Quaternion.Euler(0, rotationY, 0);
    }
}
