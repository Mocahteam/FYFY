public abstract class TagMatcher : Matcher {
	protected readonly string[] _tags;

	public TagMatcher(params string[] tags) {
		int ctLength = tags.Length;
		if (ctLength == 0)
			throw new System.ArgumentException();
		System.Array.Sort(tags);

		System.Text.StringBuilder descriptor = new System.Text.StringBuilder(this.GetType() + ":" + tags[0]);
		for (int i = 1; i < ctLength; ++i)
			descriptor.AppendFormat("/{0}", tags[i]);

		_tags = tags;
		_descriptor = descriptor.ToString();
	}
}