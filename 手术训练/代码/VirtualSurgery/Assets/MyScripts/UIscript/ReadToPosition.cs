using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ReadToPosition : MonoBehaviour {
    public Text text1;
    public GameObject obj;
    
    void Update()
    {
        //  obj.transform.position
        text1.text = "X="+obj.transform.position.x.ToString("0.00");
        text1.text += " Y= " + obj.transform.position.y.ToString("0.00");
        text1.text += " Z= " + obj.transform.position.z.ToString("0.00");
     ;
    }
	
}
