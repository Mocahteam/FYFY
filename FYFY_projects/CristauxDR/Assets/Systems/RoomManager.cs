using UnityEngine;
using FYFY;
using FYFY_plugins.TriggerManager;
using FYFY_plugins.Monitoring;

public class RoomManager : FSystem {

	private Family insideRooms = FamilyManager.getFamily(new AllOfComponents(typeof(Triggered3D), typeof(ComponentMonitoring)), new AnyOfTags("Room"));
	private Family outsideRooms = FamilyManager.getFamily(new NoneOfComponents(typeof(Triggered3D)), new AllOfComponents(typeof(ComponentMonitoring)), new AnyOfTags("Room"));

	public RoomManager (){
		insideRooms.addEntryCallback (onEnterRoom);
		outsideRooms.addEntryCallback(onExitRoom);
	}

	private void onEnterRoom (GameObject go){
		ComponentMonitoring monitor = go.GetComponent<ComponentMonitoring> ();
		MonitoringManager.trace (monitor, "turnOn", MonitoringManager.Source.PLAYER);
	}

	private void onExitRoom (GameObject go)
	{
		ComponentMonitoring monitor = go.GetComponent<ComponentMonitoring> ();
		// We check this game object still contains a ComponentMonitoring
		if (monitor != null)
			MonitoringManager.trace (monitor, "turnOff", MonitoringManager.Source.PLAYER);
	}
}