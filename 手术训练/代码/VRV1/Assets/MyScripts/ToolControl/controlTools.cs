using UnityEngine;
using System.Collections;

public class controlTools : MonoBehaviour {

	// Use this for initialization
	public float thrust;
	public Rigidbody rb;
	private Vector3 force=new Vector3(0,0,0);
	void Start () {
		rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
    void FixedUpdate()
    {
		//rb.AddForce(-transform.up * thrust);
		if (Input.GetKey ("w")) 
		{
            rb.AddRelativeForce(Vector3.forward * thrust);
			force += -Vector3.forward * thrust;
		}
		if (Input.GetKey ("s")) 
		{
            rb.AddRelativeForce(-Vector3.forward * thrust);
			force += Vector3.forward * thrust;
		}
		if (Input.GetKey ("a"))
		{
            rb.AddRelativeForce(Vector3.up * thrust);
			force += -Vector3.up * thrust;
		}
		if (Input.GetKey ("d"))
		{
            rb.AddRelativeForce(-Vector3.up * thrust);
			force += Vector3.up * thrust;
		}
        if (Input.GetKey("q"))
        {
            rb.AddRelativeForce(Vector3.right * thrust);
            force += Vector3.right * thrust;
        }
        if (Input.GetKey("e"))
        {
            rb.AddRelativeForce(-Vector3.right * thrust);
            force += -Vector3.right * thrust;
        }
        if (Input.GetKey("r"))
        {
            force.Set(0.0f, 0.0f, 0.0f);
        }
		
	}

	public Vector3 GetForce()
	{
		return force;
	}
}
