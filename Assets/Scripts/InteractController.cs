using UnityEngine;

public class InteractController : MonoBehaviour
{
    [SerializeField] private Camera sceneCamera;
    [SerializeField] private LayerMask placementLayerMask;
    [SerializeField] private float zDepth = 0f;

    private Vector3 lastPosition;

    private void Awake()
    {
        var w = sceneCamera.ScreenToWorldPoint(Input.mousePosition);
        w.z = zDepth;
        lastPosition = w;
    }

    public Vector3 GetSelectedMapPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = sceneCamera.nearClipPlane;
        Ray ray = sceneCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100, placementLayerMask))
        {
            lastPosition = hit.point;
        }
        return lastPosition;
    }
}
