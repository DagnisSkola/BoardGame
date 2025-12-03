using UnityEngine;

public class CursorSci : MonoBehaviour
{
    public Texture2D[] cursors;

    void Start()
    {
        DefaultCursor();
    }

    public void DefaultCursor()
    {
        Cursor.SetCursor(cursors[0], Vector2.zero, CursorMode.Auto);
    }

    public void OnButtonCursor()
    {
        Cursor.SetCursor(cursors[1], Vector2.zero, CursorMode.Auto);
    }

    public void ButtonClickedCursor()
    {
        Cursor.SetCursor(cursors[2], Vector2.zero, CursorMode.Auto);
    }

    public void OnPropCursror()
    {
        Cursor.SetCursor(cursors[3], Vector2.zero, CursorMode.Auto);
    }

    public void AttentionCursor()
    {
        Cursor.SetCursor(cursors[4], Vector2.zero, CursorMode.Auto);
    }
}
