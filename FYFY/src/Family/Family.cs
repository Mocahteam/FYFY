using UnityEngine;
using System.Collections.Generic;

namespace FYFY {
	/// <summary>
	/// 	<c>Family</c> is a container of <c>GameObjects</c> which respect contraints specified by <see cref="FYFY.Matcher"/>.
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

		internal readonly HashSet<int> _gameObjectIds;
		internal readonly Matcher[] _matchers;
		internal EntryCallback _entryCallbacks; // callback dissociated from the system so executed even if the system is paused
		internal ExitCallback _exitCallbacks;   // callback dissociated from the system so executed even if the system is paused

		internal Family(Matcher[] matchers){
			_gameObjectIds = new HashSet<int>();
			_matchers = matchers;
			_entryCallbacks = null;
			_exitCallbacks = null;
		}

		/// <summary>
		/// 	Gets the number of <c>GameObjects</c> belonging to this <see cref="FYFY.Family"/>.
		/// </summary>
		public int Count { get { return _gameObjectIds.Count; } }

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
			foreach(int gameObjectId in _gameObjectIds)
				yield return GameObjectManager._gameObjectWrappers[gameObjectId]._gameObject;
		}

		/// <summary>
		/// 	Checks if a <c>GameObject</c> belongs to this <see cref="FYFY.Family"/>.
		/// </summary>
		/// <param name="gameObjectId">
		/// 	The Game object identifier.
		/// </param>
		public bool contains(int gameObjectId) {
			return _gameObjectIds.Contains(gameObjectId);
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