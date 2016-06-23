using UnityEngine;
using System.Collections.Generic;

namespace Obi{
		
	/**
	 * Pooling class for short-lived constraints, such as collisions and self-collisions.
	 */
	public class ConstraintPool<T> where T : Constraint , new() 
	{

		public int maxConstraints = 1000;		/**< maximum amount of constraints this pool can handle.*/
		private int activeConstraints = 0;
		private Queue<T> pool = new Queue<T>();

		/**
		 * Returns a list of all active constraints in this pool.
		 */
		public int ActiveConstraints{
			get{return activeConstraints;}
		}

		public int AvailableConstraints{
			get{return maxConstraints - activeConstraints;}
		}
		
		
		/**
		 * Returns one of the pooled constraints. Creates
		 * a new constraint if there are none available but maxConstraints hasn't been reached.
		 * If there are no pooled constraints and no more constraints can be created, returns null.
		 */
		public T GetPooledConstraint(){

			// If there are objects in the pool, return one of them.
			if (pool.Count > 0){
				activeConstraints++;
				return pool.Dequeue();
			}
			// If there are no pooled objects but we can create more, create one and return it.
			else if (activeConstraints < maxConstraints){
				activeConstraints++;
				return new T();
			}

			// There are no pooled objects and we cannot create more.
			return null;
		}
	
		/**
		 * Adds a constraint to the pool. Returns true if the pool can handle it, false otherwise.
		 */
		public bool PoolConstraint(T constraint){
			if (pool.Count < maxConstraints){
				pool.Enqueue(constraint);
				activeConstraints--;
				return true;
			}
			return false;
		}
	}
}

