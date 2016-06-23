using System;
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using Thinksquirrel.Fluvio.Plugins;


public class Fluid_ : MonoBehaviour
{
	//private GameObject playerobj;
	//Ray r;
	//Vector3 target;
	private ParticleSystem ps;
	ParticleSystem.EmissionModule em;
	void Start()
	{
		ps = GetComponent<ParticleSystem> ();
		em = ps.emission;
		em.enabled = false;
		//playerobj = GameObject.FindGameObjectWithTag ("Paint");

	}
	void Update()
	{
		if(Input.GetMouseButton(1))
		{
			//transform.position=
			RaycastHit hit;
			if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),out hit))
			{
				//if(hit.transform.gameObject.tag.Equals("Sphere Center"))
				//{
					//em.enabled=false;
					//transform.position=hit.point;
					transform.position=hit.point;
					//playerobj.transform.LookAt(this.transform);
				//}
			}
		}
		if(Input.GetKeyDown(KeyCode.B))
		{
			em.enabled = true;
		}
	}
}


