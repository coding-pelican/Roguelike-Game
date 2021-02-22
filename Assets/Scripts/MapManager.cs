using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MapManager {
    public static Tile[,] map;
}

[Serializable]
public class Tile {
    public int posX;
    public int posY;
    [NonSerialized]
    public GameObject baseObject;   //  a floot, a wall, Etc...
    public string type;
    public bool hasPlayer = false;
}

[Serializable]
public class Wall {
    public List<Vector2Int> positions;
    public string direction;
    public int length;
    public bool hasFeature = false;
}

[Serializable]
public class Feature {
    public List<Vector2Int> positions;
    public Wall[] walls;
    public string type;
    public int width;
    public int height;
    public bool hasPlayer = false;
}
