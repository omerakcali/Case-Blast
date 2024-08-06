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
    
    [SerializeField] private SpriteRenderer BorderSprite;
    [SerializeField] private Vector2 BorderPadding;
    [SerializeField] private BoardPoolManager BoardPoolManager;
    [SerializeField, Range(0.1f, 20f)] float DropSpeed = 8f;
    [SerializeField, Range(0, 10)] int NewDropOffset = 2;

    public bool IsPlaying => !_levelFinished && !_grid.IsUndefined;
    public bool CanPlay => _busyDuration <= 0 && _boardStateLockerCount ==0 && !NeedsFilling;

    private Grid2D<BoardElement> _grid;
    public BoardElement this[int x, int y] => _grid[x, y];
    public BoardElement this[int2 c] => _grid[c];

    private List<int2> _alertedBoardElements = new();
    private float2 _tileOffset;
    private Sequence _mergeSequence;
    private bool _levelFinished;
    private float _busyDuration;
    private int _boardStateLockerCount = 0;
    public const int TNTCellCount =5;

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
                    if(_grid[x,y] == null) continue;
                    _grid[x, y].Despawn();
                }
            }
        }

        FillGrid();
        BorderSprite.size = new Vector2(Size.x, Size.y) +BorderPadding;

    }

    private void FillGrid()
    {
        for (int y = 0; y < Size.y; y++)
        {
            for (int x = 0; x < Size.x; x++)
            {
                bool isRandom = CurrentLevel.Grid[y * Size.x + x] == BoardElementType.RandomDrop;
                
                var type = isRandom
                    ? CurrentLevel.TypesToSpawn[Random.Range(0, CurrentLevel.TypesToSpawn.Count)]
                    : CurrentLevel.Grid[y * Size.x + x];
                _grid[x, y] = SpawnBoardElement(type, x, y,this);
            }
        }

        LevelLoadEvent?.Invoke(CurrentLevel);
    }

    BoardElement SpawnBoardElement(BoardElementType t, int x, int y,Board board) {
        var element =BoardPoolManager.SpawnBoardElement(t, x + _tileOffset.x, y + _tileOffset.y);
        element.SetBoard(this);
        element.PositionOnBoard = new int2(x, y);
        return element;
    }

    public void LockBoardState()
    {
        _boardStateLockerCount++;
    }

    public void UnlockBoardState()
    {
        _boardStateLockerCount--;
    }
    public bool TryMove(int2 coordinates)
    {
        return FindMatches(coordinates);
    }

    List<int2> _openList = new();
    List<int2> _closedList = new();
    private bool FindMatches(int2 originCoordinates) 
    {
        if (_grid[originCoordinates] == null) return false;
        MatchedCoordinates.Clear();
        _alertedBoardElements.Clear();
        _openList.Clear();
        _closedList.Clear();
        if (_grid[originCoordinates].ElementType.IsSpecial())
        {
            MatchedCoordinates.Add(originCoordinates);
            return true;
        }

        if (!_grid[originCoordinates].ElementType.IsDrop()) return false;

        _openList.Add(originCoordinates);
        MatchedCoordinates.Add(originCoordinates);
        var selectedDropType = _grid[originCoordinates].ElementType;
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
                    && _grid[horizontalNeighbor] !=null
                    && !_closedList.Contains(horizontalNeighbor)
                    && !_openList.Contains(horizontalNeighbor)
                    && _grid[horizontalNeighbor].ElementType == selectedDropType)
                {
                    _openList.Add(horizontalNeighbor);
                    MatchedCoordinates.Add(horizontalNeighbor);
                }

                var verticalNeighbor = tile + new int2(0, i);
                if (_grid.AreValidCoordinates(verticalNeighbor)
                    && _grid[verticalNeighbor] != null
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

    private void ProcessMatches()
    {
        bool isMerge = MatchedCoordinates.Count >= TNTCellCount;
        Merge = isMerge
            ? new TileMerge(MatchedCoordinates[0],
                BoardElementType.TNT)
            : null;

        for (int m = 0; m < MatchedCoordinates.Count; m++)
        {
            var c = MatchedCoordinates[m];
            if (Merge.HasValue)
            {
                Merge.Value.Origins.Add(c);
            }
            else
            {
                _grid[c].Pop();
            }

            AlertMatchesToNeighbors(c);
        }

        if (Merge.HasValue) // handle merges into TNT
        {
            ProcessMerge();
        }

        MatchedCoordinates.Clear();
    }

    private void ProcessMerge()
    {
        var merge = Merge.Value;
        var targetPosition = _grid[merge.Destination].transform.position;
        _mergeSequence = DOTween.Sequence();
        for (int i = 0; i < merge.Origins.Count; i++)
        {
            var c = merge.Origins[i];
            _mergeSequence.Join(_grid[c].Merge(targetPosition));
        }

        _mergeSequence.AppendCallback(() =>
        {
            _grid[merge.Destination] =
                SpawnBoardElement(merge.CreatedType, merge.Destination.x, merge.Destination.y,this);
        });
        _mergeSequence.AppendInterval(.1f); //padding
        _busyDuration = Mathf.Max(
            _mergeSequence.Duration(), _busyDuration
        );
    }

    private void DropTiles()
    {
        DroppedTiles.Clear();

        for (int x = 0; x < Size.x; x++)
        {
            int holeCount = 0;
            int indexOnColumn = 0;
            for (int y = 0; y < Size.y; y++)
            {
                if (_grid[x, y] == null)
                {
                    holeCount += 1;
                }
                else if (holeCount > 0)
                {
                    if (!_grid[x, y].DoesFall)
                    {
                        holeCount = 0;
                    }
                    else DroppedTiles.Add(new TileDrop(x, y - holeCount, holeCount,indexOnColumn++));
                }
            }

            for (int h = holeCount; h >= 1; h--)
            {
                DroppedTiles.Add(new TileDrop(x, Size.y - h, holeCount,indexOnColumn++));
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
                _grid[tile.PositionOnBoard] = null;
            }
            else
            {
                var typeToSpawn = CurrentLevel.TypesToSpawn[Random.Range(0, CurrentLevel.TypesToSpawn.Count)];
                tile = SpawnBoardElement(
                    typeToSpawn, drop.coordinates.x, drop.fromY + NewDropOffset,this
                );
            }
            
            _grid[drop.coordinates] = tile;
            tile.PositionOnBoard = drop.coordinates;
            _busyDuration = Mathf.Max(
                tile.Fall(drop.coordinates.y + _tileOffset.y, DropSpeed,drop.index), _busyDuration
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
    
    private void AlertMatchesToNeighbors(int2 tile)
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

    public void ElementDespawned(int2 positionOnBoard)
    {
        _grid[positionOnBoard] = null;
        NeedsFilling = true;
    }
}


[System.Serializable]
public struct TileDrop
{
    public int2 coordinates;
    public int fromY;
    public int index;

    public TileDrop(int x, int y, int distance, int index)
    {
        coordinates.x = x;
        coordinates.y = y;
        fromY = y + distance;
        this.index = index;
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