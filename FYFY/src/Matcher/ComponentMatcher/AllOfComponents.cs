namespace FYFY {
	/// <summary>
	///		Matcher used to exclude all the <c>GameObjects</c> which don't have
	///		a specific list of components.
	/// </summary>
	public class AllOfComponents : ComponentMatcher {
		/// <summary>
		/// 	Initializes a new instance of the <see cref="FYFY.AllOfComponents"/> class.
		/// </summary>
		public AllOfComponents(params System.Type[] componentTypes) : base(componentTypes) {
		}

		internal override bool matches(GameObjectWrapper gameObjectWrapper){
			for(int i = 0; i < _componentTypeNames.Length; ++i){
				if(gameObjectWrapper._componentTypeNames.Contains(_componentTypeNames[i]) == false){
					return false;
				}
			}
			return true;
		}
	}
}