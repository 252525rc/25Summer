using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallColor : MonoBehaviour
{
    public Color wall;
    public PlayerMove playerController;
    public int x, y; // ���̕ǂ��Ή����� maze �̈ʒu
    // Start is called before the first frame update
    void Start()
    {
        wall = GetComponent<Renderer>().material.color;
        playerController = FindObjectOfType<PlayerMove>();

        if (playerController == null)
        {
            Debug.LogError("PlayerController ��������܂���B");
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
        // �O���[�o���� maze �z����Q�Ƃ�����@�͌�q
        MazeGenerator.maze[x, y] = 0;
        Destroy(this.gameObject); // �ǂ�j��
    }
}
