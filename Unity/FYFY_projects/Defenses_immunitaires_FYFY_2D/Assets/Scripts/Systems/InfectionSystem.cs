using UnityEngine;
using FYFY;

public class InfectionSystem : FSystem {
	private Family _gameObjects;

	public InfectionSystem() {
		_gameObjects = FamilyManager.getFamily(
			new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED),
			new AllOfComponents(typeof(Infected), typeof(Health)),
			new NoneOfComponents(typeof(Death))
		);

		foreach (GameObject gameObject in _gameObjects) {
			Health health = gameObject.GetComponent<Health>();
			SpriteRenderer sr = gameObject.GetComponent<SpriteRenderer>();

			sr.sprite = health._infectedSprite;
		}

		_gameObjects.addEntryCallback(delegate(GameObject gameObject) {
			Health health = gameObject.GetComponent<Health>();
			SpriteRenderer sr = gameObject.GetComponent<SpriteRenderer>();

			sr.sprite = health._infectedSprite;
		});
	}

	protected override void onPause(int currentFrame) {}

	protected override void onResume(int currentFrame) {}

	protected override void onProcess(int currentFrame) {
		foreach (GameObject gameObject in _gameObjects) {
			Infected infected = gameObject.GetComponent<Infected>();

			Health.hit(gameObject, infected._virusProperties._damages * Time.deltaTime);

			infected._virusProductionProgress += Time.deltaTime;
			if (infected._virusProductionProgress >= infected._virusProperties._productionTime) {
				++infected._virusNumberToCreate;
				infected._virusProductionProgress -= infected._virusProperties._productionTime;
			}
		}
	}
}