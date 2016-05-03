//public static void removeComponent<T>(GameObject gameObject) where T : Component {
//	if(ReferenceEquals(gameObject, null) || gameObject == null /* go already destroyed*/)
//		throw new MissingReferenceException();
//
//	global::System.Type componentType = typeof(T);
//	Component component = gameObject.GetComponent<T>();
//	if (component == null)
//		Debug.Log(
//			"Can't remove '" + componentType + "' from " + gameObject.name + 
//			" because a '" + componentType + "' is'nt attached to the game object!"
//		);
//
//	int entityWrapperId = gameObject.GetInstanceID();
//	uint componentTypeId = TypeManager.getTypeId(componentType);
//	_entityWrappers[entityWrapperId]._componentTypeIds.Remove(componentTypeId);
//
//	Object.Destroy(component);
//}
//
//public static void removeComponent(Component component) {
//	if(ReferenceEquals(component, null) || component == null /* component already destroyed*/)
//		throw new MissingReferenceException();
//
//	int entityWrapperId = component.gameObject.GetInstanceID();
//	uint componentTypeId = TypeManager.getTypeId(component.GetType());
//	_entityWrappers[entityWrapperId]._componentTypeIds.Remove(componentTypeId);
//
//	Object.Destroy(component);
//}
//
//public static T addComponent<T>(GameObject gameObject, object componentValues = null) where T : Component {
//	if(ReferenceEquals(gameObject, null) || gameObject == null /* go already destroyed*/)
//		throw new MissingReferenceException();
//
//	T component = gameObject.AddComponent<T>();
//	if(component == null)
//		return null;
//
//	// affectation values
//
//	int entityWrapperId = gameObject.GetInstanceID();
//	uint componentTypeId = TypeManager.getTypeId(typeof(T));
//	_entityWrappers[entityWrapperId]._componentTypeIds.Add(componentTypeId);
//
//	return component;
//}
//
//public static Component addComponent(global::System.Type componentType, GameObject gameObject, object componentValues = null) {
//	if (ReferenceEquals(componentType, null) || componentType.IsSubclassOf(typeof(Component)))
//		throw new UnityException(); // write own exception
//
//	if(ReferenceEquals(gameObject, null) || gameObject == null /* go already destroyed*/)
//		throw new MissingReferenceException();
//
//	Component component = gameObject.AddComponent(componentType);
//	if(component == null)
//		return null;
//
//	// affectation values
//
//	int entityWrapperId = gameObject.GetInstanceID();
//	uint componentTypeId = TypeManager.getTypeId(componentType);
//	_entityWrappers[entityWrapperId]._componentTypeIds.Add(componentTypeId);
//
//	return component;
//}
//
//public static void removeEntity(GameObject gameObject){
//	if(ReferenceEquals(gameObject, null) || gameObject == null /* go already destroyed*/)
//		throw new MissingReferenceException();
//
//	int entityWrapperId = gameObject.GetInstanceID();
//	_entityWrappers.Remove(entityWrapperId);
//
//	Object.Destroy(gameObject);
//}
//
//public static GameObject createEntity(string name, params global::System.Type[] componentTypes) {
//	if(ReferenceEquals(name, null))
//		throw new MissingReferenceException();
//
//	GameObject gameObject = new GameObject(name, componentTypes);
//
//	int entityWrapperId = gameObject.GetInstanceID();
//	HashSet<uint> componentTypeIds = new HashSet<uint>();
//	foreach(Component c in gameObject.GetComponents<Component>())
//		componentTypeIds.Add(TypeManager.getTypeId(c.GetType()));
//
//	_entityWrappers.Add(entityWrapperId, new UECS.EntityWrapper(gameObject, componentTypeIds));
//
//	return gameObject;
//}
//
//public static GameObject createPrimitive(string name, PrimitiveType type) {
//	if(ReferenceEquals(name, null))
//		throw new MissingReferenceException();
//
//	GameObject gameObject = GameObject.CreatePrimitive(type);
//	gameObject.name = name;
//
//	int entityWrapperId = gameObject.GetInstanceID();
//	HashSet<uint> componentTypeIds = new HashSet<uint>();
//	foreach(Component c in gameObject.GetComponents<Component>())
//		componentTypeIds.Add(TypeManager.getTypeId(c.GetType()));
//
//	_entityWrappers.Add(entityWrapperId, new UECS.EntityWrapper(gameObject, componentTypeIds));
//
//	return gameObject;
//}
//
//public static GameObject instantiatePrefab(string name, string prefabName) {
//	if(ReferenceEquals(name, null) || ReferenceEquals(prefabName, null))
//		throw new MissingReferenceException();
//
//	GameObject prefabResource;
//	if (_prefabResources.TryGetValue(prefabName, out prefabResource) == false) {
//		prefabResource = Resources.Load<GameObject>(prefabName);
//
//		if (prefabResource == null) {
//			Debug.LogWarning("Can't instantiate '" + prefabName + "', because it doesn't exist or it isn't present in 'Assets/Resources' folder.");
//			return null;
//		}
//
//		_prefabResources.Add(prefabName, prefabResource);
//	}
//
//	GameObject gameObject = GameObject.Instantiate<GameObject>(prefabResource);
//	gameObject.name = name;
//
//	int entityWrapperId = gameObject.GetInstanceID();
//	HashSet<uint> componentTypeIds = new HashSet<uint>();
//	foreach(Component c in gameObject.GetComponents<Component>())
//		componentTypeIds.Add(TypeManager.getTypeId(c.GetType()));
//
//	_entityWrappers.Add(entityWrapperId, new UECS.EntityWrapper(gameObject, componentTypeIds));
//
//	return gameObject;
//}
//}