using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(menuName= "ScriptableObjects/"+nameof(LevelLibrary))]
public class LevelLibrary : ScriptableObject
{
    public LevelInfo Level;
}


[Serializable]
public class LevelInfo
{
    public int2 GridSize;
    public int MoveCount;
    public bool PopulateRandomly;
    public List<BoardElementType> Grid;
    public List<BoardElementType> TypesToSpawn;
    public List<LevelGoalInfo> Goals;
}

[Serializable]
public class LevelGoalInfo
{
    public BoardElementType GoalType;
    public int Amount;
}