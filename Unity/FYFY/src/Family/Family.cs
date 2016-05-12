using UnityEngine;
using System.Collections.Generic;

namespace FYFY {
	public class Family : IEnumerable<GameObject> {
		internal readonly string _descriptor;
		internal readonly HashSet<int> _gameObjectIds;
		internal readonly Matcher[] _matchers;
		internal readonly List<int> _entries;
		internal readonly List<int> _exits;

		internal Family(string descriptor, Matcher[] matchers){
			_descriptor = descriptor;
			_gameObjectIds = new HashSet<int>();
			_matchers = matchers;
			_entries = new List<int>();
			_exits = new List<int>();
		}

		public string Descriptor { get { return _descriptor; } }
		public int Count         { get { return _gameObjectIds.Count; } }
		public int EntriesCount  { get { return _entries.Count; } }
		public int ExitsCount    { get { return _exits.Count; } }

		global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator(){
			return this.GetEnumerator();
		}

		public IEnumerator<GameObject> GetEnumerator(){
			foreach(int id in _gameObjectIds)
				yield return GameObjectManager._gameObjectWrappers[id]._gameObject;
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