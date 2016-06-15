using UnityEngine;
using FYFY;
using FYFY_plugins.Trigger;
using System.Linq;

public class TestSystem : FSystem {
	private Family _gameObjects = FamilyManager.getFamily(
		new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED),
		new AllOfComponents(typeof(Triggered2D))
	);

	protected override void onPause(int currentFrame) {}

	protected override void onResume(int currentFrame) {}

	protected override void onProcess(int currentFrame) {
		foreach (GameObject go in _gameObjects) {
			if (go.GetComponent<Triggered2D>().Targets.Length == 0) {
				Debug.Log("111111111 " + go.name + "/"+ Time.frameCount);
				return;
			}

			if (go.GetComponent<Triggered2D>().Targets.Contains(null) == true) {
				Debug.Log ("222222222222");
				return;
			}
		}
	}
}
