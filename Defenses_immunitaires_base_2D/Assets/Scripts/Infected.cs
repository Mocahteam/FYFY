using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class Infected : MonoBehaviour {
	public VirusProperties virusProperties;

	[HideInInspector]
	public int virusNumberToCreate = 0;

	private float virusProductionProgress = 0f;
	private Health health;

	void Awake () {
		health = this.gameObject.GetComponent<Health> ();

		SpriteRenderer sr = this.gameObject.GetComponent<SpriteRenderer>();
		sr.sprite = health.infectedSprite;
	}

	void FixedUpdate () {
		health.hit(virusProperties.damages * Time.deltaTime);

		virusProductionProgress += Time.deltaTime;
		if (virusProductionProgress >= virusProperties.productionTime) {
			++virusNumberToCreate;
			virusProductionProgress -= virusProperties.productionTime;
		}
	}
}
