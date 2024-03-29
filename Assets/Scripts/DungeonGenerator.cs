using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DungeonGenerator : MonoBehaviour {
    public int mapWidth;
    public int mapHeight;

    public int widthMinRoom;
    public int widthMaxRoom;
    public int heightMinRoom;
    public int heightMaxRoom;

    public int minCorridorLength;
    public int maxCorridorLength;
    public int maxFeatures;
    int countFeatures;

    public int minEnemies;
    public int maxEnemies;

    public bool isASCII;

    public List<Feature> allFeatures;
    List<Feature> rooms;

    public GameObject playerPrefab;

    public Sprite[] walls;
    public Sprite[] floors;

    public float tileScaling = 1f;

    public void InitializeDungeon() {
        MapManager.map = new Tile[mapWidth, mapHeight];
        MapManager.enemies = new List<Enemy>();
    }

    public void GenerateDungeon() {
        GenerateFeature("Room", new Wall(), true);

        for (int i = 0; i < 500; i++) {
            Feature originFeature;

            if (allFeatures.Count == 1) {
                originFeature = allFeatures[0];
            } else {
                originFeature = allFeatures[Random.Range(0, allFeatures.Count - 1)];
            }

            Wall wall = null;

            wall = ChoseWall(originFeature);
            if (wall == null) continue;

            string type;

            if (originFeature.type == "Room") {
                type = "Corridor";
            } else {
                if (Random.Range(0, 100) < 90) {
                    type = "Room";
                } else {
                    type = "Corridor";
                }
            }

            GenerateFeature(type, wall);

            if (countFeatures == maxFeatures) break;
        }

        rooms = GetRooms();
        SpawnPlayer();
        SpawnEnmies();

        DrawMap(isASCII);
    }

    void GenerateFeature(string type, Wall wall, bool isFirst = false) {
        Feature room = new Feature();
        room.positions = new List<Vector2Int>();

        int roomWidth = 0;
        int roomHeight = 0;

        if (type == "Room") {
            roomWidth = Random.Range(widthMinRoom, widthMaxRoom);
            roomHeight = Random.Range(heightMinRoom, heightMaxRoom);
        } else {
            switch (wall.direction) {
                case "South":
                    roomWidth = 3;
                    roomHeight = Random.Range(minCorridorLength, maxCorridorLength);
                    break;
                case "North":
                    roomWidth = 3;
                    roomHeight = Random.Range(minCorridorLength, maxCorridorLength);
                    break;
                case "West":
                    roomWidth = Random.Range(minCorridorLength, maxCorridorLength);
                    roomHeight = 3;
                    break;
                case "East":
                    roomWidth = Random.Range(minCorridorLength, maxCorridorLength);
                    roomHeight = 3;
                    break;
            }
        }

        int xStartingPoint;
        int yStartingPoint;

        if (isFirst) {
            xStartingPoint = mapWidth / 2;
            yStartingPoint = mapHeight / 2;
        } else {
            int id;
            if (wall.positions.Count == 3) id = 1;
            else id = Random.Range(1, wall.positions.Count - 2);
            if (id < 1) id = 1;
            if (id > wall.positions.Count - 2) id = wall.positions.Count - 2;
            xStartingPoint = wall.positions[id].x;
            yStartingPoint = wall.positions[id].y;
        }

        Vector2Int lastWallPosition = new Vector2Int(xStartingPoint, yStartingPoint);

        if (isFirst) {
            xStartingPoint -= Random.Range(0, roomWidth - 1);
            yStartingPoint -= Random.Range(0, roomHeight - 1);
        } else {
            switch (wall.direction) {
                case "South":
                    if (type == "Room") xStartingPoint -= Random.Range(1, roomWidth - 2);
                    else xStartingPoint--;
                    yStartingPoint -= roomHeight;
                    break;
                case "North":
                    if (type == "Room") xStartingPoint -= Random.Range(1, roomWidth - 2);
                    else xStartingPoint--;
                    yStartingPoint++;
                    break;
                case "West":
                    xStartingPoint -= roomWidth;
                    if (type == "Room") yStartingPoint -= Random.Range(1, roomHeight - 2);
                    else yStartingPoint--;
                    break;
                case "East":
                    xStartingPoint++;
                    if (type == "Room") yStartingPoint -= Random.Range(1, roomHeight - 2);
                    else yStartingPoint--;
                    break;
            }
        }

        if (!CheckIfHasSpace(new Vector2Int(xStartingPoint, yStartingPoint), new Vector2Int(xStartingPoint + roomWidth - 1, yStartingPoint + roomHeight - 1))) return;

        room.walls = new Wall[4];

        for (int i = 0; i < room.walls.Length; i++) {
            room.walls[i] = new Wall();
            room.walls[i].positions = new List<Vector2Int>();
            room.walls[i].length = 0;
            room.walls[i].parent = room;

            switch (i) {
                case 0:
                    room.walls[i].direction = "South";
                    break;
                case 1:
                    room.walls[i].direction = "North";
                    break;
                case 2:
                    room.walls[i].direction = "West";
                    break;
                case 3:
                    room.walls[i].direction = "East";
                    break;
            }
        }

        for (int y = 0; y < roomHeight; y++) {
            for (int x = 0; x < roomWidth; x++) {
                Vector2Int position = new Vector2Int();
                position.x = xStartingPoint + x;
                position.y = yStartingPoint + y;

                room.positions.Add(position);

                MapManager.map[position.x, position.y] = new Tile();
                MapManager.map[position.x, position.y].posX = position.x;
                MapManager.map[position.x, position.y].posY = position.y;

                if (y == 0) {
                    room.walls[0].positions.Add(position);
                    room.walls[0].length++;
                    MapManager.map[position.x, position.y].type = "Wall";
                    MapManager.map[position.x, position.y].baseChar = "#";
                    MapManager.map[position.x, position.y].isOpaque = true;
                }
                if (y == (roomHeight - 1)) {
                    room.walls[1].positions.Add(position);
                    room.walls[1].length++;
                    MapManager.map[position.x, position.y].type = "Wall";
                    MapManager.map[position.x, position.y].baseChar = "#";
                    MapManager.map[position.x, position.y].isOpaque = true;
                }
                if (x == 0) {
                    room.walls[2].positions.Add(position);
                    room.walls[2].length++;
                    MapManager.map[position.x, position.y].type = "Wall";
                    MapManager.map[position.x, position.y].baseChar = "#";
                    MapManager.map[position.x, position.y].isOpaque = true;
                }
                if (x == (roomWidth - 1)) {
                    room.walls[3].positions.Add(position);
                    room.walls[3].length++;
                    MapManager.map[position.x, position.y].type = "Wall";
                    MapManager.map[position.x, position.y].baseChar = "#";
                    MapManager.map[position.x, position.y].isOpaque = true;
                }
                if (MapManager.map[position.x, position.y].type != "Wall") {
                    MapManager.map[position.x, position.y].type = "Floor";
                    MapManager.map[position.x, position.y].baseChar = ".";
                    MapManager.map[position.x, position.y].isWalkable = true;
                }
            }
        }

        if (!isFirst) {
            MapManager.map[lastWallPosition.x, lastWallPosition.y].type = "Floor";
            MapManager.map[lastWallPosition.x, lastWallPosition.y].isWalkable = true;
            MapManager.map[lastWallPosition.x, lastWallPosition.y].baseChar = ".";
            MapManager.map[lastWallPosition.x, lastWallPosition.y].isOpaque = false;
            switch (wall.direction) {
                case "South":
                    MapManager.map[lastWallPosition.x, lastWallPosition.y - 1].type = "Floor";
                    MapManager.map[lastWallPosition.x, lastWallPosition.y - 1].isWalkable = true;
                    MapManager.map[lastWallPosition.x, lastWallPosition.y - 1].baseChar = ".";
                    MapManager.map[lastWallPosition.x, lastWallPosition.y - 1].isOpaque = false;
                    break;
                case "North":
                    MapManager.map[lastWallPosition.x, lastWallPosition.y + 1].type = "Floor";
                    MapManager.map[lastWallPosition.x, lastWallPosition.y + 1].isWalkable = true;
                    MapManager.map[lastWallPosition.x, lastWallPosition.y + 1].baseChar = ".";
                    MapManager.map[lastWallPosition.x, lastWallPosition.y + 1].isOpaque = false;
                    break;
                case "West":
                    MapManager.map[lastWallPosition.x - 1, lastWallPosition.y].type = "Floor";
                    MapManager.map[lastWallPosition.x - 1, lastWallPosition.y].isWalkable = true;
                    MapManager.map[lastWallPosition.x - 1, lastWallPosition.y].baseChar = ".";
                    MapManager.map[lastWallPosition.x - 1, lastWallPosition.y].isOpaque = false;
                    break;
                case "East":
                    MapManager.map[lastWallPosition.x + 1, lastWallPosition.y].type = "Floor";
                    MapManager.map[lastWallPosition.x + 1, lastWallPosition.y].isWalkable = true;
                    MapManager.map[lastWallPosition.x + 1, lastWallPosition.y].baseChar = ".";
                    MapManager.map[lastWallPosition.x + 1, lastWallPosition.y].isOpaque = false;
                    break;
            }
        }

        room.width = roomWidth;
        room.height = roomHeight;
        room.type = type;
        room.id = countFeatures;
        allFeatures.Add(room);
        countFeatures++;
    }

    bool CheckIfHasSpace(Vector2Int start, Vector2Int end) {
        bool hasSpace = true;

        for (int y = start.y; y <= end.y; y++) {
            for (int x = start.x; x <= end.x; x++) {
                if (x < 0 || y < 0 || x >= mapWidth || y >= mapHeight) return false;
                if (MapManager.map[x, y] != null) {
                    return false;
                }
            }
        }

        return hasSpace;
    }

    Wall ChoseWall(Feature feature) {
        for (int i = 0; i < 10; i++) {
            int id = Random.Range(0, 100) / 25;
            if (!feature.walls[id].hasFeature) {
                return feature.walls[id];
            }
        }
        return null;
    }

    List<Feature> GetRooms() {
        List<Feature> newRooms = new List<Feature>();

        foreach (Feature feature in allFeatures) {
            if (feature.type == "Room") {
                newRooms.Add(feature);
            }
        }

        return newRooms;
    }

    void SpawnPlayer() {
        Feature room = rooms[Random.Range(0, rooms.Count - 1)];

        List<Vector2Int> positions = new List<Vector2Int>();

        foreach (Vector2Int position in room.positions) {
            if (MapManager.map[position.x, position.y].type == "Floor") {
                positions.Add(position);
            }
        }

        Vector2Int pos = positions[Random.Range(0, positions.Count - 1)];

        GameObject player = GameObject.Instantiate(playerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        player.transform.position = new Vector3(pos.x * tileScaling, pos.y * tileScaling, -1);

        player.GetComponent<PlayerMovement>().position = pos;
        MapManager.map[pos.x, pos.y].hasPlayer = true;
        MapManager.map[pos.x, pos.y].secondChar = "@";
        room.hasPlayer = true;
        GetComponent<GameManager>().player = player.GetComponent<PlayerMovement>();
    }

    void SpawnEnmies() {
        int quantity = Random.Range(minEnemies, maxEnemies);

        for (int i = 0; i < quantity; i++) {
            Feature room = rooms[Random.Range(0, rooms.Count - 1)];

            while (room.hasPlayer) {
                room = rooms[Random.Range(0, rooms.Count - 1)];
            }

            List<Vector2Int> positions = new List<Vector2Int>();

            foreach (Vector2Int position in room.positions) {
                if (MapManager.map[position.x, position.y].type == "Floor") {
                    positions.Add(position);
                }
            }

            Vector2Int pos = positions[Random.Range(0, positions.Count - 1)];

            while (MapManager.map[pos.x, pos.y].hasEnemy) {
                pos = positions[Random.Range(0, positions.Count - 1)];
            }

            GetComponent<EnemySpawn>().SpawnEnemy(pos, tileScaling);
        }
    }

    public void DrawMap(bool isASCII) {
        if (isASCII) {
            Text screen = GameObject.Find("ASCIITest").GetComponent<Text>();

            string asciiMap = "";

            string colorStep1 = "<color=";
            string colorStep2 = ">";
            string colorStep3 = "</color>";


            for (int y = (mapHeight - 1); y >= 0; y--) {
                for (int x = 0; x < mapWidth; x++) {
                    if (MapManager.map[x, y] != null) {
                        if (MapManager.map[x, y].secondChar == "") {
                            asciiMap += colorStep1 + MapManager.map[x, y].color + colorStep2 + MapManager.map[x, y].baseChar + colorStep3;
                        } else {
                            asciiMap += colorStep1 + MapManager.map[x, y].color + colorStep2 + MapManager.map[x, y].secondChar + colorStep3;
                        }
                    } else {
                        asciiMap += " ";
                    }

                    if (x == (mapWidth - 1)) {
                        asciiMap += "\n";
                    }
                }
            }

            screen.text = asciiMap;
        } else {
            GameObject parent = GameObject.Find("MapHolder");

            for (int y = 0; y < mapHeight; y++) {
                for (int x = 0; x < mapWidth; x++) {
                    if (MapManager.map[x, y] != null) {
                        GameObject newTile = new GameObject();

                        newTile.AddComponent<SpriteRenderer>();
                        newTile.AddComponent<TileInfo>();

                        switch (MapManager.map[x, y].type) {
                            case "Wall":
                                //newTile.GetComponent<SpriteRenderer>().sprite = walls[Random.Range(0, walls.Length - 1)];
                                newTile.GetComponent<SpriteRenderer>().sprite = walls[Random.Range(0, walls.Length - 1)];
                                break;
                            case "Floor":
                                //newTile.GetComponent<SpriteRenderer>().sprite = floors[Random.Range(0, floors.Length - 1)];
                                newTile.GetComponent<SpriteRenderer>().sprite = floors[Random.Range(3, 7)];
                                break;
                        }

                        newTile.transform.position = new Vector3(x * tileScaling, y * tileScaling, 0);
                        newTile.transform.parent = parent.transform;
                        newTile.name = MapManager.map[x, y].type;

                        MapManager.map[x, y].baseObject = newTile;
                        newTile.GetComponent<TileInfo>().Initialize(new Vector2Int(x, y));
                    }
                }
            }
        }
    }
}