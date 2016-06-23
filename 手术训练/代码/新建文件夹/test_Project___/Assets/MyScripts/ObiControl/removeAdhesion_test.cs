using UnityEngine;
using System.Collections;
using Obi;

[RequireComponent(typeof(ObiCloth))]
public class removeAdhesion_test : MonoBehaviour {

	ObiCloth cloth;
	GameObject go;
	GameObject go1;
	GameObject go2;
	GameObject go3;
	public float inflationSpeed = 2.5f;
	public float maxPressure = 2;
	Vector3 []old_position;
	float test;

	//bool flg=true;
	//bool flg1=true;
	//public PinConstraint element;
	//public ObiCloth.PinSet e;
	//public PinConstraintGroup e;
	// Use this for initialization
	void Awake () 
	{
		go = GameObject.Find ("Cube");
		//go1 = GameObject.Find ("3_3");
		go2 = GameObject.Find ("Cube1");
		//go3 = GameObject.Find ("test_brain");
		cloth = GetComponent<ObiCloth>();
		old_position=new Vector3[cloth.pins.Count];
		for (int i = 0; i < cloth.particles.Count; i++) 
		{
			cloth.particles [i].asleep = true;
		}
		//element=new Obi.PinConstraint(go.transform,0,go.GetComponent<Rigidbody>(),go.transform.position - cloth.particles [0].position);
		//element.pIndex = 0;
		//element.rigidbody = go.GetComponent<Rigidbody>(); 
		//element.offset = go.transform.position - cloth.particles [0].position;

		//cloth.pins.Add (element);

		for (int i = 0; i < cloth.pins.Count; i++)
		{
			old_position [i] = cloth.particles [cloth.pins [i].pIndex].position;
			//float a = (cloth.particles [cloth.pins [i].pIndex].position - cloth.particles [cloth.pins [i + 1].pIndex].position).FLength();
		}
		//cloth.particles [1].mass=10000f;
		//cloth.pins[0].
		//element = GetComponent<Obi.PinConstraint> ();

		//Destroy (gameObject);
	}

	// Use this for initialization
	//void Start () {
	//	cloth = GetComponent<ObiCloth>();
	//}
	
	// Update is called once per frame
	void Update () 
	{
		cloth.pressureConstraintsGroup.pressure = Mathf.Min(cloth.pressureConstraintsGroup.pressure +
			Time.deltaTime*inflationSpeed,maxPressure);
		if (cloth.pins.Count > 0) {
			for (int i = 0; i < cloth.pins.Count; i++) {
				test = (old_position [i] - cloth.particles [cloth.pins [i].pIndex].position).FLength ();
				if (test > 2f&&cloth.pins[i].rigidbody==go2.GetComponent<Rigidbody>())
					cloth.pins.RemoveAt (i);
			}
		}
		//cloth.pins[1].rigidbody
		//if (cloth.particles [2].velocity.FLength() > 0.5f&&flg1) 
		//{
			//cloth.pins [0].pIndex;
			//cloth.pins.RemoveAt (2);
			//flg1 = false;
		//}
		//for (int i = 0; i < cloth.particles.Count; i++) 
		//{
		//	cloth.particles [i].GoToSleep ();
		//}
		//cloth.particles [0].mass=Infinity;
		//cloth.particles [0].mass= -1;
		//cloth.pins.Clear();

	}

	void OnCollisionEnter(Collision collision) 
	{
		Destroy (gameObject);
	}

}
