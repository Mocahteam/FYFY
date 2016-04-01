using System.Collections.Generic;

internal static class TypeManager {
	private static uint _typeIdNumber = 0;
	private static readonly Dictionary<string, uint> _typeToId = new Dictionary<string, uint>();

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