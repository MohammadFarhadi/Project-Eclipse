using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RandomAnimatorStart : MonoBehaviour
{
    void Start()
    {
        Animator animator = GetComponent<Animator>();

        // Start animation at random normalized time
        animator.Play(0, 0, Random.Range(0f, 2f));
    }
}