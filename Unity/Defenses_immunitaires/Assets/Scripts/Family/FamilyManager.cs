using System.Collections.Generic;

public static class FamilyManager {
	internal static readonly Dictionary<string, Family> _families = new Dictionary<string, Family>();

	public static int Count { get { return _families.Count; } }

	public static Family getFamily(params Matcher[] matchers){
		int mLength = matchers.Length;
		if (mLength == 0)
			throw new System.ArgumentException ();
		
		string[] matchersDescriptors = new string[mLength];
		for (int i = 0; i < mLength; ++i)
			matchersDescriptors[i] = matchers[i]._descriptor;
		System.Array.Sort(matchersDescriptors);

		string familyDescriptor = string.Join ("/", matchersDescriptors);

		Family family;
		if (_families.TryGetValue (familyDescriptor, out family) == false) {
			family = new Family (familyDescriptor, matchers);
			_families.Add (familyDescriptor, family);

			if (UECS.EntityManager._sceneParsed)
				throw new UnityEngine.UnityException(); // > WARNING
		}
		return family;
	}

	internal static void updateAfterComponentsUpdated(int entityWrapperId, UECS.EntityWrapper entityWrapper){
		foreach (Family family in FamilyManager._families.Values) {
			if(family.matches(entityWrapper))
				family._entityWrapperIds.Add(entityWrapperId);
			else
				family._entityWrapperIds.Remove(entityWrapperId);
		}
	}

	internal static void updateAfterEntityAdded(int entityWrapperId, UECS.EntityWrapper entityWrapper){
		foreach(Family family in FamilyManager._families.Values)
			if(family.matches(entityWrapper))
				family._entityWrapperIds.Add(entityWrapperId);
	}

	internal static void updateAfterEntityRemoved(int entityWrapperId){
		foreach(Family family in FamilyManager._families.Values)
			family._entityWrapperIds.Remove(entityWrapperId);
	}
}

// internal static readonly Dictionary<uint, List<Family>> // uid of component Type -> seulement dans le cas des ComponentTypeMatcher .....

//			foreach(Entity e in EntityManager._entities.Values){ // ENTITYMANAGER DONC FORCEMENT CONSTRUIT AVANT LES FAMILLES (STATIC CONSTRUCTOR)
//				if(family.matches(e))
//					family._entitiesIds.Add(e._gameObject.GetInstanceID());
//			}