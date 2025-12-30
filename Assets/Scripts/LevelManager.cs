using System;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameObject[] mapTile;
    [SerializeField] private GameObject level;
    [SerializeField] private MoveController playerMove;


    // MoveController가 참조할 맵 배치 기준점(정중앙 정렬 결과)
    public Vector3 WorldStart { get; private set; }

    // 스폰 셀
    public Vector2Int SpawnCell { get; private set; }
    public bool HasSpawnCell { get; private set; }

    private int _mapWidth;
    private int _mapHeight;

    public float TileSize
    {
        get { return mapTile[0].transform.GetChild(0).GetComponent<SpriteRenderer>().sprite.bounds.size.x; }
    }

    void Start()
    {
        CreateLevel();
        ApplyPlayerSpawn();
    }

    private void CreateLevel()
    {
        string[] mapData = ReadLevelText();

        _mapWidth = mapData[0].ToCharArray().Length;
        _mapHeight = mapData.Length;

        Camera cam = Camera.main;

        float planeZ = 0f;
        float depth = planeZ - cam.transform.position.z;

        Vector3 worldCenter = cam.ScreenToWorldPoint(
            new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, depth)
        );

        float mapWidthWorld = _mapWidth * TileSize;
        float mapHeightWorld = _mapHeight * TileSize;

        // 맵이 화면 정중앙에 오도록 하는 시작점(좌상단 기준점)
        // y좌표에 -0.5f를 함으로써 그리드 내부로 맞춤
        Vector3 worldStart = new Vector3(
            worldCenter.x - mapWidthWorld * 0.5f + TileSize * 0.5f,
            worldCenter.y + mapHeightWorld * 0.5f - TileSize * 0.5f - 0.5f,
            planeZ
        );


        WorldStart = worldStart;

        var initialMap = new Dictionary<Vector2Int, MAP_STATE>(_mapWidth * _mapHeight);
        HasSpawnCell = false;

        for (int y = 0; y < _mapHeight; y++)
        {
            char[] newTiles = mapData[y].ToCharArray();

            for (int x = 0; x < _mapWidth; x++)
            {
                PlaceTile(newTiles[x].ToString(), x, y, initialMap);
            }
        }

        if (GridStateManager.i != null)
        {
            GridStateManager.i.Init(initialMap, _mapWidth, _mapHeight);
        }
        else
        {
            Debug.LogWarning("GridStateManager가 씬에 없습니다. IsWalkable 조회가 동작 X");
        }

    }

    private void PlaceTile(string tileType, int x, int y, Dictionary<Vector2Int, MAP_STATE> initialMap)
    {
        int tileIndex = int.Parse(tileType);
        MAP_STATE state = (MAP_STATE)tileIndex;

        Vector2Int cell = new Vector2Int(x, y);

        // PLAYER_POS면: 스폰셀만 저장하고, 맵 데이터는 EMPTY로 기록 (플레이어는 동적 엔티티)
        if (state == MAP_STATE.PLAYER_POS)
        {
            SpawnCell = cell;
            HasSpawnCell = true;

            initialMap[cell] = MAP_STATE.EMPTY;
            return;
        }

        // 맵 상태 기록
        initialMap[cell] = state;

        // 기존 로직 유지: EMPTY는 생성 안 함
        if (state == MAP_STATE.EMPTY) return;

        GameObject newTile = Instantiate(mapTile[tileIndex], level.transform, default);

        // 기존 중앙정렬 좌표계 유지
        newTile.transform.position = new Vector3(
            WorldStart.x + (TileSize * x),
            WorldStart.y - (TileSize * y),
            0
        );
    }

    private void ApplyPlayerSpawn()
    {
        if (!HasSpawnCell)
        {
            Debug.LogError("맵 텍스트에 PLAYER_POS가 없습니다.");
            return;
        }

        if (playerMove == null)
        {
            playerMove = FindAnyObjectByType<MoveController>();
        }

        if (playerMove == null)
        {
            Debug.LogError("MoveController(플레이어)를 찾지 못했습니다. LevelManager.playerMove를 연결해줘.");
            return;
        }

        // 시작 셀을 넘겨서 플레이어 위치를 맵 좌표계에 맞게 스냅
        playerMove.SetStartCell(new Vector2Int(SpawnCell.x, SpawnCell.y), this);
    }

    private string[] ReadLevelText()
    {
        TextAsset bindData = Resources.Load("LevelText\\Tutorial_2") as TextAsset;

        string data = bindData.text.Replace(Environment.NewLine, string.Empty);

        return data.Split('-');
    }
}
