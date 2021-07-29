using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossLvlTwoController : EnemyController
{
    public Transform player;
    public float TargetDistanceToPlayer;
    public float RetreatDistance;
    private Rigidbody2D rigidbody2D;
    public float movementSpeed;
    private bool isPlayerToTheLeft;
    private bool isTurnedLeft;

    //Shooting related variables
    public GameObject ShurikenPrefab;
    public Transform ShurikenGizmod;
    public int ShurikenDamage;
    private bool isShooting;


    //Animation related variables
    private Animator animator;
    private float actionTimer;

    //Field of view
    public float viewDistance;

    void Start()
    {
        base.Start();
        isPlayerToTheLeft = true;
        isTurnedLeft = true;
        animator = GetComponent<Animator>();
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Collision();
        if (actionTimer > 0)
        {
            actionTimer -= Time.deltaTime;
            return;
        }

        IsInvincible = false;

        TurnToPlayer();
        ChooseNextAction();
    }

    void Collision()
    {
        Vector2 boxScale = new Vector2(1f , transform.localScale.y);
        Vector2 direction = Vector2.right;
        float horizontalLengthCollider = transform.localScale.x;
        Vector2 startPosition = new Vector2(transform.position.x + horizontalLengthCollider , transform.position.y / 2);
        
        if (isTurnedLeft)
        {
            direction = Vector2.left;
            startPosition = new Vector2(transform.position.x - horizontalLengthCollider, transform.position.y / 2);
        }

        RaycastHit2D hitInfo = Physics2D.BoxCast(startPosition, boxScale, 0f,  direction, viewDistance);

        if (hitInfo.collider != null)
        {   
            if (hitInfo.collider.CompareTag("Bullet"))         
            {
                Debug.Log($"Hit by: {hitInfo.collider.tag} - At the position: {hitInfo.collider.transform.position.x}");
            }
        }
    }
    private void ChooseNextAction()
    {
        var probability = Random.Range(0.0f, 1.0f) * 100;

        //if shoot was detected (shoot delay)// do something else

        if (IsPlayerFarAway())
        {
            if (probability <= 60)
            {
                Move();
            } 
            else if (probability <= 90)
            {
                //Jump();
            }
            else if (probability <= 95) //@TODO: AND shoot delay
            {
                Shoot();
            }
            else if (probability <= 100)
            {
                 Defend();
            }
        } else
        {
            if (probability <= 50) //@TODO: and shoot delay
            {
                Shoot();
            }
            else if (probability <= 75)
            {
                Defend();
            }
            else if (probability <= 85)
            {
                //Jump();
            }
            else if (probability <= 100)
            {
                Move();
            }
        }

    }

    private void TurnToPlayer()
    {        
        isPlayerToTheLeft = player.transform.position.x < transform.position.x;

        if ( (isPlayerToTheLeft && !isTurnedLeft) || (!isPlayerToTheLeft && isTurnedLeft)  )
        {
            transform.Rotate(0f, 180f, 0f);
            isTurnedLeft = !isTurnedLeft;
        }
    }

    public void Move()
    {
        if (IsPlayerFarAway())
        {
            WaitForAnimation("Boss_Walking");
            rigidbody2D.velocity = isPlayerToTheLeft ? Vector2.left * movementSpeed : Vector2.right * movementSpeed;
        } else
        {
            WaitForAnimation("Boss_Idle");
            rigidbody2D.velocity = Vector2.zero;
        }
    }

    private bool IsPlayerFarAway()
    {
        float playerXPosition = player.transform.position.x;
        float currentXPosition = transform.position.x;
        float distance = Mathf.Abs(playerXPosition - currentXPosition);
        
        return distance > TargetDistanceToPlayer;
    }

    public void Defend()
    {
        IsInvincible = true;
        WaitForAnimation("Boss_Defend");    
    }

    private void Shoot()
    {
        WaitForAnimation("Boss_Throw");
    }

    //Called at the end of the Boss_Throw animation
    public void ThrowShuriken()
    {
        GameObject shuriken = Instantiate(ShurikenPrefab, ShurikenGizmod.position, Quaternion.identity);
        ShurikenController shurikenController = shuriken.GetComponent<ShurikenController>();
        Vector2 shootDirection = isTurnedLeft ? Vector2.left : Vector2.right;
        shurikenController.ShootDirection = shootDirection;
        shurikenController.Damage = ShurikenDamage;
        shurikenController.Shoot();
    }

    private void WaitForAnimation(string animationClipName)
    {
        animator.Play(animationClipName);
        AnimatorClipInfo[] animationInfo = animator.GetCurrentAnimatorClipInfo(0);

        actionTimer= animationInfo[0].clip.length;                
    }

    public void ReceiveDamage(int damage)
    {
        if (!IsInvincible)
        {
            CurrentLifePoints -= damage;
        }
    }
}
