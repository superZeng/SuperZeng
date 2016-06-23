using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Obi
{
	/**
	 * Use these to constraint a particle to a given rigidbody, be it kinematic or not. The particle will be affected by rigidbody dynamics, and 
	 * the rigidbody will also be affected by the particle simulation.
	 */
	[Serializable]
	public class PinConstraint : Constraint
	{
		public Rigidbody rigidbody;	/**< Rigidbody to which the particle is pinned.*/
		public int pIndex;			/**< Index of the pinned particle.*/
		public Vector3 d;			/**< position delta*/
		public Vector3 offset;		/**< Pinning position expressed in rigidbody's local space.*/
		public Vector3 wsOffset;	/**< Pinning position expressed in world space.*/
		public Vector3 lsOffset;	/**< Pinning position expressed in cloth's local space.*/
		public float rigidbodyWeight;
		
		public PinConstraint(Transform transform, int pIndex, Rigidbody rigidbody, Vector3 offset) : base(transform)
		{
			this.rigidbody = rigidbody;
			this.pIndex = pIndex;
			this.offset = offset;
			UpdateRigidBodyWeight();
		}

		public void UpdateRigidBodyWeight(){
			if (rigidbody != null){
				wsOffset = rigidbody.transform.TransformPoint(offset);
				lsOffset = transform.InverseTransformPoint(wsOffset);
				rigidbodyWeight = (rigidbody == null || rigidbody.isKinematic) ? 0 : 1/rigidbody.mass;
			}
		}
		
		public override void CalculatePositionDeltas(HalfEdge halfedge,List<ObiClothParticle> particles, float dt)
		{
			d = Vector3.zero;
			float weightSum = particles[pIndex].w + rigidbodyWeight;

			// move particle to pin position:
			if (weightSum == 0) return;
			d += (particles[pIndex].predictedPosition-lsOffset) / weightSum;

		}

		public override void DistributePositionDeltas(List<ObiClothParticle> particles, bool applyImmediately, float SORFactor,float dt){
			
			// apply impulse to rigidbody:
			if (rigidbody != null && !rigidbody.isKinematic){
				rigidbody.AddForceAtPosition(transform.TransformVector(d) / dt, wsOffset,ForceMode.Impulse);
            }

			particles[pIndex].positionDelta -= d * particles[pIndex].w;
			particles[pIndex].numConstraints++;

			if (applyImmediately)
				particles[pIndex].ApplyPositionDeltas(SORFactor);
		}

		public override int GetHashCode()
		{
			if (rigidbody != null)
				return ObiUtils.Pair(pIndex,rigidbody.GetInstanceID());
			return pIndex;
		}
		
		public override bool Equals(object obj)
		{
			DistanceConstraint other = obj as DistanceConstraint;
			return (other != null && other.GetHashCode() == GetHashCode());
		}
		
	}
}