using UnityEngine;
using FYFY;

public class MovingSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject hero_GO;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "hero_GO", hero_GO);
	}

}
