using UnityEngine;
using System.Collections;

public class OceanAnimation : MonoBehaviour {

	public Vector2 waveSpeed;
	Renderer renderer;

	// Use this for initialization
	void Start () {
		renderer = gameObject.GetComponent<Renderer>();
	}
	
	// Update is called once per frame
	void Update () {
		if (renderer != null && renderer.material != null){
			Vector2 offset = renderer.material.mainTextureOffset;
			offset += waveSpeed * Time.deltaTime;
			renderer.material.mainTextureOffset = offset;
		}
	}
}
