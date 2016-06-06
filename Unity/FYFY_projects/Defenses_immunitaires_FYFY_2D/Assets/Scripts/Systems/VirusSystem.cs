using UnityEngine;
using FYFY;
using FYFY_plugins.Trigger;
using System.Collections.Generic;

public class VirusSystem : FSystem {
	private Family _virus = FamilyManager.getFamily(
		new AnyOfTags("Virus"),
		new AllOfComponents(typeof(Triggered2D)),
		new NoneOfComponents(typeof(Death))
	);

	protected override void onPause(int currentFrame){}

	protected override void onResume(int currentFrame){}

	protected override void onProcess(int currentFrame) {
		HashSet<GameObject> infectedGameObjects = new HashSet<GameObject>();

		foreach(GameObject virus in _virus) {
			Triggered2D t2D = virus.GetComponent<Triggered2D>();
			VirusProperties virusProperties = virus.GetComponent<Virus>()._properties;

			foreach(GameObject other in t2D.Others) {
				bool dead = other.GetComponent<Death>() != null;
				bool infected = other.GetComponent<Infected>() != null || infectedGameObjects.Contains(other);
				bool tagIsValid = (other.tag == "StructureCell" || other.tag == "BCell" || other.tag == "TCell" || other.tag == "Macrophage" || other.tag == "Bactery");

				if(dead != true && infected != true && tagIsValid == true && Random.value < virusProperties._infectionChance) {
					infectedGameObjects.Add(other);
					GameObjectManager.addComponent<Infected>(other, new { _virusProperties = virusProperties });
					GameObjectManager.destroyGameObject(virus);
					break;
				}
			}
		}
	}
}
