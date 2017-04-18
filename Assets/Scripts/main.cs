using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class main : MonoBehaviour {

	public List<GameObject> mumiasLeft = new List<GameObject> (3);
	public List<GameObject> boysLeft = new List<GameObject> (3);
	public List<GameObject> mumiasRight = new List<GameObject> (3);
	public List<GameObject> boysRight = new List<GameObject> (3);
	public GameObject boat;
	public GameObject leftMargin;
	public GameObject rightMargin;

	LinkedList<GameObject> passengers = new LinkedList<GameObject>();

	static bool boatIsLeft = true;
	Queue<State> states = new Queue<State>();
	State currentState;
	State nextState;

	Vector3 destination0;
	Vector3 destination1;
	bool walking = true;
	bool goingToLand = false;

	// Use this for initialization
	void Start () {
		Tree t = new Tree (new Node (new State(3,3,0,0, BoatPosition.LEFT), null));
		var res = t.Search (new Node( new State(3,0,0,3, BoatPosition.RIGHT), null));
		boatIsLeft = true;

		foreach (Node node in res)
			states.Enqueue (node.state);
		currentState = states.Dequeue ();
		nextState = states.Dequeue ();
		int []diff = new int[]{ currentState.numCanibaisLeft - nextState.numCanibaisLeft, currentState.numBoysLeft - nextState.numBoysLeft};
	
		var canbaisToRight = diff [0];
		var boysToRight = diff [1];

		for (int i = 0; i < canbaisToRight; i++) {
			passengers.AddLast (mumiasLeft[0]);
			mumiasRight.Add (mumiasLeft[0]);
			mumiasLeft.RemoveAt (0);
		}
		for (int i = 0; i < boysToRight; i++) {
			passengers.AddLast (boysLeft[0]);
			boysRight.Add (boysLeft[0]);
			boysLeft.RemoveAt (0);
		}
		Debug.Log ("count " + res.Count);
		for(int i = 0; i < res.Count; i++){
			State a = res[i].state;
			Debug.Log (string.Format( "nCL {0}, nBL {1}, nCR {2}, nBR {3} pos={4}", a.numCanibaisLeft , a.numBoysLeft, a.numCanibaisRight, a.numBoysRight, a.boatPosition));
		}
	}
	
	// Update is called once per frame
	void Update () {

		float step = 2 * Time.deltaTime;
		if (walking) {
			
				if (GoToBoat (passengers.First.Value, passengers.Last.Value, step)) {
					passengers.First.Value.transform.parent = boat.transform;
				if (passengers.Last.Value != null)
						passengers.Last.Value.transform.parent = boat.transform;
					walking = false;
					moveBoat ();
				}
		}

		if (goingToLand){
			if (currentState.boatPosition == BoatPosition.RIGHT) {
				
					if (GoToLand (passengers.First.Value, passengers.Last.Value, step, leftMargin)) {
						//chegou na terra
						goingToLand = false;
						currentState = nextState;
					try{
						nextState = states.Dequeue ();

					}catch(InvalidOperationException ex){
						Debug.Log ("Game Over" + ex.ToString ());
						return;
					}
					passengers.Clear ();
						int[] diff = new int[] {
							currentState.numCanibaisLeft - nextState.numCanibaisLeft,
							currentState.numBoysLeft - nextState.numBoysLeft
						};
						var canbaisToRight = diff [0];
						var boysToRight = diff [1];

						for (int i = 0; i < canbaisToRight; i++) {
							passengers.AddLast (mumiasLeft [0]);
							mumiasRight.Add (mumiasLeft [0]);
							mumiasLeft.RemoveAt (0);
						}
						for (int i = 0; i < boysToRight; i++) {
							passengers.AddLast (boysLeft [0]);
							boysRight.Add (boysLeft [0]);
							boysLeft.RemoveAt (0);
						}
						if (passengers.Count < 2) {
							passengers.AddLast ((GameObject)null);
						}
						walking = true;
				}
			} else {
					if (GoToLand (passengers.First.Value, passengers.Last.Value, step, rightMargin)) {
						//chegou na terra
						goingToLand = false;
						currentState = nextState;
					try{
						nextState = states.Dequeue ();

					}catch(InvalidOperationException ex){
						Debug.Log ("Game Over" + ex.ToString());
						return;
					}
						passengers.Clear ();
						int[] diff = new int[] {
							currentState.numCanibaisRight - nextState.numCanibaisRight,
							currentState.numBoysRight - nextState.numBoysRight
						}; 
						var canbaisToRight = diff [0];
						var boysToRight = diff [1];

						for (int i = 0; i < canbaisToRight; i++) {
							passengers.AddLast (mumiasRight [0]);
							mumiasLeft.Add (mumiasRight [0]);
							mumiasRight.RemoveAt (0);
						}
						for (int i = 0; i < boysToRight; i++) {
							passengers.AddLast (boysRight [0]);
							boysLeft.Add (boysRight [0]);
							boysRight.RemoveAt (0);
						}
					if (passengers.Count < 2) {
						passengers.AddLast ((GameObject)null);
					}
						walking = true;
					}
			}
		}

//		mumias [0].transform.position = Vector3.MoveTowards (mumias[0].transform.position, 
//			destination0, step);
//		boys [0].transform.position = Vector3.MoveTowards (boys[0].transform.position, 
//			destination1, step);
	}

	void OnTriggerEnter2D(Collider2D other){
		if ( other.CompareTag ("leftMargin") ) {
			Debug.Log ("colidiu left");
			if (boat.transform.childCount != 0 ) {//deve descer
				for(int i = 0; i < boat.transform.childCount; i++){
					if (boat.transform.GetChild (i).GetComponent<Animator> () != null) {
						boat.transform.GetChild (i).GetComponent<Animator> ().SetBool ("isWalk", false);
					}

				}
					
				boat.transform.DetachChildren();
				goingToLand = true;

			} else {//first collide
				Debug.Log("mordeo");
			}
		} else if (other.CompareTag ("rightMargin")) {//deve descer
			Debug.Log ("colidiu right");
			boat.transform.DetachChildren();
			goingToLand = true;
		}
	}

	private void moveBoat(){
		if (boatIsLeft){
			boat.GetComponent<Animation> ().Play ("boatRight");
		}
		else
			boat.GetComponent<Animation> ().Play ("boatLeft");
		boatIsLeft = !boatIsLeft;
	}

	private bool GoToBoat(GameObject obj0, GameObject obj1, float step){
		bool ended0 = false;
		bool ended1 = false;

		if (obj0 != null){
			Vector3 destination = new Vector3 (boat.transform.position.x + 0.5f, obj0.transform.position.y, obj0.transform.position.z);
			//obj0.transform.position = Vector3.Lerp (obj0.transform.position, destination, 0.1f);
			obj0.transform.position = Vector3.MoveTowards (obj0.transform.position, destination, step);
			if (obj0.transform.position == destination)
				ended0 = true;
		}
		else
			ended0 = true;

		if (obj1 != null){
			Vector3 destination = new Vector3 (boat.transform.position.x - 0.5f, obj1.transform.position.y, obj1.transform.position.z);
			//obj0.transform.position = Vector3.Lerp (obj0.transform.position, destination, 0.1f);
			obj1.transform.position = Vector3.MoveTowards (obj1.transform.position, destination, step);
			if (obj1.transform.position == destination)
				ended1 = true;
		}
		else
			ended1 = true;
		return ended0 && ended1;
	}

	private bool GoToLand(GameObject obj0, GameObject obj1, float step, GameObject target){
		bool ended0 = false;
		bool ended1 = false;
		int factor;

		if (currentState.boatPosition == BoatPosition.LEFT) {
			factor = (mumiasLeft.Count + boysLeft.Count) ;
		} else {
			factor = (mumiasRight.Count + boysRight.Count) * -1;
		}

		if (obj0 != null){
			Vector3 destination = new Vector3 (target.transform.position.x + factor, obj0.transform.position.y, obj0.transform.position.z);
			//obj0.transform.position = Vector3.Lerp (obj0.transform.position, destination, 0.1f);
			obj0.transform.position = Vector3.MoveTowards (obj0.transform.position, destination, step);
			if (obj0.transform.position == destination)
				ended0 = true;
		}
		else
			ended0 = true;

		if (obj1 != null){
			Vector3 destination = new Vector3 (target.transform.position.x + factor +1, obj1.transform.position.y, obj1.transform.position.z);
			//obj0.transform.position = Vector3.Lerp (obj0.transform.position, destination, 0.1f);
			obj1.transform.position = Vector3.MoveTowards (obj1.transform.position, destination, step);
			if (obj1.transform.position == destination)
				ended1 = true;
		}
		else
			ended1 = true;
		return ended0 && ended1;
	}


	//*************************** Logical classes **********************************************************************
	public enum BoatPosition{ LEFT, RIGHT }

	private class State{
		public int numCanibaisLeft;
		public int numBoysLeft;
		public int numCanibaisRight;
		public int numBoysRight;
		public BoatPosition boatPosition;

		public State(int numCanibaisLeft, int numBoysLeft , int numCanibaisRight, int numBoysRight, BoatPosition boatPosition){
			this.numCanibaisLeft = numCanibaisLeft;
			this.numBoysLeft = numBoysLeft;
			this.numCanibaisRight = numCanibaisRight;
			this.numBoysRight = numBoysRight;
			this.boatPosition = boatPosition;
		}

		public bool isEqual(State other){
			return this.numCanibaisLeft == other.numCanibaisLeft && this.numBoysLeft == other.numBoysLeft 
				&& this.numCanibaisRight == other.numCanibaisRight && this.numBoysRight == other.numBoysRight
				&& this.boatPosition == other.boatPosition;
		}

		public bool isValid(){
			return (this.numCanibaisLeft == 0) || (this.numBoysLeft == 0 || this.numBoysLeft >= this.numCanibaisLeft) 
				&& (this.numBoysRight == 0 || this.numBoysRight >= this.numCanibaisRight) 
				&& this.numCanibaisLeft > -1 && this.numBoysLeft > -1 && this.numCanibaisRight > -1 && this.numBoysRight > -1;
		}
	}

	private class Node{
		public Node parent;
		public State state;

		public Node(State state, Node parent){
			this.state = state;
			this.parent = parent;
		}
	}

	private class Tree{
		Node root;

		public Tree( Node root ){
			this.root = root;
		}

		public List<Node> Search(Node target){
			Queue<Node> border = new Queue<Node> ();
			LinkedList<Node> children = new LinkedList<Node> ();
			List<Node> path = new List<Node> ();
			LinkedList<Node> verified = new LinkedList<Node> ();

			border.Enqueue (root);
			while (true) {
				if (border.Count == 0){
					Debug.Log ("not found");
					return null;
				}
				Node current = border.Dequeue ();
				if (current.state.isEqual (target.state)) {
					Debug.Log ("achou target");
					path.Add (current);
					while ( !current.state.isEqual(root.state) ) {
						foreach (Node node in verified) {
							if (node == current.parent) {
								path.Add (node);
								current = node;
							}
						}
						verified.Remove (current);
					}
					path.Reverse ();
					return path;
				}
				verified.AddLast (current);
				children = expand (current);
				foreach(Node child in children){
						border.Enqueue (child);
				}
		}
		}

//		public List<Node> Search(Node target){
//			Queue<Node> border = new Queue<Node> ();
//			LinkedList<Node> children = new LinkedList<Node> ();
//			List<Node> path = new List<Node> ();
//			LinkedList<Node> verified = new LinkedList<Node> ();
//
//			border.Enqueue (root);
//			while (true) {
//				if (border.Count == 0){
//					Debug.Log ("not found");
//					return null;
//				}
//				Node current = border.Dequeue ();
//				if (current.state.isEqual (target.state)) {
//					Debug.Log ("achou target");
//					Debug.Log (string.Format( "nCL {0}, nBL {1}, nCR {2}, nBR {3} pos={4}", current.state.numCanibaisLeft , current.state.numBoysLeft, current.state.numCanibaisRight, current.state.numBoysRight, current.state.boatPosition));
//
//					path.Add (current);
//					while (current.parent != null) {
//						foreach (Node node in verified) {
//							if (node == current.parent) {
//								path.Insert(0, node);
//								current = node;
//							}
//						}
//						verified.Remove (current);
//					}
//					return path;
//				}
//				verified.AddFirst (current);
//				children = expand (current);
//				foreach(Node child in children){
//					border.Enqueue (child);
//				}
//			}
//		}
//
		private LinkedList<Node> expand(Node node){
			LinkedList<Node> result = new LinkedList<Node>();
			LinkedList<State> states = new LinkedList<State>();

			if (boatIsLeft) {
				states.AddLast (new State (node.state.numCanibaisLeft - 2, node.state.numBoysLeft, node.state.numCanibaisRight + 2, node.state.numBoysRight, BoatPosition.RIGHT));
				states.AddLast (new State (node.state.numCanibaisLeft - 1, node.state.numBoysLeft, node.state.numCanibaisRight + 1, node.state.numBoysRight, BoatPosition.RIGHT));
				states.AddLast (new State (node.state.numCanibaisLeft, node.state.numBoysLeft - 2, node.state.numCanibaisRight, node.state.numBoysRight + 2, BoatPosition.RIGHT));
				states.AddLast (new State (node.state.numCanibaisLeft, node.state.numBoysLeft - 1, node.state.numCanibaisRight, node.state.numBoysRight + 1, BoatPosition.RIGHT));
				states.AddLast (new State (node.state.numCanibaisLeft -1, node.state.numBoysLeft - 1, node.state.numCanibaisRight+1, node.state.numBoysRight + 1, BoatPosition.RIGHT));
			} else {
				states.AddLast (new State (node.state.numCanibaisLeft +2, node.state.numBoysLeft, node.state.numCanibaisRight -2, node.state.numBoysRight, BoatPosition.LEFT));
				states.AddLast (new State (node.state.numCanibaisLeft + 1, node.state.numBoysLeft, node.state.numCanibaisRight - 1, node.state.numBoysRight, BoatPosition.LEFT));
				states.AddLast (new State (node.state.numCanibaisLeft, node.state.numBoysLeft + 2, node.state.numCanibaisRight, node.state.numBoysRight - 2, BoatPosition.LEFT));
				states.AddLast (new State (node.state.numCanibaisLeft, node.state.numBoysLeft + 1, node.state.numCanibaisRight, node.state.numBoysRight - 1, BoatPosition.LEFT));
				states.AddLast (new State (node.state.numCanibaisLeft + 1, node.state.numBoysLeft + 1, node.state.numCanibaisRight -1, node.state.numBoysRight -1, BoatPosition.LEFT));
			}
			boatIsLeft = !boatIsLeft;
			foreach (State state in states) {
				if (state.isValid() && state.boatPosition != node.state.boatPosition)
					result.AddLast (new Node (state, node));
			}
			return result;
		}
			
	}
}
