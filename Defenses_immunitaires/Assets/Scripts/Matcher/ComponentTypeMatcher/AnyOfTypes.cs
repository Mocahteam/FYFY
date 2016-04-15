public class AnyOfTypes : ComponentTypeMatcher {
	public AnyOfTypes(params System.Type[] componentTypes) : base(componentTypes) {
	}

	internal override bool matches(UECS.EntityWrapper ew){
		for (int i = 0; i < _componentTypeIds.Length; ++i)
			if (ew._componentTypeIds.Contains(_componentTypeIds[i]) == true)
				return true;
		return false;
	}
}