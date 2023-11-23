﻿using System;
using UnityEngine;

[Serializable]
public struct SpawnTiming
{
    public Enemy enemyPrefab;
    public float health;
    public float delay;
    public uint amount;
}
