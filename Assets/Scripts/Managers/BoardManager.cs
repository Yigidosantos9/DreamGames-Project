using UnityEngine;
using System.Collections.Generic;
public class BoardManager : MonoBehaviour
{
    // References
    public LevelManager levelManager;
    public TopBarUI topBarUI;
    public int grid_width = 6;
    public int grid_height = 6;

    // colored cubes prefab references
    public GameObject redCubePrefab;
    public GameObject blueCubePrefab;
    public GameObject greenCubePrefab;
    public GameObject yellowCubePrefab;

    // references for obstacles
    public GameObject boxPrefab;
    public GameObject stonePrefab;
    public GameObject vasePrefab;

    // reference for rocket
    public GameObject rocketPrefab;

    // this array will store all the positions of cubes
    public BaseItem[,] items;
    void Start()
    {
        items = new BaseItem[grid_width, grid_height];
    }

    public void SetupBoardFromData(System.Collections.Generic.List<string> gridData)
    {
        // Recreate the array
        items = new BaseItem[grid_width, grid_height];

        // Assume grid data is ordered row-by-row from bottom left cell upward
        for (int i = 0; i < gridData.Count; i++)
        {
            int x = i % grid_width;
            int y = i / grid_width;
            string code = gridData[i];

            GameObject prefabToSpawn = null;
            switch (code)
            {
                case "r": prefabToSpawn = redCubePrefab; break;
                case "g": prefabToSpawn = greenCubePrefab; break;
                case "b": prefabToSpawn = blueCubePrefab; break;
                case "y": prefabToSpawn = yellowCubePrefab; break;
                case "rand":
                    prefabToSpawn = GetRandomCubePrefab();
                    break;
                case "bo": prefabToSpawn = boxPrefab; break;
                case "s": prefabToSpawn = stonePrefab; break;
                case "v": prefabToSpawn = vasePrefab; break;
                case "vro": // Vertical rocket
                    prefabToSpawn = rocketPrefab; // Direction setup will be done here
                    break;
                case "hro": // Horizontal rocket
                    prefabToSpawn = rocketPrefab;
                    break;
                default: prefabToSpawn = redCubePrefab; break;
            }

            Vector2 spawnPos = GetSpawnPosition(x, y);
            GameObject obj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity, transform);
            BaseItem item = obj.GetComponent<BaseItem>();
            if (item == null)
            {
                Debug.LogError($"No BaseItem found in prefab {prefabToSpawn.name}! x={x}, y={y}");
            }
            else
            {
                item.Init(this, x, y);
                items[x, y] = item;
            }
        }
    }
    void SpawnObstacle(GameObject obstaclePrefab, int x, int y)
    {
        Vector2 spawnPos = GetSpawnPosition(x, y);
        GameObject obj = Instantiate(obstaclePrefab, spawnPos, Quaternion.identity, transform);
        BaseItem item = obj.GetComponent<BaseItem>();
        item.Init(this, x, y);
        items[x, y] = item;
    }
    // spawns random cube on the grid
    public Vector2 GetSpawnPosition(int x, int y)
    {
        float offsetX = -(grid_width - 1) / 2f;
        float offsetY = -(grid_height - 1) / 2f;
        return new Vector2(x + offsetX, y + offsetY);
    }
    public void AdjustCamera(int gridWidth, int gridHeight)
    {
        // Recalculate offsets
        float offsetX = -(gridWidth - 1) / 2f;
        float offsetY = -(gridHeight - 1) / 2f;

        // Find actual center of the grid
        float centerX = (gridWidth / 2f - 0.5f) + offsetX;
        float centerY = (gridHeight / 2f - 0.5f) + offsetY;

        Camera.main.transform.position = new Vector3(centerX, centerY, -10);

        // Adjust camera size based on aspect ratio
        float aspect = (float)Screen.width / Screen.height;
        float sizeBasedOnWidth = gridWidth / 2f / aspect;
        float sizeBasedOnHeight = gridHeight / 2f;

        Camera.main.orthographicSize = Mathf.Max(sizeBasedOnWidth, sizeBasedOnHeight) + 0.5f;
    }
    void SpawnRandomCube(int x, int y)
    {
        GameObject cubePrefab = GetRandomCubePrefab();
        Vector2 spawnPos = GetSpawnPosition(x, y);

        GameObject cubeObj = Instantiate(cubePrefab, spawnPos, Quaternion.identity, transform);

        BaseItem item = cubeObj.GetComponent<BaseItem>();
        item.Init(this, x, y);
        items[x, y] = item;
        /* debug
        if (item is CubeItem cube)
        {
            Debug.Log($"Spawned cube at ({x},{y}) with color: {cube.color}");
        }
        */
    }
    // selects random color
    GameObject GetRandomCubePrefab()
    {
        int r = Random.Range(0, 4);
        GameObject selectedPrefab = null;
        switch (r)
        {
            case 0: selectedPrefab = redCubePrefab; break;
            case 1: selectedPrefab = blueCubePrefab; break;
            case 2: selectedPrefab = greenCubePrefab; break;
            default: selectedPrefab = yellowCubePrefab; break;
        }
        //Debug.Log("Random prefab selected: " + selectedPrefab.name);
        return selectedPrefab;
    }

    public void OnCubeClicked(CubeItem origin)
    {
        if (levelManager != null && levelManager.isGameOver)
            return;

        var connectedList = FindConnectedCubes(origin.x, origin.y, origin.color);

        // If less than 2 cubes (origin + at least 1 neighbor), ignore the tap
        if (connectedList.Count < 2)
        {
            Debug.Log("Only one cube selected. Ignoring tap.");
            return;
        }

        bool createRocket = (connectedList.Count >= 4);

        // Remove all cubes in the blast group except the origin
        foreach (var cube in connectedList)
        {
            if (cube != origin)
                cube.RemoveFromBoard();
        }

        // if there can be a rocket
        if (createRocket)
        {
            CreateRocketAt(origin.x, origin.y, origin.color);
        }
        else
        {
            origin.RemoveFromBoard();
        }

        // Deal damage to obstacles around the blast group
        HashSet<Vector2Int> affectedPositions = new HashSet<Vector2Int>();
        foreach (CubeItem cube in connectedList)
        {
            foreach (Vector2Int pos in GetNeighbors(cube.x, cube.y))
            {
                affectedPositions.Add(pos);
            }
        }
        foreach (Vector2Int pos in affectedPositions)
        {
            if (items[pos.x, pos.y] != null && (items[pos.x, pos.y] is BoxObstacle || items[pos.x, pos.y] is VaseObstacle))
            {
                items[pos.x, pos.y].TakeDamage();
            }
        }

        levelManager.OnValidTap();

        MakeItemsFall();
        RefillBoard();
        UpdateUI();

        if (levelManager != null && AreAllObstaclesCleared())
        {
            levelManager.Win();
        }
    }

    // Find connected neighbors using BFS
    private List<CubeItem> FindConnectedCubes(int startX, int startY, string color)
    {
        List<CubeItem> connected = new List<CubeItem>();
        bool[,] visited = new bool[grid_width, grid_height];
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        queue.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            int cx = current.x;
            int cy = current.y;

            if (items[cx, cy] is CubeItem cube && cube.color == color)
            {
                connected.Add(cube);

                foreach (Vector2Int neighbor in GetNeighbors(cx, cy))
                {
                    int nx = neighbor.x;
                    int ny = neighbor.y;

                    if (!visited[nx, ny] && items[nx, ny] is CubeItem neighborCube && neighborCube.color == color)
                    {
                        visited[nx, ny] = true;
                        queue.Enqueue(new Vector2Int(nx, ny));
                    }
                }
            }
        }

        return connected;
    }

    // Return neighboring cells
    private List<Vector2Int> GetNeighbors(int x, int y)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        if (x > 0) neighbors.Add(new Vector2Int(x - 1, y));       // left
        if (x < grid_width - 1) neighbors.Add(new Vector2Int(x + 1, y)); // right
        if (y > 0) neighbors.Add(new Vector2Int(x, y - 1));       // down
        if (y < grid_height - 1) neighbors.Add(new Vector2Int(x, y + 1)); // up
        return neighbors;
    }

    public void MakeItemsFall()
    {
        for (int x = 0; x < grid_width; x++)
        {
            // From bottom to top
            for (int y = 0; y < grid_height; y++)
            {
                if (items[x, y] == null)
                {
                    for (int ny = y + 1; ny < grid_height; ny++)
                    {
                        if (items[x, ny] != null)
                        {
                            items[x, y] = items[x, ny];
                            items[x, ny] = null;

                            items[x, y].x = x;
                            items[x, y].y = y;
                            items[x, y].transform.position = GetSpawnPosition(x, y);

                            break;
                        }
                    }
                }
            }
        }
    }

    public void RefillBoard()
    {
        for (int x = 0; x < grid_width; x++)
        {
            for (int y = 0; y < grid_height; y++)
            {
                if (items[x, y] == null)
                {
                    SpawnRandomCube(x, y);
                }
            }
        }
    }

    void CreateRocketAt(int x, int y, string color)
    {
        items[x, y].RemoveFromBoard();

        bool isVertical = (Random.Range(0, 2) == 0);

        // Instantiate rocket
        Vector2 pos = GetSpawnPosition(x, y);
        GameObject rocketObj = Instantiate(rocketPrefab, pos, Quaternion.identity, transform);

        RocketItem rocket = rocketObj.GetComponent<RocketItem>();
        rocket.Init(this, x, y);
        rocket.SetDirection(isVertical);

        // save it
        items[x, y] = rocket;
    }
    public bool AreAllObstaclesCleared()
    {
        for (int x = 0; x < grid_width; x++)
        {
            for (int y = 0; y < grid_height; y++)
            {
                if (items[x, y] != null &&
                    (items[x, y] is BoxObstacle || items[x, y] is VaseObstacle || items[x, y] is StoneObstacle))
                {
                    return false;
                }
            }
        }
        return true;
    }
    public (int boxCount, int stoneCount, int vaseCount) GetObstacleCounts()
    {
        int boxCount = 0;
        int stoneCount = 0;
        int vaseCount = 0;

        for (int x = 0; x < grid_width; x++)
        {
            for (int y = 0; y < grid_height; y++)
            {
                if (items[x, y] is BoxObstacle) boxCount++;
                else if (items[x, y] is StoneObstacle) stoneCount++;
                else if (items[x, y] is VaseObstacle) vaseCount++;
            }
        }
        return (boxCount, stoneCount, vaseCount);
    }
    public void UpdateUI()
    {
        var (box, stone, vase) = GetObstacleCounts();
        topBarUI.SetMoves(levelManager.movesLeft);
        topBarUI.SetGoal(box, stone, vase);
    }
}
