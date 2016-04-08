namespace UECS {
	public abstract class System {
		protected virtual void onProcess(){}

		internal void process(){
			this.onProcess();
		}
	}
}