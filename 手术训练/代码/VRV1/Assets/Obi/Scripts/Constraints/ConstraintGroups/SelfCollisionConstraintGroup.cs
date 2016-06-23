using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Obi
{
	[Serializable]
	public class SelfCollisionConstraintGroup : ConstraintGroup<SelfCollisionConstraint>
	{

		[Tooltip("Maximum amount of self collisions per frame.")]
		public int maxSelfCollisions = 5000;

		[Tooltip("How much friction to apply when resolving self-collisions. with rigidbodies. 0 is no friction at all (cloth will slide off itself) and 1 is maximum friction.")]
		[Range(0,1)]
		public float friction = 0.5f;

		private ConstraintPool<SelfCollisionConstraint> collisionsPool = new ConstraintPool<SelfCollisionConstraint>();
		private List<SelfCollisionConstraint> collisions = new List<SelfCollisionConstraint>();

		public const float epsilon =  1e-8f;

		public SelfCollisionConstraintGroup() : base(){
			enabled = false;
			evaluationOrder = EvaluationOrder.PARALLEL;
			name = "Self collision";
		}

		/**
		 * Generate a list of self collision constraints to be satisfied by cloth particles.
	 	*/
		public List<SelfCollisionConstraint> GenerateSelfCollisionConstraints(Transform transform, HalfEdge edgeStructure, AdaptiveGrid grid, List<ObiClothParticle> particles){
		
			collisionsPool.maxConstraints = maxSelfCollisions;

			collisions.Clear();

			if (!enabled) return collisions;

			Profiler.BeginSample("Self collisions generation");
			
			grid.UpdateNeighbourLists((ObiClothParticle p1, ObiClothParticle p2)=>{
				return edgeStructure.AreLinked(edgeStructure.heVertices[p1.index],edgeStructure.heVertices[p2.index]);
			});
			
			for (int i = 0; i < particles.Count; i++){

				ObiClothParticle p1 = particles[i];	
				List<int> neighbours = grid.GetParticleNeighbours(p1);

				if (neighbours != null)
				for(int n = 0; n < neighbours.Count; n++){

					ObiClothParticle p2 = particles[neighbours[n]];	

					Vector3 diff = p1.position - p2.position;
					float diffMag = ObiUtils.FSqrt(diff.sqrMagnitude);
					Vector3 normal = diff / (diffMag + epsilon);
					Vector3 point = p2.position + normal * p2.radius;
					float distance = diffMag - (p1.radius + p2.radius);

					SelfCollisionConstraint constraint = collisionsPool.GetPooledConstraint();
					if (constraint != null){
						constraint.SetData(transform,p1,p2,point,normal,distance,friction);
						collisions.Add(constraint);
					}

				}
			}

			// Return all collisions to the pool, so they're available the next time GenerateSelfCollisionConstraints is called.
			for(int i = 0; i < collisions.Count; i++){
				collisionsPool.PoolConstraint(collisions[i]);
			}

			Profiler.EndSample();
			
			return collisions;
		}

	}
}

