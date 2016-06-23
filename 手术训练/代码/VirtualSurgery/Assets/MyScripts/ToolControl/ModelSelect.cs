using UnityEngine;
using System.Collections;

public class ModelSelect : MonoBehaviour {
	public GameObject hand_l_1,hand_l_2,hand_r_1,hand_r_2;
	// Use this for initialization
	void Start () {
		hand_l_1 = GameObject.Find ("L_Tools/Cube_l");
		hand_l_2 = GameObject.Find ("L_Tools/Sphere_l");
		hand_r_1 = GameObject.Find ("Cube_r");
		hand_r_2 = GameObject.Find ("Sphere_r");
		hand_l_1.SetActive (false);
		hand_l_2.SetActive (false);
		hand_r_1.SetActive (false);
		hand_r_2.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey ("z")) {
			hand_l_1.SetActive (true);
			hand_l_2.SetActive (false);
		}
		if(Input.GetKey("x")) {
			hand_l_2.SetActive (true);
			hand_l_1.SetActive (false);
		}
		if (Input.GetKey ("c")) {
			hand_r_1.SetActive (true);
			hand_r_2.SetActive (false);
		}
		if(Input.GetKey("v")) {
			hand_r_2.SetActive (true);
			hand_r_1.SetActive (false);
		}
	}
}
