using UnityEngine;

public class TargetIndicatorController : MonoBehaviour
{
    [SerializeField] private MonoBehaviour targetProviderBehaviour;
    private IGridTargetProvider targetProvider;
    private GameObject mouseIndicator;

    private void Awake()
    {
        targetProvider = targetProviderBehaviour as IGridTargetProvider;
        mouseIndicator = gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (targetProvider == null) return;
        if (!targetProvider.TryGetTarget(out GridTargetContext info)) return;

        mouseIndicator.transform.position = info.worldPos;
    }
}
