using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Obi{

	[Serializable]
	public class GraphColoring
	{

		[Serializable]
		public class ColorGroup{
			public List<int> constraintList = new List<int>();
		}
	
		// Unity cannot serialize this unfortunately... make List<int> a serializable class, see http://answers.unity3d.com/questions/49286/serialize-a-list-containing-another-list-listlisto.html
		public List<ColorGroup> colors = new List<ColorGroup>();	/**< list of colors in the graph. each color contains a list of constraint indices.*/
		[NonSerialized] public IList<Constraint> constraints;					/**< constraints under consideration.*/

		private int[] constraintColors;		/**< per-constraint color*/
		private bool[] availableColors;		/**< list of available colors.*/

		public void Colorize(HalfEdge halfEdge){

			if (halfEdge == null) return;
	
			colors.Clear();

			// Initialize per-constraint color array, set all to -1 (no color)
			constraintColors = new int[constraints.Count];
			availableColors = new bool[constraints.Count];
			for(int i = 0; i < constraints.Count; i++){
				constraintColors[i] = -1;
				availableColors[i] = true;
			}


			for(int i = 0; i < constraints.Count; i++){
				
				// Mark neighbouring colors as unavailable.
				MarkAdjacentColorsAsUnavailable(halfEdge,constraints[i]);

				// Find lowest available color.
				int color;
				for(color = 0; color < availableColors.Length; color++)
					if (availableColors[color])
						break;
					
				// Paint this constraint:
				constraintColors[i] = color;
				if (color >= colors.Count)
					colors.Add(new ColorGroup());
				colors[color].constraintList.Add(i);

				// Reset color availability flags:
				for(int j = 0; j < constraints.Count; j++){
					availableColors[j] = true;
				}
					
			}

		}

		public void MarkAdjacentColorsAsUnavailable(HalfEdge halfEdge,Constraint c){

			List<int> pi = c.GetParticlesInvolved();

			
			 // Special case for distance constraints, as we know their geometrical disposition. 
			 // So no need to iterate over all other constraints, which makes things a lot faster.

			if (c is DistanceConstraint){  

				DistanceConstraint dc = c as DistanceConstraint;
	
				// Constraints sharing p1:
				foreach (HalfEdge.HEEdge edge in halfEdge.GetNeighbourEdgesEnumerator(halfEdge.heVertices[dc.p1])){
					int adjacentColor = constraintColors[edge.index];
					if (adjacentColor >= 0) 
						availableColors[adjacentColor] = false;
				}

				// Constraints sharing p2:
				foreach (HalfEdge.HEEdge edge in halfEdge.GetNeighbourEdgesEnumerator(halfEdge.heVertices[dc.p2])){
					int adjacentColor = constraintColors[edge.index];
					if (adjacentColor >= 0) 
						availableColors[adjacentColor] = false;
				}

			}
			// For generic constraints, that we know nothing about their topological distribution,
			// we fall back to the O(n²) method.
			else{
				for(int j = 0; j < constraints.Count; j++){
					List<int> pj = constraints[j].GetParticlesInvolved();
					if (constraintColors[j] >= 0 && pi.Intersect(pj).Count() != 0){
						availableColors[constraintColors[j]] = false;
					}
				}
			}

		}

	}

}

