using UnityEngine;

public class AnimationEventHnadler : MonoBehaviour
{
    PlayerMovement2D playerMovement2D;
    Animator animator;

    void Awake()
    {
        playerMovement2D = GetComponentInParent<PlayerMovement2D>();
        animator = GetComponentInParent<Animator>();
    }

    private void OnAttackEnd()
    {
        Debug.Log("OnAttackEnd Running");
        playerMovement2D.isAttacking = false;
        Debug.Log("OnAttackEnd Running");
    }
}
