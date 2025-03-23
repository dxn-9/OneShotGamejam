using UnityEngine;

public class TrainController : MonoBehaviour
{
    [SerializeField] Animator animator;

    public void IsRunning(bool isRunning)
    {
        animator.enabled = isRunning;
    }
}