using UnityEngine;

public class CameraController : MonoBehaviour
{
    float damping = 2.5f;


    public Transform target;
    Vector3 initialOffset;
    Quaternion initialRotation;
    float referenceAspect = 16f / 9f;
    public float referenceOrthographicSize = 5f;
    Camera cam;

    void Awake()
    {
        initialOffset = transform.position;
        initialRotation = transform.rotation;

        cam = GetComponent<Camera>();
        float currentAspect = (float)Screen.width / Screen.height;
        cam.orthographicSize = (referenceAspect / currentAspect) * referenceOrthographicSize;
    }

    void Update()
    {
        if (target == null)
        {
            return;
        }

        var targetPosition = target.position + initialOffset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, damping * Time.deltaTime);
    }
}