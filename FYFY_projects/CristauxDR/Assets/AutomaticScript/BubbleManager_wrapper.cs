using UnityEngine;
using FYFY;

public class BubbleManager_wrapper : BaseWrapper
{
	public UnityEngine.GameObject bubble_GO;
	public UnityEngine.GameObject hero_GO;
	public UnityEngine.GameObject boiler_GO;
	public UnityEngine.GameObject iceWall_GO;
	public UnityEngine.GameObject door_GO;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "bubble_GO", bubble_GO);
		MainLoop.initAppropriateSystemField (system, "hero_GO", hero_GO);
		MainLoop.initAppropriateSystemField (system, "boiler_GO", boiler_GO);
		MainLoop.initAppropriateSystemField (system, "iceWall_GO", iceWall_GO);
		MainLoop.initAppropriateSystemField (system, "door_GO", door_GO);
	}

}
