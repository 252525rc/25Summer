using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Retry : MonoBehaviour
{
    public GameObject retry; 

    // Start is called before the first frame update
    void Start()
    {
       // retry.SetActive(false);
    }

    public void RetryGame()
    {
        Time.timeScale = 1f; // ゲームが停止していたら再開
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // 現在のシーンを再読込
    }
}
