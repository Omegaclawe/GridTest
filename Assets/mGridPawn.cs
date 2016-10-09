using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class mGridPawn : MonoBehaviour {

	private movementGrid grid;
	public int gridX = 0;
	public int gridY = 0;
	public int team = 1; //Currently only prevents movement through non-teammates
	public bool active = true; //Whether can move this turn
	private List<pathNode> ValidMoves; //List of valid moves. Recalc'ed every time pawn is clicked
	public GameObject tileMoveMarker; //Prototype for the movement marker.

	private GameObject TileContainer; //Since draws its own valid movement icons, save parent to delete when done

	private SpriteRenderer sprite; //Hack code to show whether or not is active

	[Range(1,10)] public int moveSpeed = 5;
	[SerializeField, EnumFlagsField] movementType moveType = movementType.LAND | movementType.SEA;

	void Start() { //Hack code to show activity
		sprite = GetComponentInChildren<SpriteRenderer> ();
	}

	void Update() { //hack code to show activity
		if (active) {
			sprite.color = Color.white;
		} else {
			sprite.color = Color.grey;
		}
	}

	public void addToGrid(movementGrid Grid, int x, int y) { //This way, can insure grid is properly instantiated when adding
		grid = Grid;
		newMove (x, y);
	}

	public bool move(int x, int y) { //Move the pawn to a new square. Fails if can't do that, star fox. Doesn't check for movement blocks or valid move distance.
		if (x >= 0 && x < grid.Width () && y >= 0 && y < grid.Height ()) {
			if (grid [x, y].occupied == 0 && grid [x, y].occupant == null) {
				grid [gridX,gridY].occupied = 0;
				grid [gridX,gridY].occupant = null;
				grid [x,y].occupied = team;
				grid [x,y].occupant = this;
				gridX = x;
				gridY = y;
				transform.position = grid.worldPos (gridX, gridY);
				active = false;
				return true;
			} else {
				Debug.LogError ("Pawn Movement Failed. Space already occupied");
				return false;
			}
		} else {
			Debug.LogError ("Pawn Movement Out of Bounds");
			return false;
		}
	}

	private bool newMove(int x, int y) { //Same as above, but for when new to grid to prevent wiping grid info for things previously added.
		if (x >= 0 && x < grid.Width () && y >= 0 && y < grid.Height ()) {
			if (grid [x, y].occupied == 0 && grid [x, y].occupant == null) {
				grid [x,y].occupied = team;
				grid [x,y].occupant = this;
				gridX = x;
				gridY = y;
				transform.position = grid.worldPos (gridX, gridY);
				return true;
			} else {
				Debug.LogError ("Pawn Movement Failed. Space already occupied");
				return false;
			}
		} else {
			Debug.LogError ("Pawn Movement Out of Bounds");
			return false;
		}
	}

	public void calcMoves() { //Djikstra's Algorithm based calculation of available moves
		List<pathNode> nNodes = new List<pathNode>(); //new nodes to be checked. Not guaranteed to have best route, yet.
		List<pathNode> fNodes = new List<pathNode>(); //Nodes added once they are they shortest new route and have had children checked. 
		nNodes.Add (new pathNode ()); //Start by adding current node to seed (but not as valid. Nodes valid = false by default)
		nNodes [0].x = gridX;
		nNodes [0].y = gridY;
		nNodes [0].rMoves = 0;

		//Vars for wgile loop.
		int cNode;
		int cMoves;
		int tx;
		int ty;
		bool found = false;
		pathNode nNode;
		while (nNodes.Count != 0) { //I.e. while there are still new nodes to check
			cNode = 0; //Prime each loop to prevent unnecessary carryover
			cMoves = 99999999;
			for (int i = 0; i < nNodes.Count; i++) { //Find the lowest number with fewest moves in new nodes
				if (nNodes [i].rMoves < cMoves) {
					cNode = i;
					cMoves = nNodes [i].rMoves;
				}
			}

			//This could possibly be broken down into a function repeated four times with the invdividual adjacent tiles. C/P'ed for now due to laziness and scope.
			//Technically runs inconsequentially and imperceptibly faster than a function call. Behaviour undefined if pawn is not in valid grid.

			//Test Top
			tx = nNodes[cNode].x;
			ty = nNodes[cNode].y + 1;
			if (ty < grid.Height () && // Not Outside Grid
				(((grid [tx, ty].restricted & moveType) ^ moveType) != 0) && //Has an active movetype that can occupy tile
			    (cMoves < moveSpeed) && //movespeed won't be exceeded
				((grid [tx, ty].occupied | team) == team) && //Tile is not occupied by a different team
				(((grid [tx, ty].slow & moveType) == 0) || //Movetype won't cause slowness, or
			    (cMoves + 1 < moveSpeed))) { //Have enough remaining move to enter slow tile
				
				nNode = new pathNode ();
				nNode.x = tx;
				nNode.y = ty;
				nNode.rMoves = cMoves + 1;
				nNode.prev = nNodes [cNode];
				if (grid [tx, ty].occupied == 0) { //if completely empty, can move to here. Otherwise, just through here.
					nNode.valid = true;
				}
				if ((grid [tx, ty].slow & moveType) != 0) { //account for slow movement
					++nNode.rMoves;
				}
				//Check if node already exists
				found = false;
				for (int i = 0; i < fNodes.Count && !found; i++) {
					if (fNodes [i].x == tx && fNodes[i].y == ty) { //Don't add if already in
						found = true; //If found in this list, required moves must be <= current moves, so no need to try harder.
					}
				}
				for (int i = 0; i < nNodes.Count && !found; i++) {
					if (nNodes [i].x == tx && nNodes[i].y == ty) { //Don't add if already in
						found = true;
						if (nNodes [i].rMoves > nNode.rMoves) { //Make sure shortest path is used
							nNodes [i].rMoves = nNode.rMoves;
							nNodes [i].prev = nNode.prev;
						}
					}
				}
				if (!found) {
					nNodes.Add (nNode);
				}
			}
			//Test Bottom
			tx = nNodes[cNode].x;
			ty = nNodes[cNode].y - 1;
			if (ty > 0 && // Not Outside Grid
				(((grid [tx, ty].restricted & moveType) ^ moveType) != 0) && //Has an active movetype that can occupy tile
				(cMoves < moveSpeed) && //movespeed won't be exceeded
				((grid [tx, ty].occupied | team) == team) && //Tile is not occupied by a different team
				(((grid [tx, ty].slow & moveType) == 0) || //Movetype won't cause slowness, or
				(cMoves + 1 < moveSpeed))) { //Have enough remaining move to enter slow tile

				nNode = new pathNode ();
				nNode.x = tx;
				nNode.y = ty;
				nNode.rMoves = cMoves + 1;
				nNode.prev = nNodes [cNode];
				if (grid [tx, ty].occupied == 0) { //if completely empty, can move to here. Otherwise, just through here.
					nNode.valid = true;
				}
				if ((grid [tx, ty].slow & moveType) != 0) { //account for slow movement
					++nNode.rMoves;
				}
				//Check if node already exists
				found = false;
				for (int i = 0; i < fNodes.Count && !found; i++) {
					if (fNodes [i].x == tx && fNodes[i].y == ty) { //Don't add if already in
						found = true; //If found in this list, required moves must be <= current moves, so no need to try harder.
					}
				}
				for (int i = 0; i < nNodes.Count && !found; i++) {
					if (nNodes [i].x == tx && nNodes[i].y == ty) { //Don't add if already in
						found = true;
						if (nNodes [i].rMoves > nNode.rMoves) { //Make sure shortest path is used
							nNodes [i].rMoves = nNode.rMoves;
							nNodes [i].prev = nNode.prev;
						}
					}
				}
				if (!found) {
					nNodes.Add (nNode);
				}
			}
			//Test Left
			tx = nNodes[cNode].x - 1;
			ty = nNodes[cNode].y;
			if (tx > 0 && // Not Outside Grid
				(((grid [tx, ty].restricted & moveType) ^ moveType) != 0) && //Has an active movetype that can occupy tile
				(cMoves < moveSpeed) && //movespeed won't be exceeded
				((grid [tx, ty].occupied | team) == team) && //Tile is not occupied by a different team
				(((grid [tx, ty].slow & moveType) == 0) || //Movetype won't cause slowness, or
				(cMoves + 1 < moveSpeed))) { //Have enough remaining move to enter slow tile

				nNode = new pathNode ();
				nNode.x = tx;
				nNode.y = ty;
				nNode.rMoves = cMoves + 1;
				nNode.prev = nNodes [cNode];
				if (grid [tx, ty].occupied == 0) { //if completely empty, can move to here. Otherwise, just through here.
					nNode.valid = true;
				}
				if ((grid [tx, ty].slow & moveType) != 0) { //account for slow movement
					++nNode.rMoves;
				}
				//Check if node already exists
				found = false;
				for (int i = 0; i < fNodes.Count && !found; i++) {
					if (fNodes [i].x == tx && fNodes[i].y == ty) { //Don't add if already in
						found = true; //If found in this list, required moves must be <= current moves, so no need to try harder.
					}
				}
				for (int i = 0; i < nNodes.Count && !found; i++) {
					if (nNodes [i].x == tx && nNodes[i].y == ty) { //Don't add if already in
						found = true;
						if (nNodes [i].rMoves > nNode.rMoves) { //Make sure shortest path is used
							nNodes [i].rMoves = nNode.rMoves;
							nNodes [i].prev = nNode.prev;
						}
					}
				}
				if (!found) {
					nNodes.Add (nNode);
				}
			}
			//Test Right
			tx = nNodes[cNode].x + 1;
			ty = nNodes[cNode].y;
			if (tx < grid.Width () && // Not Outside Grid
				(((grid [tx, ty].restricted & moveType) ^ moveType) != 0) && //Has an active movetype that can occupy tile
				(cMoves < moveSpeed) && //movespeed won't be exceeded
				((grid [tx, ty].occupied | team) == team) && //Tile is not occupied by a different team
				(((grid [tx, ty].slow & moveType) == 0) || //Movetype won't cause slowness, or
				(cMoves + 1 < moveSpeed))) { //Have enough remaining move to enter slow tile

				nNode = new pathNode ();
				nNode.x = tx;
				nNode.y = ty;
				nNode.rMoves = cMoves + 1;
				nNode.prev = nNodes [cNode];
				if (grid [tx, ty].occupied == 0) { //if completely empty, can move to here. Otherwise, just through here.
					nNode.valid = true;
				}
				if ((grid [tx, ty].slow & moveType) != 0) { //account for slow movement
					++nNode.rMoves;
				}
				//Check if node already exists
				found = false;
				for (int i = 0; i < fNodes.Count && !found; i++) {
					if (fNodes [i].x == tx && fNodes[i].y == ty) { //Don't add if already in
						found = true; //If found in this list, required moves must be <= current moves, so no need to try harder.
					}
				}
				for (int i = 0; i < nNodes.Count && !found; i++) {
					if (nNodes [i].x == tx && nNodes[i].y == ty) { //Don't add if already in
						found = true;
						if (nNodes [i].rMoves > nNode.rMoves) { //Make sure shortest path is used
							nNodes [i].rMoves = nNode.rMoves;
							nNodes [i].prev = nNode.prev;
						}
					}
				}
				if (!found) {
					nNodes.Add (nNode);
				}
			}


			//This Tile is finished.
			fNodes.Add(nNodes[cNode]);
			nNodes.RemoveAt (cNode);
		}
		if (ValidMoves != null) { //Delete old stuff if it exists, for memory purposes.
			ValidMoves.Clear ();
		}
		ValidMoves = fNodes;
		foreach (pathNode i in ValidMoves) { //Debug string to list all valid moves and some info on completion.
			Debug.Log (i.x + "," + i.y + " can move: " + i.valid + ". Requires " + i.rMoves + " Moves.");
		}
	}

	public void drawValidSpots() { //Sticks a sprite on every valid movement spot.
		if (ValidMoves.Count == 0) { //Why bother if there are none?
			return;
		}
		TileContainer = new GameObject ("TileContainer"); //Give parent object as to make easier to delete.
		GameObject nSprite;
		foreach (pathNode i in ValidMoves) {
			if (i.valid) {
				nSprite = Instantiate (tileMoveMarker,TileContainer.transform) as GameObject; //Use that prefab!
				Vector3 v3 = grid.worldPos (i.x, i.y); //Put it in the appropriate spot
				v3.x += 0.5f; // Adjust for sprite size
				v3.y += 0.5f; // Adjust for sprite size
				nSprite.transform.position = v3;
			}
		}
	}
	public void deleteSpotDraw() { //Delete all those nasty blue tiles!
		Destroy (TileContainer);
	}
	public void newTurn() { //reactivate. Function allows to do other stuff in future as well.
		active = true;
	}
	public bool isValidMove(Vector2i move) { //Check if an attempted move is in the valid array.
		foreach (pathNode i in ValidMoves) {
			if (i.x == move.x && i.y == move.y && i.valid) {
				return true;
			}
		}
		return false;
	}
}



public class pathNode { //Keep the relevant information for tiles you test if can move to.
	public pathNode prev = null; //Keep parent node saved. Mostly important just to draw a line back to source and show actual route taken (e.g. for animation)
	public int x = -1;
	public int y = -1;
	public bool valid = false;
	public int rMoves = 999999;
}