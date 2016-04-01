public abstract class ComponentTypeMatcher : Matcher {
	protected readonly uint[] _componentTypeIds;

	public ComponentTypeMatcher(params System.Type[] componentTypes) { // NE MAITRISE PAS QUEL TYPE DONC
		int ctLength = componentTypes.Length;
		if (ctLength == 0)
			throw new System.ArgumentException ();

		uint[] componentTypeIds = new uint[ctLength];
		for(int i = 0; i < ctLength; ++i)
			componentTypeIds[i] = TypeManager.getTypeId(componentTypes[i]);
		System.Array.Sort(componentTypeIds);

		System.Text.StringBuilder descriptor = new System.Text.StringBuilder (this.GetType() + ":" + componentTypeIds[0]);
		for (int i = 1; i < ctLength; ++i)
			descriptor.AppendFormat("/{0}", componentTypeIds [i]);

		_descriptor = descriptor.ToString();
		_componentTypeIds = componentTypeIds;
	}
}

public class AllOfTypes : ComponentTypeMatcher {
	public AllOfTypes(params System.Type[] componentTypes) : base(componentTypes) {
	}

	internal override bool matches(Entity e){
		for (int i = 0; i < _componentTypeIds.Length; ++i)
			if (e._componentTypeIds.Contains(_componentTypeIds[i]) == false)
				return false;
		return true;
	}
}

public class NoneOfTypes : ComponentTypeMatcher {
	public NoneOfTypes(params System.Type[] componentTypes) : base(componentTypes) {
	}

	internal override bool matches(Entity e){
		for (int i = 0; i < _componentTypeIds.Length; ++i)
			if (e._componentTypeIds.Contains(_componentTypeIds[i]) == true)
				return false;
		return true;
	}
}

public class AnyOfTypes : ComponentTypeMatcher {
	public AnyOfTypes(params System.Type[] componentTypes) : base(componentTypes) {
	}

	internal override bool matches(Entity e){
		for (int i = 0; i < _componentTypeIds.Length; ++i)
			if (e._componentTypeIds.Contains(_componentTypeIds[i]) == true)
				return true;
		return false;
	}
}