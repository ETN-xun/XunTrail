using UnityEngine;

public class CursorSwit:MonoBehaviour
{
public Texture2D defaultCursor;    // 默认光标
public Texture2D pressedCursor;    // 按下时的光标
public Vector2 hotspot = Vector2.zero; // 光标热点偏移量

void Update()
{
    if (Input.GetMouseButton(0)) // 0 表示左键
    {
        // 切换到按下状态的光标
        Cursor.SetCursor(pressedCursor, hotspot, CursorMode.Auto);
    }
    else
    {
        // 恢复默认光标
        Cursor.SetCursor(defaultCursor, hotspot, CursorMode.Auto);
    }
}
}