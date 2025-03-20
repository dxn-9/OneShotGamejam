using UnityEngine;

public class BuildingControls : MonoBehaviour
{
    [SerializeField] GameObject arrowPrefab;

    void Awake()
    {
        for (int i = -1; i <= 1; i++)
        {
            if (i == 0) continue;
            Quaternion x = Quaternion.Euler(0, 90 + 90 * i, 0);
            Quaternion z = Quaternion.Euler(0, 90 * i, 0);

            Instantiate(arrowPrefab, transform).GetComponent<ArrowIndicator>().SetDirection(x * Vector3.right);
            Instantiate(arrowPrefab, transform).GetComponent<ArrowIndicator>().SetDirection(z * Vector3.right);
        }
    }
}