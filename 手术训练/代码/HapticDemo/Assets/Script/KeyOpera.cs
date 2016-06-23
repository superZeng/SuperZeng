using UnityEngine;
using System.Collections;

public class KeyOpera : MonoBehaviour {


    GameObject cube;
    float x;
    float y;
    float z;

	// Use this for initialization
	void Start () {
        cube = GameObject.Find("Sphere");

    }

    // Update is called once per frame
    void Update()
    {
        x = cube.transform.position.x;
        y = cube.transform.position.y;
        z = cube.transform.position.z;

        if (Input.GetKey(KeyCode.W))
        {
            y = cube.transform.position.y + (float)0.1;
            
        }
        if (Input.GetKey(KeyCode.S))
        {
            y = cube.transform.position.y - (float)0.1;
            
        }
        if (Input.GetKey(KeyCode.A))
        {
            x = cube.transform.position.x + (float)0.1;
            
        }
        if (Input.GetKey(KeyCode.D))
        {
            x = cube.transform.position.x - (float)0.1;
            
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            z = cube.transform.position.z + (float)0.1;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            z = cube.transform.position.z - (float)0.1;
        }
        cube.transform.position = new Vector3(x, y, z);
    }
    
}
