using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class Tools_toggle : MonoBehaviour {
    public Toggle toggle;
    public GameObject tools;
    void Start()
    {
        //tools = GameObject.Find("");
        tools.gameObject.SetActive(false);
    }
    public void show_tool() 
    {
        if (toggle.isOn) {
            tools.gameObject.SetActive(true);
        }
        else { 
            tools.gameObject.SetActive(false);
        }
        Debug.Log("sda");
    }
}

