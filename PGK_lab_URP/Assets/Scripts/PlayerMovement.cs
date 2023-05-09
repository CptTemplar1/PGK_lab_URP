using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    // Zmienne dotycz�ce poruszania si� gracza w trybie normalnym i w sprincie
    public float movementSpeed;
    public float movementSprintSpeed;
    private float currentMovementSpeed;
    private bool doSprint = false;
    private bool doRegenerate = false;
    private int sprintEnergy = 50;
    private int maxSprintEnergy = 50;
    // Pasek sprintu
    public GameObject sprintBarObject;
    public Image sprintBar;

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
 
        UpdateSprintBar();
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
            // Warunek zapobiega wielu wywo�aniom metody naraz
            if (!doSprint)
                StartCoroutine(SprintCoroutine());

            currentMovementSpeed = movementSprintSpeed;
        }
        else
        {
            currentMovementSpeed = movementSpeed;
        }

        // Metoda wykonywana jest je�li gracz stoi w miejscu, brakuje mu energii i nie jest ona regenerowana
        if (currentVelocity == new Vector3(0, 0, 0) && sprintEnergy < maxSprintEnergy && !doRegenerate)
            StartCoroutine(RegenerateEnergyCoroutine());

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

    // Aktualizuje poziom wype�nienia i widoczno�� paska sprintu
    private void UpdateSprintBar()
    {
        // Ustala procentowy poziom wype�nienia paska sprintu
        sprintBar.fillAmount = (float)sprintEnergy / (float)maxSprintEnergy;

        // Wy��cza widoczno�� paska sprintu, je�li jest on wype�niony
        if (sprintBar.fillAmount == 1)
            sprintBarObject.SetActive(false);
        else
            sprintBarObject.SetActive(true);
    }

    // Metoda "zu�ywaj�ca" energi� gracza podczas sprintu
    private IEnumerator SprintCoroutine()
    {
        doSprint = true;

        yield return new WaitForSeconds(0.1f);

        sprintEnergy--;
        doSprint = false;
        UpdateSprintBar();
        //Debug.Log("Pozosta�a energia: " + sprintEnergy);
    }

    // Metoda regeneruj�ca energi� gracza, je�li gracz stoi w miejscu
    private IEnumerator RegenerateEnergyCoroutine()
    {
        doRegenerate = true;

        yield return new WaitForSeconds(0.1f);

        sprintEnergy++;
        doRegenerate = false;
        UpdateSprintBar();
        //Debug.Log("Pozosta�a energia: " + sprintEnergy);
    }
}
