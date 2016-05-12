using UnityEngine;

namespace FYFY {
	[DisallowMultipleComponent]
	public class TargetPositionComponent : MonoBehaviour { // : Component -> pour eviter quil utilise les fonctions des MonoBehaviours
		public float x = 0f;
		public float y = 0f;
	}
}