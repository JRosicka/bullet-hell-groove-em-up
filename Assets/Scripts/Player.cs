﻿using System;
using Rewired;
using UnityEngine;

/// <summary>
/// Handles the movement of the player, ensuring it remains inside the game boundary
/// </summary>
public class Player : MonoBehaviour {
	public Rigidbody2D rb;
	//public BoundaryEdges boundary;
	public float normalSpeed;
	public float slowSpeed;

	public GameObject WallLeft;
	public GameObject WallRight;
	public GameObject WallUp;
	public GameObject WallDown;

	private bool onWallLeft;
	private bool onWallRight;
	private bool onWallUp;
	private bool onWallDown;


	private const string HORIZONTAL_MOVEMENT_NAME = "ctr_move_horizontal";
	private const string VERTICAL_MOVEMENT_NAME = "ctr_move_vertical";
	private const string SLOW_NAME = "ctr_slow";
	private const string PAUSE_NAME = "ctr_pause";
	private const string SHOOT_NAME = "ctr_shoot";

	void Start() {
		//boundary = GameObject.Find("Boundary").GetComponent<BoundaryEdges>();
	}

	/// <summary>
	/// Checks to see the desired direction, and updates the player position and velocity based on this,
	/// as long as it remains within the game bounds
	/// </summary>
	void FixedUpdate() {
		// float moveHorizontal = Input.GetAxis("Horizontal");
		// float moveVertical = Input.GetAxis("Vertical");
		Rewired.Player playerControls = ReInput.players.GetPlayer("SYSTEM");

		if(playerControls.controllers.joystickCount == 0) {
			Joystick joystick = ReInput.controllers.GetJoystick(0);
			playerControls.controllers.AddController(joystick, true);
		}
		
		
		float moveHorizontal = playerControls.GetAxis(HORIZONTAL_MOVEMENT_NAME);
		if (onWallLeft)
			moveHorizontal = Mathf.Clamp(moveHorizontal, 0, 1);
		else if (onWallRight)
			moveHorizontal = Mathf.Clamp(moveHorizontal, -1, 0);

		float moveVertical = playerControls.GetAxis(VERTICAL_MOVEMENT_NAME);
		if (onWallUp)
			moveVertical = Mathf.Clamp(moveVertical, -1, 0);
		else if (onWallDown)
			moveVertical = Mathf.Clamp(moveVertical, 0, 1);
		// playerControls.controllers.Joysticks[0]

		float currentSpeed = normalSpeed;
		if (playerControls.GetButton(SLOW_NAME))
			currentSpeed = slowSpeed;

		if (playerControls.GetButton(PAUSE_NAME))
			Debug.Log("PAAAAAAAUSE");
		
		if (playerControls.GetButton(SHOOT_NAME))
			Debug.Log("SHOOOOT");
			
//		rb.position = new Vector2(
//			Mathf.Clamp(rb.position.x, boundary.xMinWindow, boundary.xMaxWindow),
//			Mathf.Clamp(rb.position.y, boundary.yMinWindow, boundary.yMaxWindow)
//		);

		Vector2 movement = new Vector2(moveHorizontal, moveVertical);
		
		rb.velocity = movement * currentSpeed;
		onWallLeft = false;
		onWallRight = false;
		onWallUp = false;
		onWallDown = false;

	}

	private void OnParticleCollision(GameObject other) {
		Debug.Log("Oof! Ouch!");
	}

	private void OnCollisionEnter2D(Collision2D other) {
		if (other.gameObject == WallLeft) {
			onWallLeft = true;
		} else if (other.gameObject == WallRight) {
			onWallRight = true;
		} else if (other.gameObject == WallUp) {
			onWallUp = true;
		} else if (other.gameObject == WallDown) {
			onWallDown = true;
		}
	}
}