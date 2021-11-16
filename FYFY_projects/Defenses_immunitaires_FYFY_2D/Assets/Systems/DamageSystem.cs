using UnityEngine;
using FYFY;

// Ce système applique les dommages et met à jour la bare de vie en conséquence
public class DamageSystem : FSystem {
	private Family f_cells = FamilyManager.getFamily(new AllOfComponents(typeof(Damage), typeof(Health)));

	protected override void onProcess(int currentFrame) {
		foreach (GameObject cell in f_cells) {
			Health health = cell.GetComponent<Health>();
			Damage damage = cell.GetComponent<Damage>();
			RectTransform healthBarTransform = health.healthBar;
			// appliquer les dommages
			health._currentHealth -= damage.damages;
			// mettre à jour la bare de vie
			float width = health._currentHealth / health._maximumHealth;
			float height = healthBarTransform.sizeDelta.y;
			healthBarTransform.sizeDelta = new Vector2(width, height);

			// si la cellule est morte, déclencher la mort
			if (health._currentHealth <= 0f)
			{
				// vérifier si la cellule était infectée
				Infected infected;
				if (!cell.TryGetComponent<Infected>(out infected))
				{
					GameObjectManager.addComponent<Death>(cell, new
					{
						_wastesNumber = health._wastesNumber,
						_wastePrefab = health._wastePrefab
					});
				}
				else
				{
					GameObjectManager.addComponent<Death>(cell, new
					{
						_wastesNumber = health._wastesNumber,
						_wastePrefab = health._wastePrefab,
						_virusNumber = infected._virusNumberToCreate,
						_virusProperties = infected._virusProperties,
						_virusPrefab = infected._virusPrefab
					});
				}
			}
			// dépiler le composant
			GameObjectManager.removeComponent(damage);
		}
	}
}
