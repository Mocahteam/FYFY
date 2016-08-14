namespace FYFY {
	/// <summary>
	///		Matcher used to exclude all the <c>GameObjects</c> which have
	///		at least one component among a specific list of components.
	/// </summary>
	public class NoneOfComponents : ComponentMatcher {
		/// <summary>
		/// 	Initializes a new instance of the <see cref="FYFY.NoneOfComponents"/> class.
		/// </summary>
		public NoneOfComponents(params System.Type[] componentTypes) : base(componentTypes) {
		}

		internal override bool matches(GameObjectWrapper gameObjectWrapper){
			for(int i = 0; i < _componentTypeIds.Length; ++i){
				if(gameObjectWrapper._componentTypeIds.Contains(_componentTypeIds[i]) == true){
					return false;
				}
			}
			return true;
		}
	}
}