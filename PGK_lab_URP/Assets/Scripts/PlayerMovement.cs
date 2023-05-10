using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Pr�dko�� podstawowa
    public float sprintSpeed = 10f; // Pr�dko�� po przytrzymaniu Shift

    private Rigidbody rb;
    private Vector3 movement;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // Odczytaj wej�cie z klawiatury
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Oblicz wektor ruchu
        movement = new Vector3(horizontal, 0f, vertical).normalized;

        // Je�li przytrzymano Shift, zwi�ksz pr�dko��
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            movement *= sprintSpeed;
        }
        else
        {
            movement *= moveSpeed;
        }
    }

    private void FixedUpdate()
    {
        // Wykonaj ruch gracza
        rb.MovePosition(rb.position + movement * Time.fixedDeltaTime);
    }
}
