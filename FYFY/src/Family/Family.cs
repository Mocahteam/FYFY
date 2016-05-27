using UnityEngine;
using System.Collections.Generic;

namespace FYFY {
	public class Family : IEnumerable<GameObject> {
		internal readonly HashSet<int> _gameObjectIds;
		internal readonly Matcher[] _matchers;
		internal readonly List<int> _entries;
		internal readonly List<int> _exits;

		internal Family(Matcher[] matchers){
			_gameObjectIds = new HashSet<int>();
			_matchers = matchers;
			_entries = new List<int>();
			_exits = new List<int>();
		}

		public int Count         { get { return _gameObjectIds.Count; } }
		public int EntriesCount  { get { return _entries.Count; } }
		public int ExitsCount    { get { return _exits.Count; } }

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

		public IEnumerable<GameObject> entries() {
			foreach(int gameObjectId in _entries)
				yield return GameObjectManager._gameObjectWrappers[gameObjectId]._gameObject;
		}

		public IEnumerable<int> exits() {
			return _exits;
		}

		internal bool matches(GameObjectWrapper gameObjectWrapper) {
			for (int i = 0; i < _matchers.Length; ++i)
				if (_matchers[i].matches(gameObjectWrapper) == false) 
					return false;
			return true;
		}
	}
}

// property vs autoproperty
// internal marche vraiment uniquement dans la dll ??