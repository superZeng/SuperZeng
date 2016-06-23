using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_optins : MonoBehaviour {
    public Text capton_text;  
    public GameObject penal; 

    public void option_ex()
    {
        if (capton_text.text == "连接状态")
        {
            penal.gameObject.SetActive(true);
            capton_text.text = "返回";
        }
        else {
            penal.gameObject.SetActive(false);
            capton_text.text = "连接状态";
            
        }

    }

   
}
