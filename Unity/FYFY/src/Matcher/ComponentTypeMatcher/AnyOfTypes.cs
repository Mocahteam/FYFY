namespace FYFY {
	public class AnyOfTypes : ComponentTypeMatcher {
		public AnyOfTypes(params global::System.Type[] componentTypes) : base(componentTypes) {
		}

		internal override bool matches(GameObjectWrapper gameObjectWrapper){
			for (int i = 0; i < _componentTypeIds.Length; ++i)
				if (gameObjectWrapper._componentTypeIds.Contains(_componentTypeIds[i]) == true)
					return true;
			return false;
		}
	}
}