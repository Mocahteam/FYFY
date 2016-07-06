namespace FYFY {
	/// <summary>
	///		Matcher used to exclude all the <c>GameObjects</c> which don't have
	///		at least one component among a specific list of components.
	/// </summary>
	public class AnyOfComponents: ComponentMatcher {
		public AnyOfComponents(params System.Type[] componentTypes) : base(componentTypes) {
		}

		internal override bool matches(GameObjectWrapper gameObjectWrapper){
			for(int i = 0; i < _componentTypeIds.Length; ++i){
				if(gameObjectWrapper._componentTypeIds.Contains(_componentTypeIds[i]) == true){
					return true;
				}
			}
			return false;
		}
	}
}