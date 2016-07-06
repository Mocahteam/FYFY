using System.Collections.Generic;

namespace FYFY {
	/// <summary>
	/// 	Manager of <see cref="FYFY.Family"/>.
	/// </summary>
	public static class FamilyManager {
		internal static readonly Dictionary<string, Family> _families = new Dictionary<string, Family>(); // key is familyDescriptor ie the ordered aggregation of the descriptions of its matchers

		/// <summary>
		/// 	Gets the number of families created.
		/// </summary>
		public static int Count { get { return _families.Count; } }

		/// <summary>
		/// 	Gets the family defined by a set of <see cref="FYFY.Matcher"/>.
		/// </summary>
		/// <remarks>
		/// 	<para>
		/// 		You get always a family that is initialized, ie which contains the <c>GameObjects</c>
		///			of the actual scene which respect all the contraints and which are known by <c>FYFY</c>.
		/// 		So you can parse it directly.
		/// 	</para>
		/// 	<para>
		/// 		To be known by <c>FYFY</c>, a <c>GameObject</c> must be created in editor outside runtime
		/// 		or in code with <see cref="FYFY.GameObjectManager">functions</see>.
		/// 	<para>
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
					throw new System.ArgumentNullException();
				}
				matchersDescriptors[i] = matcher._descriptor;
			}
			System.Array.Sort(matchersDescriptors);

			string familyDescriptor = string.Join("/", matchersDescriptors);

			Family family;
			if(_families.TryGetValue(familyDescriptor, out family) == false) { // if its doesn't exist, create it and add it in the dict
				family = new Family(matchers);
				_families.Add(familyDescriptor, family);

				if(GameObjectManager._gameObjectWrappers.Count > 0) { // FYFY knows some GameObjects
					foreach (KeyValuePair<int, GameObjectWrapper> valuePair in GameObjectManager._gameObjectWrappers) {
						int gameObjectId = valuePair.Key;
						GameObjectWrapper gameObjectWrapper = valuePair.Value;

						if(family.matches(gameObjectWrapper)) {
							family._gameObjectIds.Add(gameObjectId);  // not need to execute callbacks because they don't exist yet
						}
					}
				}
			}
			return family;
		}

		internal static void updateAfterGameObjectModified(int gameObjectId){
			GameObjectWrapper gameObjectWrapper = GameObjectManager._gameObjectWrappers[gameObjectId];
			UnityEngine.GameObject gameObject = gameObjectWrapper._gameObject;

			foreach(Family family in FamilyManager._families.Values) {
				if(family.matches(gameObjectWrapper)) {
					if(family._gameObjectIds.Add(gameObjectId) && family._entryCallbacks != null) {
						family._entryCallbacks(gameObject); // execute family's entry callbacks on the GameObject if added
					}
				} else if(family._gameObjectIds.Remove(gameObjectId) && family._exitCallbacks != null) {
					family._exitCallbacks(gameObjectId);    // execute family's exit callbacks on the GameObject if removed
				}
			}
		}

		internal static void updateAfterGameObjectDestroyed(int gameObjectId){
			foreach(Family family in FamilyManager._families.Values) {
				if (family._gameObjectIds.Remove(gameObjectId) && family._exitCallbacks != null) {
					family._exitCallbacks(gameObjectId);    // execute family's exit callbacks on the GameObject if removed
				}
			}
		}
	}
}