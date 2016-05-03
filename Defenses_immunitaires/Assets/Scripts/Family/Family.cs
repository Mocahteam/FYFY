using UnityEngine;
using System.Collections.Generic;

public class Family : IEnumerable<GameObject> {
	public delegate void GameObjectsEnteredCallback(GameObject[] gameObjectsEntered);
	public delegate void GameObjectsExitedCallback(int[] gameObjectIdsExited);

	internal GameObjectsEnteredCallback _gameObjectsEnteredCallbacks;
	internal GameObjectsExitedCallback _gameObjectsExitedCallbacks;

	internal readonly string _descriptor;
	internal readonly HashSet<int> _gameObjectIds;
	internal readonly Matcher[] _matchers;
	internal readonly Queue<int> _gameObjectIdsEntered;
	internal readonly Queue<int> _gameObjectIdsExited;

	internal Family(string descriptor, Matcher[] matchers){
		_gameObjectsEnteredCallbacks = null;
		_gameObjectsExitedCallbacks = null;
		_descriptor = descriptor;
		_gameObjectIds = new HashSet<int>();
		_matchers = matchers;
		_gameObjectIdsEntered = new Queue<int>();
		_gameObjectIdsExited = new Queue<int>();
	}

	public string Descriptor { get { return _descriptor; } }
	public int Count { get { return _gameObjectIds.Count; } }

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator(){
		return this.GetEnumerator();
	}

	public IEnumerator<GameObject> GetEnumerator(){
		foreach (int id in _gameObjectIds) {
			GameObject go = EntityManager._gameObjectWrappers[id]._gameObject;
			yield return go;
		}
	}

	public void addGameObjectsEnteredCallback(GameObjectsEnteredCallback f) {
		if(f.Method.DeclaringType.IsSubclassOf(typeof(UECS.System)) == false /*|| getInstanceOf == null*/)
			throw new UnityException();

		_gameObjectsEnteredCallbacks += f;
	}

	public void addGameObjectsExitedCallback(GameObjectsExitedCallback f) {
		if(f.Method.DeclaringType.IsSubclassOf(typeof(UECS.System)) == false /*|| getInstanceOf == null*/)
			throw new UnityException();

		_gameObjectsExitedCallbacks += f;
	}

	public bool contains(int gameObjectId) {
		return _gameObjectIds.Contains(gameObjectId);
	}

	internal void invokeGameObjectsEnteredCallbacks() {
		int enteredCount = _gameObjectIdsEntered.Count;
		if (_gameObjectsEnteredCallbacks == null || enteredCount == 0)
			return; 

		GameObject[] gameObjectsEntered = new GameObject[enteredCount];
		for (int i = 0; i < enteredCount; ++i) {
			int gameObjectId = _gameObjectIdsEntered.Dequeue();
			GameObjectWrapper gameObjectWrapper = EntityManager._gameObjectWrappers[gameObjectId];
			gameObjectsEntered[i] = gameObjectWrapper._gameObject;
		}

		_gameObjectsEnteredCallbacks(gameObjectsEntered);
	}

	internal void invokeGameObjectsExitedCallbacks() {
		int exitedCount = _gameObjectIdsExited.Count;
		if (_gameObjectsExitedCallbacks == null || exitedCount == 0)
			return;

		int[] gameObjectIdsExited = new int[exitedCount];
		for (int i = 0; i < exitedCount; ++i)
			gameObjectIdsExited[i] = _gameObjectIdsExited.Dequeue();

		_gameObjectsExitedCallbacks(gameObjectIdsExited);
	}

	internal bool matches(GameObjectWrapper gameObjectWrapper) {
		for (int i = 0; i < _matchers.Length; ++i)
			if (_matchers[i].matches(gameObjectWrapper) == false)
				return false;
		return true;
	}
}

// property vs autoproperty
// internal marche vraiment uniquement dans la dll ??