namespace FYFY {
	/// <summary>
	/// 	Base class every matcher derives from.
	/// </summary>
	/// <remarks>
	///		A matcher is a filter on <c>GameObject</c> features which allows to specify on what a family works.
	/// </remarks>
	public abstract class Matcher {
		internal string _descriptor; // used to define the family descriptor in the FamilyManager to store Families
		
		/// <summary>
		/// 	Gets the string description of this <see cref="FYFY.Matcher"/>.
		/// </summary>
		/// <remarks>
		///		It is composed of its type and the values on which it operates.
		/// </remarks>
		public string Descriptor { get { return _descriptor; } }

		internal abstract bool matches(GameObjectWrapper gameObjectWrapper);
	}
}