using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Obi
{
/**
 * Very similar to a regular collision constraint, but uses barycentric interpolation to apply position correction to all three particles
 * of a triangle simultaneously in response to a virtual particle collision.
 */	
public class VirtualCollisionConstraint : CollisionConstraint{
	
	public ObiClothParticle particle2;
	public ObiClothParticle particle3;
	public Vector3 vpPosition;
	public Vector3 vpBarycentricCoords;
	public Vector3 impulse;
	public float vpWeight;
	public float scaleFactor;

	public VirtualCollisionConstraint(){}

	public void SetData(Transform transform, ObiClothParticle particle,  ObiClothParticle particle2,  ObiClothParticle particle3, 
		                Vector3 vpPosition,Vector3 vpBarycentricCoords,  Rigidbody rigidbody, Vector3 point, Vector3 normal, float distance, float friction){

		base.SetData(transform,particle,rigidbody,point,normal,distance,friction,0,0);

		this.particle2 = particle2;
		this.particle3 = particle3;
		this.vpPosition = vpPosition;
		this.vpBarycentricCoords = vpBarycentricCoords;
		this.vpWeight = ObiUtils.BarycentricInterpolation(particle.w,particle2.w,particle3.w,vpBarycentricCoords);
		this.scaleFactor = ObiUtils.BarycentricExtrapolationScale(vpBarycentricCoords);
		this.weightSum = vpWeight + rigidbodyWeight;

	}
	
	public VirtualCollisionConstraint(Transform transform, ObiClothParticle particle,  ObiClothParticle particle2,  ObiClothParticle particle3, 
		                              Vector3 vpPosition,Vector3 vpBarycentricCoords,  Rigidbody rigidbody, Vector3 point, Vector3 normal, float distance, float friction) : 
									  base(transform,particle,rigidbody,point,normal,distance,friction,0,0){

		SetData(transform,particle,particle2,particle3,vpPosition,vpBarycentricCoords,rigidbody,point,normal,distance,friction);
	}
	
	public override void CalculatePositionDeltas(HalfEdge halfedge,List<ObiClothParticle> particles, float dt){

		impulse = Vector3.zero;

		// If there is no rigidbody, it is kinematic or is sleeping, and the particle is asleep, we can skip this constraint.
		if (rigidbodyIsSleeping && particle.asleep && particle2.asleep && particle3.asleep) return;
		
		// If both the particle and the rigidbody are fixed, skip this.
		if (weightSum == 0) return;
		
		//Calculate relative normal and tangent velocities at nearest point:
		Vector3 vpPredictedPosition = ObiUtils.BarycentricInterpolation(particle.predictedPosition,
																		particle2.predictedPosition,
																		particle3.predictedPosition,
																		vpBarycentricCoords);

		Vector3 relativeVelocity = (vpPredictedPosition - vpPosition) / dt - rigidbodyVelocityAtContact;		
		float relativeNormalVelocity = Vector3.Dot(relativeVelocity,normal);
		Vector3 tangentSpeed = relativeVelocity - relativeNormalVelocity * normal;	
		float relativeTangentVelocity = ObiUtils.FSqrt(tangentSpeed.sqrMagnitude);
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
		
		if (normalChange != 0 || tangentChange != 0){
			// wake the particle up:
			particle.asleep = false;
			particle2.asleep = false;
			particle3.asleep = false;
		}
		
		//Add impulse to particle and rigidbody
		impulse = tangent * tangentChange - normal * normalChange;
			
	}

	public override void DistributePositionDeltas(List<ObiClothParticle> particles, bool applyImmediately, float SORFactor,float dt){

		// add impulse to rigid body, if any:
		if (rigidbody != null){
			rigidbody.AddForceAtPosition(transform.TransformVector(-impulse),wspoint,ForceMode.Impulse);
		}

		if (particle.w > 0)  particle.positionDelta += impulse * vpWeight * dt * vpBarycentricCoords.x * scaleFactor;
		if (particle2.w > 0) particle.positionDelta += impulse * vpWeight * dt * vpBarycentricCoords.y * scaleFactor;
		if (particle3.w > 0) particle.positionDelta += impulse * vpWeight * dt * vpBarycentricCoords.z * scaleFactor;

		particle.numConstraints++;
		particle2.numConstraints++;
		particle3.numConstraints++;

		if (applyImmediately){
			particle.ApplyPositionDeltas(SORFactor);
			particle2.ApplyPositionDeltas(SORFactor);
			particle3.ApplyPositionDeltas(SORFactor);
		}
	}
	
}
}
