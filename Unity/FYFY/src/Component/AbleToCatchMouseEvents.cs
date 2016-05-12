using UnityEngine;

namespace FYFY {
	[RequireComponent(typeof(Collider))]
	[DisallowMultipleComponent]
	public class AbleToCatchMouseEvents : MonoBehaviour {
	}
}

//	private void OnMouseDown() {
//		Debug.Log ("DOWN " + this.gameObject.name);
//	}

// Make sure that your other cameras are tagged MainCamera when you switch to them

//internal static readonly HashSet<int> _affectedGameobjectIds = new HashSet<int>();
//
//private int _gameObjectId;
//
//private void Awake() {
//	_gameObjectId = this.gameObject.GetInstanceID();
//	_affectedGameobjectIds.Add(_gameObjectId);
//}
//
//private void OnDestroy() {
//	_affectedGameobjectIds.Remove(_gameObjectId);
//}