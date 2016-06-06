using FYFY;
using FYFY_plugins.Mouse;
using FYFY_plugins.Trigger;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SelectionSystem : FSystem {
	private Family _selected;
	private Dictionary<int, GameObject> _gameObjects;

	public SelectionSystem() {
		_selected = FamilyManager.getFamily(
			new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED),
			new AllOfComponents(typeof(Selected)),
			new NoneOfComponents(typeof(Death))
		);

		_gameObjects = new Dictionary<int, GameObject>(); // pour remettre alpha a 1f quand lobjet sort (on a que l'id dans exits)

		_selected.addEntryCallback(delegate(GameObject gameObject) {
			_gameObjects.Add(gameObject.GetInstanceID(), gameObject);

			Material material = gameObject.GetComponent<Renderer>().material;
			material.color = new Vector4(
				material.color.r,
				material.color.g,
				material.color.b,
				0.75f
			);
		});

		_selected.addExitCallback(delegate(int gameObjectId) {
			GameObject gameObject = _gameObjects[gameObjectId];

			if (gameObject != null) {
				Material material = gameObject.GetComponent<Renderer>().material;
				material.color = new Vector4 (
					material.color.r,
					material.color.g,
					material.color.b,
					1f
				);
			}

			_gameObjects.Remove(gameObjectId);
		});
	}

	protected override void onPause(int currentFrame) {}

	protected override void onResume(int currentFrame) {}

	protected override void onProcess(int currentFrame) {}
}