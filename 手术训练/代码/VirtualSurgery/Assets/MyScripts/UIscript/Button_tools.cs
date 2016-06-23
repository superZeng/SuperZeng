using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Button_tools : MonoBehaviour
{
    public Text button_tool;
    public GameObject panel_tool;

    public void button_tool_dropdown()
    {
        panel_tool.gameObject.SetActive(false);

        if (button_tool.text == "手术工具")
        {
            panel_tool.gameObject.SetActive(true);
            button_tool.text = "收起";
        }
        else {
            panel_tool.gameObject.SetActive(false);
            button_tool.text = "手术工具";
        }


    }
}


