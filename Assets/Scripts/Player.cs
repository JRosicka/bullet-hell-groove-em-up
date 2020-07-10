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
		float moveVertical = playerControls.GetAxis(VERTICAL_MOVEMENT_NAME);
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
	}
}