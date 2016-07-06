namespace FYFY {
	/// <summary>
	///		Matcher used to exclude all the <c>GameObjects</c> which don't have
	///		a specific list of components.
	/// </summary>
	public class AllOfComponents : ComponentMatcher {
		public AllOfComponents(params System.Type[] componentTypes) : base(componentTypes) {
		}

		internal override bool matches(GameObjectWrapper gameObjectWrapper){
			for(int i = 0; i < _componentTypeIds.Length; ++i){
				if(gameObjectWrapper._componentTypeIds.Contains(_componentTypeIds[i]) == false){
					return false;
				}
			}
			return true;
		}
	}
}