using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;
using FYFY_plugins.Monitoring;
using System.Collections.Generic;

public class RoomManager : FSystem {

	private Family insideRooms = FamilyManager.getFamily(new AllOfComponents(typeof(Triggered3D), typeof(ComponentMonitoring)), new AnyOfTags("Room"));
	private Dictionary<int, GameObject> id2GO = new Dictionary<int, GameObject>();

	public RoomManager (){
		insideRooms.addEntryCallback (onEnterRoom);
		insideRooms.addExitCallback (onExitRoom);
	}

	private void onEnterRoom (GameObject go){
		ComponentMonitoring monitor = go.GetComponent<ComponentMonitoring> ();
		monitor.trace ("turnOn", MonitoringManager.Source.PLAYER);
		id2GO.Add (go.GetInstanceID(), go);
	}

	private void onExitRoom (int goId){
		GameObject go;
		if (id2GO.TryGetValue(goId, out go)){
			ComponentMonitoring monitor = go.GetComponent<ComponentMonitoring> ();
			// We check this game object still contains a ComponentMonitoring
			if (monitor != null)
				monitor.trace ("turnOff", MonitoringManager.Source.PLAYER);
			id2GO.Remove (goId);
		}
	}
}