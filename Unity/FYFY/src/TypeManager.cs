using System.Collections.Generic;

namespace FYFY {
	internal static class TypeManager {
		private static uint _typeIdNumber = 0;
		private static readonly Dictionary<string, uint> _typeToId = new Dictionary<string, uint>();

		//* <summary>
		//* 	Gets the <c>Type</c> identifier.
		//* </summary>
		//* <remarks>
		//* 	<para>The identifier is unique over the run. Two differents <c>Types</c> don't have the same identifier.</para>
		//* 	<para>Used only to manage Component type.</para>
		//* </remarks>
		internal static uint getTypeId(System.Type type){
			uint id;
			string typeFullName = type.FullName;
			if (_typeToId.TryGetValue(typeFullName, out id) == false) {
				id = _typeIdNumber++;
				_typeToId.Add (typeFullName, id);
			}
			return id;
		}
	}
}