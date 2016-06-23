using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class connection : MonoBehaviour {
    public Image connection_image;
    public bool signal;


    void Update() {
        
        if (signal)
        {
            connection_image.color = new Color(0, 255, 0, 255);
        }
        else
        {
            connection_image.color = new Color(255, 0, 0, 255);
        }
    }

}
