
using UnityEngine;
using System;
using System.Collections.Generic;

public enum MoveDir { Up, Down, Left, Right }

public class GameManager : MonoBehaviour
{
    [SerializeField] private BoardManager board;
    [SerializeField] private int size = 4;
    [SerializeField] private int startTiles = 2;
    [SerializeField] private float probFour = 0.10f; // 4の生成確率

    public int Score { get; private set; }

    private int[,] grid;
    private System.Random rng = new System.Random();

    // Undo用（直前）
    private int[,] prevGrid;
    private int prevScore;

    private void Start()
    {
        grid = new int[size, size];
        NewGame();
    }

    public void NewGame()
    {
        Score = 0;
        Array.Clear(grid, 0, grid.Length);
        board.ClearAll();

        for (int i = 0; i < startTiles; i++)
            SpawnRandomTile();

        UpdateBoardViewFull();
    }

    public void HandleMove(MoveDir dir)
    {
        // スナップショット（Undo用）
        prevGrid = (int[,])grid.Clone();
        prevScore = Score;

        bool movedOrMerged = SlideAndMerge(dir);

        if (movedOrMerged)
        {
            SpawnRandomTile();
            UpdateBoardViewFull();
            if (IsGameOver())
            {
                Debug.Log("Game Over");
                // 必要ならUI表示
            }
        }
        else
        {
            // 何も起きない場合はUndo不要ならスナップショット破棄でもOK
        }
    }

    public void Undo()
    {
        if (prevGrid == null) return;
        grid = (int[,])prevGrid.Clone();
        Score = prevScore;
        UpdateBoardViewFull();
    }

    private bool SlideAndMerge(MoveDir dir)
    {
        bool changed = false;

        // 方向ごとの走査順を決定（Leftなら左→右に詰める、Rightなら右→左に）
        int startX = (dir == MoveDir.Right) ? size - 1 : 0;
        int startY = (dir == MoveDir.Down) ? size - 1 : 0;
        int stepX = (dir == MoveDir.Right) ? -1 : 1;
        int stepY = (dir == MoveDir.Down) ? -1 : 1;

        // 行単位/列単位で処理を分ける
        if (dir == MoveDir.Left || dir == MoveDir.Right)
        {
            for (int y = 0; y < size; y++)
            {
                // 1) 値リスト抽出
                List<int> line = new List<int>(size);
                for (int x = 0; x < size; x++)
                {
                    int gx = (dir == MoveDir.Left) ? x : (size - 1 - x);
                    if (grid[gx, y] != 0) line.Add(grid[gx, y]);
                }

                // 2) マージ（左詰め）
                List<int> merged = MergeLineLeft(line);

                // 3) 再配置
                for (int x = 0; x < size; x++)
                {
                    int gx = (dir == MoveDir.Left) ? x : (size - 1 - x);
                    int newVal = (x < merged.Count) ? merged[x] : 0;
                    if (grid[gx, y] != newVal) changed = true;
                    grid[gx, y] = newVal;
                }
            }
        }
        else // Up / Down
        {
            for (int x = 0; x < size; x++)
            {
                List<int> line = new List<int>(size);
                for (int y = 0; y < size; y++)
                {
                    int gy = (dir == MoveDir.Up) ? y : (size - 1 - y);
                    if (grid[x, gy] != 0) line.Add(grid[x, gy]);
                }

                List<int> merged = MergeLineLeft(line);

                for (int y = 0; y < size; y++)
                {
                    int gy = (dir == MoveDir.Up) ? y : (size - 1 - y);
                    int newVal = (y < merged.Count) ? merged[y] : 0;
                    if (grid[x, gy] != newVal) changed = true;
                    grid[x, gy] = newVal;
                }
            }
        }

        return changed;
    }

    private List<int> MergeLineLeft(List<int> line)
    {
        // 圧縮済み配列に対して、左から順に1回だけマージ
        List<int> res = new List<int>(line.Count);
        int i = 0;
        while (i < line.Count)
        {
            if (i < line.Count - 1 && line[i] == line[i + 1])
            {
                int merged = line[i] * 2;
                res.Add(merged);
                Score += merged; // スコア加算
                i += 2; // 2つ消費
            }
            else
            {
                res.Add(line[i]);
                i += 1;
            }
        }
        // 左詰め（右側は0補充は呼び側で対応）
        return res;
    }

    private void SpawnRandomTile()
    {
        var empties = new List<Vector2Int>();
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                if (grid[x, y] == 0) empties.Add(new Vector2Int(x, y));
        if (empties.Count == 0) return;

        var pos = empties[rng.Next(empties.Count)];
        int value = (rng.NextDouble() < probFour) ? 4 : 2;
        grid[pos.x, pos.y] = value;

        // UI反映（タイルがなければ生成）
        var existing = board.GetTile(pos);
        if (existing == null)
            board.SpawnTile(value, pos);
        else
            existing.SetValue(value);
    }

    private void UpdateBoardViewFull()
    {
        // 現状のgridにUIを同期
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                var pos = new Vector2Int(x, y);
                int v = grid[x, y];
                var t = board.GetTile(pos);
                if (v == 0 && t != null)
                    board.RemoveTile(pos);
                else if (v != 0 && t == null)
                    board.SpawnTile(v, pos);
                else if (v != 0 && t != null && t.Value != v)
                    t.SetValue(v);
            }
    }

    private bool IsGameOver()
    {
        // 空きマスがあるなら継続
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                if (grid[x, y] == 0) return false;

        // いずれか方向でマージ可能なら継続
        return !(CanMerge(MoveDir.Left) || CanMerge(MoveDir.Right) ||
                 CanMerge(MoveDir.Up) || CanMerge(MoveDir.Down));
    }

    private bool CanMerge(MoveDir dir)
    {
        // 「スライド後に合体が起きる可能性があるか」のざっくりチェック
        if (dir == MoveDir.Left || dir == MoveDir.Right)
        {
            for (int y = 0; y < size; y++)
            {
                int last = -1;
                for (int x = 0; x < size; x++)
                {
                    int gx = (dir == MoveDir.Left) ? x : (size - 1 - x);
                    int v = grid[gx, y];
                    if (v == 0) continue;
                    if (last == v) return true;
                    last = v;
                }
            }
        }
        else
        {
            for (int x = 0; x < size; x++)
            {
                int last = -1;
                for (int y = 0; y < size; y++)
                {
                    int gy = (dir == MoveDir.Up) ? y : (size - 1 - y);
                    int v = grid[x, gy];
                    if (v == 0) continue;
                    if (last == v) return true;
                    last = v;
                }
            }
        }
        return false;
    }

    // 便利API（UIボタンから呼ぶ用）
    public void MoveLeft() => HandleMove(MoveDir.Left);
    public void MoveRight() => HandleMove(MoveDir.Right);
    public void MoveUp() => HandleMove(MoveDir.Up);
    public void MoveDown() => HandleMove(MoveDir.Down);
}
