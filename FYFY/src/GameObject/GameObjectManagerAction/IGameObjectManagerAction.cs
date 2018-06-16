using UnityEngine;

namespace FYFY {
	internal interface IGameObjectManagerAction {
		void perform();
		GameObject getTarget();
	}
}