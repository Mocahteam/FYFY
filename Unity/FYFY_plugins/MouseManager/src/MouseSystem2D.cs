using UnityEngine;
using System.Linq;

namespace FYFY_plugins.MouseManager {
	public class MouseSystem2D : MouseSystem {
		protected override GameObject[] getHitGameObjects(Ray ray) {
			return (from hit in Physics2D.RaycastAll(ray.origin, ray.direction)
					where(_mouseSensitiveFamily.contains(hit.transform.gameObject.GetInstanceID()) == true)
					orderby hit.distance
					select hit.transform.gameObject).ToArray();
		}
	}
}