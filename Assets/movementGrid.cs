using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class movementGrid : MonoBehaviour {

	private List<mGridNode> grid;
	private int width = 0;
	private int height = 0;
	[Range(1,255)] public int initWidth = 10;
	[Range(1,255)] public int initHeight = 10;
	[Range(0.1f,256)] public float squareSize = 1;
	public float lineWidth = 0.1f;

	public int Width() { //There's almost certainly a better way to return this, but it's important that widht and height don't get editted willy nilly on the grid.
		return width;
	}
	public int Height() {
		return height;
	}

	public mGridNode this[int x, int y] { //Whee! Allows grid access without fancy intermediate function, or trying to make the list complicated by making it a list of list.
		get {
			if (x >= 0 && y >= 0 && x < width && y < height) {
				return grid [x + y * width];
			} else {
				Debug.LogError ("Grid Access Out of Bounds");
				return null;
			}
		}
		set {
			if (x >= 0 && y >= 0 && x < width && y < height) {
				grid [x + y * width] = value;
			} else {
				Debug.LogError ("Grid Access Out of Bounds");
			}
		}
	}

	public mGridNode this[Vector2i v] { //Same as above, but with swanky combined type.
		get {
			if (v.x >= 0 && v.y >= 0 && v.x < width && v.y < height) {
				return grid [v.x + v.y * width];
			} else {
				Debug.LogError ("Grid Access Out of Bounds");
				return null;
			}
		}
		set {
			if (v.x >= 0 && v.y >= 0 && v.x < width && v.y < height) {
				grid [v.x + v.y * width] = value;
			} else {
				Debug.LogError ("Grid Access Out of Bounds");
			}
		}
	}

	public void addChildren() {
		List<mGridPawn> pawns = new List<mGridPawn> (); //To Add Dem Pawns.
		pawns.AddRange (GetComponentsInChildren<mGridPawn> ());
		foreach (mGridPawn i in pawns) {
			i.addToGrid (this, i.gridX, i.gridY);
		}
		List<gridObstacle> obst = new List<gridObstacle> (); //To Add Dem Obstacles (which are just a prototype version to make it easy to adjust map in-editor. Should be replaced.
		obst.AddRange (GetComponentsInChildren<gridObstacle> ());
		foreach (gridObstacle i in obst) {
			i.addToGrid (this);
		}
	}

	public void setup(int _Width, int _Height) { //Sets up the grid. Also appropriate for resizing grid, if willing to lose all previous info.
		if (_Width > 0 && _Height > 0) {
			grid.Clear ();
			grid.Capacity = _Width * _Height;
			for (int i = 0; i < grid.Capacity; i++) {
				grid.Add (new mGridNode ());
				//Could Add code here to load info from other systems
			}
			width = _Width;
			height = _Height;
		} else {
			Debug.LogError ("Cannot set grid to negative size!");
		}
	}
		

	// Use this for initialization
	void Start () {
		grid = new List<mGridNode> ();
		setup (initWidth, initHeight);
		gridLines ();
		addChildren (); //one way to make things work... adds child objects with appropriate components to grid and lets them do their thing.
	}

	public void gridLines() { //Draws a bunch of shoddy gridlines. Probably should be removed once we get a proper drawing system in place. Works for now.
		//Draw horizontal
		for (int i = 0; i <= width; i++) {
			GameObject LineObj = new GameObject ();
			LineObj.transform.SetParent (transform);
			LineRenderer line = LineObj.AddComponent<LineRenderer> ();
			//line.transform.position.x = i * squareSize;
			Vector3 start = new Vector3 (transform.position.x + i * squareSize - width * squareSize * 0.5f, transform.position.y - height * squareSize * 0.5f, transform.position.z);
			Vector3 end = new Vector3 (transform.position.x + i * squareSize - width * squareSize * 0.5f, transform.position.y + height * squareSize - height * squareSize * 0.5f, transform.position.z);
			line.receiveShadows = false;
			line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			line.material = new Material (Shader.Find ("Particles/Alpha Blended Premultiply"));
			line.SetColors (Color.black, Color.black);
			line.SetWidth (lineWidth, lineWidth);
			line.SetPosition (0, start);
			line.SetPosition (1, end);
		}
		//Draw vertical
		for (int i = 0; i <= height; i++) {
			GameObject LineObj = new GameObject ();
			LineObj.transform.SetParent (transform);
			LineRenderer line = LineObj.AddComponent<LineRenderer> ();
			//line.transform.position.x = i * squareSize;
			Vector3 start = new Vector3 (transform.position.x - width * squareSize * 0.5f, transform.position.y + i * squareSize - height * squareSize * 0.5f, transform.position.z);
			Vector3 end = new Vector3 (transform.position.x + width * squareSize - width * squareSize * 0.5f, transform.position.y + i * squareSize - height * squareSize * 0.5f, transform.position.z);
			line.material = new Material (Shader.Find ("Particles/Alpha Blended Premultiply"));
			line.SetColors (Color.black, Color.black);
			line.SetWidth (lineWidth, lineWidth);
			line.SetPosition (0, start);
			line.SetPosition (1, end);
		}
	}

	public Vector3 worldPos(int x, int y) { //Convert grid position to world position
		return new Vector3 (transform.position.x + x * squareSize - width * squareSize * 0.5f, transform.position.y + y * squareSize - height * squareSize * 0.5f, transform.position.z);
	}

	public Vector2i gridPos(Vector3 wPos) { //Convert world position to grid position
		Vector2i rtn;
		rtn.x = Mathf.FloorToInt((wPos.x + width * squareSize * 0.5f - transform.position.x) / squareSize);
		rtn.y = Mathf.FloorToInt((wPos.y + height * squareSize * 0.5f - transform.position.y) / squareSize);
		return rtn;
	}
}

public class mGridNode {
	public movementType restricted = 0; //Movement types which cannot enter square
	public movementType slow = 0; //Movement types which require double movement to exit
	public int occupied = 0; //Which team occupies this square. 0 is unnoccupied
	public mGridPawn occupant; //Pawn in square. Should be null if occupied=0. Probably don't have anythign that actually checks.
}

[System.Flags]public enum movementType { //Set of flags for movement type to allow appropriate comparisons. Note that each value is expressed in binary as a single 1 but in a different position. Use powers of 2 to add new ones.
	LAND = 1,
	AIR = 2,
	UNDERGROUND = 4,
	SEA = 8,
	HEAVY = 16,
	BEAST = 32,
	MOUNT = 64
}

public struct Vector2i { //This was added late in the design process so it could be a return value. Should probably have been added at the begining.
	public int x;
	public int y;
}
 