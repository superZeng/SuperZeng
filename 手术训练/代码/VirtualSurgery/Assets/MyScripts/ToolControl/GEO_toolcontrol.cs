using UnityEngine;
using System.Collections;

public class GEO_toolcontrol : MonoBehaviour {

    public float thrust;
    public Rigidbody rb;
    public GameObject Cursor;
    private Vector3 force = new Vector3(0, 0, 0);
    private double[] myProxyDirection = new double[3];
    //private int count = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    //void Update()
    //{
    //    if (PluginImport.GetButton1State())
    //    {
    //        if (count % 2 == 0)
    //        {
    //            Cursor.gameObject.SetActive(false);
    //        }
    //        else
    //        {
    //            Cursor.gameObject.SetActive(true);
    //        }
    //        count++;

    //    }
        
    //}

    void FixedUpdate()
    {
        myProxyDirection = ConverterClass.ConvertIntPtrToDouble3(PluginImport.GetProxyDirection());
        Vector3 directionCursor = new Vector3();
        directionCursor = ConverterClass.ConvertDouble3ToVector3(myProxyDirection);
        //rb.AddRelativeForce(directionCursor* thrust);
        force = directionCursor * thrust;

    }

    public Vector3 GetForce()
    {
        return force;
    }
}
