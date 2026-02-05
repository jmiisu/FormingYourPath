using UnityEngine;

public class FYPCursorManager : MonoBehaviour
{
    [SerializeField] private Texture2D defaultTexture;
    [SerializeField] private Texture2D clickTexture;
    [SerializeField] private Texture2D placeTexture;

    void Start()
    {
        Cursor.SetCursor(defaultTexture, new Vector2(0.5f, 0.5f), CursorMode.Auto);
    }

    public void OnMouseOver()
    {
        Cursor.SetCursor(placeTexture, new Vector2(0.5f, 0.5f), CursorMode.Auto);
    }

    public void OnMouseExit()
    {
        Cursor.SetCursor(defaultTexture, new Vector2(0.5f, 0.5f), CursorMode.Auto);
    }
}
