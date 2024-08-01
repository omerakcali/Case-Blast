using System;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[DefaultExecutionOrder(-100000)]
public class Board : MonoBehaviour
{
    public List<int2> MatchedCoordinates { get; private set; } = new();
    public bool HasMatches => MatchedCoordinates.Count > 0;
    public List<TileDrop> DroppedTiles { get; private set; }
    public TileMerge? Merge { get; private set; }
    public bool NeedsFilling { get; private set; }
    public LevelInfo CurrentLevel { get; private set; }
    public int2 Size => CurrentLevel.GridSize;

    public static event Action<LevelInfo> LevelLoadEvent;

    [SerializeField] private BoardPoolManager BoardPoolManager;
    [SerializeField, Range(0.1f, 20f)] float DropSpeed = 8f;
    [SerializeField, Range(0f, 10f)] float NewDropOffset = 2f;

    public bool IsPlaying => !_levelFinished && !_grid.IsUndefined;
    private float _busyDuration;
    private float _animationDuration;
    public bool CanPlay => _busyDuration <= 0 && _animationDuration <= 0;

    private Grid2D<BoardElement> _grid;

    public BoardElement this[int x, int y] => _grid[x, y];
    public BoardElement this[int2 c] => _grid[c];

    private List<int2> _alertedBoardElements = new();

    private float2 _tileOffset;
    private Sequence _mergeSequence;
    private bool _levelFinished;

    public void StartNewGame(LevelInfo level)
    {
        CurrentLevel = level;

        _levelFinished = false;
        _busyDuration = 0f;
        _tileOffset = -0.5f * (float2)(Size - 1);
        if (_grid.IsUndefined)
        {
            _grid = new(Size);
            MatchedCoordinates = new();
            DroppedTiles = new();
        }
        else
        {
            for (int y = 0; y < _grid.SizeY; y++)
            {
                for (int x = 0; x < _grid.SizeX; x++)
                {
                    _grid[x, y].Despawn();
                    _grid[x, y] = null;
                }
            }
        }

        FillGrid();
    }

    private void FillGrid()
    {
        for (int y = 0; y < Size.y; y++)
        {
            for (int x = 0; x < Size.x; x++)
            {
                var type = CurrentLevel.PopulateRandomly
                    ? CurrentLevel.TypesToSpawn[Random.Range(0, CurrentLevel.TypesToSpawn.Count)]
                    : CurrentLevel.Grid[y * Size.x + x];
                _grid[x, y] = SpawnBoardElement(type, x, y);
            }
        }

        LevelLoadEvent?.Invoke(CurrentLevel);
    }

    BoardElement SpawnBoardElement(BoardElementType t, float x, float y) =>
        BoardPoolManager.SpawnBoardElement(t, x + _tileOffset.x, y + _tileOffset.y);


    public bool TryMove(int2 coordinates)
    {
        return FindMatches(coordinates);
    }

    List<int2> _openList = new();
    List<int2> _closedList = new();
    private bool FindMatches(int2 selectedCoordinates) 
    {
        MatchedCoordinates.Clear();
        _alertedBoardElements.Clear();
        _openList.Clear();
        _closedList.Clear();
        if (_grid[selectedCoordinates].ElementType.IsSpecial())
        {
            MatchedCoordinates.Add(selectedCoordinates);
            return true;
        }

        if (!_grid[selectedCoordinates].ElementType.IsDrop()) return false;

        _openList.Add(selectedCoordinates);
        MatchedCoordinates.Add(selectedCoordinates);
        var selectedDropType = _grid[selectedCoordinates].ElementType;
        while (_openList.Count > 0)
        {
            var tile = _openList[^1];
            _openList.RemoveAt(_openList.Count - 1);
            if (_closedList.Contains(tile)) continue;
            _closedList.Add(tile);

            for (int i = -1; i <= +1; i++)
            {
                if (i == 0) continue;

                var horizontalNeighbor = tile + new int2(i, 0);
                if (_grid.AreValidCoordinates(horizontalNeighbor)
                    && !_closedList.Contains(horizontalNeighbor)
                    && !_openList.Contains(horizontalNeighbor)
                    && _grid[horizontalNeighbor].ElementType == selectedDropType)
                {
                    _openList.Add(horizontalNeighbor);
                    MatchedCoordinates.Add(horizontalNeighbor);
                }

                var verticalNeighbor = tile + new int2(0, i);
                if (_grid.AreValidCoordinates(verticalNeighbor)
                    && !_closedList.Contains(verticalNeighbor)
                    && !_openList.Contains(verticalNeighbor)
                    && _grid[verticalNeighbor].ElementType == selectedDropType)
                {
                    _openList.Add(verticalNeighbor);
                    MatchedCoordinates.Add(verticalNeighbor);
                }
            }
        }

        if (MatchedCoordinates.Count == 1) MatchedCoordinates.Clear();
        return MatchedCoordinates.Count > 0;
    }

    public void DoWork()
    {
        if (_animationDuration > 0f)
        {
            _animationDuration -= Time.deltaTime;
        }

        if (_busyDuration > 0f)
        {
            _busyDuration -= Time.deltaTime;
            if (_busyDuration > 0f)
            {
                return;
            }
        }

        if (HasMatches)
        {
            ProcessMatches();
        }
        else if (NeedsFilling)
        {
            DropTiles();
        }
    }

    void ProcessMatches()
    {
        bool isMerge = MatchedCoordinates.Count >= 115000;
        Merge = isMerge
            ? new TileMerge(MatchedCoordinates[0],
                BoardElementType.TNT)
            : null;

        for (int m = 0; m < MatchedCoordinates.Count; m++)
        {
            var c = MatchedCoordinates[m];
            _grid[c].Pop();
            _grid[c] = null;
            if (isMerge)
            {
                Merge.Value.Origins.Add(c);
            }

            AlertMatchNeighbors(c);
        }

        NeedsFilling = true;
        if (Merge.HasValue) // handle merges into TNT
        {
            ProcessMerge();
        }

        MatchedCoordinates.Clear();
    }

    void ProcessMerge()
    {
        var merge = Merge.Value;
        var targetPosition = _grid[merge.Destination].transform.position;
        _mergeSequence = DOTween.Sequence();
        for (int i = 0; i < merge.Origins.Count; i++)
        {
            var c = merge.Origins[i];
            _mergeSequence.Join(_grid[c].Merge(targetPosition));
            _grid[c] = null;
        }

        _mergeSequence.AppendCallback(() =>
        {
            _grid[merge.Destination] =
                SpawnBoardElement(merge.CreatedType, merge.Destination.x, merge.Destination.y);
        });
        _mergeSequence.AppendInterval(.1f); //padding
        //_mergeSequence.AppendCallback(DropTiles);
        _busyDuration = Mathf.Max(
            _mergeSequence.Duration(), _busyDuration
        );
    }

    void DropTiles()
    {
        DroppedTiles.Clear();

        for (int x = 0; x < Size.x; x++)
        {
            int holeCount = 0;
            for (int y = 0; y < Size.y; y++)
            {
                if (_grid[x, y] == null)
                {
                    holeCount += 1;
                }
                else if (holeCount > 0)
                {
                    DroppedTiles.Add(new TileDrop(x, y - holeCount, holeCount));
                }
            }

            for (int h = 1; h <= holeCount; h++)
            {
                DroppedTiles.Add(new TileDrop(x, Size.y - h, holeCount));
            }
        }

        NeedsFilling = false;

        for (int i = 0; i < DroppedTiles.Count; i++)
        {
            TileDrop drop = DroppedTiles[i];
            BoardElement tile;
            if (drop.fromY < _grid.SizeY)
            {
                tile = _grid[drop.coordinates.x, drop.fromY];
            }
            else
            {
                var typeToSpawn = CurrentLevel.TypesToSpawn[Random.Range(0, CurrentLevel.TypesToSpawn.Count)];
                tile = SpawnBoardElement(
                    typeToSpawn, drop.coordinates.x, drop.fromY + NewDropOffset
                );
            }

            _grid[drop.coordinates] = tile;
            tile.SetSortingOrder(drop.coordinates.y);
            _busyDuration = Mathf.Max(
                tile.Fall(drop.coordinates.y + _tileOffset.y, DropSpeed), _busyDuration
            );
        }
    }

    public int2? ScreenToTileSpace(Vector3 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        Vector3 p = ray.origin - ray.direction * (ray.origin.z / ray.direction.z);
        var coordinates = new int2(Mathf.FloorToInt(p.x - _tileOffset.x + 0.5f),
            Mathf.FloorToInt(p.y - _tileOffset.y + 0.5f));
        return _grid.AreValidCoordinates(coordinates) ? coordinates : null;
    }

    public void FinishLevel()
    {
        _levelFinished = true;
    }


    private void AlertMatchNeighbors(int2 tile)
    {
        for (int i = -1; i <= +1; i++)
        {
            if (i == 0) continue;
            var horizontalNeighbor = tile + new int2(i, 0);
            if (_grid.AreValidCoordinates(horizontalNeighbor)
                && !MatchedCoordinates.Contains(horizontalNeighbor)
                && _grid[horizontalNeighbor] != null
                && !_alertedBoardElements.Contains(horizontalNeighbor))
            {
                _alertedBoardElements.Add(horizontalNeighbor);
                _grid[horizontalNeighbor].AlertMatchOnNeighborCell();
            }

            var verticalNeighbor = tile + new int2(0, i);
            if (_grid.AreValidCoordinates(verticalNeighbor)
                && !MatchedCoordinates.Contains(verticalNeighbor)
                && _grid[verticalNeighbor] != null
                && !_alertedBoardElements.Contains(verticalNeighbor))
            {
                _alertedBoardElements.Add(verticalNeighbor);
                _grid[verticalNeighbor].AlertMatchOnNeighborCell();
            }
        }
    }

    public bool AreValidCoordinates(int2 c)
    {
        return _grid.AreValidCoordinates(c);
    }
}


[System.Serializable]
public struct TileDrop
{
    public int2 coordinates;

    public int fromY;

    public TileDrop(int x, int y, int distance)
    {
        coordinates.x = x;
        coordinates.y = y;
        fromY = y + distance;
    }
}

[Serializable]
public struct TileMerge
{
    public List<int2> Origins;

    public int2 Destination;
    public BoardElementType CreatedType;

    public TileMerge(int2 destination, BoardElementType type)
    {
        Destination = destination;
        CreatedType = type;
        Origins = new();
    }
}