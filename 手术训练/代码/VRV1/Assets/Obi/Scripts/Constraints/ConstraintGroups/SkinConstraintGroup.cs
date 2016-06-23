using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Obi
{
	[Serializable]
	public class SkinConstraintGroup : ConstraintGroup<SkinConstraint>
	{

		[Range(0,1)]
		[Tooltip("How strongly the skin constraints should be enforced.")]
		public float stiffness = 1;		   /**< Resistance of structural spring constraints to stretch..*/  

		[Tooltip("If enabled, skinned tangents will be transferred to the cloth. For use with normal-mapped meshes.")]
		public bool transferTangents = false;	/**< Check this to force skin constraints to recalculate mesh tangents. For use with normalmapped meshes.*/
		
		[HideInInspector] public float[] pp_radius = new float[0];	/**< Per particle skin radius. Maximum distance between the skinned 
														 vertex position and the corresponding cloth particle. */
		[HideInInspector] public float[] pp_backstop = new float[0];	/**< Per particle skin backstop. Minimum distance from the skinned 
														 surface cloth particles can be at. Negative values allow penetration, 
														 positive values add space between the skinned surface and the cloth.*/

		public SkinConstraintGroup(){
			name = "Skin";
			enabled = false;
		}

		public void ResizeArrays(int newSize){

			int oldSize = pp_radius.Length;
			Array.Resize(ref pp_radius,newSize);
			for (int i = oldSize; i < pp_radius.Length; i++) {
				pp_radius[i] = 0.1f; //default value for radius.
			}

			Array.Resize(ref pp_backstop,newSize);
		}
		
		public override void SetupConstraintParameters(IList<SkinConstraint> constraints){

			foreach(SkinConstraint c in constraints){
				c.SolverIterations = iterations;
				c.Stiffness = stiffness;
				c.backstop = pp_backstop[c.pIndex]; //this can throw outofindex is things aren't correctly set up.
				c.radius = pp_radius[c.pIndex];
			}
			
		}
	}
}

