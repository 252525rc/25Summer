using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallColor : MonoBehaviour
{
    public Color wall;
    public PlayerMove playerController;
    public int x, y; // この壁が対応する maze の位置
    // Start is called before the first frame update
    void Start()
    {
        wall = GetComponent<Renderer>().material.color;
        playerController = FindObjectOfType<PlayerMove>();

        if (playerController == null)
        {
            Debug.LogError("PlayerController が見つかりません。");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Sword" && playerController.Attack && playerController.brokenCount > 0)
        {
            
            switch (playerController.maxAttack)
            {
                case 1:
                    GetComponent<Renderer>().material.color = new Color(0f, 0f, 0f, 1f);
                    break;
                case 2:
                    GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 0.5f,1f);
                    break;
                case 3:
                    GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.3f, 1f);
                    Break();
                    break;
            }
        }
    }
    public void Break()
    {
        // グローバルの maze 配列を参照する方法は後述
        MazeGenerator.maze[x, y] = 0;
        Destroy(this.gameObject); // 壁を破壊
    }
}
