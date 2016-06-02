using UnityEngine;
using System.Collections.ObjectModel;

namespace FYFY_plugins.Trigger {
	public abstract class Triggered : MonoBehaviour {
		protected ReadOnlyCollection<GameObject> _others;

		public ReadOnlyCollection<GameObject> Others { get { return _others; } }
	}
}