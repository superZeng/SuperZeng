using UnityEngine;
using System.Collections;

public class Dice : HapticClassScript {

    private GenericFunctionsClass genericFunctionClass;
    //bool button1State = false;


    void Awake()
    {
        genericFunctionClass = transform.GetComponent<GenericFunctionsClass>();
    }

    // Use this for initialization
    void Start()
    {
        if (PluginImport.InitHapticDevice())
        {
            genericFunctionClass.SetHapticWorkSpace();
            genericFunctionClass.GetHapticWorkSpace();

            PluginImport.UpdateWorkspace(myHapticCamera.transform.rotation.eulerAngles.y);

            PluginImport.SetMode(ModeIndex);

            genericFunctionClass.IndicateMode();
            PluginImport.SetTouchableFace(ConverterClass.ConvertStringToByteToIntPtr(TouchableFace));
        }
        else
        {
            Debug.Log("Haptic Device cannot be launched");
        }

        genericFunctionClass.SetEnvironmentConstantForce();
        genericFunctionClass.SetEnvironmentFriction();
        genericFunctionClass.SetEnvironmentSpring();


    }

    // Update is called once per frame
    void Update()
    {
        PluginImport.UpdateWorkspace(myHapticCamera.transform.rotation.eulerAngles.y);
        genericFunctionClass.UpdateGraphicalWorkspace();
        PluginImport.RenderHaptic();
        genericFunctionClass.GetProxyValues();
        genericFunctionClass.GetTouchedObject();


        //if (PluginImport.GetButton1State())
        //{

        //    button1State = true;
        //}
        //else
        //{

        //    button1State = false;

        //}

    }

    void OnDisable()
    {
        if (PluginImport.HapticCleanUp())
        {
            Debug.Log("Haptic Context CleanUp");
            Debug.Log("Desactivate Device");
            Debug.Log("OpenGL Context CleanUp");
        }
    }
}
