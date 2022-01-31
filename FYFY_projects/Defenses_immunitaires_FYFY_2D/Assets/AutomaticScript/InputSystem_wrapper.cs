using UnityEngine;
using FYFY;

public class InputSystem_wrapper : BaseWrapper
{
	public UnityEngine.KeyCode _selectionButton;
	public UnityEngine.KeyCode _actionButton;
	public UnityEngine.KeyCode _multipleSelectionKey;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "_selectionButton", _selectionButton);
		MainLoop.initAppropriateSystemField (system, "_actionButton", _actionButton);
		MainLoop.initAppropriateSystemField (system, "_multipleSelectionKey", _multipleSelectionKey);
	}

}
