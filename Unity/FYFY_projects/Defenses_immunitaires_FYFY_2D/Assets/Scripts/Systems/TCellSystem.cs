using UnityEngine;
using FYFY;
using FYFY_plugins.Trigger;
using System.Collections.Generic;

public class TCellSystem : FSystem {
	private Family _tcells = FamilyManager.getFamily(
		new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED),
		new AnyOfTags("TCell"),
		new AllOfComponents(typeof(Triggered2D)),
		new NoneOfComponents(typeof(Death))
	);

	protected override void onPause(int currentFrame) {}

	protected override void onResume(int currentFrame) {}

	protected override void onProcess(int currentFrame) {
		HashSet<GameObject> dead = new HashSet<GameObject>();

		foreach(GameObject tcell in _tcells) {
			Triggered2D t2d = tcell.GetComponent<Triggered2D>();
			foreach(GameObject other in t2d.Others) {
				if(other.GetComponent<Death>() != null || dead.Contains(other))
					continue;

				Infected infected = other.GetComponent<Infected>();
				Health health = other.GetComponent<Health>();
				if( infected != null && health != null) {
					infected._virusNumberToCreate = 0;
					Health.hit(other, float.PositiveInfinity);
					dead.Add(other);
				}
			}
		}
	}
}
