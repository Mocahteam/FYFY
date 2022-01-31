using UnityEngine;
using FYFY;

public class TemperatureManager_wrapper : BaseWrapper
{
	public Temperature temp;
	public Boiler boiler;
	public UnityEngine.GameObject iceCollider_GO;
	public FYFY_plugins.Monitoring.ComponentMonitoring iceMonitor;
	public UnityEngine.GameObject puddle_GO;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "temp", temp);
		MainLoop.initAppropriateSystemField (system, "boiler", boiler);
		MainLoop.initAppropriateSystemField (system, "iceCollider_GO", iceCollider_GO);
		MainLoop.initAppropriateSystemField (system, "iceMonitor", iceMonitor);
		MainLoop.initAppropriateSystemField (system, "puddle_GO", puddle_GO);
	}

}
