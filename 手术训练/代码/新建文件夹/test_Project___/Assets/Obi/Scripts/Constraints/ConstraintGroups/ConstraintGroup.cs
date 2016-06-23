using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Obi
{
	/**
	 * Types of constraint evaluation order.
	 */
	public enum EvaluationOrder{
		SEQUENTIAL, /**< Sequential constraint evaluation projects constraints in a fixed order, determined
		             	 by constraint creation order. Each constraint "sees" the previous position deltas applied
		                 by previously evaluated constraints, which tipically leads to faster convergence than parallel evaluation.
		                 However this also introduces biasing, which can lead to instabilities caused by some constraint position adjustments 
		                 having more "preference" than others.

		             	 Use this mode if you are not having stability issues of any kind.*/
		
		PARALLEL	/**< With parallel constraint evaluation each constraint knows nothing about corrections made by 
						 other constraints. All constraints calculate their desired corrections to the initial prediction,
		          		 then corrections are averaged and the final overall correction applied to the particles after each step. 
		          		 This eliminates any ordering bias, but also causes slower convergence. 

						 Use this mode if you are having stability issues, if you can afford to have a few more iterations per step,
		          		 or if you don't need fast convergence.

		          		 Common use cases:
		          		 - Self collisions should almost always use parallel evaluation mode, as they're prone to make the cloth implode when lots of self collisions happen at once.
		          		 - Pressure constraint has a very high pressure value and the stretch constraints are having a hard time preserving the original mesh shape -> Use parallel mode for the stretch constraints.
	          			 - Stretch scale is very low and stretch constraints can't keep the mesh shape -> Use parallel mode for the stretch constraints.
		          		 - Cloth is trapped between several rigidbodies and starts shaking because collision constraints can't seem to decide where to place cloth vertices -> Use parallel mode for the collision constraints.*/
	}

	/**
	 * Evaluates a group of constraints of the same type together. Each group evaluates all its constraints a certain number
	 * of times during a physics step, which can be set using the "iterations" variable. 
	 */
	[Serializable]
	public class ConstraintGroup<T> where T : Constraint
	{
		protected string name = "";

		protected GraphColoring coloring; // constraint coloring

		[Tooltip("Whether this constraint group affects the cloth or not.")]
		public bool enabled = true;

		[Tooltip("Number of relaxation iterations performed by the constraint solver. A low number of iterations will perform better, but be less accurate.")]
		public int iterations = 2;													/**< Amount of solver iterations per step for this constraint group.*/

		[Tooltip("Order in which constraints are evaluated. SEQUENTIAL converges faster but is not very stable. PARALLEL is very stable but converges slowly, requiring more iterations to achieve the same result.")]
		public EvaluationOrder evaluationOrder = EvaluationOrder.SEQUENTIAL;		/**< Constraint evaluation order.*/

		[Tooltip("Over (or under if < 1) relaxation factor used. At 1, no overrelaxation is performed. At 2, constraints double their relaxation rate. High values reduce stability but improve convergence.")]
		[Range(0.1f,2)]
		public float SORFactor = 1.0f;												/**< Sucessive over-relaxation factor for parallel evaluation order.*/

		public int effectiveIterations{												/**< Returns 0 if the group is disabled, and the amount of iterations otherwise.*/
			get{return enabled?iterations:0;}
		}

		/**
		 * Updates constraint parameters.
		 */
		public virtual void SetupConstraintParameters(IList<T> constraints){
			for(int i = 0; i < constraints.Count; i++){
				constraints[i].SolverIterations = iterations;
			}
		}

		/**
		 * Treats the constraint list as a graph, and assigns each one a different color so that no 
		 * constraint has the same color as a constraint which shares any particles with it.
    	 */
		public virtual void ColorizeConstraintGraph(HalfEdge halfEdge,IList<DistanceConstraint> constraints){
			coloring.constraints = constraints.Cast<Constraint>().ToList();
			coloring.Colorize(halfEdge);
		}

		/**
		 * Evaluates all constraints provided, in parallel or sequential order.
		 */
		public void Evaluate(IList<T> constraints, HalfEdge edgeStructure,List<ObiClothParticle> particles, int numThreads, float dt)
		{
			if (!enabled || constraints == null) return;

			Profiler.BeginSample(name+" evaluation");
			switch(evaluationOrder){
				case EvaluationOrder.SEQUENTIAL:{

					// Apply deltas directly for this constraint, so changes are immediately visible for next constraint.
					// This method is not directly parallelizable. Constraints sharing particles cannot be resolved in the same thread, so
					// a constraint coloring method has to be used (similar to Red-Black Gauss-Seidel)
			
					if (coloring != null && coloring.colors.Count > 0){
						
						// For each color:
						for(int i = 0; i < coloring.colors.Count; i++){
	
							// Evaluate constraints of the same color in parallel: 
							ObiThreads.DoTask(numThreads,(int totalThreads, int threadIndex,object state)=>{

								List<int> c = state as List<int>;
								int start,end;
								ObiThreads.GetArrayStartEndForThread(c.Count,totalThreads,threadIndex, out start, out end);

								for (int j = start; j < end; j++){
									constraints[c[j]].CalculatePositionDeltas(edgeStructure,particles,dt);
									constraints[c[j]].DistributePositionDeltas(particles,true,SORFactor,dt);
		                        }

							},coloring.colors[i].constraintList);

						}

					}else{
			
						// We cannot safely parallelize without colored constraints.
		                for(int i = 0; i < constraints.Count; i++){
							constraints[i].CalculatePositionDeltas(edgeStructure,particles,dt);
							constraints[i].DistributePositionDeltas(particles,true,SORFactor,dt); 
						}
					}

				}break;
				case EvaluationOrder.PARALLEL:{

					// Average position deltas and apply them only at the end of the projection step.
					// One advantage of this method is that it is easily parallelizable.

					ObiThreads.DoTask(numThreads,(int totalThreads, int threadIndex,object state)=>{
						int start,end;
						ObiThreads.GetArrayStartEndForThread(constraints.Count,totalThreads,threadIndex, out start, out end);
						for (int j = start; j < end; j++){
							constraints[j].CalculatePositionDeltas(edgeStructure,particles,dt);
						}
					},null);

					// For each constraint, distribute its deltas to affected particles, but dont apply them yet.
					for(int i = 0; i < constraints.Count; i++){
						constraints[i].DistributePositionDeltas(particles,false,SORFactor,dt); 
					}

					// For each particle, average and apply the deltas.
					ObiThreads.DoTask(numThreads,(int totalThreads, int threadIndex,object state)=>{
						int start,end;
						ObiThreads.GetArrayStartEndForThread(particles.Count,totalThreads,threadIndex, out start, out end);
						for (int j = start; j < end; j++){
							particles[j].ApplyPositionDeltas(SORFactor);
						}
					},null);

				}break;
			}
			Profiler.EndSample();
		}

		/**
		 * Returns iteration padding value for a given number of total solver iterations. Will return 1 if the 
		 * group is disabled or the amount of iterations is less than 1.
		 */
		public int IterationPadding(int maxIterations){
			return (enabled && iterations > 0) ? Mathf.CeilToInt(maxIterations/(float)iterations):1;
		}

	}

}

