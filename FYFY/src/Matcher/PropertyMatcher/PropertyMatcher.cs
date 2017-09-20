namespace FYFY {
	/// <summary>
	/// 	Base class every matcher working on <c>Unity property</c> derives from.
	/// </summary>
	/// <remarks>
	///		Matcher can filter on parentality and activation.
	/// </remarks>
	public abstract class PropertyMatcher : Matcher {
		/// <summary>
		/// 	List of properties of GameObject which can be used by the PropertyMatcher.
		/// </summary>
		public enum PROPERTY {
			/// <summary>
			/// 	The GameObject has the variables <c>activeInHierarchy</c> and <c>activeSelf</c> true.
			/// </summary>
			ENABLED,
			/// <summary>
			/// 	The GameObject has at least one of the variables <c>activeInHierarchy</c> or <c>activeSelf</c> false.
			/// </summary>
			DISABLED,
			/// <summary>
			/// 	The GameObject has one direct parent.
			/// </summary>
			HAS_PARENT, 
			/// <summary>
			/// 	The GameObject has at least one direct child.
			/// </summary>
			HAS_CHILD 
		};

		/// <summary>
		/// 	Targets used to match.
		/// </summary>
		protected readonly PROPERTY[] _properties;

		internal PropertyMatcher(params PROPERTY[] properties) {
			if(properties.Length == 0) {
				throw new System.ArgumentException("It is not allowed to provide a PropertyMatcher without at least one property defined.");
			}

			System.Array.Sort(properties);

			System.Text.StringBuilder descriptor = new System.Text.StringBuilder(this.GetType() + ":" + properties[0].ToString("d"));
			for(int i = 1; i < properties.Length; ++i){
				descriptor.AppendFormat("/{0}", properties[i].ToString("d"));
			}

			_descriptor = descriptor.ToString();
			_properties = properties;
		}

		/// <summary>
		/// 	Checks if gameobject has a certain property.
		/// </summary>
		protected bool hasProperty(UnityEngine.GameObject gameObject, PROPERTY property) {
			switch(property) {
				case PROPERTY.ENABLED:
					return (gameObject.activeInHierarchy && gameObject.activeSelf) == true;
				case PROPERTY.DISABLED:
					return (gameObject.activeInHierarchy && gameObject.activeSelf) == false;
				case PROPERTY.HAS_PARENT:
					return gameObject.transform.parent != null;
				case PROPERTY.HAS_CHILD:
					return gameObject.transform.childCount != 0;
				default:
					return false;
			}
		}
	}
}