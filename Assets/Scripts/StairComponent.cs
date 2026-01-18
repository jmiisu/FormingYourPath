using UnityEngine;

public enum STAIR_DIR { LEFT, RIGHT };

public class StairComponent : MonoBehaviour
{
    [SerializeField] private STAIR_DIR dir = STAIR_DIR.RIGHT;
    [SerializeField] private SpriteRenderer targetRenderer;

    public STAIR_DIR Dir => dir;

    private void Awake()
    {
        ApplyVisual();
    }

    public void SetDir(STAIR_DIR newDir)
    {
        dir = newDir;
        ApplyVisual();
    }

    private void ApplyVisual()
    {
        if (targetRenderer == null) return;
        targetRenderer.flipX = (dir == STAIR_DIR.LEFT);
    }
}
