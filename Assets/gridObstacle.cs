using UnityEngine;
using System.Collections;

public class gridObstacle : MonoBehaviour {
	[SerializeField, EnumFlagsField] public movementType Blocked = 0; //Movement types which cannot enter square
	[SerializeField, EnumFlagsField] public movementType Slowed = 0; //Movement Types slowed by Square
	public int gridX = 0;
	public int gridY = 0;
	private movementGrid grid;


	public void addToGrid(movementGrid Grid) {
		grid = Grid;
		if (gridX >= 0 && gridY >= 0 && gridX < grid.Width () && gridY < grid.Height ()) {
			grid [gridX, gridY].restricted = Blocked;
			grid [gridX, gridY].slow = Slowed;
			transform.position = grid.worldPos (gridX, gridY);
		} else {
			Debug.LogError ("Invalid grid obstacle position");
		}
	}
}
