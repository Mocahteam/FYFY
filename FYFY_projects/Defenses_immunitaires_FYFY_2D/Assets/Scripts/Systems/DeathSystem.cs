using UnityEngine;
using FYFY;

public class DeathSystem : FSystem {
	private Family _gameObjects = FamilyManager.getFamily(
		new AllOfComponents(typeof(Death))
	);

	protected override void onPause(int currentFrame){}

	protected override void onResume(int currentFrame){}

	protected override void onProcess(int currentFrame) {
		foreach (GameObject gameObject in _gameObjects) {
			Death death = gameObject.GetComponent<Death>();

			death._progress += Time.deltaTime;
			if (death._progress < death._duration) {
				Material material = gameObject.GetComponent<Renderer>().material;
				Color color = material.color;

				float a = 1f - (death._progress / death._duration);
				material.color = new Color(color.r, color.g, color.b, a);
			} else {
				for (int i = 0; i < death._wastesNumber; ++i) {
					GameObject waste = GameObjectManager.instantiatePrefab("Waste");
					waste.transform.position = gameObject.transform.position;
					waste.GetComponent<Target>()._target = new Vector3(Random.Range(-3.5f, 3.5f), Random.Range (-2.5f, 2.5f));
					waste.transform.Rotate(new Vector3(0f, 0f, Random.Range(-180f, 180f)));
				}

				for (int i = 0; i < death._virusNumber; ++i) {
					GameObject virus = GameObjectManager.instantiatePrefab("Virus");
					virus.transform.position = gameObject.transform.position;
					virus.GetComponent<Target>()._target = new Vector3(Random.Range(-3.5f, 3.5f), Random.Range (-2.5f, 2.5f));
					virus.GetComponent<Virus>()._properties = death._virusProperties;
				}

				GameObjectManager.destroyGameObject(gameObject);
			}
		}
	}
}
