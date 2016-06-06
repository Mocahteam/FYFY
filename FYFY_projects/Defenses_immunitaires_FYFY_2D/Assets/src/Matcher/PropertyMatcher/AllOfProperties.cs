namespace FYFY {
	public class AllOfProperties : PropertyMatcher {
		public AllOfProperties(params PROPERTY[] properties) : base(properties) {
		}

		internal override bool matches(GameObjectWrapper gameObjectWrapper){
			UnityEngine.GameObject gameObject = gameObjectWrapper._gameObject;

			for(int i = 0; i < _properties.Length; ++i)
				if(hasProperty(gameObject, _properties[i]) == false)
					return false;
			return true;
		}
	}
}