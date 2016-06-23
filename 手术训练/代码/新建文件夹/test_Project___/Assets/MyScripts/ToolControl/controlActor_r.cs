using UnityEngine;
using System.Collections;

public class controlActor_r : MonoBehaviour {
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
		if(Input.GetKey("i"))
			transform.Translate (Vector3.up * Time.deltaTime*10f);
		if(Input.GetKey("k"))
			transform.Translate (-Vector3.up * Time.deltaTime*10f);
		if (Input.GetKey ("j"))
			transform.Rotate (Vector3.right * Time.deltaTime*10f);
		if (Input.GetKey ("l"))
			transform.Rotate (-Vector3.right * Time.deltaTime*10f);
	}
}
