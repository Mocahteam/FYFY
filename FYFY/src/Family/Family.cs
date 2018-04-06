using UnityEngine;
using System.Collections.Generic;

namespace FYFY {
	/// <summary>
	/// 	<c>Family</c> is a container of <c>GameObjects</c> which respect constraints specified by <see cref="FYFY.Matcher"/>.
	/// </summary>
	/// <remarks>
	/// 	<para>The family is updated before each <c>FixedUpdate</c>, <c>Update</c>, <c>LateUpdate</c> blocks of the <see cref="FYFY.MainLoop"/>.</para>
	/// 	<para>The family state is the same for each system in a same block of update.</para>
	/// 	<para>
	/// 		The family works only with the <c>GameObjects</c> known by <c>FYFY</c> (create in editor outside runtime or in code with 
	/// 		<see cref="FYFY.GameObjectManager">functions</see>).
	/// 	</para>
	/// </remarks>
	public class Family : IEnumerable<GameObject> {
		/// <summary>
		/// 	Type of the entry callbacks.
		/// </summary>
		public delegate void EntryCallback(GameObject gameObject);
		/// <summary>
		/// 	Type of the exit callbacks.
		/// </summary>
		public delegate void ExitCallback(int gameObjectId);
		/// <summary>
		/// 	Show game objects included into this family
		/// </summary>
		public bool showContent = false;
		
		private Dictionary<int, int> _gameObjectIdToCacheId; // store for each GameObject id its ref inside cache
		private List<GameObject> _cashedGameObjects;
		private int _count;
		
		private Matcher[] _matchers;
		internal EntryCallback _entryCallbacks; // callback dissociated from the system so executed even if the system is paused
		internal ExitCallback _exitCallbacks;   // callback dissociated from the system so executed even if the system is paused

		internal Family(Matcher[] matchers){
			_gameObjectIdToCacheId = new Dictionary<int, int>();
			_cashedGameObjects = new List<GameObject>();
			_count = 0;
			_matchers = matchers;
			_entryCallbacks = null;
			_exitCallbacks = null;
		}
		
		internal bool Add (int gameObjectId, GameObject gameObject){
			// Check if this GameObjectId is already known
			if (contains(gameObjectId))
				return false;
			// Add the GameObject at the end of the cache
			_cashedGameObjects.Add(gameObject);
			// Add entry into dictionary to store GameObject position into the cache
			_gameObjectIdToCacheId.Add(gameObjectId, _count);
			_count++;
			return true;
		}

		internal bool Remove (int gameObjectId){
			// Check if this GameObjectId is not known
			if (!contains(gameObjectId))
				return false;
			// Get position into caches
			int cachePos = _gameObjectIdToCacheId[gameObjectId];
			if (cachePos < _count){
				_gameObjectIdToCacheId.Remove(gameObjectId);
				// If we don't remove the last element we copy the last element into this new hole and we remove the last element
				if (cachePos < _count-1){
					if (_cashedGameObjects[_count-1] != null){
						_cashedGameObjects[cachePos] = _cashedGameObjects[_count-1];
						// Update cache position of this GameObject
						_gameObjectIdToCacheId[_cashedGameObjects[cachePos].GetInstanceID()] = cachePos;
					}
				}
				_cashedGameObjects.RemoveAt(_count-1);
			} else {
				throw new System.Exception("No GameObject cached for this gameObjectId.");
			}
			_count--;
			return true;
		}
		
		/// <summary>
		/// 	Gets the number of <c>GameObjects</c> belonging to this <see cref="FYFY.Family"/>.
		/// </summary>
		public int Count { get { return _count; } }

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator(){
			return this.GetEnumerator();
		}

		/// <summary>
		/// 	Gets the enumerator over this <see cref="FYFY.Family"/>.
		/// </summary>
		/// <returns>
		/// 	The enumerator.
		/// </returns>
		public IEnumerator<GameObject> GetEnumerator(){
			foreach(GameObject go in _cashedGameObjects){
				if (go)
					yield return go;
				else
					Debug.LogWarning("Family includes null values, this means you forget to unbind game objects before destroying them. See \"FYFY.GameObjectManager.unbind(GameObject gameObject)\".");
			}
		}
		
		/// <summary>
		/// 	Gets the GameObject at the specified index <see cref="FYFY.Family"/>.
		///		Warning: this function can return null if a Game Object is Destroyed without beeing unbinded, you still have to unbind Game Object before destroying them.
		/// </summary>
		/// <returns>
		/// 	The GameObject at the specified index.
		/// </returns>
		public GameObject getAt(int index){
			return _cashedGameObjects[index];
		}

		/// <summary>
		/// 	Checks if a <c>GameObject</c> belongs to this <see cref="FYFY.Family"/>.
		/// </summary>
		/// <param name="gameObjectId">
		/// 	The Game object identifier.
		/// </param>
		public bool contains(int gameObjectId) {
			return _gameObjectIdToCacheId.ContainsKey(gameObjectId);
		}

		/// <summary>
		/// 	Adds callback function which will be executed when an entry occurs in this <see cref="FYFY.Family"/>.
		/// </summary>
		/// <param name="callback">
		/// 	Callback function.
		/// </param>
		public void addEntryCallback(EntryCallback callback) {
			_entryCallbacks += callback;
		}

		/// <summary>
		/// 	Adds callback function which will be executed when an exit occurs in this <see cref="FYFY.Family"/>.
		/// </summary>
		/// <param name="callback">
		///		Callback function.
		/// </param>
		public void addExitCallback(ExitCallback callback) {
			_exitCallbacks += callback;
		}
		
		/// <summary>
		/// 	Check if two families are equals.
		/// </summary>
		public bool Equals(Family other){
			if (_matchers.Length != other._matchers.Length)
				return false;
			return this.IncludedInto(other) && other.IncludedInto(this);
		}
		
		/// <summary>
		///		Get the first Game Object included into the family
		/// </summary>
		/// <returns>
		/// 	The first GameObject or null if the family is empty.
		/// </returns>
		public GameObject First(){
			IEnumerator<GameObject> goEnum = GetEnumerator();
			if (goEnum.MoveNext())
				return goEnum.Current;
			else
				return null;
		}
		
		// Check if "this" is included into "other"
		private bool IncludedInto(Family other){
			for (int i = 0; i < _matchers.Length; ++i){
				bool found = false;
				for (int j = 0 ; !found && j < other._matchers.Length ; j++){
					if (_matchers[i]._descriptor == other._matchers[j]._descriptor)
						found = true;
				}
				if (!found)
					return false;
			}
			return true;
		}

		internal bool matches(GameObjectWrapper gameObjectWrapper) {
			for (int i = 0; i < _matchers.Length; ++i)
				if (_matchers[i].matches(gameObjectWrapper) == false) 
					return false;
			return true;
		}
	}
}