
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    [SerializeField] private GameManager gm;
    private Vector2 touchStart;
    private bool touching;

    // 任意：スワイプ閾値（調整しやすいよう定数化）
    [SerializeField] private float swipeThreshold = 30f;

    private void Update()
    {
        // キーボード
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) gm.MoveLeft();
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) gm.MoveRight();
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) gm.MoveUp();
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) gm.MoveDown();

        // モバイルスワイプ
        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                touching = true;
                touchStart = t.position;
            }
            else if (t.phase == TouchPhase.Ended && touching)
            {
                touching = false;
                var delta = (Vector2)t.position - touchStart;
                HandleSwipe(delta);
            }
        }
        else
        {
            // マウスドラッグ対応（任意）
            if (Input.GetMouseButtonDown(0))
            {
                touching = true;
                touchStart = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0) && touching)
            {
                touching = false;
                var delta = (Vector2)Input.mousePosition - touchStart;
                HandleSwipe(delta);
            }
        }
    }

    private void HandleSwipe(Vector2 delta)
    {
        if (delta.magnitude <= swipeThreshold) return;

        // 横方向の方が強い場合
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            if (delta.x > 0) gm.MoveRight();
            else gm.MoveLeft();
        }
        else // 縦方向の方が強い場合
        {
            if (delta.y > 0) gm.MoveUp();
            else gm.MoveDown();
        }
    }
}
