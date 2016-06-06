using UnityEngine;
using FYFY;
using FYFY_plugins.Mouse;
using FYFY_plugins.Trigger;

public class Health : MonoBehaviour {
	public float _currentHealth;
	public float _maximumHealth;
	public int _wastesNumber;
	public Sprite _infectedSprite;

	public static bool hit(GameObject gameObject, float damages) { // fonction non securisee !!
		Health health = gameObject.GetComponent<Health>();
		RectTransform healthBarTransform = (RectTransform) gameObject.transform.FindChild("HealthCanvas").FindChild("HealthBar");

		health._currentHealth -= damages;

		float width = health._currentHealth / health._maximumHealth;
		float height = healthBarTransform.sizeDelta.y;
		healthBarTransform.sizeDelta = new Vector2(width, height);

		if (health._currentHealth <= 0f) {
			Infected infected = gameObject.GetComponent<Infected>();
			if(infected == null) {
				GameObjectManager.addComponent<Death>(gameObject, new { 
					_wastesNumber = health._wastesNumber
				});
			} else {
				GameObjectManager.addComponent<Death>(gameObject, new {
					_wastesNumber = health._wastesNumber,
					_virusNumber = infected._virusNumberToCreate,
					_virusProperties = infected._virusProperties
				});
			}
			return true;
		}
		return false;
	}
}
