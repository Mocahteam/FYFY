namespace FYFY {
	/// <summary>
	///		Matcher used to exclude all the <c>GameObjects</c> which don't have
	///		a specific list of properties.
	/// </summary>
	public class AllOfProperties : PropertyMatcher {
		/// <summary>
		/// 	Initializes a new instance of the <see cref="FYFY.AllOfProperties"/> class.
		/// </summary>
		public AllOfProperties(params PROPERTY[] properties) : base(properties) {
		}

		internal override bool matches(GameObjectWrapper gameObjectWrapper){
			// Prevent user error if a GameObject is destroyed while it is still binded. It's a mistake, the user has to unbind game objects before destroying them.
			if (gameObjectWrapper._gameObject != null){
				UnityEngine.GameObject gameObject = gameObjectWrapper._gameObject;

				for(int i = 0; i < _properties.Length; ++i){
					if(hasProperty(gameObject, _properties[i]) == false){
						return false;
					}
				}
				return true;
			} else
				return false;
		}
	}
}