using Rewired;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Handles the movement of the player, ensuring it remains inside the game boundary
/// </summary>
public class Player : MonoBehaviour {
	public Rigidbody2D rb;
	public float normalSpeed;
	public float slowSpeed;
	public bool Invincible;
	
	private bool onWallLeft;
	private bool onWallRight;
	private bool onWallUp;
	private bool onWallDown;


	private const string HORIZONTAL_MOVEMENT_NAME = "ctr_move_horizontal";
	private const string VERTICAL_MOVEMENT_NAME = "ctr_move_vertical";
	private const string SLOW_NAME = "ctr_slow";
	private const string PAUSE_NAME = "ctr_pause";
	private const string SHOOT_NAME = "ctr_shoot";
	private const string QUIT_NAME = "ctr_quit";

	private float quitButtonHeldDownLength;
	private const float REQUIRED_TIME_TO_QUIT = 1;
	
	/// <summary>
	/// Checks to see the desired direction, and updates the player position and velocity based on this,
	/// as long as it remains within the game bounds
	/// </summary>
	void FixedUpdate() {
		Rewired.Player playerControls = ReInput.players.GetPlayer("SYSTEM");

		// Check to see if we should quit
		if (playerControls.GetButton(QUIT_NAME)) {
			quitButtonHeldDownLength += Time.deltaTime;
			if (quitButtonHeldDownLength >= REQUIRED_TIME_TO_QUIT) {
				Debug.Log("Quitting");
				Application.Quit();
			}
		} else {
			quitButtonHeldDownLength = 0;
		}

		if (GameController.Instance.IsResetting()) {
			rb.velocity = Vector2.zero;
			return;
		}
		
		if(playerControls.controllers.joystickCount == 0) {
			Joystick joystick = ReInput.controllers.GetJoystick(0);
			playerControls.controllers.AddController(joystick, true);
		}
		
		float moveHorizontal = playerControls.GetAxis(HORIZONTAL_MOVEMENT_NAME);
		float moveVertical = playerControls.GetAxis(VERTICAL_MOVEMENT_NAME);
		Vector2 movement = GameController.Instance.EvaluateMove(new Vector2(moveHorizontal, moveVertical), transform.position);

		float currentSpeed = normalSpeed;
		if (playerControls.GetButton(SLOW_NAME))
			currentSpeed = slowSpeed;

		rb.velocity = movement * currentSpeed;
	}
	
	private void OnTriggerEnter2D(Collider2D other) {
		if (Invincible)
			return;
		
		if (other.gameObject.CompareTag("Bullet")) {
			other.gameObject.SendMessage("DestroyBullet");
			KillPlayer();
		} else if (other.gameObject.CompareTag("BulletTrail")) {
			Destroy(other.gameObject);
			KillPlayer();
		}
	}

	private void KillPlayer() {
		// Add spin for no reason
		rb.AddTorque(1f * (Random.Range(0, 2) == 0 ? 1 : -1));
		GameController.Instance.ResetGame(false);
	}
}