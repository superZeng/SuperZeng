using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Obi
{
	[Serializable]
	public class CollisionConstraintGroup : ConstraintGroup<CollisionConstraint>
	{
		[Tooltip("Maximum amount of collisions per frame.")]
		public int maxCollisions = 5000;

		[Tooltip("How much space to leave between the cloth and colliders.")]
		public float contactOffset = 0.02f;

		[Tooltip("How much friction to apply when resolving collisions with rigidbodies. 0 is no friction at all (cloth will slide as if rigidbodies were made of ice) and 1 is maximum friction.")]
		[Range(0,1)]
		public float friction = 0.5f;

		[Tooltip("How much do cloth and other surfaces stick together. A value of 0 means no stickiness at all, 1 as sticky as possible.")]
		[Range(0,1)]
		public float stickiness = 0.0f;

		[Tooltip("Minimum distance at which particles and surfaces begin to stick together. This distance is measured from the contactOffset, so it can be lower than it.")]
		public float stickDistance = 0.05f;

		[Tooltip("Whether to use or not virtual particles. These are particles placed inside cloth triangles, and used for collision detection only. Using them allows to detect collisions with objects that would otherwise slip between cloth vertices.")]
		public bool virtualParticles = false;			/**< If true, mesh faces will be sprinkled with additional particles used only on the collision detection phase. 
														 This allows for finer collision detection on coarse meshes. However, in order to avoid missing collisions, make sure the gridSize parameter is not smaller 
														 than the average triangle in your meshes, because for performance reasons virtual particles are only tested if at least one regular particle resides in the same grid cell
		                                         		 as the rigidbody being tested for collisions.*/

		[Tooltip("Maximum amount of virtual collisions per frame.")]
		public int maxVirtualCollisions = 1000;
		
		[Tooltip("Barycentric coordinates of virtual particles.")]
		public Vector3[] virtualParticleCoordinates;	/**< Barycentric coordinates of virtual particles on the mesh*/

		private ConstraintPool<CollisionConstraint> collisionsPool = new ConstraintPool<CollisionConstraint>();
		private ConstraintPool<VirtualCollisionConstraint> vcollisionsPool = new ConstraintPool<VirtualCollisionConstraint>();
		private List<CollisionConstraint> collisions = new List<CollisionConstraint>();

		public CollisionConstraintGroup(){
			evaluationOrder = EvaluationOrder.PARALLEL;
			name = "Collision";
		}

		/**
		 * Generate a list of collision constraints to be satisfied by cloth particles.
		 */
		public List<CollisionConstraint> GenerateCollisionConstraints(float dt,Transform transform, Bounds clothBounds, ObiWorld world, HalfEdge edgeStructure, AdaptiveGrid grid, List<ObiClothParticle> particles){
			
			collisionsPool.maxConstraints = maxCollisions;
			vcollisionsPool.maxConstraints = maxVirtualCollisions;

			collisions.Clear();
			
			if (enabled && world != null && 
			    grid != null && 
			    particles != null && 
			    edgeStructure != null && 
			    transform != null){

				List<GameObject> colliders = world.PotentialColliders(clothBounds);

				foreach(GameObject go in colliders){
					
					if (go != null){
	
						Collider c = go.GetComponent<Collider>();
						DistanceFieldCollider dfc = go.GetComponent<DistanceFieldCollider>();
						ObiActor actor = go.GetComponent<ObiActor>();
	
						// If the object does not contain a regular collider or a distance field collider, skip it.
						if (actor == null || (c == null && dfc == null)) continue;
	
						// Create a set of faces that should be sprinkled with virtual particles.
						HashSet<HalfEdge.HEFace> facesWVirtualCollisions = new HashSet<HalfEdge.HEFace>();
	
						// iterate over cells intersecting with the collider bounds. 
						List<AdaptiveGrid.Cell> collidingCells = grid.GetCellsInsideBounds(actor.bounds,0,50);

						Vector3 point;
						Vector3 normal;
						float distance;

						Rigidbody rigidbody = go.GetComponent<Rigidbody>();	

						for(int i = 0; i < collidingCells.Count; i++){

							AdaptiveGrid.Cell cell = collidingCells[i];
							
							// check each particle in the cell for collisions:
							int contacts = Mathf.Min(collisionsPool.AvailableConstraints,cell.particles.Count);
							for(int j = 0; j < contacts; j++){

								ObiClothParticle p = cell.particles[j];

								// Get speculative contact info: nearest point, feature normal and distance.
								ColliderEscapePoint(c,dfc,transform.TransformPoint(p.position),contactOffset,out point, out normal, out distance);

								// Create speculative contact:
								CollisionConstraint constraint = collisionsPool.GetPooledConstraint();
								if (constraint != null){
									constraint.SetData(transform,p,rigidbody,point,normal,distance,friction,stickiness,stickDistance);
									collisions.Add(constraint);
								}

								// if using virtual particles, check all triangles that have at least one vertex in a colliding cell.
								if (virtualParticles && virtualParticleCoordinates != null){
									foreach(HalfEdge.HEFace face in edgeStructure.GetNeighbourFacesEnumerator(edgeStructure.heVertices[p.index])){
										facesWVirtualCollisions.Add(face);
									}
								}
								
							}
							
						}
						
						// Test virtual particles for collisions:
						foreach (HalfEdge.HEFace triangle in facesWVirtualCollisions){ //TODO: test if the set is doing its work.
							
							ObiClothParticle p1 = particles[edgeStructure.heEdges[triangle.edges[0]].endVertex];
							ObiClothParticle p2 = particles[edgeStructure.heEdges[triangle.edges[1]].endVertex];
							ObiClothParticle p3 = particles[edgeStructure.heEdges[triangle.edges[2]].endVertex];
							
							int contacts = Mathf.Min(vcollisionsPool.AvailableConstraints,virtualParticleCoordinates.Length);
							for (int j = 0; j < contacts; j++){
								
								Vector3 vpPosition = ObiUtils.BarycentricInterpolation(p1.position,p2.position,p3.position,virtualParticleCoordinates[j]);

								// Get speculative contact info: nearest point, feature normal and distance.
								ColliderEscapePoint(c,dfc,transform.TransformPoint(vpPosition),contactOffset,out point, out normal, out distance);
								
								// Create speculative contact:
								VirtualCollisionConstraint constraint = vcollisionsPool.GetPooledConstraint();
								if (constraint != null){
									constraint.SetData(transform,p1,p2,p3,vpPosition,virtualParticleCoordinates[j],rigidbody,point,normal,distance,friction);
									collisions.Add(constraint);
								}

							}
						}
					}
					
				}

				// Return all collisions to the pool, so they're available the next time GenerateCollisionConstraints is called.
				for(int i = 0; i < collisions.Count; i++){
					CollisionConstraint c = collisions[i];
					if (c is VirtualCollisionConstraint)
						vcollisionsPool.PoolConstraint(c as VirtualCollisionConstraint);
					else
						collisionsPool.PoolConstraint(c);
				}

			}
			
			return collisions;
		}

		private void ColliderEscapePoint(Collider c, DistanceFieldCollider dfc, Vector3 position, float contactOffset, out Vector3 point, out Vector3 normal, out float distance){
			
			point = position;
			normal = Vector3.zero;
			distance = 0;

			if (dfc != null){
				
				dfc.EscapePoint(position,contactOffset,out point,out normal,out distance);
				
			}
		
			else if (c is SphereCollider){
				
				((SphereCollider) c).EscapePoint(position,contactOffset,out point,out normal,out distance);
				
			}
		
			else if (c is BoxCollider){
				
				((BoxCollider) c).EscapePoint(position,contactOffset,out point,out normal,out distance);
				
			} 

			else if (c is CapsuleCollider){
				
				((CapsuleCollider) c).EscapePoint(position,contactOffset,out point,out normal,out distance);
				
			}

			else if (c is TerrainCollider){
				
				((TerrainCollider) c).EscapePoint(position,contactOffset,out point,out normal,out distance);
				
			}
			
		}

	}
}

