namespace UECS {
	public abstract class System {
		public bool Pause { 
			get { 
				return false;
				// recuperer la valeur du tableau des activators !!!
			}
			set { 
				// mettre a jour le tableau des activators !!!
			} 
		}

		public abstract void process(int currentFrame);

		// ensure no more than 1 singleton by subclass
	}
}




//public System () {
//	UnityEngine.Debug.Log(this.GetType());
//}