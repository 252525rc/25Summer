using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControll : MonoBehaviour
{

    private GameObject Camera1;      //上から視点のカメラ格納用
    private GameObject Camera2;       //キャラターを映すカメラ格納用 

    // micro:bitのボタンの状態(0: なし、1: Aボタン、-1: Bボタン)
    private int buttonState = 0;

    //呼び出し時に実行される関数
    void Start()
    {
        //それぞれのカメラを取得
        Camera1 = GameObject.Find("Camera1");
        Camera2 = GameObject.Find("Camera2");

        //上から視点のカメラを非アクティブにする
        Camera1.SetActive(false);
    }


    //単位時間ごとに実行される関数
    void Update()
    {
        //スペースキーまたはBボタンが押されている間、上から視点のカメラをアクティブにする
        if (Input.GetKey("space") || buttonState == -1)
        {
            //上から視点のカメラをアクティブに設定
            Camera2.SetActive(false);
            Camera1.SetActive(true);
        }
        else
        {
            //キャラターを映すカメラをアクティブに設定
            Camera1.SetActive(false);
            Camera2.SetActive(true);
        }
    }

    //Bluetooth接続の信号を取得
    public void OnButtonBChanged(int state)
    {
        buttonState = (state == 0 ? 0 : -1);
    }
}