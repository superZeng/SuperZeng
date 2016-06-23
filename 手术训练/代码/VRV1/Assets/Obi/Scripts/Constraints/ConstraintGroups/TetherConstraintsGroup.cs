using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Obi
{
	[Serializable]
	public class TetherConstraintGroup : ConstraintGroup<DistanceConstraint>
	{
		[Range(1,2)]
		[Tooltip("Scale of tether constraints. Values > 1 will expand initial tether distance, values < 1 will make it shrink.")]
		public float tetherScale = 1;				/**< Stiffness of structural spring constraints.*/
		
		[Range(0,1)]
		[Tooltip("Tether constraint stiffness. Lower values will allow particles to violate the constraint easier.")]
		public float stiffness = 1;		   /**< Resistance of structural spring constraints to stretch..*/
		
		public TetherConstraintGroup(){
			name = "Tether";
			iterations = 1;
		}
		
		public override void SetupConstraintParameters(IList<DistanceConstraint> constraints){
			
			foreach(DistanceConstraint c in constraints){
				c.shouldBreak = false;
				c.SolverIterations = iterations;
				c.Stiffness = stiffness;
				c.CompressionStiffness = 0;
				c.scale = tetherScale;
			}
			
		}
	}
}

