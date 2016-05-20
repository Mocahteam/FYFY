namespace FYFY {
	public class NoneOfProperties : PropertyMatcher {
		public NoneOfProperties(params PROPERTY[] properties) : base(properties) {
		}

		internal override bool matches(GameObjectWrapper gameObjectWrapper){
			UnityEngine.GameObject gameObject = gameObjectWrapper._gameObject;

			for (int i = 0; i < _properties.Length; ++i)
				if(hasProperty(gameObject, _properties[i]) == true)
					return false;
			return true;
		}
	}
}