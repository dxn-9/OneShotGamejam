using UnityEngine;

public class CameraAspectRatioFix : MonoBehaviour
{
    public float referenceAspect = 16f / 9f; // Your reference aspect ratio
    public float referenceOrthographicSize = 5f; // Your reference orthographic size

    void Start()
    {
        Camera cam = GetComponent<Camera>();
        if (cam.orthographic)
        {
            float currentAspect = (float)Screen.width / Screen.height;
            cam.orthographicSize = referenceOrthographicSize * (referenceAspect / currentAspect);
        }
    }

    void Update()
    {
        transform.localPosition = Vector3.zero;
        // if (Mathf.Round(Time.time) % 5 == 0)
        // {
        //     // Debug.Log(transform.position);
        //     // Debug.Log(transform.localPosition);
        // }
    }
}