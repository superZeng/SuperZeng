using UnityEngine;
using System.Collections;

public class rotate : MonoBehaviour {

    public float speed;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * Time.deltaTime * speed);
    }

    public void c(float i)
    {
        speed = i;
    }
}
