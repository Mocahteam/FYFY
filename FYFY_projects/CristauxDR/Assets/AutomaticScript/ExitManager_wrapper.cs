using UnityEngine;
using FYFY;

public class ExitManager_wrapper : BaseWrapper
{
	public UnityEngine.GameObject exit_GO;
	public UnityEngine.GameObject endScreen_GO;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "exit_GO", exit_GO);
		MainLoop.initAppropriateSystemField (system, "endScreen_GO", endScreen_GO);
	}

}
