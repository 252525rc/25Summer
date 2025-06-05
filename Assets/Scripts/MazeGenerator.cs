using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    //迷路の縦と横の大きさ(奇数推奨)
    public int width = 31; 
    public int height = 31;

    //壁と床を作るための材料
    public GameObject wallPrefab;
    public GameObject floorPrefab;

    //ゴールと敵を作る材料
    public GameObject goalPrefab;
    public GameObject enemyPrefab;

    //マス目1つの大きさ
    public float cellSize = 2f;
    public Transform playerTransform;

    //スタートとゴールのマスの座標
    public Vector2Int startPos;
    private Vector2Int goalPos;

    //敵のいる場所をリストで管理
    private List<Vector2Int> enemyPositions = new List<Vector2Int>();

    //迷路の情報(0 = 通路、1 = 壁)
    public static int[,] maze;

    //他のスクリプトから迷路や場所を取り出せるようにする
    public int[,] GetMaze() => maze;
    public Vector2Int GetStartPos() => startPos;
    public Vector2Int GetGoalPos() => goalPos;


    private void Awake()
    {
        GenerateMaze();
        SetStartGoalAndEnemies();
        DrawMaze3D();

    }

    //迷路を作る関数
    void GenerateMaze()
    {
        maze = new int[width, height];

        //まず全てを「壁」にする
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                maze[x, y] = 1; // 全部壁にして初期化

        //通路を(1, 1)から掘り始める
        RecursiveDig(1, 1);
    }

    //通路を掘る関数
    void RecursiveDig(int x, int y)
    {
        maze[x, y] = 0; // 今いるマスを通路にする
        List<Vector2Int> directions = new List<Vector2Int>
        {
            new Vector2Int(2, 0), //右
            new Vector2Int(-2, 0), //左
            new Vector2Int(0, 2), //上
            new Vector2Int(0, -2) //下
        };

        Shuffle(directions);//毎回ランダムな順番で掘る

        //それぞれの方向に掘っていく
        foreach (var dir in directions)
        {
            int nx = x + dir.x;
            int ny = y + dir.y;

            //迷路内であり、まだ壁なら
            if (IsInBounds(nx, ny) && maze[nx, ny] == 1)
            {
                //壁を1マス分だけ壊す
                maze[x + dir.x / 2, y + dir.y / 2] = 0; 
                //その先にまた進む(繰り返し)
                RecursiveDig(nx, ny);
            }
        }
    }

    //作った迷路に壁や床を配置
    void DrawMaze3D()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(x * cellSize, 0, y * cellSize);

                if (floorPrefab != null)
                {
                    Instantiate(floorPrefab, pos + Vector3.down * 1.5f, Quaternion.identity, transform);
                }

                if (maze[x, y] == 1) // 壁
                {
                    GameObject wall = Instantiate(wallPrefab, pos, Quaternion.identity, transform);

                    // 外枠かどうかを判定
                    bool isOuterWall = (x == 0 || x == width - 1 || y == 0 || y == height - 1);

                    if (isOuterWall)
                    {
                        // 外枠の場合は BoxCollider を外す
                        BoxCollider collider = wall.GetComponent<BoxCollider>();
                        if (collider != null)
                        {
                            Destroy(collider); // この壁から BoxCollider を削除
                        }
                    }

                    WallColor script = wall.GetComponent<WallColor>();
                    if (script != null)
                    {
                        script.x = x;
                        script.y = y;
                    }
                }

            }
        }

        // ゴールの設置
        if (goalPrefab != null)
        {
            Vector3 gpos = new Vector3(goalPos.x * cellSize, 0f, goalPos.y * cellSize);
            Instantiate(goalPrefab, gpos, Quaternion.identity, transform);
        }

        // 敵の生成
        foreach (Vector2Int enemyPos in enemyPositions)
        {
            Vector3 epos = new Vector3(enemyPos.x * cellSize, 0.5f, enemyPos.y * cellSize);
            GameObject enemy = Instantiate(enemyPrefab, epos, Quaternion.identity, transform);

            //敵の動きを制御するスクリプトを付ける
            var controller = enemy.GetComponent<EnemyController>();
            if (controller == null)
            {
                controller = enemy.AddComponent<EnemyController>();
            }

            //プレイヤーの位置や迷路の情報を渡す
            controller.mazeGen = this;
            controller.player = playerTransform; 
        }

    }

    void Shuffle(List<Vector2Int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            var temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }

    bool IsInBounds(int x, int y)
    {
        return x > 0 && x < width - 1 && y > 0 && y < height - 1;
    }

    void SetStartGoalAndEnemies()
    {
        // 4つの角をリストに追加してシャッフル
        List<Vector2Int> corners = new List<Vector2Int>
        {
            new Vector2Int(1, 1),
            new Vector2Int(1, height - 2),
            new Vector2Int(width - 2, 1),
            new Vector2Int(width - 2, height - 2)
        };

        Shuffle(corners);

        startPos = corners[0];
        goalPos = corners[1];

        // 残りの2つの角に敵を配置
        enemyPositions.Add(corners[2]);
        enemyPositions.Add(corners[3]);

        // 通路にする
        maze[startPos.x, startPos.y] = 0;
        maze[goalPos.x, goalPos.y] = 0;
        foreach (var pos in enemyPositions)
        {
            maze[pos.x, pos.y] = 0;
        }
    }
}
