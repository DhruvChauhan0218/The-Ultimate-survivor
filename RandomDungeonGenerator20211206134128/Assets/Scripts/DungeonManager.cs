using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class DungeonManager : MonoBehaviour
{
    [Range(50, 1000)]
    public int totalFloorCount = 500;

    [Range(0, 100)]
    public int itemSpawnProbability = 0;

    [Range(0, 100)]
    public int enemySpawnProbability = 0;

    [Range(0, 100)]
    public int windingHallProbability = 20;

    public bool roundedEdges;

    public DungeonType dungeonType;

    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject tilePrefab;
    public GameObject exitPrefab;
    public static GameObject door;
    public GameObject keyPrefab;

    public GameObject[] randomItems;
    public GameObject[] randomEnemies;
    public GameObject[] wallRoundedEdges;

    [HideInInspector]
    public float minX;

    [HideInInspector]
    public float maxX;

    [HideInInspector]
    public float minY;

    [HideInInspector]
    public float maxY;

    [NotNull]
    private static readonly Vector3[] Directions = { Vector3.up, Vector3.right, Vector3.down, Vector3.left };

    [NotNull]
    private readonly List<Vector3> _floorList = new List<Vector3>();

    public Dictionary<Vector2, GameObject> FloorTiles = new Dictionary<Vector2, GameObject>();

    public List<Vector2> FloorTilesAI = new List<Vector2>();

    private readonly Vector2 _hitSize = Vector2.one * 0.8f;
    private LayerMask _floorMask;
    private LayerMask _wallMask;
    private Vector3? _doorPos, _keyPos;

    private static Vector3 RandomDirection() => Directions[Random.Range(0, Directions.Length)];

    public static DungeonManager Instance;

    public GameObject waitingPopup;

    private void Awake()
    {
        Debug.Log("Current Enemy Spawn Probability: " + enemySpawnProbability);

        Instance = this;
        FloorTiles.Clear();
        if (PlayerPrefs.GetInt("Objective") == 0)
        {
            GameManager.Instance.item.SetActive(false);
            GameManager.Instance.PurchaseBox.SetActive(false);
            enemySpawnProbability = 1;
            itemSpawnProbability = 3;
            GameManager.Instance.levelText.text = PlayerPrefs.GetInt("Objective").ToString();
            Debug.LogWarning("Updated enemy spawn probablity: " + enemySpawnProbability);
            if (PlayerPrefs.GetInt("PurchaseBox") == 0)
            {
                if (PlayerPrefs.GetInt("Coins") > 0 && !GameManager.Instance.PurchaseBox.activeInHierarchy)
                {
                    GameManager.Instance.purchaseItemImage.sprite = GameManager.Instance.itemSprites[PlayerPrefs.GetInt("ItemSprite")];
                    GameManager.Instance.playerItemPurchasedImage = GameManager.Instance.itemSprites[PlayerPrefs.GetInt("ItemSprite")];
                    GameManager.Instance.PurchaseBox.SetActive(true);
                }
            }
        }

        if (PlayerPrefs.GetInt("Objective") == 1)
        {
            GameManager.Instance.item.SetActive(false);
            GameManager.Instance.PurchaseBox.SetActive(false);
            enemySpawnProbability = 3;
            itemSpawnProbability = 4;
            GameManager.Instance.levelText.text = PlayerPrefs.GetInt("Objective").ToString();
            Debug.LogWarning("Upgraded enemy spawn probablity: " + enemySpawnProbability);
            if (PlayerPrefs.GetInt("PurchaseBox") == 1)
            {
                if (PlayerPrefs.GetInt("Coins") > 0 && !GameManager.Instance.PurchaseBox.activeInHierarchy)
                {
                    player.speed += 2f;
                    GameManager.Instance.purchaseItemImage.sprite = GameManager.Instance.itemSprites[PlayerPrefs.GetInt("ItemSprite")];
                    GameManager.Instance.playerItemPurchasedImage = GameManager.Instance.itemSprites[PlayerPrefs.GetInt("ItemSprite")];
                    GameManager.Instance.PurchaseBox.SetActive(true);
                }
            }
        }
        if (PlayerPrefs.GetInt("Objective") == 2)
        {
            GameManager.Instance.item.SetActive(false);
            GameManager.Instance.PurchaseBox.SetActive(false);
            enemySpawnProbability = 6;
            itemSpawnProbability = 5;
            GameManager.Instance.levelText.text = PlayerPrefs.GetInt("Objective").ToString();
            Debug.LogWarning("Upgraded enemy spawn probablity: " + enemySpawnProbability);
            if (PlayerPrefs.GetInt("PurchaseBox") == 2)
            {
                if (PlayerPrefs.GetInt("Coins") > 0) { GameManager.Instance.purchaseItemImage.sprite = GameManager.Instance.itemSprites[PlayerPrefs.GetInt("ItemSprite")]; GameManager.Instance.PurchaseBox.SetActive(true); }
            }
        }
        if (PlayerPrefs.GetInt("Objective") == 3)
        {
            GameManager.Instance.item.SetActive(false);
            GameManager.Instance.PurchaseBox.SetActive(false);
            enemySpawnProbability = 7;
            itemSpawnProbability = 6;
            GameManager.Instance.levelText.text = PlayerPrefs.GetInt("Objective").ToString();
            Debug.LogWarning("Upgraded enemy spawn probablity: " + enemySpawnProbability);
        }
    }

    private void Start()
    {
        _floorMask = LayerMask.GetMask("Floor");
        _wallMask = LayerMask.GetMask("Wall");

        switch (dungeonType)
        {
            case DungeonType.Caverns: RandomWalker(); break;
        }

        // Wait for the awakes and starts to be called.
        StartCoroutine(DelayProgress());
    }

    private void Update()
    {
        // Reload scene on hotkey.
        if (Application.isEditor && Input.GetKeyDown(KeyCode.Backspace))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    #region Astar path finding

    private Astar astar;
    private Vector3Int[,] spots;
    private List<Spot> roadPath = new List<Spot>();

    private void CreateGrid()
    {
        astar = new Astar(2000, 2000);
        spots = new Vector3Int[2000, 2000];
        for (int i = 0; i < _floorList.Count; i++)
        {
            int x = Mathf.RoundToInt(_floorList[i].x);
            int y = Mathf.RoundToInt(_floorList[i].y);
            spots[x, y] = new Vector3Int(x, y, 0);
        }
    }

    public List<Spot> GetWalkablePath(Vector2 startPos, Vector2 endPos, int reachableHeight)
    {
        Vector3 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (roadPath != null && roadPath.Count > 0)
            roadPath.Clear();

        // Here starting position is for AI and ending position is for player.
        roadPath = astar.CreatePath(spots, ToInt2(startPos), ToInt2(endPos), 2000, reachableHeight);
        
        if (roadPath == null)
            return roadPath = new List<Spot>();
        else
            return roadPath;
    }

    public Vector2Int ToInt2(Vector2 v)
    {
        return new Vector2Int((int)v.x, (int)v.y);
    }

    public Player player;
    public HeroController heroController;

    public void TestPathFindingAlgo()
    {
        foreach(KeyValuePair<Vector2, GameObject> f in FloorTiles)
            f.Value.GetComponent<SpriteRenderer>().color = Color.white;

        FloorTilesAI.Clear();
        List<Spot> path = new List<Spot>();
        Vector3 startPos = heroController.transform.position;// For testing purpose, you can set any!
        Vector3 endPos = player.transform.position; // For testing purpose, you can set any!

        path = GetWalkablePath(startPos, endPos, 0);
        Debug.Log("Path Length: " + path.Count);

        for (int i = path.Count - 1; i >= 0; i--)
        {
            FloorTiles[new Vector2(path[i].X, path[i].Y)].GetComponent<SpriteRenderer>().color = Color.red;
            FloorTilesAI.Add(new Vector2(path[i].X, path[i].Y));
        }
        heroController.Move(endPos, path);
    }

    #endregion

    private void RandomWalker()
    {
        // Starting point of the PLAYER (... is also the random walker)
        var curPos = new Vector3(1000f, 1000f, 0f);
        _floorList.Add(curPos);

        while (_floorList.Count < totalFloorCount)
        {
            curPos += RandomDirection();

            // TODO: Should we use a hashset?
            if (_floorList.Contains(curPos)) continue;
            _floorList.Add(curPos);
        }

        int random = Random.Range(0, _floorList.Count);
        heroController.transform.position = new Vector3(_floorList[random].x, _floorList[random].y, 0);
    }

    private void RandomRoom(Vector3 position)
    {
        // Randomly select a half-width of a room; e.g.,
        // a value of 4 implies a 2x4+1=9 cell wide room.
        var width = Random.Range(1, 5);
        var height = Random.Range(1, 5);
        for (var w = -width; w <= width; ++w)
        {
            for (var h = -height; h <= height; ++h)
            {
                var offset = new Vector3(w, h);
                var candidateTile = position + offset;

                // TODO: Should we use a hashset?
                if (_floorList.Contains(candidateTile)) continue;
                _floorList.Add(candidateTile);
            }
        }
    }

    private void TakeLongWalk(ref Vector3 curPos)
    {
        var walkDirection = RandomDirection();

        // We want to create 3x3 up to 9x9 rooms later on.
        var walkLength = Random.Range(9, 18);
        for (var i = 0; i < walkLength; ++i)
        {
            curPos += walkDirection;

            // TODO: Should we use a hashset?
            if (_floorList.Contains(curPos)) continue;
            _floorList.Add(curPos);
        }
    }

    private IEnumerator DelayProgress()
    {
        // Instantiate tiles.
        for (var i = 0; i < _floorList.Count; ++i)
        {
            var goTile = Instantiate(tilePrefab, _floorList[i], Quaternion.identity, transform);
            goTile.name = tilePrefab.name;
        }

        // Wait for all tile spawners to be created before continuing to place level elements.
        while (FindObjectsOfType<TileSpawner>().Length > 0)
        {
            yield return null;
        }

        CreateExitDoor();
        CreateRandomItems();
        yield return new WaitForEndOfFrame();
        CreateKey();
        PlayerMovement.localCoinsCount = GameObject.FindGameObjectsWithTag("Coin").Length;
        Debug.LogError("Coins: " + PlayerMovement.localCoinsCount);

        CreateGrid();
    }

    private void CreateExitDoor()
    {
        // We're assuming that the random walker ended up somewhere distant from the player.
        // This is the location were we place our exit dor.
        _doorPos = _floorList[_floorList.Count - 1];

        GameObject goDoor = Instantiate(exitPrefab, _doorPos.Value, Quaternion.identity, transform);
        goDoor.name = exitPrefab.name;
        door = goDoor;
        door.SetActive(false);
    }

    private void CreateKey()
    {
        _keyPos = _floorList[Random.Range(0, _floorList.Count)];
        Debug.Log("Instantiating Key...");
        GameObject key = Instantiate(keyPrefab, _keyPos.Value, Quaternion.identity, transform);
        key.tag = "Key";
    }

    private void CreateRandomItems()
    {
        Debug.Assert(_doorPos.HasValue, "Exit door must be instantiated before placing items.");

        const int offset = 2; // TODO: Why though?
        for (var x = (int) minX - offset; x <= (int) maxX + offset; ++x)
        {
            for (var y = (int) minY - offset; y <= (int) maxY + offset; ++y)
            {
                // Note that the angle (of 0) is hugely important. If unspecified, all the
                // areas surrounding a floor tile (i.e., walls and other open floors) will be triggering
                // collisions as well.
                var hitFloor = Physics2D.OverlapBox(new Vector2(x, y), _hitSize, 0, _floorMask);
                if (hitFloor)
                {
                    // Ensure we're not placing something onto the exit door.
                    // ReSharper disable once PossibleInvalidOperationException
                    var positionIsExitDoor = Vector2.Equals(hitFloor.transform.position, _doorPos.Value);
                    if (positionIsExitDoor) continue;

                    var hitTop = Physics2D.OverlapBox(new Vector2(x, y + 1), _hitSize, 0, _wallMask);
                    var hitRight = Physics2D.OverlapBox(new Vector2(x + 1, y), _hitSize, 0, _wallMask);
                    var hitBottom = Physics2D.OverlapBox(new Vector2(x, y - 1), _hitSize, 0, _wallMask);
                    var hitLeft = Physics2D.OverlapBox(new Vector2(x - 1, y), _hitSize, 0, _wallMask);

                    CreateRandomItem(hitFloor, hitTop, hitRight, hitBottom, hitLeft);
                    CreateRandomEnemy(hitFloor, hitTop, hitRight, hitBottom, hitLeft);
                }

                RoundedEdges(x, y);
            }
        }
    }

    private void RoundedEdges(int x, int y)
    {
        if (!roundedEdges) return;

        var position = new Vector2(x, y);
        var hitWall = Physics2D.OverlapBox(position, _hitSize, 0, _wallMask);
        if (!hitWall) return;

        var hitTop = Physics2D.OverlapBox(new Vector2(x, y + 1), _hitSize, 0, _wallMask);
        var hitRight = Physics2D.OverlapBox(new Vector2(x + 1, y), _hitSize, 0, _wallMask);
        var hitBottom = Physics2D.OverlapBox(new Vector2(x, y - 1), _hitSize, 0, _wallMask);
        var hitLeft = Physics2D.OverlapBox(new Vector2(x - 1, y), _hitSize, 0, _wallMask);

        var bitValue = 0;
        bitValue += hitTop ? 0 : 1;
        bitValue += hitRight ? 0 : 2;
        bitValue += hitBottom ? 0 : 4;
        bitValue += hitLeft ? 0 : 8;

        if (bitValue == 0) return;

        var edgePrefab = wallRoundedEdges[bitValue];
        var goItem = Instantiate(edgePrefab, position, Quaternion.identity, hitWall.transform);
        goItem.name = edgePrefab.name;
    }

    private void CreateRandomItem([NotNull] Collider2D hitFloor,
        [NotNull] Collider2D hitTop, [NotNull] Collider2D hitRight, [NotNull] Collider2D hitBottom,
        [NotNull] Collider2D hitLeft)
    {
        var hasSurroundingWall = hitTop || hitRight || hitBottom || hitLeft;
        var isHorizontalTunnel = hitTop && hitBottom;
        var isVerticalTunnel = hitLeft && hitRight;
        var isTunnel = isHorizontalTunnel || isVerticalTunnel;
        if (!hasSurroundingWall || isTunnel) return;

        var roll = Random.Range(1, 101);
        if (roll > itemSpawnProbability) return;

        var itemIndex = Random.Range(0, randomItems.Length);
        var itemPrefab = randomItems[itemIndex];

        var floorTransform = hitFloor.transform;
        var goItem = Instantiate(itemPrefab, floorTransform.position, Quaternion.identity, floorTransform);
        goItem.name = itemPrefab.name;
    }

    private void CreateRandomEnemy([NotNull] Collider2D hitFloor,
        [NotNull] Collider2D hitTop, [NotNull] Collider2D hitRight, [NotNull] Collider2D hitBottom,
        [NotNull] Collider2D hitLeft)
    {
        var hasSurroundingWall = hitTop || hitRight || hitBottom || hitLeft;
        if (hasSurroundingWall) return;

        var roll = Random.Range(1, 101);
        if (roll > enemySpawnProbability) return;

        var enemyIndex = Random.Range(0, randomEnemies.Length);
        var enemyPrefab = randomEnemies[enemyIndex];

        var floorTransform = hitFloor.transform;
        var goEnemy = Instantiate(enemyPrefab, floorTransform.position, Quaternion.identity, floorTransform);
        if (PlayerPrefs.GetInt("Objective") == 1) { goEnemy.GetComponent<Renderer>().material.color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 0.8f); }
        if (PlayerPrefs.GetInt("Objective") == 2) { goEnemy.GetComponent<Renderer>().material.color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.6f, 1f); }
        if (PlayerPrefs.GetInt("Objective") == 3) { goEnemy.GetComponent<Renderer>().material.color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.7f, 1.2f); }
        goEnemy.name = enemyPrefab.name;
    }
}
