public abstract class ComponentTypeMatcher : Matcher {
	protected readonly uint[] _componentTypeIds;

	public ComponentTypeMatcher(params System.Type[] componentTypes) { // NE MAITRISE PAS QUEL TYPE DONC
		int ctLength = componentTypes.Length;
		if (ctLength == 0)
			throw new System.ArgumentException();

		uint[] componentTypeIds = new uint[ctLength];
		for(int i = 0; i < ctLength; ++i)
			componentTypeIds[i] = TypeManager.getTypeId(componentTypes[i]);
		System.Array.Sort(componentTypeIds);

		System.Text.StringBuilder descriptor = new System.Text.StringBuilder(this.GetType() + ":" + componentTypeIds[0]);
		for (int i = 1; i < ctLength; ++i)
			descriptor.AppendFormat("/{0}", componentTypeIds [i]);

		_descriptor = descriptor.ToString();
		_componentTypeIds = componentTypeIds;
	}
}