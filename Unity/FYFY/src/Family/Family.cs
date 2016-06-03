using UnityEngine;
using System.Collections.Generic;

namespace FYFY {
	public class Family : IEnumerable<GameObject> {
		public delegate void EntryCallback(GameObject gameObject);
		public delegate void ExitCallback(int gameObjectId);

		internal readonly HashSet<int> _gameObjectIds;
		internal readonly Matcher[] _matchers;
		internal EntryCallback _entryCallbacks; // callback dissocié des systemes donc execute meme en pause (a mettre dans la doc)
		internal ExitCallback _exitCallbacks;   // donc a charge du developpeur de tester dans sa callback si le systeme est en pause ou non ;)

		internal Family(Matcher[] matchers){
			_gameObjectIds = new HashSet<int>();
			_matchers = matchers;
			_entryCallbacks = null;
			_exitCallbacks = null;
		}

		public int Count { get { return _gameObjectIds.Count; } }

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator(){
			return this.GetEnumerator();
		}

		public IEnumerator<GameObject> GetEnumerator(){
			foreach(int gameObjectId in _gameObjectIds)
				yield return GameObjectManager._gameObjectWrappers[gameObjectId]._gameObject;
		}

		public bool contains(int gameObjectId) {
			return _gameObjectIds.Contains(gameObjectId);
		}

		public void addEntryCallback(EntryCallback callback) {
			_entryCallbacks += callback;
		}

		public void addExitCallback(ExitCallback callback) {
			_exitCallbacks += callback;
		}

		internal bool matches(GameObjectWrapper gameObjectWrapper) {
			for (int i = 0; i < _matchers.Length; ++i)
				if (_matchers[i].matches(gameObjectWrapper) == false) 
					return false;
			return true;
		}
	}
}