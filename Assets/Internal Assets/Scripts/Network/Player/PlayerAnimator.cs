using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Player player;
    private Animator animator;

    private void Start()
    {
        player = GetComponent<Player>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!player.hasAuthority)
            return;

        animator.SetFloat("Horizontal", Input.GetAxis("Horizontal"));
        animator.SetFloat("Vertical", Input.GetAxis("Vertical"));
    }
}
