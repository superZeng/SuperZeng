using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using Obi;

[RequireComponent(typeof(ObiCloth))]
public class ShipController : MonoBehaviour {

	public float randomWindChangeRate = 3;
	public float randomWindIntensity = 8;

	private ObiCloth sail;

	private Vector3 wind;
	private float noiseCoord;

	// Use this for initialization
	void Start () {
		sail = GetComponent<ObiCloth>();
	}
	
	public void ChangeWindDirection(BaseEventData  data){

		PointerEventData pointerData = data as PointerEventData;
		Vector3 drag = pointerData.position - pointerData.pressPosition;
		wind = new Vector3(Mathf.Clamp(drag.x*0.1f,-20,20),0,Mathf.Clamp(drag.y*0.1f,-20,20));

	}

	public void Update(){

		float randomWindX = (Mathf.PerlinNoise(noiseCoord,0)-0.5f)*2;
		float randomWindZ = (Mathf.PerlinNoise(0,noiseCoord)-0.5f)*2;
		noiseCoord += randomWindChangeRate * Time.deltaTime;

		sail.aerodynamics.wind = wind + new Vector3(randomWindX,0,randomWindZ) * randomWindIntensity;

	}

	public void SetRandomWindIntensity(float intensity){
		randomWindIntensity = intensity;
	}

}
