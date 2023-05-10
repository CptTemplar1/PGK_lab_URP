using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    // Zmienne dotycz�ce poruszania si� gracza w trybie normalnym i w sprincie
    public float movementSpeed;
    public float movementSprintSpeed;
    private float currentMovementSpeed;
    private int sprintEnergy = 50;

    // Dowi�zanie do obiektu przechowuj�cego rotacj� kamery w osi Y
    public Transform orientation;

    // Zmienne przechowuj�ce dane pobrane z osi poruszania - domy�lnie z klawiszy WASD
    float horizontalInput;
    float verticalInput;

    // Komponent Rigidbody gracza
    Rigidbody rgbody;

    void Start()
    {
        // Dowi�zanie do komponentu Rigidbody obiektu gracza
        rgbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Pobranie danych z osi odpowiedzialnych za poruszanie gracza
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Tworzy wektor poruszania na bazie aktualnych danych z Rigidbody
        Vector3 currentVelocity = new Vector3(rgbody.velocity.x, 0f, rgbody.velocity.z);

        // Warunek okre�laj�cy, czy jest to zwyk�y ruch, czy sprint
        if (Input.GetKey(KeyCode.LeftShift) && sprintEnergy > 0)
        {
            currentMovementSpeed = movementSprintSpeed;
        }
        else
        {
            currentMovementSpeed = movementSpeed;
        }


        // Sprawzdanie, czy d�ugo�� wektora (pr�dko�� poruszania) nie przekracza maksymalnej ustalonej pr�dko�ci
        if (currentVelocity.magnitude > currentMovementSpeed)
        {
            // Je�li pr�dko�� przekracza maksymaln� to jest tworzony nowy wektor z aktualnymi danymi i maksymaln� pr�dko�ci�
            Vector3 newVelocity = currentVelocity.normalized * currentMovementSpeed;
            // Aktualizowane s� dane wektora poruszania w komponenecie Rigidbody
            rgbody.velocity = new Vector3(newVelocity.x, rgbody.velocity.y, newVelocity.z);
        }
    }

    private void FixedUpdate()
    {
        // Tworzenie wektora ruchu
        Vector3 moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Dodawanie do komponentu Rigidbody si�y w kierunku i zwrocie okre�lonym przez wektor ruchu
        rgbody.AddForce(moveDirection.normalized * currentMovementSpeed * 10f, ForceMode.Force);
    }
}
