using UnityEngine;
using System.Collections;

public class HapticProperties : MonoBehaviour {

	public float stiffness;//硬度
	public float damping;//阻尼
	public float staticFriction;//静摩擦
	public float dynamicFriction;//动摩擦
	public float tangentialStiffness;//
	public float tangentialDamping;
	public float popThrough;
	public float puncturedStaticFriction;
	public float puncturedDynamicFriction;
	public float mass;
	public bool fixedObj;
}
