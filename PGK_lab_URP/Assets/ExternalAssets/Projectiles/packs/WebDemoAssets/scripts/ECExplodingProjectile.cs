using UnityEngine;
using System.Collections;

/* THIS CODE IS JUST FOR PREVIEW AND TESTING */
// Feel free to use any code and picking on it, I cannot guaratnee it will fit into your project
public class ECExplodingProjectile : MonoBehaviour
{
    public GameObject impactPrefab;
    public GameObject explosionPrefab;
    public float thrust;

    public Rigidbody thisRigidbody;

    public GameObject particleKillGroup;
    private Collider thisCollider;

    public bool LookRotation = true;
    public bool Missile = false;
    public Transform missileTarget;
    public float projectileSpeed;
    public float projectileSpeedMultiplier;

    public bool ignorePrevRotation = false;

    public bool explodeOnTimer = false;
    public float explosionTimer;
    float timer;

    private Vector3 previousPosition;

    // Use this for initialization
    void Start()
    {
        thisRigidbody = GetComponent<Rigidbody>();
        if (Missile)
        {
            missileTarget = GameObject.FindWithTag("Target").transform;
        }
        thisCollider = GetComponent<Collider>();
        previousPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        /*     if(Input.GetButtonUp("Fire2"))
             {
                 Explode();
             }*/
        timer += Time.deltaTime;
        if (timer >= explosionTimer && explodeOnTimer == true)
        {
            Explode();
        }

    }

    void FixedUpdate()
    {
        if (Missile)
        {
            projectileSpeed += projectileSpeed * projectileSpeedMultiplier;
            //   transform.position = Vector3.MoveTowards(transform.position, missileTarget.transform.position, 0);

            transform.LookAt(missileTarget);

            thisRigidbody.AddForce(transform.forward * projectileSpeed);
        }

        if (LookRotation && timer >= 0.05f)
        {
            transform.rotation = Quaternion.LookRotation(thisRigidbody.velocity);
        }

        CheckCollision(previousPosition);

        previousPosition = transform.position;
    }

    void CheckCollision(Vector3 prevPos)
    {
        RaycastHit hit;
        Vector3 direction = transform.position - prevPos;
        Ray ray = new Ray(prevPos, direction);
        float dist = Vector3.Distance(transform.position, prevPos);
        if (Physics.Raycast(ray, out hit, dist))
        {
            transform.position = hit.point;
            Quaternion rot = Quaternion.FromToRotation(Vector3.forward, hit.normal);
            Vector3 pos = hit.point;
            Instantiate(impactPrefab, pos, rot);

            //zrespienie efektu krwi po trafieniu w obiekt o tagu Enemy
            if (hit.transform.CompareTag("Enemy"))
            {
                SpawnBloodEffect(pos);
            }

            if (!explodeOnTimer && Missile == false)
            {
                Destroy(gameObject);
            }
            else if (Missile == true)
            {
                thisCollider.enabled = false;
                particleKillGroup.SetActive(false);
                thisRigidbody.velocity = Vector3.zero;
                Destroy(gameObject, 5);
            }

        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag != "FX")
        {
            ContactPoint contact = collision.contacts[0];
            Quaternion rot = Quaternion.FromToRotation(Vector3.forward, contact.normal);
            if (ignorePrevRotation)
            {
                rot = Quaternion.Euler(0, 0, 0);
            }
            Vector3 pos = contact.point;
            Instantiate(impactPrefab, pos, rot);
            if (!explodeOnTimer && Missile == false)
            {
                Destroy(gameObject);
            }
            else if (Missile == true)
            {

                thisCollider.enabled = false;
                particleKillGroup.SetActive(false);
                thisRigidbody.velocity = Vector3.zero;

                Destroy(gameObject, 5);

            }
        }
    }

    void Explode()
    {
        Instantiate(explosionPrefab, gameObject.transform.position, Quaternion.Euler(0, 0, 0));
        Destroy(gameObject);
    }



    // --------------- //
    // VOLUMETRIC BLOOD//
    // --------------- //

    public bool InfiniteDecal;
    public GameObject BloodAttach;
    public GameObject[] BloodFX;

    public Vector3 direction;
    int effectIdx;


    //metoda spawnująca efekt krwi
    void SpawnBloodEffect(Vector3 pos)
    {
        //OBSŁUGA ROZBRYZGU KRWI
        // var randRotation = new Vector3(0, Random.value * 360f, 0);
        // var dir = CalculateAngle(Vector3.forward, hit.normal);

        float angle = Mathf.Atan2(pos.normalized.x, pos.normalized.z) * Mathf.Rad2Deg + 180; //Wersja - krew rozbryzguje się w tył
        //float angle = Mathf.Atan2(pos.normalized.x, pos.normalized.z) * Mathf.Rad2Deg; //Wersja - krew rozbryzguje się w przód

        //var effectIdx = Random.Range(0, BloodFX.Length);
        if (effectIdx == BloodFX.Length) effectIdx = 0;

        var instance = Instantiate(BloodFX[effectIdx], pos, Quaternion.Euler(0, angle + 90, 0));
        effectIdx++;

        var settings = instance.GetComponent<BFX_BloodSettings>();
        //settings.FreezeDecalDisappearance = InfiniteDecal;

        var attachBloodInstance = Instantiate(BloodAttach);
        var bloodT = attachBloodInstance.transform;
        bloodT.position = pos;
        bloodT.localRotation = Quaternion.identity;
        bloodT.localScale = Vector3.one * Random.Range(0.75f, 1.2f);

        bloodT.LookAt(pos + pos.normalized, direction); //Wersja - krew rozbryzguje się w tył
        //bloodT.LookAt(pos - pos.normalized, direction); //Wersja - krew rozbryzguje się w przód

        bloodT.Rotate(90, 0, 0);
        //Destroy(attachBloodInstance, 20);

        // if (!InfiniteDecal) Destroy(instance, 20);
    }

    Transform GetNearestObject(Transform hit, Vector3 hitPos)
    {
        var closestPos = 100f;
        Transform closestBone = null;
        var childs = hit.GetComponentsInChildren<Transform>();

        foreach (var child in childs)
        {
            var dist = Vector3.Distance(child.position, hitPos);
            if (dist < closestPos)
            {
                closestPos = dist;
                closestBone = child;
            }
        }

        var distRoot = Vector3.Distance(hit.position, hitPos);
        if (distRoot < closestPos)
        {
            closestPos = distRoot;
            closestBone = hit;
        }
        return closestBone;
    }

    public float CalculateAngle(Vector3 from, Vector3 to)
    {
        return Quaternion.FromToRotation(Vector3.up, to - from).eulerAngles.z;
    }

}