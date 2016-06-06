namespace FYFY {
	public abstract class LayerMatcher : Matcher {
		protected readonly int[] _layers;

		public LayerMatcher(params int[] layers) {
			int lLength = layers.Length;
			if (lLength == 0)
				throw new System.ArgumentException();

			System.Array.Sort(layers);

			System.Text.StringBuilder descriptor = new System.Text.StringBuilder(this.GetType() + ":" + layers[0]);
			for (int i = 1; i < lLength; ++i)
				descriptor.AppendFormat("/{0}", layers[i]);

			_descriptor = descriptor.ToString();
			_layers = layers;
		}
	}
}