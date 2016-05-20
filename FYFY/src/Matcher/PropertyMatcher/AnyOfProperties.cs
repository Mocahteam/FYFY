namespace FYFY {
	public class AnyOfProperties : PropertyMatcher {
		public AnyOfProperties(params PROPERTY[] properties) : base(properties) {
		}

		internal override bool matches(GameObjectWrapper gameObjectWrapper){
			UnityEngine.GameObject gameObject = gameObjectWrapper._gameObject;

			for (int i = 0; i < _properties.Length; ++i)
				if(hasProperty(gameObject, _properties[i]) == true)
					return true;
			return false;
		}
	}
}