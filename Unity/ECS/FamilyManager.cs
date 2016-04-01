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
		}
		return family;
	}
}

// Gerer la creation des familles ie creation milieu de jeu ?!? 
// Comment etre certain qu elle est cree avant toute creation d entite ??