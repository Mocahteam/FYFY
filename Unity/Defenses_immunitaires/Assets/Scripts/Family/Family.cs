using UnityEngine;
using System.Collections.Generic;

public class Family : IEnumerable<GameObject> {
	public delegate void OnEntityEnteredCallback();
	public delegate void OnEntityExitedCallback();

	internal OnEntityEnteredCallback _onEntityEnteredCallbacks;
	internal OnEntityExitedCallback _onEntityExitedCallbacks;

	internal readonly string _descriptor;
	internal readonly HashSet<int> _entityWrapperIds;
	internal readonly Matcher[] _matchers;
	internal readonly Queue<int> _enteredEntities;
	internal readonly Queue<int> _exitedEntities;

	internal Family(string descriptor, Matcher[] matchers){
		_onEntityEnteredCallbacks = null;
		_onEntityExitedCallbacks  = null;
		_descriptor = descriptor;
		_entityWrapperIds = new HashSet<int> ();
		_matchers = matchers;
		_enteredEntities = new Queue<int>();
		_exitedEntities = new Queue<int>();
	}

	public string Descriptor { get { return _descriptor; } }
	public int Count { get { return _entityWrapperIds.Count; } }

	public void addOnEntityEnteredCallback(OnEntityEnteredCallback f) {
		_onEntityEnteredCallbacks += f;
	}

	public void addOnEntityExitedCallback(OnEntityExitedCallback f) {
		_onEntityExitedCallbacks += f;
	}

	internal bool matches(UECS.EntityWrapper ew) {
		for (int i = 0; i < _matchers.Length; ++i)
			if (_matchers [i].matches(ew) == false)
				return false;
		return true;
	}

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator(){
		return this.GetEnumerator();
	}

	public IEnumerator<GameObject> GetEnumerator(){
		foreach (int id in _entityWrapperIds) {
			GameObject go = UECS.EntityManager._entityWrappers[id]._gameObject;
			yield return go;
		}
	}
}

// Famille -> signaler quand entrée / sortie dans la famille, car on veut pe qu'un systeme travaille sur une famille quand une entite rentre ou sort
// property vs autoproperty
// equivalent to _entityAdded : Signal in Genome
// internal marche vraiment uniquement dans la dll ??