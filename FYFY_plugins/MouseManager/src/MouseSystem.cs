using UnityEngine;
using FYFY;

namespace FYFY_plugins.MouseManager {
	public abstract class MouseSystem : FSystem {
		protected Family _mouseSensitiveFamily;
		protected Family _mouseOverFamily;

		public MouseSystem(){
			_mouseSensitiveFamily = FamilyManager.getFamily(new AllOfComponents(typeof(MouseSensitive)), new AnyOfProperties(PropertyMatcher.PROPERTY.ENABLED));
			_mouseOverFamily      = FamilyManager.getFamily(new AllOfComponents(typeof(MouseOver)));
		}

		protected override void onPause(int currentFrame){
		}

		protected override void onResume(int currentFrame){
		}

		protected override void onProcess(int currentFrame){
			foreach(GameObject gameObject in _mouseOverFamily)
				GameObjectManager.removeComponent<MouseOver>(gameObject);

			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // Make sure that your other cameras are tagged MainCamera when you switch to them
			GameObject[] hits = this.getHitGameObjects(ray);

			for (int i = 0; i < hits.Length; ++i)
				GameObjectManager.addComponent<MouseOver> (hits [i], new { rank = i });
		}

		protected abstract GameObject[] getHitGameObjects(Ray ray);
	}
}