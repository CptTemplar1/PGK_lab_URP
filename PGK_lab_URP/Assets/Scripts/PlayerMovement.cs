using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Prêdkoœæ podstawowa
    public float sprintSpeed = 10f; // Prêdkoœæ po przytrzymaniu Shift

    private Rigidbody rb;
    private Vector3 movement;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // Odczytaj wejœcie z klawiatury
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Oblicz wektor ruchu
        movement = new Vector3(horizontal, 0f, vertical).normalized;

        // Jeœli przytrzymano Shift, zwiêksz prêdkoœæ
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
