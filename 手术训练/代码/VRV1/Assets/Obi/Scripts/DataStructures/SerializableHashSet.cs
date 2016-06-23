using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class SerializableHashSet<T> : IEnumerable, IList<T>
{

	[SerializeField] private List<T> elements = new List<T>();
	
	// save the set to a list
	/*public void OnBeforeSerialize()
	{
		elements.Clear();
		elements.AddRange(this);
	}
	
	// load set from list
	public void OnAfterDeserialize()
	{
		Clear();
		UnionWith(elements);
	}*/

	public T this[int index]
	{
		get { return elements[index]; }
		set { elements[index] = value; }
	}

	public int IndexOf(T item)
	{
		return elements.IndexOf(item);
	}

	public void Insert(int index, T item)
	{
		elements.Insert(index, item);
	}
	
	public void RemoveAt(int index)
	{
		elements.RemoveAt(index);
	}

	public bool IsReadOnly
	{
		get { return false; }
	}

	public void Clear()
	{
		elements.Clear();
	}

	public bool Contains(T item)
	{
		return elements.Contains(item);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		elements.CopyTo(array, arrayIndex);
	}

	public bool Remove(T item)
	{
		return elements.Remove(item);
	}

	/**
	 * Terrible workaround until unity ISerializationCallbackReceiver works properly and we can use a HashSet.
	 */
	public void AddNoDuplicates(T element){
		if (!elements.Contains(element)) 
			elements.Add(element);
	}

	public void Add(T element){
		elements.Add(element);
	}

	public void RemoveWhere(Predicate<T> match){
		elements.RemoveAll(match);
	}

	public List<T> FindAll(Predicate<T> match){
		return elements.FindAll(match);
	}

	public int Count{
		get{return elements.Count;}
	}

	public IEnumerator<T> GetEnumerator()
	{
		return elements.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
