using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    // Zmienne dotycz¹ce poruszania siê gracza w trybie normalnym i w sprincie
    public float movementSpeed;
    public float movementSprintSpeed;
    private float currentMovementSpeed;
    private int sprintEnergy = 50;

    // Dowi¹zanie do obiektu przechowuj¹cego rotacjê kamery w osi Y
    public Transform orientation;

    // Zmienne przechowuj¹ce dane pobrane z osi poruszania - domyœlnie z klawiszy WASD
    float horizontalInput;
    float verticalInput;

    // Komponent Rigidbody gracza
    Rigidbody rgbody;

    void Start()
    {
        // Dowi¹zanie do komponentu Rigidbody obiektu gracza
        rgbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Pobranie danych z osi odpowiedzialnych za poruszanie gracza
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Tworzy wektor poruszania na bazie aktualnych danych z Rigidbody
        Vector3 currentVelocity = new Vector3(rgbody.velocity.x, 0f, rgbody.velocity.z);

        // Warunek okreœlaj¹cy, czy jest to zwyk³y ruch, czy sprint
        if (Input.GetKey(KeyCode.LeftShift) && sprintEnergy > 0)
        {
            currentMovementSpeed = movementSprintSpeed;
        }
        else
        {
            currentMovementSpeed = movementSpeed;
        }


        // Sprawzdanie, czy d³ugoœæ wektora (prêdkoœæ poruszania) nie przekracza maksymalnej ustalonej prêdkoœci
        if (currentVelocity.magnitude > currentMovementSpeed)
        {
            // Jeœli prêdkoœæ przekracza maksymaln¹ to jest tworzony nowy wektor z aktualnymi danymi i maksymaln¹ prêdkoœci¹
            Vector3 newVelocity = currentVelocity.normalized * currentMovementSpeed;
            // Aktualizowane s¹ dane wektora poruszania w komponenecie Rigidbody
            rgbody.velocity = new Vector3(newVelocity.x, rgbody.velocity.y, newVelocity.z);
        }
    }

    private void FixedUpdate()
    {
        // Tworzenie wektora ruchu
        Vector3 moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Dodawanie do komponentu Rigidbody si³y w kierunku i zwrocie okreœlonym przez wektor ruchu
        rgbody.AddForce(moveDirection.normalized * currentMovementSpeed * 10f, ForceMode.Force);
    }
}
