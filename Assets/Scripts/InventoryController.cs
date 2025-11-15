using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [SerializeField] private GameObject mouseIndicator;
    [SerializeField] private InteractController interactController;

    private void Update()
    {
        Vector3 mousePosition = interactController.GetSelectedMapPosition();
        mouseIndicator.transform.position = mousePosition;
        Debug.Log(mousePosition);
    }
}
