using UnityEngine;

[DisallowMultipleComponent]
public class Health : MonoBehaviour {
	public float currentHealth;
	public float maximumHealth;
	public int wastesNumber;
	public Sprite infectedSprite;

	private RectTransform healthBarTransform;
	
	void Awake(){
		healthBarTransform = (RectTransform) this.gameObject.transform.FindChild ("HealthCanvas").FindChild ("HealthBar");
	}

	public void hit(float damages){
		currentHealth -= damages;

		if (currentHealth <= 0f) {
			Destroy (healthBarTransform.gameObject); // DESTROY HEAlTHBAR
			foreach (var component in this.gameObject.GetComponents<MonoBehaviour>()) // DESACTIVE ENTITY
				Destroy(component); // always delayed until after current update loop, cf doc

			Infected infected = this.gameObject.GetComponent<Infected> ();
			Death deathC = this.gameObject.AddComponent<Death>();
			deathC.virusNumber = (infected != null) ? infected.virusNumberToCreate : 0;
			deathC.virusProperties = (infected != null) ? infected.virusProperties: null;
			deathC.wastesNumber = wastesNumber;
		}
		else {
			float width = (float)currentHealth / (float)maximumHealth;
			float height = healthBarTransform.sizeDelta.y;

			healthBarTransform.sizeDelta = new Vector2 (width, height);
		}
	}
}
