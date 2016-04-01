public abstract class Matcher {
	internal string _descriptor;

	public string Descriptor { get { return _descriptor; } }

	internal abstract bool matches(Entity e);
}