using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FYFY_plugins.Trigger {
	public abstract class Triggered : MonoBehaviour {
		protected List<GameObject> _others;
		protected ReadOnlyCollection<GameObject> _othersReadOnly;

		public ReadOnlyCollection<GameObject> Others { 
			get {
				_others.RemoveAll(gameObject => gameObject == null); // enlever les gameobjects detruits ! (degueu mais pas dautre solution pr que ca soit invisible pour luser)
				return _othersReadOnly;
			}
		}
	}
}