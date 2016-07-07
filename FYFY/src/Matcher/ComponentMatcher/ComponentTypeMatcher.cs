namespace FYFY {
	/// <summary>
	/// 	Base class every matcher working on <c>Component</c> derives from.
	/// </summary>
	public abstract class ComponentMatcher : Matcher {
		protected readonly uint[] _componentTypeIds; // targets used to match

		public ComponentMatcher(params System.Type[] componentTypes) {
			int ctLength = componentTypes.Length;
			if(ctLength == 0) {
				throw new System.ArgumentException();
			}

			uint[] componentTypeIds = new uint[ctLength];
			for(int i = 0; i < ctLength; ++i) {
				System.Type componentType = componentTypes[i];
				if(componentType == null) {
					throw new System.ArgumentNullException();
				}

				componentTypeIds[i] = TypeManager.getTypeId(componentType);
			}
			System.Array.Sort(componentTypeIds);

			System.Text.StringBuilder descriptor = new System.Text.StringBuilder(this.GetType() + ":" + componentTypeIds[0]);
			for(int i = 1; i < ctLength; ++i){
				descriptor.AppendFormat("/{0}", componentTypeIds[i]);
			}
			
			_descriptor = descriptor.ToString();
			_componentTypeIds = componentTypeIds;
		}
	}
}