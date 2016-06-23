using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Button_options : MonoBehaviour {
    public Text button_tool;
    public GameObject panel_tool;
    public Toggle toggle_learn;
    public Toggle toggle_G_M_options;


    public void button_options_dropdown()
    {
        panel_tool.gameObject.SetActive(false);

        if (button_tool.text == "选项")
        {
            panel_tool.gameObject.SetActive(true);
            button_tool.text = "收起";
        }
        else {
            panel_tool.gameObject.SetActive(false);
            button_tool.text = "选项";
            toggle_learn.isOn=(false);
            toggle_G_M_options.isOn=(false);
        }


    }

}
