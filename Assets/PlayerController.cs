using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour {

	private Rigidbody2D rgb;
	public Transform top_left;
	public Transform bottom_right;
	public LayerMask ground_layer;
	// Move variables
	public float moveScale = 20f;
	private bool facingRight;
	// Jump variables
	public float minJumpForce = 2.0f;
	public float addlJumpForce = 0.5f;
	private float _timeHeld = 0.0f;
	public float _timeForFullJump = 0.5f;
	// Dash variables
	private bool isDashing = false;
	public float dashSpeed = 50f;
	public float _dashTime = 0.25f;
	private float _timeOfLastDash = 0.0f;
	public float _dashCooldown = 2.0f;

	public override void OnStartLocalPlayer()
	{
		GetComponent<SpriteRenderer> ().color = new Color (0.6f, 0.8f, 0.85f);
	}

	private void Awake() {
		rgb = GetComponent<Rigidbody2D>();
	}

	void FixedUpdate() {
		if (!isLocalPlayer) {
			return;
		}

		// Move
		if (!isDashing) {
			float move = Input.GetAxis ("Horizontal");
			float upVelocity = rgb.velocity.y;
			rgb.velocity = new Vector2 (move * moveScale, upVelocity);

			if (facingRight && move < 0 || !facingRight && move > 0) {
				facingRight = !facingRight;
			}
		}

		// Keep player within bounds of window
		var pos = Camera.main.WorldToViewportPoint(transform.position);
		pos.x = Mathf.Clamp(pos.x, 0f, 1f);
		pos.y = Mathf.Clamp(pos.y, 0f, 1f);
		transform.position = Camera.main.ViewportToWorldPoint(pos);
	}

	void Update() {
		if (!isLocalPlayer) {
			return;
		}

		// Jump
		bool grounded = Physics2D.OverlapArea(top_left.position, bottom_right.position, ground_layer);    
		if (grounded && Input.GetKeyDown(KeyCode.UpArrow)) {
			_timeHeld = 0f;
			Jump ();
		} else if (Input.GetKey(KeyCode.UpArrow) && _timeHeld < _timeForFullJump) {
			_timeHeld += Time.deltaTime;
			addJumpForce ();
		}

		// Dash
		bool canDash = Time.time > _timeOfLastDash + _dashCooldown;
		if (canDash && Input.GetKeyDown (KeyCode.Space)) {
			Dash ();
		} else if (isDashing) {
			if (Time.time > _timeOfLastDash + _dashTime) {
				isDashing = false;
			}
		}
	}

	public void Jump() {
		rgb.AddForce(new Vector2(0, minJumpForce), ForceMode2D.Impulse);
	}

	public void addJumpForce() {
		rgb.AddForce (new Vector2 (0, addlJumpForce), ForceMode2D.Impulse);
	}

	public void Dash() {
		isDashing = true;
		_timeOfLastDash = Time.time;
		rgb.velocity = new Vector2 ((facingRight ? 1 : -1) * dashSpeed, 0);
	}

	void OnTriggerEnter2D(Collider2D other) {
		PlayerController hitPlayer = other.gameObject.GetComponent<PlayerController> ();
		if (hitPlayer != null && isDashing) {
			CmdDealDamage (other.gameObject);
		}
	}

	[Command]
	void CmdDealDamage(GameObject otherPlayer) {
		Health otherHealth = otherPlayer.GetComponent<Health> ();
		otherHealth.TakeDamage (30);
	}
}
