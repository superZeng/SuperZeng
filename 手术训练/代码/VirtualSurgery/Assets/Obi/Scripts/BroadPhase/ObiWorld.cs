using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Obi{

[ExecuteInEditMode]
public class ObiWorld : MonoBehaviour
{

	public float boundsPadding = 0.5f;
	public float minimumContactOffset = 0.1f;

	[HideInInspector] public DBVH dynamicBVH;
	[SerializeField][HideInInspector] List<GameObject> objects = new List<GameObject>();

	public IList<GameObject> Objects{
		get{return objects.AsReadOnly();}
	}

	public void OnEnable(){
		if (dynamicBVH == null)
			dynamicBVH = new DBVH();
	}

	public void AddObject(GameObject obj){

		if (obj != null){

			ObiCloth cloth = obj.GetComponent<ObiCloth>();

			if (cloth != null){
				cloth.world = this;
				objects.Add(obj);
			}else{
			
				ObiActor actor = obj.GetComponent<ObiActor>();
				if (actor != null){
					
					actor.boundsPadding = boundsPadding;
					actor.minimumContactOffset = minimumContactOffset;
					dynamicBVH.Insert(actor);
					objects.Add(obj);
					
				}else{
					Debug.LogError("Could not add object from ObiWorld because ObiActor component is missing. Re-create the ObiWorld component.");
				}

			}

		}

	}

	/**
	 * Removes an actor from the world. Never call this directly, as ObiActors remove themselves from the world upon being destroyed.
	 */
	public void RemoveObject(GameObject obj){
		
		if (obj != null){

			ObiCloth cloth = obj.GetComponent<ObiCloth>();
			
			if (cloth != null){

				cloth.world = null;
				objects.Remove(obj);

			}else{

				ObiActor actor = obj.GetComponent<ObiActor>();
				if (actor != null){

					dynamicBVH.Remove(actor);
					objects.Remove(obj);

				}else{
					Debug.LogError("Could not remove object from ObiWorld because ObiActor component is missing. Re-create the ObiWorld component.");
				}

			}

		}
		
	}

	public List<GameObject> PotentialColliders(Bounds bounds){
		List<GameObject> result = new List<GameObject>();
		List<DBVH.DBVHNode> nodes = dynamicBVH.BoundsQuery(bounds);
		foreach(DBVH.DBVHNode n in nodes){
			result.Add(n.content.gameObject);
		}
		return result;
	}

	// Update is called once per frame
	void LateUpdate ()
	{
		dynamicBVH.Update();
	}

	public void OnDestroy(){

		// Remove all objects and their ObiActor components associated to this world.

		while(objects.Count > 0){

			GameObject obj = objects[objects.Count-1];
			objects.RemoveAt(objects.Count-1);

			if (obj != null){ // == null should never happen, but just in case.

				ObiActor[] actors = obj.GetComponents<ObiActor>();

				for(int i = 0; i < actors.Length;i++){	
					if (actors[i].world == this || actors[i].world == null)
						GameObject.DestroyImmediate(actors[i]);
				}

			}

		}

	}


}
}

