using System.Collections.Generic;

namespace FYFY {
	public static class FamilyManager {
		internal static readonly Dictionary<string, Family> _families = new Dictionary<string, Family>();

		public static int Count { get { return _families.Count; } }

		public static Family getFamily(params Matcher[] matchers){
			int mLength = matchers.Length;
			if (mLength == 0)
				throw new global::System.ArgumentException();
			
			string[] matchersDescriptors = new string[mLength];
			for (int i = 0; i < mLength; ++i)
				matchersDescriptors[i] = matchers[i]._descriptor;
			global::System.Array.Sort(matchersDescriptors);

			string familyDescriptor = string.Join("/", matchersDescriptors);

			Family family;
			if (_families.TryGetValue(familyDescriptor, out family) == false) {
				family = new Family (familyDescriptor, matchers);
				_families.Add(familyDescriptor, family);

				if(GameObjectManager._gameObjectWrappers.Count > 0) {
					// UnityEngine.Debug.LogWarning("FAMILY " + family._descriptor);

					foreach (KeyValuePair<int, GameObjectWrapper> valuePair in GameObjectManager._gameObjectWrappers) {
						int gameObjectId = valuePair.Key;
						GameObjectWrapper gameObjectWrapper = valuePair.Value;

						if (family.matches (gameObjectWrapper)) {
							//
							if(family._gameObjectIds.Contains(gameObjectId))
								UnityEngine.Debug.LogWarning ("WTFFFFFFFFFFFFF");
							//
							family._gameObjectIds.Add(gameObjectId);
							family._entries.Add (gameObjectId);
						}
					}
				}
			}
			return family;
		}

		internal static void updateAfterGameObjectModified(int gameObjectId){
			GameObjectWrapper gameObjectWrapper = GameObjectManager._gameObjectWrappers[gameObjectId];

			foreach (Family family in FamilyManager._families.Values) {
				if (family.matches(gameObjectWrapper)) {
					if (family._gameObjectIds.Add(gameObjectId))
						family._entries.Add(gameObjectId);
				} else if(family._gameObjectIds.Remove(gameObjectId)) {
						family._exits.Add(gameObjectId);
				}
			}
		}

		internal static void updateAfterGameObjectDestroyed(int gameObjectId){
			foreach (Family family in FamilyManager._families.Values) {
				if(family._gameObjectIds.Remove(gameObjectId))
					family._exits.Add(gameObjectId);
			}
		}
	}
}