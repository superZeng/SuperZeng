using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Obi
{
	[Serializable]
	public class PressureConstraintGroup : ConstraintGroup<PressureConstraint>
	{

		[Tooltip("Amount of pressure applied to the cloth.")]
		public float pressure = 1;

		[HideInInspector][NonSerialized] public int numThreads = 0;

		public PressureConstraintGroup() : base(){
			enabled = false;
			name = "Pressure";
		}

		public override void SetupConstraintParameters(IList<PressureConstraint> constraints){
			
			foreach(PressureConstraint c in constraints){
				c.enabled = enabled;
				c.pressure = pressure;
				c.numThreads = numThreads;
				c.SolverIterations = iterations;
			}
			
		}
	}
}

