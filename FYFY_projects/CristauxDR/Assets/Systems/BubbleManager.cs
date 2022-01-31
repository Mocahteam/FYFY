using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;
using FYFY_plugins.CollisionManager;

public class BubbleManager : FSystem {
	private Family levers = FamilyManager.getFamily(new AllOfComponents(typeof(Triggered3D), typeof(Lever)));
	private Family inGameObjects = FamilyManager.getFamily(new AllOfComponents(typeof(Triggered3D), typeof(Takable)));
	public GameObject bubble_GO;
	public GameObject hero_GO;
	public GameObject boiler_GO;
	public GameObject iceWall_GO;
	public GameObject door_GO;
	private bool hintLever = false;
	private bool hintBoiler = false;
	private bool hintIceWall = false;
	private bool hintKey = false;
	private bool hintMatchstick = false;
	private bool hintDoor = false;
	private float startDisplaying = 0;

	// Use to process your families.
	protected override void onProcess(int familiesUpdateCount) {
		if (!hintLever) {
			// parse all levers near to hero (only hero can produce Triggered3D component thanks to Unity Physics layers)
			if (levers.Count > 0) {
				GameObjectManager.setGameObjectState(bubble_GO, true);
				TextMesh text = bubble_GO.GetComponentInChildren<TextMesh> ();
				text.text = "+2 is written\non theses levers";
				startDisplaying = Time.timeSinceLevelLoad;
				hintLever = true;
			}
		}

		if (!hintBoiler) {
			// Check if boiler is near to hero (only hero can produce Triggered3D component thanks to Unity Physics layers)
			Triggered3D triggered = boiler_GO.GetComponent<Triggered3D> ();
			if (triggered != null){
				GameObjectManager.setGameObjectState(bubble_GO, true);
				TextMesh text = bubble_GO.GetComponentInChildren<TextMesh> ();
				text.text = "Wow a boiler!\nIt is turned off.";
				startDisplaying = Time.timeSinceLevelLoad;
				hintBoiler = true;
			}
		}

		if (!hintDoor) {
			// Check if door is near to hero (only hero can produce Triggered3D component thanks to Unity Physics layers)
			Triggered3D triggered = door_GO.GetComponent<Triggered3D> ();
			if (triggered != null){
				GameObjectManager.setGameObjectState(bubble_GO, true);
				TextMesh text = bubble_GO.GetComponentInChildren<TextMesh> ();
				text.text = "This door is locked\nI need a key!";
				startDisplaying = Time.timeSinceLevelLoad;
				hintDoor = true;
			}
		}

		if (!hintIceWall && iceWall_GO.GetComponent<BoxCollider> ().enabled) {
			InCollision3D collision = iceWall_GO.GetComponent<InCollision3D> ();
			if (collision != null) {
				foreach (GameObject target in collision.Targets) {
					if (target == hero_GO) {
						GameObjectManager.setGameObjectState (bubble_GO, true);
						TextMesh text = bubble_GO.GetComponentInChildren<TextMesh> ();
						text.text = "Damn! The path\nis blocked by an\nice wall!";
						startDisplaying = Time.timeSinceLevelLoad;
						hintIceWall = true;
					}
				}
			}
		}

		if (!hintKey || !hintMatchstick) {
			// Parse all active GO takable in game and near to player (only hero can produce Triggered3D component thanks to Unity Physics layers)
			foreach (GameObject go in inGameObjects) {
				if (!hintKey && go.name == "Key") {
					GameObjectManager.setGameObjectState (bubble_GO, true);
					TextMesh text = bubble_GO.GetComponentInChildren<TextMesh> ();
					text.text = "A key, It can\nbe useful!";
					startDisplaying = Time.timeSinceLevelLoad;
					hintKey = true;
				}
				if (!hintMatchstick && go.name == "Matchstick") {
					GameObjectManager.setGameObjectState (bubble_GO, true);
					TextMesh text = bubble_GO.GetComponentInChildren<TextMesh> ();
					text.text = "Some matchsticks:\nuseful to make\na fire!";
					startDisplaying = Time.timeSinceLevelLoad;
					hintMatchstick = true;
				}
			}
		}

		if (bubble_GO.activeSelf && startDisplaying + 4 < Time.timeSinceLevelLoad) {
			GameObjectManager.setGameObjectState (bubble_GO, false);
		}
	}
}