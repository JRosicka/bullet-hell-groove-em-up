using UnityEngine;

/// <summary>
/// Handles the movement of the player, ensuring it remains inside the game boundary
/// </summary>
public class Player : MonoBehaviour {
	public Rigidbody2D rb;
	//public BoundaryEdges boundary;
	public float normalSpeed;
	public float slowSpeed;
	
	void Start() {
		//boundary = GameObject.Find("Boundary").GetComponent<BoundaryEdges>();
	}

	/// <summary>
	/// Checks to see the desired direction, and updates the player position and velocity based on this,
	/// as long as it remains within the game bounds
	/// </summary>
	void FixedUpdate() {
		float moveHorizontal = Input.GetAxis("Horizontal");
		float moveVertical = Input.GetAxis("Vertical");

//		rb.position = new Vector2(
//			Mathf.Clamp(rb.position.x, boundary.xMinWindow, boundary.xMaxWindow),
//			Mathf.Clamp(rb.position.y, boundary.yMinWindow, boundary.yMaxWindow)
//		);

		Vector2 movement = new Vector2(moveHorizontal, moveVertical);
		float currentSpeed = normalSpeed;
		rb.velocity = movement * currentSpeed;
	}
}