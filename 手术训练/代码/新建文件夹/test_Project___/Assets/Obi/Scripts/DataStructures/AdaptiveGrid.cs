using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Obi
{
/**
 * This class is a spatial partition structure that uses a dictionary to store cells. Particles are assigned to cells
 * using a hash comptuted from their world position. The structure can then be used to speed up nearest-neighbour queries between particles 
 * or as a broad-phase for particle-rigidbody collisions.
 */
public class AdaptiveGrid
{

	private Vector3[] neighborCellOffsets = new Vector3[]
	{  // Offsets to neighboring cells whose indices exceed this one:
		new Vector3(1,0,0),    // + , 0 , 0 ( 1)
		new Vector3(-1,1,0),   // - , + , 0 ( 2)
		new Vector3(0,1,0),    // 0 , + , 0 ( 3)
		new Vector3(1,1,0),    // + , + , 0 ( 4)
		new Vector3(-1,-1,1),  // - , - , + ( 5)
		new Vector3(0,-1,1),   // 0 , - , + ( 6)
		new Vector3(1,-1,1),   // + , - , + ( 7)
		new Vector3(-1,0,1),   // - , 0 , + ( 8)
		new Vector3(0,0,1),    // 0 , 0 , + ( 9)
		new Vector3(1,0,1),    // + , 0 , + (10)
		new Vector3(-1,1,1),   // - , + , + (11)
		new Vector3(0,1,1),    // 0 , + , + (12)
		new Vector3(1,1,1)     // + , + , + (13)
	};

	/**
	 * This class represents a single cell of an AdaptiveGrid. Holds an array of all particles associated to this cell, which may or may not be
	 * inside the cell. It is responsibility of the caller to keep the grid up to date by either calling AdaptiveGrid.UpdateParticle or manually
	 * adding and removing particles from cells where necessary.
	 */
	public class Cell{

		public List<ObiClothParticle> particles = new List<ObiClothParticle>();
		private AdaptiveGrid grid = null;
		private int hash;
		private Vector3 index;

		public AdaptiveGrid Grid{
			get{return grid;}
		}

		public int Hash{
			get{return hash;}
		}
		
		/**
		 * Returns the 3-dimensional index of this cell (readonly)
		 */
		public Vector3 Index{
			get{return index;}
		}
	
		public Cell(AdaptiveGrid grid, Vector3 position, int hash){
			this.hash = hash;
			this.grid = grid;
			this.index = position;
		}

		/**
		 * Adds to this cell a reference to the supplied particle.
		 */
		public void AddParticle(ObiClothParticle p){
			
			//Add the particle index to the cell:
			particles.Add(p); //TODO: maybe use a set, which is faster.
			
		}

		/**
		 * Removes the supplied particle from this cell, then destroys the cell if no particles are stored in it.
		 */
		public void RemoveParticle(ObiClothParticle p){

			//Remove the particle from the cell.
			particles.Remove(p);

			//If the cell is now empty, remove the cell from the grid:
			if (particles.Count == 0)
				grid.cells.Remove(hash);

		}

	}

	[NonSerialized] public Queue<int> freeIndices = new Queue<int>(); 					  /**< Queue of free indices for both neighbours and particleCellHash.*/
	[NonSerialized] public List<List<int>> neighbours = new List<List<int>>();		  	  /**< Per-particle neighbour indices. This is used in a sparse array fashion.*/
	[NonSerialized] public List<int> particleCellHash = new List<int>();				  /**< Per-particle cell hash in the grid structure. This is used in a sparse array fashion.*/

	public Dictionary<int,Cell> cells = new Dictionary<int,Cell>();		/**< A dictionary of <hash,cell> pairs that holds the whole grid structure.*/
	private float cellSize = 0.25f;										/**< Size of a grid cell.*/

	/**
	 * Returns cell size in world units (readonly).
	 */
	public float CellSize{
		get{ return cellSize; }
	}

	public AdaptiveGrid(float cellSize){
		this.cellSize = cellSize;
	}

	/**
	 * Queries the grid structure to find cells completely or partially inside the supplied bounds.
	 * \param cellIndex 3-dimensional index of cell. It doesn't have to be an existing cell, can be one that hasn't been created yet.
	 * \return an integer that uniquely identifies the cell.
	 */
	public int ComputeCellHash(Vector3 cellIndex){
		return (int) (541*cellIndex.x + 79*cellIndex.y + 31*cellIndex.z);
	}

	/**
	 * Use this to find out which cell (existing or not) contains a position in world space
	 * \param position world space position.
	 * \return 3-dimensional index of the cell that contains that position.
	 */
	public Vector3 GetParticleCell(Vector3 position){

		return new Vector3(Mathf.FloorToInt(position.x / cellSize),
		                   Mathf.FloorToInt(position.y / cellSize),
		                   Mathf.FloorToInt(position.z / cellSize));

	}

	/**
	 * Queries the grid structure to find cells completely or partially inside the supplied bounds.
	 * \param bounds bounds that we are interested to get intersecting cells for.
	 * \param boundsExpansion how much to expand the bounds before checking for cell overlap.
	 * \param boundsExpansion how many cells we can afford to check for overlap after the initial guess. If there are more than this, we will just return all cells.
	 * \return the list of cells that intersect or are completely inside the supplied bounds.
	 */
	public List<Cell> GetCellsInsideBounds(Bounds bounds, float boundsExpansion, int maxCellOverlap){
		
		List<Cell> result;		

		bounds.Expand(boundsExpansion);

		int mincellX = Mathf.FloorToInt(bounds.min.x / cellSize);
		int maxcellX = Mathf.FloorToInt(bounds.max.x / cellSize);

		int mincellY = Mathf.FloorToInt(bounds.min.y / cellSize);
		int maxcellY = Mathf.FloorToInt(bounds.max.y / cellSize);

		int mincellZ = Mathf.FloorToInt(bounds.min.z / cellSize);
		int maxcellZ = Mathf.FloorToInt(bounds.max.z / cellSize);

		int cellCount = (maxcellX-mincellX)*(maxcellY-mincellY)*(maxcellZ-mincellZ);

		if (cellCount > maxCellOverlap) {
			result = new List<Cell>(cells.Values);
			return result;
		}

		// Give list an initial size equal to the upper bound of cells inside the bounds, to prevent size reallocations. 
		result = new List<Cell>(cellCount);

		Vector3 cellpos = Vector3.zero;
		Cell cell = null;

		for (int x = mincellX ; x <= maxcellX; x++){
			for (int y = mincellY ; y <= maxcellY; y++){
				for (int z = mincellZ ; z <= maxcellZ; z++){
					cellpos.Set(x,y,z);
					int hash = ComputeCellHash(cellpos);
					if (cells.TryGetValue(hash, out cell))
						result.Add(cell);
				}
			}
		}

		return result;

	}

	public List<int> GetParticleNeighbours(ObiClothParticle p){
		if (p.gridIndex >= neighbours.Count) 
			return null;
		return neighbours[p.gridIndex];
	}
	
	/**
	 * Add a particle to the grid structure.
	 * \param hash cell hash, can be computed from the cellIndex (next parameter) using ComputeCellHash.
     * \param cellIndex 3-dimensional index of the cell to add the particle to.
     * \param p the particle that should be added to the grid.
	 */
	public void AddParticle(int hash, Vector3 cellIndex, ObiClothParticle p){

		// If the particle seems to be new in town, insert it in an empty place:
		if (p.gridIndex >= particleCellHash.Count){

			// If there are free indices, allocate the particle data in its new home.
			if (freeIndices.Count > 0){
				int freeIndex = freeIndices.Dequeue();
				p.gridIndex = freeIndex;
				particleCellHash[p.gridIndex] = hash;
			}
			// If not, enlarge arrays:
			else{
				p.gridIndex = particleCellHash.Count;
				particleCellHash.Add(hash);
				neighbours.Add(new List<int>());
			}

		}
		// If it's already in the hood, just update its hash:
		else{ 
			particleCellHash[p.gridIndex] = hash;
		}

		//See if the cell exists and insert the particle in it:
		Cell cell = null;
		if (cells.TryGetValue(hash,out cell)){
			cell.AddParticle(p);
		}else{
			cell = new Cell(this,cellIndex,hash);
			cell.AddParticle(p);
			cells[hash] = cell;
		}

	}

	/**
	 * Remove a particle from the grid structure.
	 * \param hash cell hash, can be computed using ComputeCellHash.
     * \param p the particle that should be removed from the grid.
	 */
	public void RemoveParticle(int hash, ObiClothParticle p){

		//See if the cell exists, and in that case, remove the particle from it:
		Cell cell = null;
		if (cells.TryGetValue(hash,out cell)){

			cell.RemoveParticle(p);

			// Delete particle info from all lists and mark its index as free.
			if (p.gridIndex < particleCellHash.Count){
				particleCellHash[p.gridIndex] = int.MaxValue;
				neighbours[p.gridIndex].Clear();
				freeIndices.Enqueue(p.gridIndex);
			}
			
			p.gridIndex = int.MaxValue;
		}
		
	}

	/**
	 * If the supplied particle cell hash is equal to the one calculated from the supplied cellIndex, nothing is done. Else,
	 * the particle is removed from its current cell and then added to the cell indicated by cellIndex. Can be used to insert 
     * new particles in the grid, too.
	 */
	public void UpdateParticle(ObiClothParticle p, Vector3 cellIndex){

		int currentHash = int.MaxValue;
		if (p.gridIndex < particleCellHash.Count){
			currentHash = particleCellHash[p.gridIndex];
		}

		int newHash = ComputeCellHash(cellIndex);

		if (newHash != currentHash){
			RemoveParticle(currentHash,p);
			AddParticle(newHash,cellIndex,p);
		}

	}

	/**
	 * Updates the neighbours list for each particle in the grid.
	 */
	public void UpdateNeighbourLists(Func<ObiClothParticle, ObiClothParticle, bool> ShouldIgnore){
		
		Profiler.BeginSample("Grid update neighbours");

		// Clear neighbours list of all particles:
		foreach (List<int> neighbourList in neighbours){
			neighbourList.Clear();
		}
		
		foreach(AdaptiveGrid.Cell currentCell in cells.Values.ToArray()){
			
			//For each particle in the current cell:
			for (int j = 0; j < currentCell.particles.Count; j++){
				ObiClothParticle i = currentCell.particles[j];
				
				//For each particle in the current cell that follows us:
				for(int k = j+1; k < currentCell.particles.Count; k++){
					ObiClothParticle n = currentCell.particles[k];
					
					//Calculate radius of each bounding sphere using particle area contributions:
					float radius = i.radius + n.radius;
					
					if ((n.predictedPosition - i.predictedPosition).sqrMagnitude < radius*radius && !ShouldIgnore(n,i)){
						neighbours[i.gridIndex].Add(n.index);
						neighbours[n.gridIndex].Add(i.index);
					}
				}
			}
			
			
			//For each neighbour cell:
			for (int nx = 0; nx < neighborCellOffsets.Length; nx++){
				
				int hash = ComputeCellHash(currentCell.Index + neighborCellOffsets[nx]);
				
				AdaptiveGrid.Cell cell;
				if (cells.TryGetValue(hash,out cell)){
					
					//For each particle in the current cell:
					for (int j = 0; j < currentCell.particles.Count; j++){
						ObiClothParticle i = currentCell.particles[j];
						
						//For each particle in the neighbour search cell:
						for(int k = 0; k < cell.particles.Count; k++){
							ObiClothParticle n = cell.particles[k];
							
							float radius = i.radius + n.radius;
							
							if ((n.predictedPosition - i.predictedPosition).sqrMagnitude < radius*radius && !ShouldIgnore(n,i)){
								neighbours[i.gridIndex].Add(n.index);
								neighbours[n.gridIndex].Add(i.index);
							}	
						}		
					}
				}
			}
		}

		Profiler.EndSample();
		
	}

}
}

