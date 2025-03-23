using Extensions;
using UnityEngine;

public class ArrowIndicator : MonoBehaviour
{
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] Shader materialShader;
    [SerializeField] MeshCollider meshCollider;

    Material material;
    Vector3 direction;


    public void SetDirection(Vector3 dir)
    {
        transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        transform.position += dir;
        direction = dir.normalized;
    }

    void Start()
    {
        string directionStr = direction.x >= 1.0f ? "Right" :
            direction.x <= -1.0f ? "Left" :
            direction.z >= 1.0f ? "Up" : "Down";
        meshRenderer.material = new Material(materialShader);
        material = meshRenderer.material;
        transform.name = "ArrowIndicator" + directionStr;
    }

    void Update()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var activeNode = Game.I.ActiveNode;

        if (Game.I.CanPlaceNode((activeNode.position + direction * activeNode.Range).SnapToGrid()))
        {
            material.SetFloat("_Alpha", 0.5f);
            if (Physics.Raycast(ray, out var hit, 1000f, LayerMask.GetMask("Overlay")) &&
                hit.transform.parent == transform)
            {
                material.SetFloat("_Alpha", 0.9f);
                if (Input.GetMouseButtonDown(0))
                {
                    Game.I.PlaceBlock(direction * activeNode.Range);
                }
            }
        }
        else
        {
            material.SetFloat("_Alpha", 0.0f);
        }
    }
}