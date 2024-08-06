using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NaughtyAttributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/" + nameof(LevelLibrary))]
public class LevelLibrary : ScriptableObject
{
    [SerializeField] private List<TextAsset> LevelFiles;
    public List<LevelInfo> Levels;

    [Button()]
    void ReadLevels()
    {
        Levels = new();
        foreach (var file in LevelFiles)
        {
            Levels.Add(CreateLevel(file));
        }
    }

    private LevelInfo CreateLevel(TextAsset levelFile)
    {
        JsonTextReader reader = new JsonTextReader(new StringReader(levelFile.text));

        LevelInfo info = new();
        var jObject = JObject.Load(reader);
        var levelNo = jObject.GetValue("level_number")!.Value<int>();
        var width = jObject.GetValue("grid_width")!.Value<int>();
        var height = jObject.GetValue("grid_height")!.Value<int>();
        var moveCount = jObject.GetValue("move_count")!.Value<int>();
        var grid = jObject.GetValue("grid")!.Value<JArray>();
        info.Id = levelNo;
        info.GridSize.x = width;
        info.GridSize.y = height;
        info.MoveCount = moveCount;

        Dictionary<BoardElementType, LevelGoalInfo> goals = new();
        for (int i = 0; i < grid.Count; i++)
        {
            var s = grid[i].Value<string>();
            BoardElementType type = BoardElementType.RandomDrop;

            switch (s)
            {
                case "r":
                    type = BoardElementType.RedDrop;
                    break;
                case "g":
                    type = BoardElementType.GreenDrop;
                    break;
                case "b":
                    type = BoardElementType.BlueDrop;
                    break;
                case "y":
                    type = BoardElementType.YellowDrop;
                    break;
                case "rand":
                    type = BoardElementType.RandomDrop;
                    break;
                case "t":
                    type = BoardElementType.TNT;
                    break;
                case "bo":
                    type = BoardElementType.Box;
                    break;
                case "s":
                    type = BoardElementType.Stone;
                    break;
                case "v":
                    type = BoardElementType.Vase;
                    break;
            }

            if (type.IsObstacle())
            {
                goals.TryAdd(type, new LevelGoalInfo(type));
                goals[type].Amount++;
            }

            info.Grid.Add(type);
        }

        foreach (var goal in goals.Values)
        {
            info.Goals.Add(goal);
        }

        return info;
    }
}


[Serializable]
public class LevelInfo
{
    public int Id;
    public int2 GridSize;
    public int MoveCount;
    public List<BoardElementType> Grid = new();

    public List<BoardElementType> TypesToSpawn = new()
    {
        BoardElementType.GreenDrop,
        BoardElementType.BlueDrop,
        BoardElementType.RedDrop,
        BoardElementType.YellowDrop
    };

    public List<LevelGoalInfo> Goals = new();
}

[Serializable]
public class LevelGoalInfo
{
    public BoardElementType GoalType;
    public int Amount;

    public LevelGoalInfo(BoardElementType type)
    {
        GoalType = type;
        Amount = 0;
    }
}