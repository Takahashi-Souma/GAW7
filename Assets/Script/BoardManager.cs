
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [SerializeField] private RectTransform[] slots; // 16個、行優先など任意
    [SerializeField] private Tile tilePrefab;

    // 盤面上のタイル参照（null=空）
    private Tile[,] tiles;

    public int Size { get; private set; } = 4;

    private void Awake()
    {
        tiles = new Tile[Size, Size];
    }

    public bool HasTile(Vector2Int pos) => tiles[pos.x, pos.y] != null;

    public Tile GetTile(Vector2Int pos) => tiles[pos.x, pos.y];

    public Tile SpawnTile(int value, Vector2Int pos)
    {
        var rt = slots[pos.y * Size + pos.x]; // x:列, y:行 で参照（UI並び注意）
        var tile = Instantiate(tilePrefab, rt.parent); // Canvas上に生成
        tile.transform.SetSiblingIndex(rt.GetSiblingIndex() + 1); // スロットの上に
        var tileRT = tile.transform as RectTransform;
        // スロット位置に合わせる
        tileRT.anchorMin = rt.anchorMin;
        tileRT.anchorMax = rt.anchorMax;
        tileRT.pivot = rt.pivot;
        tileRT.anchoredPosition = rt.anchoredPosition;
        tileRT.sizeDelta = rt.sizeDelta;

        tile.Init(value, pos);
        tiles[pos.x, pos.y] = tile;
        return tile;
    }

    public void MoveTile(Tile tile, Vector2Int to)
    {
        // UI移動（アニメ演出は任意）
        var toSlot = slots[to.y * Size + to.x];
        var tr = tile.transform as RectTransform;
        tr.anchorMin = toSlot.anchorMin;
        tr.anchorMax = toSlot.anchorMax;
        tr.pivot = toSlot.pivot;
        tr.anchoredPosition = toSlot.anchoredPosition;
        tr.sizeDelta = toSlot.sizeDelta;

        // 参照更新
        tiles[tile.GridPos.x, tile.GridPos.y] = null;
        tiles[to.x, to.y] = tile;
        tile.SetGridPos(to);
    }

    public void RemoveTile(Vector2Int pos)
    {
        var t = tiles[pos.x, pos.y];
        if (t == null) return;
        tiles[pos.x, pos.y] = null;
        Destroy(t.gameObject);
    }

    public void ClearAll()
    {
        for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
                RemoveTile(new Vector2Int(x, y));
    }
}
        
   