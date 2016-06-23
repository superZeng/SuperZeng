using UnityEngine;
using System.Collections;

public class controlActor : MonoBehaviour {
	Camera cam ;
	// Use this for initialization
	void Start () {
		cam = Camera.main;
	}
	
	// Update is called once per frame
	void Update () {
		/*if(Input.GetKey("w"))
			transform.Translate (Vector3.forward * Time.deltaTime*10f);
		if(Input.GetKey("s"))
			transform.Translate (-Vector3.forward * Time.deltaTime*10f);
		if(Input.GetKey("a"))
			transform.Translate (-Vector3.right * Time.deltaTime*10f);
		if(Input.GetKey("d"))
			transform.Translate (Vector3.right * Time.deltaTime*10f);*/
		if(Input.GetKey("z"))
			transform.Translate (Vector3.right * Time.deltaTime*5f);
		if(Input.GetKey("x"))
			transform.Translate (-Vector3.right * Time.deltaTime*5f);
		if(Input.GetKey("w"))
			transform.Translate (-Vector3.forward * Time.deltaTime*5f);
		if(Input.GetKey("s"))
			transform.Translate (Vector3.forward * Time.deltaTime*5f);
		if (Input.GetKey ("a"))
			transform.Rotate (Vector3.right * Time.deltaTime*5f);
		if (Input.GetKey ("d"))
			transform.Rotate (-Vector3.right * Time.deltaTime*5f);
	}
}
