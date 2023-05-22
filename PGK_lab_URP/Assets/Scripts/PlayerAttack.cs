using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.PlayerSettings;

public class PlayerAttack : MonoBehaviour
{
    PlayerInput playerInput;
    PlayerInput.MainActions input;

    Animator animator;
    AudioSource audioSource;

    [Header("Attacking")]
    public float attackDistance = 3f;
    public float attackDelay = 0.4f;
    public float attackSpeed = 1f;
    public int attackDamage = 1;
    public LayerMask attackLayer;

    public GameObject hitEffect;
    public AudioClip swordSwing;
    public AudioClip hitSound;

    bool attacking = false;
    bool readyToAttack = true;
    int attackCount;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();

        playerInput = new PlayerInput();
        input = playerInput.Main;
        AssignInputs();
    }

    void Update()
    {
        if (input.Attack.IsPressed())
        {
            Attack();
        }
    }

    void OnEnable()
    {
        input.Enable();
    }

    void OnDisable()
    {
        input.Disable();
    }

    // ------------------- //
    // ATTACKING BEHAVIOUR //
    // ------------------- //

    public const string ATTACK1 = "Attack 1";
    public const string ATTACK2 = "Attack 2";

    public void Attack()
    {
        if (!readyToAttack || attacking) return;

        readyToAttack = false;
        attacking = true;

        Invoke(nameof(ResetAttack), attackSpeed);
        Invoke(nameof(AttackRaycast), attackDelay);

        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(swordSwing);

        if (attackCount == 0)
        {
            ChangeAnimationState(ATTACK1);
            attackCount++;
        }
        else
        {
            ChangeAnimationState(ATTACK2);
            attackCount = 0;
        }
    }

    void ResetAttack()
    {
        attacking = false;
        readyToAttack = true;
    }

    void AttackRaycast()
    {
        Camera cam = Camera.main;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, attackDistance, attackLayer))
        {
            HitTarget(hit.point);

            //zrespienie efektu krwi po trafieniu w obiekt o tagu Enemy
            if (hit.transform.CompareTag("Enemy"))
            {
                SpawnBloodEffect(hit.point);
            }

            //jeœli trafiony posiada obiekt Actor to zabierze mu ¿ycie
            if (hit.transform.TryGetComponent<Actor>(out Actor T))
            {
                T.TakeDamage(attackDamage);
            }
        }
    }

    void HitTarget(Vector3 pos)
    {
        audioSource.pitch = 1;
        audioSource.PlayOneShot(hitSound);

        GameObject GO = Instantiate(hitEffect, pos, Quaternion.identity);
        //usuwanie efektu trafienia po 20 sekundach
        Destroy(GO, 20);

    }

    // ---------- //
    // ANIMATIONS //
    // ---------- //

    public void ChangeAnimationState(string newState)
    {
        // STOP THE SAME ANIMATION FROM INTERRUPTING WITH ITSELF //
        if (animator.GetCurrentAnimatorStateInfo(0).IsName(newState)) return;

        // PLAY THE ANIMATION //
        animator.CrossFadeInFixedTime(newState, 0.2f);
    }

    void AssignInputs()
    {
        input.Attack.started += ctx => Attack();
    }





    // --------------- //
    // VOLUMETRIC BLOOD//
    // --------------- //

    public bool InfiniteDecal;
    public GameObject BloodAttach;
    public GameObject[] BloodFX;

    public Vector3 direction;
    int effectIdx;

    
    //metoda spawnuj¹ca efekt krwi
    void SpawnBloodEffect(Vector3 pos)
    {
        //OBS£UGA ROZBRYZGU KRWI
        // var randRotation = new Vector3(0, Random.value * 360f, 0);
        // var dir = CalculateAngle(Vector3.forward, hit.normal);
        
        //float angle = Mathf.Atan2(pos.normalized.x, pos.normalized.z) * Mathf.Rad2Deg + 180; //Wersja - krew rozbryzguje siê w ty³
        float angle = Mathf.Atan2(pos.normalized.x, pos.normalized.z) * Mathf.Rad2Deg; //Wersja - krew rozbryzguje siê w przód

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

        //bloodT.LookAt(pos + pos.normalized, direction); //Wersja - krew rozbryzguje siê w ty³
        bloodT.LookAt(pos - pos.normalized, direction); //Wersja - krew rozbryzguje siê w przód

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
