using System.Collections.Generic;

namespace FYFY {
	/// <summary>
	/// 	Manager of <see cref="FYFY.Family"/>.
	/// </summary>
	public static class FamilyManager {
		// FamilyDescriptor (ie the ordered aggregation of the descriptions of family's matchers) / Family corresponding
		internal static readonly Dictionary<string, Family> _families = new Dictionary<string, Family>(); 

		internal struct StackedCallback{
			public string type; // "entry" or "exit"
			public Family family;
			public UnityEngine.GameObject gameObject;
			public int gameObjectId;
			public StackedCallback(string type, Family family, UnityEngine.GameObject gameObject, int gameObjectId){
				this.type = type;
				this.family = family;
				this.gameObject = gameObject;
				this.gameObjectId = gameObjectId;
			}
		}

		// The list of callbacks to call
		internal static List<StackedCallback> stackedCallbacks = new List<StackedCallback>();
		
		/// <summary>
		/// 	Gets the number of families created.
		/// </summary>
		public static int Count { get { return _families.Count; } }

		/// <summary>
		/// 	Gets the family defined by a set of <see cref="FYFY.Matcher"/>.
		/// </summary>
		/// <remarks>
		/// 	<para>
		/// 		A <c>Family</c> is a container of <c>GameObjects</c> which respect constraints specified by
		///			<see cref="FYFY.Matcher"/>. Only <c>GameObjects</c> <see cref="FYFY.GameObjectManager.bind">binded</see> to FYFY are available in families.
		/// 	</para>
		/// 	<para>
		/// 		This is the only way to get family reference. 
		/// 		You cannot create a <see cref="FYFY.Family"/> object by yourself.
		/// 	</para>
		/// </remarks>
		/// <returns>
		/// 	The reference of the corresponding family.
		/// </returns>
		/// <param name="matchers">
		/// 	Matchers.
		/// </param>
		public static Family getFamily(params Matcher[] matchers){
			
			int mLength = matchers.Length;
			if (mLength == 0) {
				throw new System.ArgumentException("It is not allowed to get family without at least one matcher.");
			}
			
			string[] matchersDescriptors = new string[mLength];
			for (int i = 0; i < mLength; ++i) {
				Matcher matcher = matchers[i];
				if(matcher == null) {
					throw new System.ArgumentNullException("One of the matchers is null, family recovery aborted.");
				}
				matchersDescriptors[i] = matcher._descriptor;
			}
			System.Array.Sort(matchersDescriptors);

			string familyDescriptor = string.Join("/", matchersDescriptors);

			Family family;
			// If it doesn't exist yet, create it and add it in the families dictionary.
			if(_families.TryGetValue(familyDescriptor, out family) == false) { 
				family = new Family(matchers);
				_families.Add(familyDescriptor, family);

				// Initialize with known GameObjects if they respect conditions.
				foreach(KeyValuePair<int, GameObjectWrapper> valuePair in GameObjectManager._gameObjectWrappers) {
					int gameObjectId = valuePair.Key;
					GameObjectWrapper gameObjectWrapper = valuePair.Value;

					if(family.matches(gameObjectWrapper)) {
						family.Add(gameObjectId, gameObjectWrapper._gameObject);
					}
				}
			}
			return family;
		}

		internal static void updateAfterGameObjectModified(int gameObjectId){
			if (GameObjectManager._gameObjectWrappers.ContainsKey(gameObjectId)){
				GameObjectWrapper gameObjectWrapper = GameObjectManager._gameObjectWrappers[gameObjectId];
				UnityEngine.GameObject gameObject = gameObjectWrapper._gameObject;

				// Prevent user error if a GameObject is destroyed while it is still binded. It's a mistake, the user has to unbind game objects before destroying them.
				if (gameObject != null){
					foreach(Family family in FamilyManager._families.Values) {
						if(family.matches(gameObjectWrapper)) {
							if(family.Add(gameObjectId, gameObject) && family._entryCallbacks != null) {
								// stack a family's entry callbacks on the GameObject if added
								stackedCallbacks.Add(new StackedCallback("entry", family, gameObject, -1));
							}
						} else if(family.Remove(gameObjectId) && family._exitCallbacks != null) {
							// stack a family's exit callbacks on the GameObject if removed
							stackedCallbacks.Add(new StackedCallback("exit", family, null, gameObjectId));
						}
					}
				}
			}
		}

		internal static void updateAfterGameObjectUnbinded(int gameObjectId){
			foreach(Family family in FamilyManager._families.Values) {
				if (family.Remove(gameObjectId) && family._exitCallbacks != null) {
					// stack a family's exit callbacks on the unbinded GameObject
					stackedCallbacks.Add(new StackedCallback("exit", family, null, gameObjectId));
				}
			}
		}
		
		internal static void popStackedCallbacks(){
			// call all callbacks
			foreach(StackedCallback sc in stackedCallbacks){
				if (sc.type == "entry")
					sc.family._entryCallbacks(sc.gameObject);
				else if (sc.type == "exit")
					sc.family._exitCallbacks(sc.gameObjectId);
			}
			stackedCallbacks = new List<StackedCallback>();
		}
	}
}