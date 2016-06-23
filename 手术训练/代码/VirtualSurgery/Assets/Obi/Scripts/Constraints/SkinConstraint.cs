using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Obi
{
	/**
	 * These constraints try to maintain the particle inside a sphere defined by center and radius.
	 */
	[Serializable]
	public class SkinConstraint : Constraint
	{
		
		public int pIndex;				/**< Index of the particle.*/
		public float radius;			/**< Distance that should be kept between the two particles.*/
		public float backstop;
		public Vector3 point;
		public Vector3 normal;
		private Vector3 d;
		
		public const float epsilon = 1e-6f;
		
		public SkinConstraint(Transform transform, int pIndex, Vector3 point, float radius, Vector3 normal, float backstop, float stiffness) : base(transform)
		{
			this.pIndex = pIndex;
			this.radius = radius;
			this.point = point;
			this.normal = normal;
			this.backstop = backstop;
			this.Stiffness = stiffness;
			this.point = point;
		}
		
		public override void CalculatePositionDeltas(HalfEdge halfedge,List<ObiClothParticle> particles, float dt)
		{
			d = Vector3.zero;

			// We can skip this for fixed particles:
			if (particles[pIndex].w == 0) return;

			if (LinearStiffness > 0){

				// Wake the particle up.
				particles[pIndex].asleep = false;

				Vector3 positionDiff = particles[pIndex].predictedPosition - point;
				float surfaceDistance = Vector3.Dot(positionDiff,normal);

				Vector3 correctionVector = Vector3.zero;

				// backstop constraint:
				if (surfaceDistance < backstop){
					correctionVector += normal * Mathf.Min(surfaceDistance - backstop,0);
				}

				// update position diff and get distance to point:
				positionDiff -= correctionVector;
				float distance = ObiUtils.FSqrt(positionDiff.sqrMagnitude);

				// radius constraint:
				if (distance > radius){
					float correctionFactor = distance - radius;
					correctionVector += positionDiff / distance * correctionFactor;
				}	

				if (correctionVector != Vector3.zero){
					d -= LinearStiffness * correctionVector;
				}

			}
			
		}
		
		public override void DistributePositionDeltas(List<ObiClothParticle> particles, bool applyImmediately, float SORFactor,float dt){

			particles[pIndex].positionDelta += d;
			particles[pIndex].numConstraints++;

			if (applyImmediately)
				particles[pIndex].ApplyPositionDeltas(SORFactor);

		}
		
		public override int GetHashCode()
		{
			return pIndex;
		}
		
		public override bool Equals(object obj)
		{
			DistanceConstraint other = obj as DistanceConstraint;
			return (other != null && other.GetHashCode() == GetHashCode());
		}
		
	}
}



