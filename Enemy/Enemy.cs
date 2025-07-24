using System.Collections;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

[System.Serializable]
public class BasicComponents
{
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Rigidbody2D rb;

    public SpriteRenderer SpriteRenderer => spriteRenderer;
    public Animator Animator => animator;
    public Rigidbody2D Rigidbody => rb;

    public void Init(GameObject go)
    {
        spriteRenderer = go.GetComponentInChildren<SpriteRenderer>();
        animator = go.GetComponentInChildren<Animator>();
        rb = go.GetComponent<Rigidbody2D>();
    }
}

[System.Serializable]
public class BasicInfo
{
    [SerializeField]
    private float speed = 2f;
    private bool isMoving = true;
    [SerializeField]
    private float waitingTime = 2f;
    [SerializeField]
    private float speedForTracing = 4f;
    private bool isTracing = true;
    private Vector2 moveDirection;

    public float Speed => speed;
    public bool IsMoving
    {
        get => isMoving;
        set => isMoving = value;
    }
    public float WaitingTime => waitingTime;
    public float SpeedForTracing => speedForTracing;
    public bool IsTracing
    {
        get => isTracing;
        set => isTracing = value;
    }
    public Vector2 MoveDirection
    {
        get => moveDirection;
        set => moveDirection = value;
    }
}

[System.Serializable]
public class Detection
{
    [SerializeField]
    private Transform pointA;
    [SerializeField]
    private Transform pointB;
    private Transform target;
    [SerializeField]
    private float detectRadius = 3f;
    [SerializeField]
    private LayerMask playerLayer;
    private Transform player;
    [SerializeField]
    private Transform detectionCenter;
    [SerializeField]
    private Transform attackCenter;
    [SerializeField]
    private float attackRadius = 2.5f;
    private bool isAttacking = false;
    private bool isTeleporting = false;

    public Transform PointA => pointA;
    public Transform PointB => pointB;
    public Transform Target
    {
        get => target;
        set => target = value;
    }
    public float DetectRadius => detectRadius;
    public LayerMask PlayerLayer => playerLayer;
    public Transform Player
    {
        get => player;
        set => player = value;
    }
    public Transform DetectionCenter => detectionCenter;
    public Transform AttackCenter => attackCenter;

    public float AttackRadius => attackRadius;
    public bool IsAttacking
    {
        get => isAttacking;
        set => isAttacking = value;
    }

    public bool IsTeleporting
    {
        get => isTeleporting;
        set => isTeleporting = value;
    }

}

public class Enemy : MonoBehaviour
{
    [Header("BasicComponents")]
    [SerializeField]
    private BasicComponents basicComponents = new BasicComponents();

    [Header("BasicInfo")]
    [SerializeField]
    private BasicInfo basicInfo = new BasicInfo();

    [Header("Detection")]
    [SerializeField]
    private Detection detection = new Detection();


    void Start()
    {
        basicComponents.Init(gameObject);

        if (detection == null)
            return;
        detection.Target = detection.PointB;

        basicComponents.Animator.SetBool("isMoving", basicInfo.IsMoving);
        detection.Player = GameObject.FindWithTag("Player")?.transform;
    }
    void Update()
    {
        bool inRange = CheckPlayerInRange(transform.position, detection.DetectRadius, detection.PlayerLayer);
        bool inAttackRange = CheckPlayerInRange(transform.position, detection.AttackRadius, detection.PlayerLayer);
        if (detection.Player != null && inRange)
        {
            MoveTowardsPlayer();
            if (inAttackRange)
            {
                Attack();
            }
        }
        else if (basicInfo.IsMoving && detection.Target != null)
        {
            Vector2 targetPos = new Vector2(detection.Target.position.x, transform.position.y);
            Vector2 direction = (targetPos - (Vector2)transform.position).normalized;
            basicInfo.MoveDirection = direction;
            basicComponents.SpriteRenderer.flipX = targetPos.x > transform.position.x;

            if (Vector2.Distance(transform.position, targetPos) < 0.1f)
            {
                basicInfo.MoveDirection = Vector2.zero;
                StartCoroutine(Wait());
                detection.Target = (detection.Target == detection.PointA) ? detection.PointB : detection.PointA;
            }
        }
        else
        {
            basicInfo.MoveDirection = Vector2.zero;
        }

        if(basicInfo.IsMoving && !inRange)
        {
            basicInfo.IsTracing = false;
        }
    }

    void FixedUpdate()
    {
        if (basicInfo.IsMoving)
        {
            if (basicInfo.IsTracing)
                basicComponents.Rigidbody.MovePosition(basicComponents.Rigidbody.position + basicInfo.MoveDirection * basicInfo.SpeedForTracing * Time.fixedDeltaTime);
            else
                basicComponents.Rigidbody.MovePosition(basicComponents.Rigidbody.position + basicInfo.MoveDirection * basicInfo.Speed * Time.fixedDeltaTime);
        }
    }


    private IEnumerator Wait()
    {
        basicInfo.IsMoving = false;
        basicComponents.Animator.SetBool("isMoving", basicInfo.IsMoving);
        yield return new WaitForSeconds(basicInfo.WaitingTime);

        basicInfo.IsMoving = true;
        basicComponents.Animator.SetBool("isMoving", basicInfo.IsMoving);
    }
    
    bool CheckPlayerInRange(Vector2 origin, float radius, LayerMask layer)
    {
        return Physics2D.OverlapCircle(origin, radius, layer) != null;
    }

    void MoveTowardsPlayer()
    {
        Vector2 direction = new Vector2(detection.Player.position.x - transform.position.x, 0f).normalized;
        basicInfo.MoveDirection = direction;
        basicInfo.IsTracing = true;
        basicInfo.IsMoving = true;
        basicComponents.Animator.SetBool("isMoving", basicInfo.IsMoving);
        basicComponents.SpriteRenderer.flipX = direction.x > 0;
    }

    void Attack()
    {
        detection.IsAttacking = true;
        basicComponents.Animator.SetBool("isAttacking", detection.IsAttacking);
        basicInfo.IsTracing = false;
        basicInfo.IsMoving = false;
        
        // 추가된코드
        Vector2 attackPoint = new Vector2(detection.Player.position.x, transform.position.y);
        Vector2 returnPoint = transform.position;
        if (!detection.IsTeleporting)
            StartCoroutine(TeleportAndShake(attackPoint, returnPoint));
        //
    }

    private IEnumerator TeleportAndShake(Vector2 targetPos, Vector2 returnPos)
    {
        detection.IsTeleporting = true;
        yield return new WaitForSeconds(0.5f);
        transform.position = targetPos;
        StartCoroutine(CameraShake(0.3f, 0.1f));
        yield return new WaitForSeconds(1f);      

        transform.position = returnPos;
        detection.IsAttacking = false;
        basicComponents.Animator.SetBool("isAttacking", detection.IsAttacking);
        basicInfo.IsMoving = true;
        basicComponents.Animator.SetBool("isMoving", basicInfo.IsMoving);
         
        yield return new WaitForSeconds(2.3f);
        detection.IsTeleporting = false;
    }

    private IEnumerator CameraShake(float duration, float magnitude)
    {
        Camera cam = Camera.main;
        if (cam == null) yield break;

        Vector3 OriginPos = cam.transform.position;
        float elapsed = 0f;

        while(elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            cam.transform.position = new Vector3(OriginPos.x + x, OriginPos.y + y, OriginPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.transform.position = OriginPos;
    }

    private void OnDrawGizmosSelected()
    {
        if (detection.DetectionCenter != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(detection.DetectionCenter.position, detection.DetectRadius);
        }

        if(detection.AttackCenter != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(detection.AttackCenter.position, detection.AttackRadius);
        }
    }
}