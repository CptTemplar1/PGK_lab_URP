using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ECprojectileActor : MonoBehaviour {

    public Transform spawnLocator; 
    public Transform shellLocator;

    [System.Serializable]
    public class Projectile
    {
        public string name;
        public Rigidbody bombPrefab;
        public float min, max;
        public bool rapidFire;
        public float rapidFireCooldown;   

        public GameObject shellPrefab;
    }
    public Projectile projectile;

    public float rapidFireDelay;

    public bool firing;
    public int bombType = 0;

    public bool Torque = false;
    public float Tor_min, Tor_max;

    public bool MinorRotate;
    public bool MajorRotate = false;
    int seq = 0;

	
	// Update is called once per frame
	void Update ()
    {
	    if(Input.GetButtonDown("Fire1"))
        {
            firing = true;
            Fire();
        }

        if (Input.GetButtonUp("Fire1"))
        {
            firing = false;
        }
	}

    public void Fire()
    {
        //respienie łuski
        Instantiate(projectile.shellPrefab, shellLocator.position, shellLocator.rotation);

        Rigidbody rocketInstance;
        rocketInstance = Instantiate(projectile.bombPrefab, spawnLocator.position,spawnLocator.rotation) as Rigidbody;
        // Quaternion.Euler(0,90,0)
        rocketInstance.AddForce(spawnLocator.forward * Random.Range(projectile.min, projectile.max));

        if (Torque)
        {
            rocketInstance.AddTorque(spawnLocator.up * Random.Range(Tor_min, Tor_max));
        }
        if (MinorRotate)
        {
            RandomizeRotation();
        }
        if (MajorRotate)
        {
            Major_RandomizeRotation();
        }
    }


    void RandomizeRotation()
    {
        if (seq == 0)
        {
            seq++;
            transform.Rotate(0, 1, 0);
        }
      else if (seq == 1)
        {
            seq++;
            transform.Rotate(1, 1, 0);
        }
      else if (seq == 2)
        {
            seq++;
            transform.Rotate(1, -3, 0);
        }
      else if (seq == 3)
        {
            seq++;
            transform.Rotate(-2, 1, 0);
        }
       else if (seq == 4)
        {
            seq++;
            transform.Rotate(1, 1, 1);
        }
       else if (seq == 5)
        {
            seq = 0;
            transform.Rotate(-1, -1, -1);
        }
    }

    void Major_RandomizeRotation()
    {
        if (seq == 0)
        {
            seq++;
            transform.Rotate(0, 25, 0);
        }
        else if (seq == 1)
        {
            seq++;
            transform.Rotate(0, -50, 0);
        }
        else if (seq == 2)
        {
            seq++;
            transform.Rotate(0, 25, 0);
        }
        else if (seq == 3)
        {
            seq++;
            transform.Rotate(25, 0, 0);
        }
        else if (seq == 4)
        {
            seq++;
            transform.Rotate(-50, 0, 0);
        }
        else if (seq == 5)
        {
            seq = 0;
            transform.Rotate(25, 0, 0);
        }
    }
}
