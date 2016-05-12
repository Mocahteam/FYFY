namespace FYFY {
	public abstract class TagMatcher : Matcher {
		protected readonly string[] _tags;

		public TagMatcher(params string[] tags) {
			int ctLength = tags.Length;
			if (ctLength == 0)
				throw new global::System.ArgumentException();
			global::System.Array.Sort(tags);

			global::System.Text.StringBuilder descriptor = new global::System.Text.StringBuilder(this.GetType() + ":" + tags[0]);
			for (int i = 1; i < ctLength; ++i)
				descriptor.AppendFormat("/{0}", tags[i]);

			_tags = tags;
			_descriptor = descriptor.ToString();
		}
	}
}