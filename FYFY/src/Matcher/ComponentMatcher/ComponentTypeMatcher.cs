namespace FYFY {
	/// <summary>
	/// 	Base class every matcher working on <c>Component</c> derives from.
	/// </summary>
	public abstract class ComponentMatcher : Matcher {
		/// <summary>
		/// 	Targets used to match.
		/// </summary>
		protected readonly string[] _componentTypeNames;

		internal ComponentMatcher(params System.Type[] componentTypes) {
			int ctLength = componentTypes.Length;
			if(ctLength == 0) {
				throw new System.ArgumentException("It is not allowed to provide a ComponentMatcher without at least one Component type defined.");
			}

			string[] componentTypeNames = new string[ctLength];
			for(int i = 0; i < ctLength; ++i) {
				System.Type componentType = componentTypes[i];
				if(componentType == null) {
					throw new System.ArgumentNullException("One of the Component type is null");
				}

				componentTypeNames[i] = componentType.FullName;
			}
			System.Array.Sort(componentTypeNames);

			System.Text.StringBuilder descriptor = new System.Text.StringBuilder(this.GetType() + ":" + componentTypeNames[0]);
			for(int i = 1; i < ctLength; ++i){
				descriptor.AppendFormat("/{0}", componentTypeNames[i]);
			}
			
			_descriptor = descriptor.ToString();
			_componentTypeNames = componentTypeNames;
		}
	}
}