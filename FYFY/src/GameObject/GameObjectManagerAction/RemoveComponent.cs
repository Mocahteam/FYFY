﻿using UnityEngine;

namespace FYFY {
	internal class RemoveComponent<T> : IGameObjectManagerAction where T : Component {
		private readonly GameObject _gameObject;
		private readonly string _exceptionStackTrace;

		internal RemoveComponent(GameObject gameObject, string exceptionStackTrace) {
			_gameObject = gameObject;
			_exceptionStackTrace = exceptionStackTrace;
		}
		
		GameObject IGameObjectManagerAction.getTarget(){
			return _gameObject;
		}

		void IGameObjectManagerAction.perform() {
			if(_gameObject == null) {
				if (_exceptionStackTrace != "")
					throw new DestroyedGameObjectException("You try to remove a Component from a GameObject that will be destroyed during this frame. In a same frame, you must not destroy a GameObject and ask Fyfy to perform an action on it.", _exceptionStackTrace);
				else
					return;
			}

			int gameObjectId = _gameObject.GetInstanceID();
			if(GameObjectManager._gameObjectWrappers.ContainsKey(gameObjectId) == false){
				if (_exceptionStackTrace != "")
					throw new UnknownGameObjectException("You try to remove a Component from a GameObject which is not already binded to FYFY.", _exceptionStackTrace);
				else
					return;
			}

			T component = _gameObject.GetComponent<T>();
			System.Type componentType = typeof(T);
			if (component == null) {
				if (_exceptionStackTrace != "")
					throw new FyfyException("Can't remove \"" + componentType + "\" from \"" + _gameObject.name + "\" GameObject because no component of this type is attached to the game object.", _exceptionStackTrace);
				else
					return;
			}

			if(GameObjectManager._gameObjectWrappers[gameObjectId]._componentTypeNames.Contains(componentType.FullName) == false) {
				if (_exceptionStackTrace != "")
					throw new UnknownComponentException("You try to remove a component not registered by Fyfy. You should use \"FYFY.GameObjectManager.AddComponent\" instead of \"UnityEngine.GameObject.AddComponent\" to add a new component to a GameObject.", _exceptionStackTrace);
				else
					return;
			}

			Object.DestroyImmediate(component);
			
			// Check if an other component of this type is already included into the GO => if no, update wrapper
			if (_gameObject.GetComponent<T>() == null){
				GameObjectManager._gameObjectWrappers[gameObjectId]._componentTypeNames.Remove(componentType.FullName);
				GameObjectManager._modifiedGameObjectIds.Add(gameObjectId);
			}
		}
	}

	internal class RemoveComponent : IGameObjectManagerAction {
		private readonly GameObject _gameObject;
		private readonly Component _component;
		private readonly string _exceptionStackTrace;

		internal RemoveComponent(GameObject gameObject, Component component, string exceptionStackTrace) {
			_gameObject = gameObject;
			_component = component;
			_exceptionStackTrace = exceptionStackTrace;
		}
		
		GameObject IGameObjectManagerAction.getTarget(){
			return _gameObject;
		}

		void IGameObjectManagerAction.perform() {
			if(_gameObject == null) {
				if (_exceptionStackTrace != "")
					throw new DestroyedGameObjectException("You try to remove a Component from a GameObject that will be destroyed during this frame. In a same frame, you must not destroy a GameObject and ask Fyfy to perform an action on it.", _exceptionStackTrace);
				else
					return;
			}

			int gameObjectId = _gameObject.GetInstanceID();
			if(GameObjectManager._gameObjectWrappers.ContainsKey(gameObjectId) == false){
				if (_exceptionStackTrace != "")
					throw new UnknownGameObjectException("You try to remove a Component from a GameObject which is not already binded to FYFY.", _exceptionStackTrace);
				else
					return;
			}

			if(_component == null) {
				if (_exceptionStackTrace != "")
					throw new DestroyedComponentException("You try to remove a Component that will be destroyed during this frame. In a same frame, your must not destroy a Component and ask Fyfy to perform an action on it.", _exceptionStackTrace);
				else
					return;
			}
			
			System.Type componentType = _component.GetType();
			if(GameObjectManager._gameObjectWrappers[gameObjectId]._componentTypeNames.Contains(componentType.FullName) == false) {
				if(_exceptionStackTrace != "")
					throw new UnknownComponentException("You try to remove a component not registered by Fyfy. You should use \"FYFY.GameObjectManager.AddComponent\" instead of \"UnityEngine.GameObject.AddComponent\" to add a new component to a GameObject.", _exceptionStackTrace);
				else 
					return;
			}

			Object.DestroyImmediate(_component);
			
			// Check if an other component of this type is already included into the GO => if no, update wrapper
			if (_gameObject.GetComponent(componentType) == null){
				GameObjectManager._gameObjectWrappers[gameObjectId]._componentTypeNames.Remove(componentType.FullName);
				GameObjectManager._modifiedGameObjectIds.Add(gameObjectId);
			}
		}
	}
}