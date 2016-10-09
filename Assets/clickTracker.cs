using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class clickTracker : MonoBehaviour {

	private movementGrid grid;
	private Vector2i aPos;
	private bool selected = false;


	// Use this for initialization
	void Start () {
		grid = GetComponentInParent<movementGrid> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown ("Click")) {
			Vector2i pos = grid.gridPos (Camera.main.ScreenToWorldPoint (Input.mousePosition)); //Find where the mouse clicked
			if (pos.x >= 0 && pos.y >= 0 && pos.x < grid.Width () && pos.y < grid.Height ()) {
				if (selected) { // A pawn is already selected.
					grid [aPos].occupant.deleteSpotDraw (); //Clear active drawing
					if (grid [pos].occupied != 0) { //Aw shucks. Something's here. Let's select it!
						aPos = pos;
						selected = true;
						if (grid [pos].occupant.active) { //If it's ready to move, let's get it some valid moves and draw them!
							grid [pos].occupant.calcMoves ();
							grid [pos].occupant.drawValidSpots ();
						}
					} else if (grid [aPos].occupant.isValidMove (pos) && grid [aPos].occupant.active) { //Alright. Can move. Lets do it.
						grid [aPos].occupant.move (pos.x, pos.y);
						selected = false;
					} else { //Othrewise, if you don't click on anythign, you don't select things.
						selected = false;
					}
				} else {
					if (grid [pos].occupied != 0) {
						aPos = pos;
						selected = true;
						if (grid [pos].occupant.active) {
							grid [pos].occupant.calcMoves ();
							grid [pos].occupant.drawValidSpots ();
						}
					} else {
						selected = false;
					}
				}
			} else {
				selected = false;
			}
		}
		if (Input.GetButtonDown ("NewTurn")) { //Prototypey way to make a new turn start.
			List<mGridPawn> pawns = new List<mGridPawn> ();
			pawns.AddRange (GetComponentsInChildren<mGridPawn> ());
			foreach (mGridPawn i in pawns) {
				i.newTurn ();
			}
		}
	}
}
