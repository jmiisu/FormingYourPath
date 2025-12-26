using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameObject[] mapTile;
    [SerializeField] private GameObject level;

    // (선택) 인스펙터에서 플레이어를 물려도 되고, 없으면 Find로 찾음
    [SerializeField] private MoveController playerMove;

    // LevelManager가 소유하는 맵 상태
    public Dictionary<Vector2Int, MAP_STATE> GridMap { get; private set; } = new Dictionary<Vector2Int, MAP_STATE>();

    // MoveController가 참조할 맵 배치 기준점(정중앙 정렬 결과)
    public Vector3 WorldStart { get; private set; }

    // 스폰 셀
    public Vector2Int SpawnCell { get; private set; }
    public bool HasSpawnCell { get; private set; }

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

        int mapX = mapData[0].ToCharArray().Length;
        int mapY = mapData.Length;

        Camera cam = Camera.main;

        float planeZ = 0f;
        float depth = planeZ - cam.transform.position.z;

        Vector3 worldCenter = cam.ScreenToWorldPoint(
            new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, depth)
        );

        float mapWidth = mapX * TileSize;
        float mapHeight = mapY * TileSize;

        // 맵이 화면 정중앙에 오도록 하는 시작점(좌상단 기준점)
        Vector3 worldStart = new Vector3(
            worldCenter.x - mapWidth * 0.5f + TileSize * 0.5f,
            worldCenter.y + mapHeight * 0.5f - TileSize * 0.5f,
            planeZ
        );
        WorldStart = worldStart;

        GridMap.Clear();
        HasSpawnCell = false;

        for (int y = 0; y < mapY; y++)
        {
            char[] newTiles = mapData[y].ToCharArray();

            for (int x = 0; x < mapX; x++)
            {
                PlaceTile(newTiles[x].ToString(), x, y);
            }
        }
    }

    private void PlaceTile(string tileType, int x, int y)
    {
        int tileIndex = int.Parse(tileType);
        MAP_STATE state = (MAP_STATE)tileIndex;

        Vector2Int cell = new Vector2Int(x, y);

        // PLAYER_POS면: 스폰셀만 저장하고, 맵 데이터는 EMPTY로 기록 (플레이어는 동적 엔티티)
        if (state == MAP_STATE.PLAYER_POS)
        {
            SpawnCell = cell;
            HasSpawnCell = true;

            GridMap[cell] = MAP_STATE.EMPTY;
            return;
        }

        // gridMap 기록
        GridMap[cell] = state;

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

    // MoveController가 묻는 이동 가능 여부 (규칙은 여기로 모으기)
    public bool IsWalkable(Vector2Int cell)
    {
        if (!GridMap.TryGetValue(cell, out var state))
            return false; // 맵 밖

        // 최소 규칙: 막힌 블록은 이동 불가
        if (state == MAP_STATE.STAGE_BLOCK) return false;
        if (state == MAP_STATE.BASIC) return false;

        // EMPTY/EXIT/STAIR 등은 이동 가능
        return true;
    }

    private void ApplyPlayerSpawn()
    {
        if (!HasSpawnCell)
        {
            Debug.LogError("맵 텍스트에 PLAYER_POS가 없습니다.");
            return;
        }

        if (playerMove == null)
            playerMove = FindAnyObjectByType<MoveController>();

        if (playerMove == null)
        {
            Debug.LogError("MoveController(플레이어)를 찾지 못했습니다. LevelManager.playerMove를 연결해줘.");
            return;
        }

        // 시작 셀을 넘겨서 플레이어 위치를 맵 좌표계에 맞게 스냅
        playerMove.SetStartCell(new Vector3Int(SpawnCell.x, SpawnCell.y, 0), this);
    }

    private string[] ReadLevelText()
    {
        TextAsset bindData = Resources.Load("FirstLevel") as TextAsset;

        string data = bindData.text.Replace(Environment.NewLine, string.Empty);

        return data.Split('-');
    }
}
