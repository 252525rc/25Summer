using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI GameOvertxt;

    // Start is called before the first frame update
    void Start()
    {
        GameOvertxt.gameObject.SetActive(false);

    }

   public void Gameover()
    {
        GameOvertxt.gameObject.SetActive(true);
    }
}
