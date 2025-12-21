using System;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameObject[] mapTile;
    [SerializeField] private GameObject level;

    public float TileSize 
    { 
        get { return mapTile[0].transform.GetChild(0).GetComponent<SpriteRenderer>().sprite.bounds.size.x; } 
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateLevel();
    }

    private void CreateLevel()
    {
        // 작성한 텍스트 파일 읽어들임
        string[] mapData = ReadLevelText();

        // 맵 사이즈
        int mapX = mapData[0].ToCharArray().Length;
        int mapY = mapData.Length;

        Camera cam = Camera.main;

        // 타일이 놓일 z=0 평면 기준으로 ScreenToWorldPoint 깊이 설정
        float planeZ = 0f;
        float depth = planeZ - cam.transform.position.z; // 보통 카메라 z가 -10이면 depth는 10

        Vector3 worldCenter = cam.ScreenToWorldPoint(
            new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, depth)
        );

        float mapWidth = mapX * TileSize;
        float mapHeight = mapY * TileSize;

        // "좌상단 시작점" (x는 왼쪽으로 반폭, y는 위로 반높이)
        Vector3 worldStart = new Vector3(
            worldCenter.x - mapWidth * 0.5f + TileSize * 0.5f,
            worldCenter.y + mapHeight * 0.5f - TileSize * 0.5f,
            planeZ
        );

        for (int y = 0; y < mapY; y++)
        {
            char[] newTiles = mapData[y].ToCharArray();

            for (int x = 0; x < mapX; x++)
            {
                //newTiles의 x번째 요소인 타일을 worldStart로부터 x,y만큼 떨어진 곳에 위치하도록 함
                PlaceTile(newTiles[x].ToString(), x, y, worldStart);
            }
        }
    }

    private void PlaceTile(string tileType, int x, int y, Vector3 worldStart)
    {
        int tileIndex = int.Parse(tileType);

        if (tileIndex == (int)MAP_STATE.EMPTY)
        {

        }
        else
        {
            GameObject newTile = Instantiate(mapTile[tileIndex], level.transform, default);
            newTile.transform.position = new Vector3(worldStart.x + (TileSize * x), worldStart.y - (TileSize * y), 0);
        }
    }

    private string[] ReadLevelText()
    {
        TextAsset bindData = Resources.Load("FirstLevel") as TextAsset;

        string data = bindData.text.Replace(Environment.NewLine, string.Empty);

        return data.Split('-');
    }
}
