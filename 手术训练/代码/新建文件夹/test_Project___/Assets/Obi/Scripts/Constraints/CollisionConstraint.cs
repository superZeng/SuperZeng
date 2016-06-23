using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Obi
{
/**
 * Collision constraints are created and destroyed dynamically during a normal solver step. They are used to allow cloth particles to collide
 * with regular colliders, and to inject energy into rigidbodies when needed.
 */
public class CollisionConstraint : Constraint{
	
	public ObiClothParticle particle;	

	public Vector3 normal;
	public Vector3 point;
	public float distance;
	public Vector3 wspoint;

	public float rigidbodyWeight;
	public Rigidbody rigidbody;
	public Vector3 rigidbodyVelocityAtContact;
	public bool rigidbodyIsSleeping;
	public float weightSum;
	public float frictionCoeff;
	public float stickinessCoeff;
	public float stickDistance;

	protected Vector3 impulse;
	protected float normalImpulse;
	protected float tangentImpulse;
	protected float stickyImpulse;

	public const float epsilon =  1e-8f;

	public CollisionConstraint():base(null){}	

	public void SetData(Transform transform, ObiClothParticle particle, Rigidbody rigidbody, Vector3 point, Vector3 normal, float distance, float friction, float stickinessCoeff, float stickDistance){
		
		// Reset impulses.
		impulse = Vector3.zero;
		normalImpulse = 0;
		tangentImpulse = 0;
		stickyImpulse = 0;

		this.transform = transform;
		this.particle = particle;
		this.wspoint = point;
		this.point = transform.InverseTransformPoint(point);
		this.normal = transform.InverseTransformDirection(normal);
		this.distance = distance;
		
		// We store a lot of rigidbody state: 1.- to reuse it trough several iterations 2.- because rigidbody cannot be queried from worker threads in parallel.
		this.rigidbody = rigidbody;
		this.rigidbodyWeight = (rigidbody == null || rigidbody.isKinematic) ? 0 : 1/rigidbody.mass;
		this.rigidbodyVelocityAtContact = (rigidbody == null || rigidbody.isKinematic) ? Vector3.zero : transform.InverseTransformVector(rigidbody.GetPointVelocity(wspoint));
		this.rigidbodyIsSleeping = (rigidbody == null) ? true : rigidbody.IsSleeping();
		this.weightSum = particle.w + rigidbodyWeight;
		this.frictionCoeff = friction;
		this.stickinessCoeff = stickinessCoeff;
		this.stickDistance = stickDistance;

	}
	
	public CollisionConstraint(Transform transform, ObiClothParticle particle, Rigidbody rigidbody, Vector3 point, Vector3 normal, float distance, float friction, float stickinessCoeff, float stickDistance) : base(transform){
		SetData(transform,particle,rigidbody,point,normal,distance,friction,stickinessCoeff,stickDistance);
	}
	
	public override void CalculatePositionDeltas(HalfEdge halfedge,List<ObiClothParticle> particles, float dt)
	{
		impulse = Vector3.zero;

		// If there is no rigidbody, it is kinematic or is sleeping, and the particle is asleep, we can skip this constraint.
		if (rigidbodyIsSleeping && particle.asleep) return;
	
		// If both the particle and the rigidbody are fixed, skip this.
		if (weightSum == 0) return;

		//Calculate relative normal and tangent velocities at nearest point:
		Vector3 relativeVelocity = (particle.predictedPosition - particle.position) / dt - rigidbodyVelocityAtContact;		
		float relativeNormalVelocity = Vector3.Dot(relativeVelocity,normal);
		Vector3 tangentSpeed = relativeVelocity - relativeNormalVelocity * normal;	
		float relativeTangentVelocity = tangentSpeed.magnitude;
		Vector3 tangent = tangentSpeed / (relativeTangentVelocity + epsilon);		

		//Calculate normal impulse correction:
		float nvCorrection = relativeNormalVelocity + distance / dt;  
		float niCorrection = nvCorrection / weightSum;
		
		//Accumulate impulse:
		float newImpulse = Mathf.Min(normalImpulse + niCorrection,0);
	
		//Calculate change impulse change and set new impulse:
		float normalChange = newImpulse - normalImpulse;
		normalImpulse = newImpulse;

		// If this turns out to be a real (non-speculative) contact, compute friction impulse.
		float tangentChange = 0;
		if (nvCorrection < 0 && frictionCoeff > 0){ // Real contact

			float tiCorrection = - relativeTangentVelocity / weightSum;

			//Accumulate tangent impulse using coulomb friction model:
			float frictionCone = - normalImpulse * frictionCoeff;
			float newTangentImpulse = Mathf.Clamp(tangentImpulse + tiCorrection,-frictionCone, frictionCone);
			
			//Calculate change impulse change and set new impulse:
			tangentChange = newTangentImpulse - tangentImpulse;
			tangentImpulse = newTangentImpulse;
		}

		//Calculate stickiness impulse correction:
		float stickyChange = 0;
		if (nvCorrection > 0 && stickinessCoeff > 0 && stickDistance > 0){

			float siCorrection = stickinessCoeff * niCorrection * (1 - distance / stickDistance);
			float newStickyImpulse = Mathf.Max(stickyImpulse + siCorrection,0);
			
			stickyChange = newStickyImpulse - stickyImpulse;
			stickyImpulse = newStickyImpulse;

		}

		if (normalChange != 0 || tangentChange != 0 || stickyChange != 0){
			// wake the particle up:
			particle.asleep = false;
		}

		//Compute final impulse:
		impulse = tangent * tangentChange - normal * (normalChange + stickyChange);

	}

	public override void DistributePositionDeltas(List<ObiClothParticle> particles, bool applyImmediately, float SORFactor,float dt){

		// dstribute impulse to the rigidbody too (this only can be done in the main thread):
		if (rigidbody != null){
			rigidbody.AddForceAtPosition(transform.TransformVector(-impulse),wspoint,ForceMode.Impulse);
		}

		particle.positionDelta += impulse * particle.w * dt;
		particle.numConstraints++;

		if (applyImmediately)
			particle.ApplyPositionDeltas(SORFactor);

	}
	
	
}
}

