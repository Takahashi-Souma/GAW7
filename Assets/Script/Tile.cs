
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Tile : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI label;

    public int Value { get; private set; }
    public Vector2Int GridPos { get; private set; }

    public void Init(int value, Vector2Int gridPos)
    {
        Value = value;
        GridPos = gridPos;
        UpdateView();
    }

    public void SetGridPos(Vector2Int gridPos)
    {
        GridPos = gridPos;
    }

    public void SetValue(int value)
    {
        Value = value;
        UpdateView();
    }

    private void UpdateView()
    {
        if (label != null) label.text = Value.ToString();
        if (background != null) background.color = GetColor(Value);
        // 例：アニメーション（ポップ）
        StopAllCoroutines();
        StartCoroutine(Pop());
    }

    private Color GetColor(int v)
    {
        // 値に応じて段階的に色を変える（お好みで）
        switch (v)
        {
            case 2: return new Color(0.93f, 0.89f, 0.85f);
            case 4: return new Color(0.93f, 0.88f, 0.78f);
            case 8: return new Color(0.95f, 0.69f, 0.47f);
            case 16: return new Color(0.96f, 0.58f, 0.38f);
            case 32: return new Color(0.96f, 0.48f, 0.37f);
            case 64: return new Color(0.96f, 0.37f, 0.23f);
            case 128: return new Color(0.93f, 0.81f, 0.44f);
            case 256: return new Color(0.93f, 0.80f, 0.38f);
            case 512: return new Color(0.93f, 0.78f, 0.32f);
            case 1024: return new Color(0.93f, 0.76f, 0.26f);
            case 2048: return new Color(0.93f, 0.74f, 0.20f);
            default: return new Color(0.20f, 0.20f, 0.20f);
        }
    }

    private System.Collections.IEnumerator Pop()
    {
        var rt = transform as RectTransform;
        float t = 0f;
        Vector3 start = Vector3.one * 0.9f;
        Vector3 end = Vector3.one;
        while (t < 0.12f)
        {
            t += Time.deltaTime;
            rt.localScale = Vector3.Lerp(start, end, t / 0.12f);
            yield return null;
        }
        rt.localScale = end;
    }
}
