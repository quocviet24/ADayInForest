using UnityEngine;
using System.Collections.Generic;

public class AutoMapGenerator : MonoBehaviour
{
    [Header("1. Kích thước bản đồ tùy ý")]
    public int width = 50;
    public int height = 34;

    [Header("2. Thiết lập con đường mòn")]
    [Range(1, 3)]
    public int roadWidth = 2;

    [Header("3. Mật độ rừng cây")]
    [Range(0.2f, 0.6f)]
    public float forestDensity = 0.40f;

    [Header("4. Bề rộng dòng sông")]
    [Range(2, 4)]
    public int riverWidth = 3;

    [Header("5. Sprite Nền")]
    public Sprite grassSprite;
    public Sprite pathSprite;
    public Sprite waterSprite;

    [Header("6. Sprite Cầu & Đá sông")]
    public Sprite bridgeSprite;
    public Sprite riverRockSprite;
    public Sprite triggerIconSprite;

    [Header("7. Cây cối & Vật cản")]
    public Sprite[] treeObstacles;

    [Header("8. Ngôi nhà Trạm Kiểm Lâm")]
    public Sprite houseSprite;

    [Header("9. Tiểu cảnh môi trường riêng (Q12 -> Q19)")]
    public Sprite animalSprite;
    public Sprite smokeFireSprite;
    public Sprite lostPersonProps;
    public Sprite stumpLogSprite;
    public Sprite crateTableSprite;
    public Sprite signPostSprite;
    public Sprite backpackSprite;

    [Header("10. Danh sách 10 tình huống Màn 2 (Q11 -> Q20)")]
    public string[] scenarioNames = new string[]
    {
        "Trigger_Q11_TroiMuaLon",
        "Trigger_Q12_ThuBiThuong",
        "Trigger_Q13_KhoiTrongRung",
        "Trigger_Q14_NguoiBiLac",
        "Trigger_Q15_DauHieuViPham",
        "Trigger_Q16_AmThanhBatThuong",
        "Trigger_Q17_BoDamLienLac",
        "Trigger_Q18_BanDoTuanTra",
        "Trigger_Q19_BaLoKiemLam",
        "Trigger_Q20_TramKiemLamReflection"
    };

    [ContextMenu("Generate Map 2")]
    public void GenerateMap()
    {
        ClearMap();

        Transform groundParent = new GameObject("Ground_Layer").transform;
        groundParent.SetParent(this.transform);

        Transform riverParent = new GameObject("River_Layer").transform;
        riverParent.SetParent(this.transform);

        Transform obstacleParent = new GameObject("Obstacles_Layer").transform;
        obstacleParent.SetParent(this.transform);

        Transform triggerParent = new GameObject("--- INTERACTION_TRIGGERS ---").transform;
        triggerParent.SetParent(this.transform);

        Transform propsParent = new GameObject("Scenario_Props_Layer").transform;
        propsParent.SetParent(this.transform);

        HashSet<Vector2Int> roadCoords = new HashSet<Vector2Int>();
        HashSet<Vector2Int> riverCoords = new HashSet<Vector2Int>();
        HashSet<Vector2Int> bridgeCorridor = new HashSet<Vector2Int>();

        int halfW = width / 2;
        int halfH = height / 2;

        int bridgeSouthY = -Mathf.RoundToInt(halfH * 0.22f);
        int bridgeNorthY = Mathf.RoundToInt(halfH * 0.52f);
        Vector2Int bridgeSouthPos = new Vector2Int(0, bridgeSouthY);
        Vector2Int bridgeNorthPos = new Vector2Int(0, bridgeNorthY);

        for (int y = -halfH; y <= halfH; y++)
        {
            int curveX = 0;
            bool nearBridge = Mathf.Abs(y - bridgeSouthY) <= 2 || Mathf.Abs(y - bridgeNorthY) <= 2;
            if (!nearBridge)
            {
                curveX = Mathf.RoundToInt(Mathf.Sin(y * 0.2f) * 1.8f);
            }

            for (int rx = -riverWidth / 2; rx <= riverWidth / 2; rx++)
            {
                riverCoords.Add(new Vector2Int(curveX + rx, y));
            }
        }

        Vector2 pStart = new Vector2(-halfW + 4, -halfH + 4);
        Vector2 pFork = new Vector2(-halfW * 0.5f, -halfH * 0.15f);
        Vector2 pMerge = new Vector2(halfW * 0.5f, halfH * 0.35f);
        Vector2 pEndHouse = new Vector2(halfW - 6, halfH - 6);

        List<Vector2> pathSouth = new List<Vector2>
        {
            pStart,
            new Vector2(-halfW * 0.75f, -halfH * 0.4f),
            pFork,
            new Vector2(-riverWidth - 2, bridgeSouthY),
            new Vector2(-riverWidth / 2f - 0.5f, bridgeSouthY)
        };

        List<Vector2> pathSouthExit = new List<Vector2>
        {
            new Vector2(riverWidth / 2f + 0.5f, bridgeSouthY),
            new Vector2(riverWidth + 2, bridgeSouthY),
            new Vector2(halfW * 0.25f, -halfH * 0.05f),
            pMerge,
            pEndHouse
        };

        List<Vector2> pathNorth = new List<Vector2>
        {
            pFork,
            new Vector2(-halfW * 0.45f, halfH * 0.25f),
            new Vector2(-halfW * 0.2f, halfH * 0.52f),
            new Vector2(-riverWidth - 2, bridgeNorthY),
            new Vector2(-riverWidth / 2f - 0.5f, bridgeNorthY)
        };

        List<Vector2> pathNorthExit = new List<Vector2>
        {
            new Vector2(riverWidth / 2f + 0.5f, bridgeNorthY),
            new Vector2(riverWidth + 2, bridgeNorthY),
            new Vector2(halfW * 0.25f, halfH * 0.65f),
            new Vector2(halfW * 0.42f, halfH * 0.48f),
            pMerge
        };

        DrawConnectedPath(pathSouth, roadCoords);
        DrawConnectedPath(pathSouthExit, roadCoords);
        DrawConnectedPath(pathNorth, roadCoords);
        DrawConnectedPath(pathNorthExit, roadCoords);

        RegisterBridgeWalkway(bridgeSouthPos, roadCoords, bridgeCorridor);
        RegisterBridgeWalkway(bridgeNorthPos, roadCoords, bridgeCorridor);

        Vector2Int[] stations = new Vector2Int[10];
        stations[0] = Vector2Int.RoundToInt(pStart);
        stations[1] = Vector2Int.RoundToInt(new Vector2(-halfW * 0.75f, -halfH * 0.4f));

        stations[2] = new Vector2Int(-riverWidth - 3, bridgeSouthY);
        stations[3] = new Vector2Int(-riverWidth - 1, bridgeSouthY);
        stations[6] = new Vector2Int(riverWidth + 2, bridgeSouthY);

        stations[4] = Vector2Int.RoundToInt(new Vector2(-halfW * 0.45f, halfH * 0.25f));
        stations[5] = new Vector2Int(-riverWidth - 1, bridgeNorthY);
        stations[7] = Vector2Int.RoundToInt(new Vector2(halfW * 0.25f, halfH * 0.65f));

        stations[8] = Vector2Int.RoundToInt(pMerge);
        stations[9] = Vector2Int.RoundToInt(pEndHouse);

        for (int i = 0; i < scenarioNames.Length; i++)
        {
            Vector2Int pos = stations[i];

            for (int rx = -roadWidth; rx <= roadWidth; rx++)
            {
                for (int ry = -roadWidth; ry <= roadWidth; ry++)
                {
                    roadCoords.Add(new Vector2Int(pos.x + rx, pos.y + ry));
                }
            }

            GameObject trg = new GameObject(scenarioNames[i]);
            trg.transform.position = new Vector3(pos.x, pos.y, 0);
            trg.transform.SetParent(triggerParent);

            CircleCollider2D col = trg.AddComponent<CircleCollider2D>();
            col.radius = 1.3f;
            col.isTrigger = true;

            Sprite specificProp = null;
            switch (i)
            {
                case 1: specificProp = animalSprite; break;
                case 2: specificProp = smokeFireSprite; break;
                case 3: specificProp = lostPersonProps; break;
                case 4: specificProp = stumpLogSprite; break;
                case 5: specificProp = riverRockSprite; break;
                case 6: specificProp = crateTableSprite; break;
                case 7: specificProp = signPostSprite; break;
                case 8: specificProp = backpackSprite; break;
            }

            SpriteRenderer sr = trg.AddComponent<SpriteRenderer>();
            sr.sprite = specificProp != null ? specificProp : (triggerIconSprite != null ? triggerIconSprite : riverRockSprite);
            sr.color = specificProp != null ? Color.white : new Color(1f, 0.95f, 0.35f, 1f);
            sr.sortingOrder = 10;

            if (specificProp != null)
            {
                GameObject decor = new GameObject($"Decor_{scenarioNames[i]}");
                decor.transform.position = new Vector3(pos.x + 0.8f, pos.y + 0.4f, 0);
                decor.transform.SetParent(propsParent);
                SpriteRenderer dsr = decor.AddComponent<SpriteRenderer>();
                dsr.sprite = (riverRockSprite != null) ? riverRockSprite : specificProp;
                dsr.sortingOrder = 4;
            }
        }

        for (int x = -halfW; x <= halfW; x++)
        {
            for (int y = -halfH; y <= halfH; y++)
            {
                Vector2Int currentCoord = new Vector2Int(x, y);
                bool isBridgeTile = bridgeCorridor.Contains(currentCoord);
                bool isRiver = riverCoords.Contains(currentCoord);
                bool isRoad = roadCoords.Contains(currentCoord);

                if (isRiver)
                {
                    GameObject waterTile = new GameObject($"Water_{x}_{y}");
                    waterTile.transform.position = new Vector3(x, y, 0);
                    waterTile.transform.SetParent(riverParent);
                    SpriteRenderer sr = waterTile.AddComponent<SpriteRenderer>();
                    sr.sprite = (waterSprite != null) ? waterSprite : grassSprite;
                    sr.sortingOrder = -2;

                    if (!isBridgeTile)
                    {
                        BoxCollider2D col = waterTile.AddComponent<BoxCollider2D>();
                        col.size = Vector2.one;

                        if (riverRockSprite != null && Random.value < 0.15f)
                        {
                            GameObject rock = new GameObject($"RiverRock_{x}_{y}");
                            rock.transform.position = new Vector3(x, y, 0);
                            rock.transform.SetParent(riverParent);
                            SpriteRenderer rsr = rock.AddComponent<SpriteRenderer>();
                            rsr.sprite = riverRockSprite;
                            rsr.sortingOrder = -1;
                        }
                    }
                }
                else
                {
                    GameObject tile = new GameObject(isRoad ? $"Path_{x}_{y}" : $"Grass_{x}_{y}");
                    tile.transform.position = new Vector3(x, y, 0);
                    tile.transform.SetParent(groundParent);
                    SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
                    sr.sprite = (isRoad && pathSprite != null) ? pathSprite : grassSprite;
                    sr.sortingOrder = -3;
                }

                bool isBorder = (x <= -halfW + 1 || x >= halfW - 1 || y <= -halfH + 1 || y >= halfH - 1);
                if (isBorder && treeObstacles != null && treeObstacles.Length > 0)
                {
                    SpawnTree(x, y, obstacleParent, true);
                }
                else if (!isRoad && !isRiver && !isBridgeTile && treeObstacles != null && treeObstacles.Length > 0)
                {
                    if (Random.value < forestDensity)
                    {
                        SpawnTree(x, y, obstacleParent, false);
                    }
                }
            }
        }

        if (bridgeSprite != null)
        {
            PlaceFittedBridge(bridgeSouthPos, "Wooden_Bridge_South", groundParent);
            PlaceFittedBridge(bridgeNorthPos, "Wooden_Bridge_North", groundParent);
        }

        if (houseSprite != null)
        {
            GameObject house = new GameObject("Ranger_Station_House");
            house.transform.position = new Vector3(pEndHouse.x + 1.5f, pEndHouse.y + 1.5f, 0);
            house.transform.SetParent(obstacleParent);
            SpriteRenderer sr = house.AddComponent<SpriteRenderer>();
            sr.sprite = houseSprite;
            sr.sortingOrder = 5;

            BoxCollider2D col = house.AddComponent<BoxCollider2D>();
            col.size = new Vector2(2.2f, 1.4f);
            col.offset = new Vector2(0, -0.6f);
        }
    }

    private void RegisterBridgeWalkway(Vector2Int bridgePos, HashSet<Vector2Int> roadSet, HashSet<Vector2Int> bridgeSet)
    {
        int halfRiver = riverWidth / 2;

        for (int x = bridgePos.x - halfRiver; x <= bridgePos.x + halfRiver; x++)
        {
            for (int y = bridgePos.y - 1; y <= bridgePos.y + 1; y++)
            {
                bridgeSet.Add(new Vector2Int(x, y));
            }
        }

        for (int y = bridgePos.y - 1; y <= bridgePos.y + 1; y++)
        {
            roadSet.Add(new Vector2Int(bridgePos.x - halfRiver - 1, y));
            roadSet.Add(new Vector2Int(bridgePos.x + halfRiver + 1, y));
        }
    }

    private void PlaceFittedBridge(Vector2Int pos, string name, Transform parent)
    {
        GameObject bridge = new GameObject(name);
        bridge.transform.position = new Vector3(pos.x, pos.y, 0);
        bridge.transform.SetParent(parent);

        bridge.transform.rotation = Quaternion.identity;
        bridge.transform.localScale = Vector3.one;

        SpriteRenderer sr = bridge.AddComponent<SpriteRenderer>();
        sr.sprite = bridgeSprite;
        sr.sortingOrder = 2;
    }

    private void DrawConnectedPath(List<Vector2> waypoints, HashSet<Vector2Int> roadSet)
    {
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            Vector2 a = waypoints[i];
            Vector2 b = waypoints[i + 1];
            float dist = Vector2.Distance(a, b);
            int steps = Mathf.Max(Mathf.RoundToInt(dist * 6f), 10);

            for (int s = 0; s <= steps; s++)
            {
                float t = (float)s / steps;
                Vector2 pos = Vector2.Lerp(a, b, t);

                Vector2Int cell = new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
                for (int rx = -roadWidth / 2; rx <= roadWidth / 2; rx++)
                {
                    for (int ry = -roadWidth / 2; ry <= roadWidth / 2; ry++)
                    {
                        roadSet.Add(new Vector2Int(cell.x + rx, cell.y + ry));
                    }
                }
            }
        }
    }

    private void SpawnTree(int x, int y, Transform parent, bool isBorder)
    {
        GameObject tree = new GameObject(isBorder ? $"BorderTree_{x}_{y}" : $"Tree_{x}_{y}");
        tree.transform.position = new Vector3(x, y, 0);
        tree.transform.SetParent(parent);

        SpriteRenderer sr = tree.AddComponent<SpriteRenderer>();
        sr.sprite = treeObstacles[Random.Range(0, treeObstacles.Length)];
        sr.sortingOrder = 3;

        BoxCollider2D col = tree.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.85f, 0.85f);
    }

    [ContextMenu("Clear Map")]
    public void ClearMap()
    {
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }
}