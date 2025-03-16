using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float damping = 0.3f;


    public Transform target;
    Vector3 initialOffset;
    Quaternion initialRotation;

    void Awake()
    {
        initialOffset = transform.position;
        initialRotation = transform.rotation;
    }

    void Update()
    {
        if (target == null)
        {
            return;
        }

        var targetPosition = target.position + initialOffset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, damping);
    }
}