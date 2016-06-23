using UnityEngine;
using System.Collections;

public class Key_get_tool : MonoBehaviour
{
    public GameObject tool1;
    public GameObject tool2;
    public GameObject tool3;

    void Update()
    {
        /*int count = 0;
         if (Input.GetKey(KeyCode.X))
         {
             tool1.gameObject.SetActive(true);
             count = count + 1;
             tool1.gameObject.SetActive(false);
         }
         if (Input.GetKey(KeyCode.C))
         {
             tool2.gameObject.SetActive(true);
             count = count + 1;
             tool2.gameObject.SetActive(false);
         }*/


        if (tool3.gameObject.active == (false)) { 
            if (Input.GetKeyDown(KeyCode.V))
            {
                tool3.gameObject.SetActive(true);
            }
        }
        if (tool3.gameObject.active == (true)){
            if (Input.GetKeyDown(KeyCode.V))
            {
                tool3.gameObject.SetActive(false);
            }

        }
    }
}




