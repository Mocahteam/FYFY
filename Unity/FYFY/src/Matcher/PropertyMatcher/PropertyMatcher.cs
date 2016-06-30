namespace FYFY {
	public abstract class PropertyMatcher : Matcher {
		public enum PROPERTY { ENABLED, DISABLED, HAS_PARENT, HAS_CHILD };

		protected readonly PROPERTY[] _properties;

		public PropertyMatcher(params PROPERTY[] properties) {
			if (properties.Length == 0) {
				throw new System.ArgumentException();
			}

			System.Array.Sort(properties);

			System.Text.StringBuilder descriptor = new System.Text.StringBuilder(this.GetType() + ":" + properties[0].ToString("d"));
			for (int i = 1; i < properties.Length; ++i)
				descriptor.AppendFormat("/{0}", properties[i].ToString("d"));

			_descriptor = descriptor.ToString();
			_properties = properties;
		}

		internal bool hasProperty(UnityEngine.GameObject gameObject, PROPERTY property) {
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