using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEditor.Experimental.GraphView;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System;

public class MazeRenderer : MonoBehaviour
{
    /**
     * Much of this code was borrowed from Assignment 5.
     **/

    public float floor_height;
    public float wall_thickness_x;
    public float wall_thickness_z;
    public float corner_thickness_x;
    public float corner_thickness_z;
    public float poof_trap_radius;
    public float storey_height;
    private float RENDER_EPSILON = 1e-6f;
    private float EDGE_EPSILON = 0.02f;
    public GameObject fps_prefab;
    internal GameObject fps_player_obj;
    public static readonly int playerStartingHealth = 5;
    public int playerHealth;
    public static readonly float SPEED_BOOST_MODIFIER = 0.5f;
    public static readonly float SPEED_BOOST_TIME = 4.0f;  // Seconds
    public static readonly float POWER_BOOST_TIME = 5.0f;  // Seconds
    public float playerSpeedModifier;
    public float speedBoostTimer;
    public float powerBoostTimer;
    public static string PLAYER_NAME = "PLAYER";
    private float playerHeight;
    public Bounds bounds;
    private List<GameObject> nullFacedPickups;

    public GameObject treasureChestPrefab;
    public GameObject treasureChest;
    public Color GROUND_COLOR = Color.grey;
    private Color DEFAULT_COLOR = new Color(0.3f, 0.4f, 0.2f);
    private Color LIGHTEST_FLOOR_COLOR = new Color(0.6f, 0.8f, 0.8f);
    public Color[] floorColors;
    public Material[] floorMaterials;
    public Material[] wallMaterials;
    public Color START_TILE_COLOR = new Color(0.0f, 0.0f, 1.0f);
    public Material POOF_TRAP_MATERIAL;
    public static int PICKUP_FONT_SIZE = 15;
    public static Color PICKUP_FONT_COLOR_DEFAULT = new Color(1.0f, 1.0f, 1.0f);
    public static Color BOOST_FONT_COLOR_DEFAULT = new Color(1.0f, 0.0f, 0.0f);
    public TMP_FontAsset defaultFont;
    public TMP_FontAsset arrowFont;
    public static Material PICKUP_MATERIAL;
    public Canvas canvas;
    private Minimap[] minimaps;
    private GameObject playerPointer;
    public string pathToMinimapBackground;
    public string pathToMinimapFilter;
    public string pathToPlayerPointer;
    public Color minimapWallColor;
    public Color minimapFloorColor;
    public int minimapWidth;
    public int minimapHeight;
    private Vector2 minimapOffset;

    public AudioClip chaserStepClip;
    public AudioSource chaserStepSource;
    public AudioClip patrollerStepClip;
    public AudioSource patrollerStepSource;
    public AudioClip hunterStepClip;
    public AudioSource hunterStepSource;
    public AudioClip chaserAttackClip;
    public AudioSource chaserAttackSource;
    public AudioClip patrollerAttackClip;
    public AudioSource patrollerAttackSource;
    public AudioClip hunterAttackClip;
    public AudioSource hunterAttackSource;
    public float chaserAudioDistance;
    public float patrollerAudioDistance;
    public float hunterAudioDistance;
    public AudioClip itemPingClip;
    public AudioSource itemPingSource;
    public AudioClip poofWarpClip;
    public AudioSource poofWarpSource;
    public AudioClip enemyDestructionClip;
    public AudioSource enemyDestructionSource;

    public GameObject inventoryItemPrefab;

    private MazeGenerator generator;
    private string[][,][] maze;
    public GameObject Patroller;
    public GameObject Chaser;
    public GameObject Hunter;
    private CanvasManager canvasManager;
    public RectTransform healthBar;
    public float healthBarWidth, healthBarHeight;
    private void Awake()
    {
        PICKUP_MATERIAL = Resources.Load<Material>("PickupMaterial");
    }

    // Start is called before the first frame update
    void Start()
    {
        canvasManager = canvas.GetComponent<CanvasManager>();
        playerHealth = playerStartingHealth;
        speedBoostTimer = 0.0f;
        powerBoostTimer = 0.0f;
        playerSpeedModifier = 1.0f;

        if (defaultFont != null && arrowFont != null)
        {
            if (defaultFont.fallbackFontAssetTable == null)
                defaultFont.fallbackFontAssetTable = new List<TMP_FontAsset>();
            if (!defaultFont.fallbackFontAssetTable.Contains(arrowFont))
            {
                defaultFont.fallbackFontAssetTable.Add(arrowFont);
            }
            Debug.Log(defaultFont.fallbackFontAssetTable[0]);
        }

        nullFacedPickups = new List<GameObject>();

        // fps_prefab.GetComponent<RigidbodyFirstPersonController>().enabled = true;
        bounds = GetComponent<Collider>().bounds;
        Debug.Log(bounds.min);
        Debug.Log(bounds.max);
        Debug.Log(bounds.size);

        playerHeight = storey_height / 3.0f;  // TODO : CHANGE THIS TO BE INSTANTIATED RELATIVE TO CAMERA

        generator = gameObject.GetComponent<MazeGenerator>();

        Color gradient = (LIGHTEST_FLOOR_COLOR - DEFAULT_COLOR) / generator.numFloors;
        List<Color> colors = new List<Color>() { DEFAULT_COLOR + gradient };
        for (int i = 1; i < generator.numFloors; i++)
        {
            colors.Add(colors[i - 1] + gradient);
        }
        floorColors = colors.ToArray();

        maze = generator.Generate();

        float offsetOffsetInWidth = 0.01f;
        float offsetOffsetInHeight = (Screen.width / Screen.height) * offsetOffsetInWidth;
        minimapOffset = new Vector2(((minimapWidth / Screen.width) / 2) + offsetOffsetInWidth, ((minimapHeight / Screen.height) / 2) + offsetOffsetInHeight);

        // Generate the minimaps
        minimaps = new Minimap[maze.Length];
        for (int i = 0; i < maze.Length; i++)
        {
            Minimap minimap = new Minimap();
            minimap.canvas = canvas;
            minimap.minimapOffset = minimapOffset;
            minimap.minimapWidth = minimapWidth;
            minimap.minimapHeight = minimapHeight;
            minimap.pathToMinimapBackground = pathToMinimapBackground;
            minimap.pathToMinimapFilter = pathToMinimapFilter;
            minimap.wallColor = minimapWallColor;
            minimap.floorColor = minimapFloorColor;
            minimaps[i] = minimap;
        }
        // Generate the player pointer on top of the minimaps
        playerPointer = new GameObject("Player Pointer");
        RectTransform trans = playerPointer.AddComponent<RectTransform>();
        trans.transform.SetParent(canvas.transform); // setting parent
        trans.localScale = Vector3.one;
        trans.anchorMin = new Vector2(1.0f - (minimapWidth / Screen.width), 1.0f - (minimapHeight / Screen.height));
        trans.anchorMax = new Vector2(1.0f, 1.0f);
        trans.pivot = new Vector2(0.5f, 0.5f);
        //Debug.Log(Screen.width);
        trans.anchoredPosition = new Vector2(-minimapWidth / 2, -minimapHeight / 2);
        trans.sizeDelta = new Vector2(minimapWidth, minimapHeight); // custom size
        Image image = playerPointer.AddComponent<Image>();
        byte[] imageData = File.ReadAllBytes(Application.dataPath + pathToPlayerPointer);
        Texture2D tex = new Texture2D(2, 2);  // This width and height are overridden by LoadIMage()
        tex.LoadImage(imageData);
        image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        playerPointer.transform.SetParent(canvas.transform);

        wall_thickness_x = ((bounds.size[0] / maze[0].GetLength(0)) / 11.0f) / 2;
        wall_thickness_z = ((bounds.size[2] / maze[0].GetLength(1)) / 11.0f) / 2;
        // I added the corner_thickness initialization here too, I'm assuming they should always be 2x
        corner_thickness_x = wall_thickness_x * 3;
        corner_thickness_z = wall_thickness_z * 3;
        generator.draw(maze);
        render(maze);
    }

    public GameObject createPickup(string tileType, Vector3 position, Vector3 size)
    {
        return createPickup(tileType, position, size, PICKUP_FONT_COLOR_DEFAULT);
    }

    public GameObject createPickup(string tileType, Vector3 position, Vector3 size, Color textColor)
    {
        return createPickup(tileType, position, size, null, textColor);
    }

    public GameObject createPickup(string tileType, Vector3 position, Vector3 size, GameObject thingToFace, Color textColor)
    {
        GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pickup.GetComponent<Renderer>().material = PICKUP_MATERIAL;
        pickup.GetComponent<Renderer>().enabled = true;
        pickup.name = tileType;
        pickup.transform.position = position;
        pickup.transform.localScale = size;
        Rigidbody rb = pickup.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        BoxCollider collider = pickup.GetComponent<BoxCollider>();
        Debug.Log(collider);
        collider.size = size;
        collider.isTrigger = true;
        pickup.AddComponent<PickUp>();
        if (thingToFace is not null)
        {
            pickup.GetComponent<PickUp>().thingToFace = fps_player_obj;
        }
        else
        {
            nullFacedPickups.Add(pickup);
        }

        TextMeshPro textMesh = pickup.AddComponent<TextMeshPro>();
        textMesh.text = tileType;
        textMesh.fontSize = PICKUP_FONT_SIZE;
        textMesh.color = textColor;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.lineSpacing = 0;
        textMesh.font = defaultFont;

        GameObject backGroundBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backGroundBox.GetComponent<Renderer>().material = PICKUP_MATERIAL;
        backGroundBox.GetComponent<Renderer>().enabled = true;
        backGroundBox.name = tileType + " BackGroundBox";
        backGroundBox.transform.position = position;
        backGroundBox.transform.localScale = Vector3.Scale(size, new Vector3(1.5f, 1.5f, 0.1f));
        BoxCollider collider2 = backGroundBox.GetComponent<BoxCollider>();
        collider2.isTrigger = true;
        pickup.GetComponent<PickUp>().backGroundBox = backGroundBox;
        pickup.GetComponent<PickUp>().renderer = this;
        pickup.GetComponent<PickUp>().pingClip = itemPingClip;
        pickup.GetComponent<PickUp>().pingSource = itemPingSource;

        // attach the script to make items float up/down
        backGroundBox.AddComponent<PickUpAnimation>();

        return pickup;
    }

    private GameObject createCorner(Vector3 position, int floor, float widthUnitSize)
    {
        GameObject cornerBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cornerBlock.name = MazeGenerator.WALL;
        cornerBlock.transform.localScale = new Vector3(corner_thickness_x, storey_height + RENDER_EPSILON, corner_thickness_z);
        cornerBlock.transform.position = position;
        cornerBlock.GetComponent<Renderer>().material.color = floorColors[floor % wallMaterials.Length];
        cornerBlock.GetComponent<Renderer>().material = wallMaterials[floor % wallMaterials.Length];
        cornerBlock.GetComponent<Renderer>().material.mainTextureScale = new Vector2(0.75f * corner_thickness_x / widthUnitSize, 0.75f);

        return cornerBlock;
    }

    private GameObject createEnemy(string tileType, Vector3 pos)
    {
        GameObject enemy = new GameObject();
        int floor = Mathf.FloorToInt(pos.y / storey_height);
        if (tileType == MazeGenerator.CHASER)
        {
            enemy = GameObject.Instantiate(Chaser);
            enemy.SetActive(true);
            enemy.transform.position = pos;
            enemy.name = "CHASER";
            enemy.AddComponent<EnemyChaserController>();
            enemy.GetComponent<EnemyChaserController>().storey_height = storey_height;
            enemy.GetComponent<EnemyChaserController>().floor = floor;
            enemy.GetComponent<EnemyChaserController>().stepClip = chaserStepClip;
            enemy.GetComponent<EnemyChaserController>().stepSource = chaserStepSource;
            enemy.GetComponent<EnemyChaserController>().attackClip = chaserAttackClip;
            enemy.GetComponent<EnemyChaserController>().attackSource = chaserAttackSource;
            enemy.GetComponent<EnemyChaserController>().audioDistance = chaserAudioDistance;
            enemy.GetComponent<EnemyChaserController>().renderer = this;
            enemy.GetComponent<EnemyChaserController>().destructionClip = enemyDestructionClip;
            enemy.GetComponent<EnemyChaserController>().destructionSource = enemyDestructionSource;
        }
        else if (tileType == MazeGenerator.PATROLLER)
        {
            enemy = GameObject.Instantiate(Patroller);
            enemy.SetActive(true);
            enemy.transform.position = pos;
            enemy.name = "PATROLLER";
            enemy.AddComponent<EnemyPatrollerController>();
            enemy.GetComponent<EnemyPatrollerController>().storey_height = storey_height;
            enemy.GetComponent<EnemyPatrollerController>().floor = floor;
            enemy.GetComponent<EnemyPatrollerController>().stepClip = patrollerStepClip;
            enemy.GetComponent<EnemyPatrollerController>().stepSource = patrollerStepSource;
            enemy.GetComponent<EnemyPatrollerController>().attackClip = patrollerAttackClip;
            enemy.GetComponent<EnemyPatrollerController>().attackSource = patrollerAttackSource;
            enemy.GetComponent<EnemyPatrollerController>().audioDistance = patrollerAudioDistance;
            enemy.GetComponent<EnemyPatrollerController>().renderer = this;
            enemy.GetComponent<EnemyPatrollerController>().destructionClip = enemyDestructionClip;
            enemy.GetComponent<EnemyPatrollerController>().destructionSource = enemyDestructionSource;
        }
        else
        {
            enemy = GameObject.Instantiate(Hunter);
            enemy.SetActive(true);
            enemy.transform.position = pos;
            enemy.name = "HUNTER";
            enemy.AddComponent<EnemyHunterController>();
            enemy.GetComponent<EnemyHunterController>().storey_height = storey_height;
            enemy.GetComponent<EnemyHunterController>().floor = floor;
            enemy.GetComponent<EnemyHunterController>().stepClip = hunterStepClip;
            enemy.GetComponent<EnemyHunterController>().stepSource = hunterStepSource;
            enemy.GetComponent<EnemyHunterController>().attackClip = hunterAttackClip;
            enemy.GetComponent<EnemyHunterController>().attackSource = hunterAttackSource;
            enemy.GetComponent<EnemyHunterController>().audioDistance = hunterAudioDistance;
            enemy.GetComponent<EnemyHunterController>().renderer = this;
            enemy.GetComponent<EnemyHunterController>().destructionClip = enemyDestructionClip;
            enemy.GetComponent<EnemyHunterController>().destructionSource = enemyDestructionSource;
        }
        enemy.AddComponent<NavMeshAgent>();
        enemy.GetComponent<NavMeshAgent>().Warp(pos);

        return enemy;
    }

    private void render(string[][,][] floors)
    {
        // Minimaps
        for (int i = 0; i < floors.Length; i++)
        {
            minimaps[i].mapDraw(i, floors[i]);
            minimaps[i].setVisible(true);
        }

        GetComponent<Renderer>().material.color = GROUND_COLOR;

        // used for determining if the FLOOR tile is in the center area, so that it can be excluded from the NavMeshSurface
        int centerWideStart = ((generator.width - 1) / 2) - (generator.centerWidth / 2);
        int centerWideEnd = ((generator.width - 1) / 2) + (generator.centerWidth / 2);
        int centerHighStart = ((generator.height - 1) / 2) - (generator.centerHeight / 2);
        int centerHighEnd = ((generator.height - 1) / 2) + (generator.centerHeight / 2);

        // a List of tuples for each enemy, so that all enemies can be instantiated AFTER the NavMeshSurface has been baked
        List<(string tileType, Vector3 pos)> enemies = new List<(string tileType, Vector3 pos)>();

        for (int f = 0; f < floors.Length; f++)
        {
            string[,][] floor = floors[f];
            float width = (float)floor.GetLength(0);
            float length = (float)floor.GetLength(1);

            // create an empty gameObject in the hierarchy, whose children will be all the gameObjects generated for that floor
            // used for organization only
            GameObject floorObj = new GameObject();
            floorObj.name = "Floor " + f;
            // create a NavMeshSurface gameobject, whose children will be all floor+wall gameObjects required for baking the NavMeshSurface
            GameObject navMeshSurface = new GameObject();
            navMeshSurface.name = "NavMeshSurface " + f;
            navMeshSurface.transform.SetParent(floorObj.transform);
            navMeshSurface.AddComponent<NavMeshSurface>();
            // ensure the NavMeshSurface only bakes the mesh with its children gameObjects
            navMeshSurface.GetComponent<NavMeshSurface>().collectObjects = CollectObjects.Children;

            for (int i = 0; i < floor.GetLength(0); i++)
            {
                for (int j = 0; j < floor.GetLength(1); j++)
                {
                    float floorY = bounds.min[1] + (f * storey_height) + RENDER_EPSILON;
                    float wallY = bounds.min[1] + ((f + 1) * storey_height - (storey_height / 2));

                    float widthUnitSize = (bounds.size[0] / width - RENDER_EPSILON);
                    float lengthUnitSize = (bounds.size[2] / length - RENDER_EPSILON);
                    float widthStart = bounds.min[0];
                    float lengthStart = bounds.min[2];
                    Vector2 centerCoords = new Vector2(widthStart + (i * widthUnitSize) + widthUnitSize / 2,
                                                       lengthStart + (j * lengthUnitSize) + lengthUnitSize / 2);
                    Vector2 upLeft = new Vector2(widthStart + (i * widthUnitSize) + EDGE_EPSILON,
                                                 lengthStart + ((j + 1) * lengthUnitSize) + EDGE_EPSILON);
                    Vector2 downRight = new Vector2(widthStart + ((i + 1) * widthUnitSize) + EDGE_EPSILON,
                                                    lengthStart + (j * lengthUnitSize) + EDGE_EPSILON);
                    float centerX = centerCoords.x;
                    float centerZ = centerCoords.y;
                    float leftX = upLeft.x;
                    float rightX = downRight.x;
                    float upZ = upLeft.y;
                    float downZ = downRight.y;
                    string tileType = floor[i, j][MazeGenerator.directionIndex(MazeGenerator.CENTER)];
                    if (!tileType.Equals(MazeGenerator.EMPTY) && !tileType.Contains(MazeGenerator.PIT_TRAP))
                    {
                        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        block.name = MazeGenerator.FLOOR;
                        block.transform.localScale = new Vector3(bounds.size[0] / width + RENDER_EPSILON, floor_height, bounds.size[2] / length + RENDER_EPSILON);
                        block.transform.position = new Vector3(centerX, floorY, centerZ);
                        block.GetComponent<Renderer>().material.color = floorColors[f % wallMaterials.Length];
                        if ((f == 0) && (i == generator.start.Item1) && (j == generator.start.Item1))
                        {
                            block.GetComponent<Renderer>().material.color = START_TILE_COLOR;
                            treasureChest = Instantiate(treasureChestPrefab);
                            treasureChest.name = "Prize Chest";
                            treasureChest.transform.position = new Vector3(centerX, floorY + 0.0f, centerZ);
                            treasureChest.transform.rotation = Quaternion.Euler(0, 180, 0);
                            treasureChest.transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);  // Adjust as necessary
                            treasureChest.AddComponent<BoxCollider>();
                            // add the chest script for equation solving logic
                            treasureChest.AddComponent<Chest>();
                            treasureChest.GetComponent<Chest>().canvas = canvas;
                        }
                        block.GetComponent<Renderer>().material = floorMaterials[f % floorMaterials.Length];
                        // floor blocks should be considered in baking the NavMeshSurface only if they are not in the ground floor's center area
                        if (f == 0 && (i >= centerWideStart) && (i <= centerWideEnd) && (j >= centerHighStart) && (j <= centerHighEnd))
                        {
                            block.transform.SetParent(floorObj.transform);
                        }
                        else
                        {
                            block.transform.SetParent(navMeshSurface.transform);
                        }
                    }
                    if (tileType.Equals(MazeGenerator.CHASER) || tileType.Equals(MazeGenerator.PATROLLER) || tileType.Equals(MazeGenerator.HUNTER))
                    {
                        // add an enemy to the list to be instantiated after the NavMeshSurface is baked
                        Vector3 pos = new Vector3(centerX, floorY, centerZ);
                        enemies.Add((tileType, pos));
                    }
                    if (tileType.Contains(MazeGenerator.PIT_TRAP))
                    {
                        // Leave hole in floor.
                    }
                    if (tileType.Contains(MazeGenerator.POOF_TRAP))
                    {
                        GameObject barrier = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                        barrier.name = MazeGenerator.POOF_TRAP;
                        barrier.transform.localScale = new Vector3(poof_trap_radius, storey_height * 0.2f, poof_trap_radius);
                        float placementRange_x = poof_trap_radius - wall_thickness_x;
                        float placementRange_z = poof_trap_radius - wall_thickness_z;
                        barrier.transform.position = new Vector3(centerX + UnityEngine.Random.Range(-placementRange_x, placementRange_x),
                                                                 wallY - (storey_height * 0.1f),
                                                                 centerZ + UnityEngine.Random.Range(-placementRange_z, placementRange_z));
                        barrier.GetComponent<Renderer>().material = POOF_TRAP_MATERIAL;
                        barrier.AddComponent<PoofTrap>();
                        barrier.GetComponent<PoofTrap>().floor = f;
                        barrier.GetComponent<PoofTrap>().storey_height = storey_height;
                        barrier.GetComponent<PoofTrap>().bounds = bounds;
                        barrier.GetComponent<PoofTrap>().maze = maze;
                        barrier.GetComponent<PoofTrap>().warpClip = poofWarpClip;
                        barrier.GetComponent<PoofTrap>().warpSource = poofWarpSource;

                        // attach the script to make poof traps pulsate
                        barrier.AddComponent<PoofTrapAnimation>();

                        // poof traps should not be considered in baking the NavMeshSurface
                        barrier.transform.SetParent(floorObj.transform);
                    }
                    if (MazeGenerator.digits.Contains(tileType) || MazeGenerator.operators.Contains(tileType))
                    {
                        Vector3 pos = new Vector3(centerX,
                                                  floorY + (playerHeight / 2),
                                                  centerZ);
                        Vector3 size = new Vector3((playerHeight / 4) / 2, (playerHeight / 4) / 2, storey_height / 50.0f);
                        // pickups should not be considered in baking the NavMeshSurface
                        createPickup(tileType, pos, size).transform.SetParent(floorObj.transform);
                    }
                    if (MazeGenerator.boosts.Contains(tileType))
                    {
                        Vector3 pos = new Vector3(centerX,
                                                  floorY + (playerHeight / 2),
                                                  centerZ);
                        Vector3 size = new Vector3((playerHeight / 4) / 2, (playerHeight / 4) / 2, storey_height / 50.0f);
                        createPickup(tileType, pos, size, MazeGenerator.boostColors[Array.IndexOf(MazeGenerator.boosts, tileType)]).transform.SetParent(floorObj.transform);
                    }
                    if (floor[i, j][MazeGenerator.directionIndex(MazeGenerator.UP)].Equals(MazeGenerator.WALL))
                    {
                        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        block.name = MazeGenerator.WALL;
                        block.transform.localScale = new Vector3(bounds.size[0] / width + RENDER_EPSILON, storey_height + RENDER_EPSILON, wall_thickness_z);
                        block.transform.position = new Vector3(centerX, wallY, upZ);
                        block.GetComponent<Renderer>().material.color = ((f == 0) && (i == generator.start.Item1) && (j == generator.start.Item1)) ? START_TILE_COLOR : floorColors[f % wallMaterials.Length];
                        block.GetComponent<Renderer>().material = wallMaterials[f % wallMaterials.Length];

                        if ((i > 0) && (j < floor.GetLength(1) - 1) &&
                            floor[i - 1, j + 1][MazeGenerator.directionIndex(MazeGenerator.RIGHT)].Equals(MazeGenerator.WALL) &&
                            !floor[i - 1, j][MazeGenerator.directionIndex(MazeGenerator.RIGHT)].Equals(MazeGenerator.WALL) &&
                            !floor[i - 1, j][MazeGenerator.directionIndex(MazeGenerator.UP)].Equals(MazeGenerator.WALL))
                        {
                            // corners should be considered in baking the NavMeshSurface
                            createCorner(new Vector3(leftX, wallY, upZ), f, widthUnitSize).transform.SetParent(navMeshSurface.transform);
                        }
                        if ((i < floor.GetLength(0) - 1) && (j < floor.GetLength(1) - 1) &&
                            floor[i + 1, j + 1][MazeGenerator.directionIndex(MazeGenerator.LEFT)].Equals(MazeGenerator.WALL) &&
                            !floor[i + 1, j][MazeGenerator.directionIndex(MazeGenerator.LEFT)].Equals(MazeGenerator.WALL) &&
                            !floor[i + 1, j][MazeGenerator.directionIndex(MazeGenerator.UP)].Equals(MazeGenerator.WALL))
                        {
                            // corners should be considered in baking the NavMeshSurface
                            createCorner(new Vector3(rightX, wallY, upZ), f, widthUnitSize).transform.SetParent(navMeshSurface.transform);
                        }
                        // wall blocks should be considered in baking the NavMeshSurface
                        block.transform.SetParent(navMeshSurface.transform);
                    }
                    if (floor[i, j][MazeGenerator.directionIndex(MazeGenerator.DOWN)].Equals(MazeGenerator.WALL))
                    {
                        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        block.name = MazeGenerator.WALL;
                        block.transform.localScale = new Vector3(bounds.size[0] / width + RENDER_EPSILON, storey_height + RENDER_EPSILON, wall_thickness_z);
                        block.transform.position = new Vector3(centerX, wallY, downZ);
                        block.GetComponent<Renderer>().material.color = ((f == 0) && (i == generator.start.Item1) && (j == generator.start.Item1)) ? START_TILE_COLOR : floorColors[f % wallMaterials.Length];
                        block.GetComponent<Renderer>().material = wallMaterials[f % wallMaterials.Length];

                        if ((i > 0) && (j > 0) &&
                            floor[i - 1, j - 1][MazeGenerator.directionIndex(MazeGenerator.RIGHT)].Equals(MazeGenerator.WALL) &&
                            !floor[i - 1, j][MazeGenerator.directionIndex(MazeGenerator.RIGHT)].Equals(MazeGenerator.WALL) &&
                            !floor[i - 1, j][MazeGenerator.directionIndex(MazeGenerator.DOWN)].Equals(MazeGenerator.WALL))
                        {
                            // corners should be considered in baking the NavMeshSurface
                            createCorner(new Vector3(leftX, wallY, downZ), f, widthUnitSize).transform.SetParent(navMeshSurface.transform);
                        }
                        if ((i < floor.GetLength(0) - 1) && (j > 0) &&
                            floor[i + 1, j - 1][MazeGenerator.directionIndex(MazeGenerator.LEFT)].Equals(MazeGenerator.WALL) &&
                            !floor[i + 1, j][MazeGenerator.directionIndex(MazeGenerator.LEFT)].Equals(MazeGenerator.WALL) &&
                            !floor[i + 1, j][MazeGenerator.directionIndex(MazeGenerator.DOWN)].Equals(MazeGenerator.WALL))
                        {
                            // corners should be considered in baking the NavMeshSurface
                            createCorner(new Vector3(rightX, wallY, downZ), f, widthUnitSize).transform.SetParent(navMeshSurface.transform);
                        }
                        // wall blocks should be considered in baking the NavMeshSurface
                        block.transform.SetParent(navMeshSurface.transform);
                    }
                    if (floor[i, j][MazeGenerator.directionIndex(MazeGenerator.RIGHT)].Equals(MazeGenerator.WALL))
                    {
                        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        block.name = MazeGenerator.WALL;
                        block.transform.localScale = new Vector3(wall_thickness_x, storey_height + RENDER_EPSILON, bounds.size[2] / length + RENDER_EPSILON);
                        block.transform.position = new Vector3(rightX, wallY, centerZ);
                        block.GetComponent<Renderer>().material.color = ((f == 0) && (i == generator.start.Item1) && (j == generator.start.Item1)) ? START_TILE_COLOR : floorColors[f % wallMaterials.Length];
                        block.GetComponent<Renderer>().material = wallMaterials[f % wallMaterials.Length];
                        // wall blocks should be considered in baking the NavMeshSurface
                        block.transform.SetParent(navMeshSurface.transform);
                    }
                    if (floor[i, j][MazeGenerator.directionIndex(MazeGenerator.LEFT)].Equals(MazeGenerator.WALL))
                    {
                        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        block.name = MazeGenerator.WALL;
                        block.transform.localScale = new Vector3(wall_thickness_x, storey_height + RENDER_EPSILON, bounds.size[2] / length + RENDER_EPSILON);
                        block.transform.position = new Vector3(leftX, wallY, centerZ);
                        block.GetComponent<Renderer>().material.color = ((f == 0) && (i == generator.start.Item1) && (j == generator.start.Item1)) ? START_TILE_COLOR : floorColors[f % wallMaterials.Length];
                        block.GetComponent<Renderer>().material = wallMaterials[f % wallMaterials.Length];
                        // wall blocks should be considered in baking the NavMeshSurface
                        block.transform.SetParent(navMeshSurface.transform);
                    }
                }
            }
            // once the entire floor has been rendered, bake that floor's NavMeshSurface
            navMeshSurface.GetComponent<NavMeshSurface>().BuildNavMesh();
            // once the floor's NavMeshSurface has been baked, instantiate all that floor's enemies
            foreach ((string tileType, Vector3 pos) in enemies)
            {
                createEnemy(tileType, pos).transform.SetParent(floorObj.transform);
            }
            enemies.Clear();
        }

        // create the player at center of maze 0,0
        fps_player_obj = Instantiate(fps_prefab);
        fps_player_obj.name = PLAYER_NAME;
        fps_player_obj.transform.position = new Vector3(0, storey_height, 0);
        fps_player_obj.transform.localScale = new Vector3(1f, 1f, 1f);
        fps_player_obj.AddComponent<Inventory>();
        fps_player_obj.GetComponent<Inventory>().canvas = canvas;
        fps_player_obj.GetComponent<Inventory>().itemPrefab = inventoryItemPrefab;
        foreach (GameObject pickup in nullFacedPickups)
        {
            pickup.GetComponent<PickUp>().thingToFace = fps_player_obj;
        }
    }

    // Update is called once per frame
    void Update()
    {
        minimaps[Mathf.Max(0, (int)(fps_player_obj.transform.position.y / storey_height))].imgObject.GetComponent<RectTransform>().SetAsLastSibling();
        playerPointer.transform.SetAsLastSibling();
        playerPointer.GetComponent<RectTransform>().anchoredPosition = new Vector2((-minimapWidth / 2.0f) * (1 + (fps_player_obj.transform.position.x / bounds.max[0])), (-minimapHeight / 2.0f) * (1 + (fps_player_obj.transform.position.z / bounds.max[2])));
        playerPointer.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, (180 - fps_player_obj.transform.rotation.eulerAngles.y) % 360.0f);
        if (playerHealth <= 0)
        {
            // DISPLAY LOSS SCREEN
            canvasManager.showLossScreen();
        }
        speedBoostTimer -= Time.deltaTime;
        if (speedBoostTimer <= 0.0f)
        {
            speedBoostTimer = 0.0f;
            playerSpeedModifier = 1.0f;
        }
        powerBoostTimer -= Time.deltaTime;
        if(powerBoostTimer <= 0.0f)
        {
            powerBoostTimer = 0.0f;
        }
        RigidbodyFirstPersonController controller = fps_player_obj.GetComponent<RigidbodyFirstPersonController>();
        controller.movementSettings.RunMultiplier = playerSpeedModifier;
        if (playerSpeedModifier != 1.0f)
        {
            controller.movementSettings.dashing = true;
        }
        else
        {
            controller.movementSettings.dashing = false;
        }

        renderHealthBar();
    }

    void renderHealthBar()
    {
        playerHealth = Mathf.Clamp(playerHealth, 0, playerStartingHealth);
        float newWidth = (float)playerHealth / (float)playerStartingHealth * healthBarWidth;
        healthBar.sizeDelta = new Vector2(newWidth, healthBarHeight);
    }


}
