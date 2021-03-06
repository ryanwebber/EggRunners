using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CMF;

public class CMFMovementController : Controller
{
	public class KeyState
    {
		public string Name { get; private set; }
		public float LastPressedTime { get; private set; }
		public bool IsKeyPressed { get; private set; }
		public bool WasKeyPressed { get; private set; }
		public bool WasKeyReleased { get; private set; }

		public bool IsKeyLocked { get; set; }

		public KeyState(string name)
        {
			Name = name;
			LastPressedTime = -1000f;
			IsKeyPressed = false;
			WasKeyPressed = false;
			WasKeyReleased = false;
			IsKeyLocked = false;
        }

		public void Update(bool newState)
        {
			if (newState == true)
				LastPressedTime = Time.time;

			if (IsKeyPressed == false && newState == true)
				WasKeyPressed = true;

			if (IsKeyPressed == true && newState == false)
			{
				WasKeyReleased = true;
				IsKeyLocked = false;
			}

			IsKeyPressed = newState;
		}

		public void Reset()
        {
			WasKeyReleased = false;
			WasKeyPressed = false;
		}
    }

	//References to attached components;
	protected Transform tr;
	protected Mover mover;
	protected CharacterInput characterInput;
	protected CeilingDetector ceilingDetector;

	public Mover Mover => mover ?? GetComponent<Mover>();

	public KeyState jumpKeyState = new KeyState("Jump");

	//Movement speed;
	public float movementSpeed = 7f;

	[Tooltip("Maximum movement speed in any state")]
	public float maximumVelocity = 200;

	//How fast the controller can change direction while in the air;
	//Higher values result in more air control;
	public float airControlRate = 2f;

	//Jump speed;
	public float jumpSpeed = 10f;

	//Jump duration variables;
	public float jumpDuration = 0.2f;
	float currentJumpStartTime = 0f;

	[Tooltip("Amount of time a pre-jump can be buffered for and be used as a jump press")]
	public float preJumpBufferTime = 0.1f;

	[Tooltip("Amount of time we will respect jump input since the user has become ungrounded")]
	public float lateJumpForgivenessTime = 0.05f;

	// Refers to whether we can jump after becoming un-grounded for a short time
	bool isLateForgivenessJumpPrevented = false;

	//'AirFriction' determines how fast the controller loses its momentum while in the air;
	//'GroundFriction' is used instead, if the controller is grounded;
	public float airFriction = 0.5f;
	public float groundFriction = 100f;

	// Time stamp of being grounded last
	float timeLastGrounded = 0f;

	//Amount of downward gravity;
	public float gravity = 30f;

	[Tooltip("Changes the strength of gravity when y velocity is negative")]
	public float gravityFallMultiplier = 1.25f;

	[Tooltip("How fast the character will slide down steep slopes.")]
	public float slideGravity = 5f;

	//Acceptable slope angle limit;
	public float slopeLimit = 80f;

	[Tooltip("Whether to calculate and apply momentum relative to the controller's transform.")]
	public bool useLocalMomentum = false;

	[Tooltip("Layermask of objects to apply impulse momentum from on collision")]
	public LayerMask impulseLayerMask;

	public float baseImpulseScale = 1.5f;

	//Current momentum;
	[ReadOnly]
	[SerializeField]
	protected Vector3 momentum = Vector3.zero;

	//Saved velocity from last frame;
	[ReadOnly]
	[SerializeField]
	Vector3 savedVelocity = Vector3.zero;

	//Saved horizontal movement velocity from last frame;
	[ReadOnly]
	[SerializeField]
	Vector3 savedMovementVelocity = Vector3.zero;

	//Enum describing basic controller states; 
	public enum ControllerState
	{
		Grounded,
		Sliding,
		Falling,
		Rising,
		Jumping
	}

	ControllerState currentControllerState = ControllerState.Falling;
	GroundContact currentContact = null;

	//Get references to all necessary components;
	void Awake()
	{
		mover = GetComponent<Mover>();
		tr = transform;
		characterInput = GetComponent<CharacterInput>();
		ceilingDetector = GetComponent<CeilingDetector>();

		if (characterInput == null)
			Debug.LogWarning("No character input script has been attached to this gameobject", this.gameObject);

		Setup();
	}

	//This function is called right after Awake(); It can be overridden by inheriting scripts;
	protected virtual void Setup()
	{
	}

	void Update()
	{
		jumpKeyState.Update(characterInput?.IsJumpKeyPressed() ?? false);
	}

	void FixedUpdate()
	{
		// If the user is on a moving surface, update their position
		if (currentContact != null)
			mover.AddDisplacement(currentContact.Displacement);

		ControllerUpdate();

		// Disconnect if we're attached to a new ground collider
		if ((IsGrounded() && currentContact != null && currentContact.GroundCollider != mover.GetGroundCollider()) || !IsGrounded())
		{
			// Detach from a contact if we're on one
			currentContact?.Detach();
			currentContact = null;
		}

		// Check if we're newly grounded to a moving platform
		if (IsGrounded() && currentContact == null)
		{
			// Newly attached to the ground contact
			if (mover.GetGroundCollider().TryGetComponent<GroundTaxi>(out var taxi))
			{
				Debug.Log("Newly attached to ground taxi...", this);
				currentContact = taxi.CreateContact(mover.GetGroundPoint(), gameObject);
			}
		}
		else if (IsGrounded() && currentContact != null)
        {
			// We are still attached to the same taxi. Update our ground point on it
			currentContact.transform.position = mover.GetGroundPoint();
        }
	}

	//Update controller;
	//This function must be called every fixed update, in order for the controller to work correctly;
	void ControllerUpdate()
	{
		//Check if mover is grounded;
		mover.CheckForGround();
		if (mover.IsGrounded())
			timeLastGrounded = Time.time;

		//Determine controller state;
		currentControllerState = DetermineControllerState();

		//Apply friction and gravity to 'momentum';
		HandleMomentum();

		//Check if the player has initiated a jump;
		HandleJumping();

		//Calculate movement velocity;
		Vector3 _velocity = Vector3.zero;
		if (currentControllerState == ControllerState.Grounded)
			_velocity = CalculateMovementVelocity();

		//If local momentum is used, transform momentum into world space first;
		Vector3 _worldMomentum = momentum;
		if (useLocalMomentum)
			_worldMomentum = tr.localToWorldMatrix * momentum;

		//Add current momentum to velocity;
		_velocity += _worldMomentum;

		//If player is grounded or sliding on a slope, extend mover's sensor range;
		//This enables the player to walk up/down stairs and slopes without losing ground contact;
		mover.SetExtendSensorRange(IsGrounded());

		//Set mover velocity;		
		mover.SetVelocity(_velocity);

		//Store velocity for next frame;
		savedVelocity = _velocity;

		//Save controller movement velocity;
		savedMovementVelocity = CalculateMovementVelocity();

		//Reset jump key booleans;
		jumpKeyState.Reset();

		//Reset ceiling detector, if one is attached to this gameobject;
		if (ceilingDetector != null)
			ceilingDetector.ResetFlags();
	}

	//Calculate and return movement direction based on player input;
	//This function can be overridden by inheriting scripts to implement different player controls;
	protected virtual Vector3 CalculateMovementDirection()
	{
		//If no character input script is attached to this object, return;
		if (characterInput == null)
			return Vector3.zero;

		Vector3 _velocity = Vector3.zero;

		// Use the character's transform axes to calculate the movement direction;$
		_velocity += tr.right * characterInput.GetHorizontalMovementInput();
		_velocity += tr.forward * characterInput.GetVerticalMovementInput();

		//If necessary, clamp movement vector to magnitude of 1f;
		if (_velocity.magnitude > 1f)
			_velocity.Normalize();

		return _velocity;
	}

	//Calculate and return movement velocity based on player input, controller state, ground normal [...];
	protected virtual Vector3 CalculateMovementVelocity()
	{
		//Calculate (normalized) movement direction;
		Vector3 _velocity = CalculateMovementDirection();

		//Multiply (normalized) velocity with movement speed;
		_velocity *= movementSpeed;

		return _velocity;
	}

	//Determine current controller state based on current momentum and whether the controller is grounded (or not);
	//Handle state transitions;
	ControllerState DetermineControllerState()
	{
		//Check if vertical momentum is pointing upwards;
		bool _isRising = IsRisingOrFalling() && (VectorMath.GetDotProduct(GetMomentum(), tr.up) > 0f);
		//Check if controller is sliding;
		bool _isSliding = mover.IsGrounded() && IsGroundTooSteep();

		//Grounded;
		if (currentControllerState == ControllerState.Grounded)
		{
			if (_isRising)
			{
				OnGroundContactLost();
				return ControllerState.Rising;
			}
			if (!mover.IsGrounded())
			{
				OnGroundContactLost();
				return ControllerState.Falling;
			}
			if (_isSliding)
			{
				OnGroundContactLost();
				return ControllerState.Sliding;
			}
			return ControllerState.Grounded;
		}

		//Falling;
		if (currentControllerState == ControllerState.Falling)
		{
			if (_isRising)
			{
				return ControllerState.Rising;
			}
			if (mover.IsGrounded() && !_isSliding)
			{
				OnGroundContactRegained();
				return ControllerState.Grounded;
			}
			if (_isSliding)
			{
				return ControllerState.Sliding;
			}
			return ControllerState.Falling;
		}

		//Sliding;
		if (currentControllerState == ControllerState.Sliding)
		{
			if (_isRising)
			{
				OnGroundContactLost();
				return ControllerState.Rising;
			}
			if (!mover.IsGrounded())
			{
				OnGroundContactLost();
				return ControllerState.Falling;
			}
			if (mover.IsGrounded() && !_isSliding)
			{
				OnGroundContactRegained();
				return ControllerState.Grounded;
			}
			return ControllerState.Sliding;
		}

		//Rising;
		if (currentControllerState == ControllerState.Rising)
		{
			if (!_isRising)
			{
				if (mover.IsGrounded() && !_isSliding)
				{
					OnGroundContactRegained();
					return ControllerState.Grounded;
				}
				if (_isSliding)
				{
					return ControllerState.Sliding;
				}
				if (!mover.IsGrounded())
				{
					return ControllerState.Falling;
				}
			}

			//If a ceiling detector has been attached to this gameobject, check for ceiling hits;
			if (ceilingDetector != null)
			{
				if (ceilingDetector.HitCeiling())
				{
					OnCeilingContact();
					return ControllerState.Falling;
				}
			}
			return ControllerState.Rising;
		}

		//Jumping;
		if (currentControllerState == ControllerState.Jumping)
		{
			//Check for jump timeout;
			if ((Time.time - currentJumpStartTime) > jumpDuration)
				return ControllerState.Rising;

			//Check if jump key was let go;
			if (jumpKeyState.WasKeyReleased)
				return ControllerState.Rising;

			//If a ceiling detector has been attached to this gameobject, check for ceiling hits;
			if (ceilingDetector != null)
			{
				if (ceilingDetector.HitCeiling())
				{
					OnCeilingContact();
					return ControllerState.Falling;
				}
			}
			return ControllerState.Jumping;
		}

		return ControllerState.Falling;
	}

	//Check if player has initiated a jump;
	void HandleJumping()
	{
		bool CanJump(KeyState keyState, bool isLatePressAllowed, float prePressBufferTime)
        {
			switch (currentControllerState)
			{
				case ControllerState.Grounded:
					{
						return (keyState.IsKeyPressed == true || keyState.WasKeyPressed || Time.time < keyState.LastPressedTime + prePressBufferTime) && !keyState.IsKeyLocked;
					}
				case ControllerState.Falling:
				case ControllerState.Sliding:
					{
						return (keyState.IsKeyPressed == true || keyState.WasKeyPressed) && Time.time < keyState.LastPressedTime + prePressBufferTime && !keyState.IsKeyLocked && isLatePressAllowed;
					}
				default:
					{
						return false;
					}
			}
		}

		if (CanJump(jumpKeyState, !isLateForgivenessJumpPrevented, preJumpBufferTime))
        {
			// We don't do late forgiveness jumps if you stopped being grounded because you jumped. That's just a double-jump
			isLateForgivenessJumpPrevented = true;

			//Call events;
			OnGroundContactLost();
			OnJumpStart();

			currentControllerState = ControllerState.Jumping;
		}
	}

	//Apply friction to both vertical and horizontal momentum based on 'friction' and 'gravity';
	//Handle movement in the air;
	//Handle sliding down steep slopes;
	void HandleMomentum()
	{
		//If local momentum is used, transform momentum into world coordinates first;
		if (useLocalMomentum)
			momentum = tr.localToWorldMatrix * momentum;

		Vector3 _verticalMomentum = Vector3.zero;
		Vector3 _horizontalMomentum = Vector3.zero;

		//Split momentum into vertical and horizontal components;
		if (momentum != Vector3.zero)
		{
			_verticalMomentum = VectorMath.ExtractDotVector(momentum, tr.up);
			_horizontalMomentum = momentum - _verticalMomentum;
		}

		//Add gravity to vertical momentum;
		float gravityMultiplier = _verticalMomentum.y < 0 ? gravityFallMultiplier : 1f;
		_verticalMomentum -= tr.up * gravity * gravityMultiplier * Time.deltaTime;

		//Remove any downward force if the controller is grounded;
		if (currentControllerState == ControllerState.Grounded && VectorMath.GetDotProduct(_verticalMomentum, tr.up) < 0f)
			_verticalMomentum = Vector3.zero;

		//Manipulate momentum to steer controller in the air (if controller is not grounded or sliding);
		if (!IsGrounded())
		{
			Vector3 _movementVelocity = CalculateMovementVelocity();

			//If controller has received additional momentum from somewhere else;
			if (_horizontalMomentum.magnitude > movementSpeed)
			{
				//Prevent unwanted accumulation of speed in the direction of the current momentum;
				if (VectorMath.GetDotProduct(_movementVelocity, _horizontalMomentum.normalized) > 0f)
					_movementVelocity = VectorMath.RemoveDotVector(_movementVelocity, _horizontalMomentum.normalized);

				//Lower air control slightly with a multiplier to add some 'weight' to any momentum applied to the controller;
				float _airControlMultiplier = 0.25f;
				_horizontalMomentum += _movementVelocity * Time.deltaTime * airControlRate * _airControlMultiplier;
			}
			//If controller has not received additional momentum;
			else
			{
				//Clamp _horizontal velocity to prevent accumulation of speed;
				_horizontalMomentum += _movementVelocity * Time.deltaTime * airControlRate;
				_horizontalMomentum = Vector3.ClampMagnitude(_horizontalMomentum, movementSpeed);
			}
		}

		//Steer controller on slopes;
		if (currentControllerState == ControllerState.Sliding)
		{
			//Calculate vector pointing away from slope;
			Vector3 _pointDownVector = Vector3.ProjectOnPlane(mover.GetGroundNormal(), tr.up).normalized;

			//Calculate movement velocity;
			Vector3 _slopeMovementVelocity = CalculateMovementVelocity();
			//Remove all velocity that is pointing up the slope;
			_slopeMovementVelocity = VectorMath.RemoveDotVector(_slopeMovementVelocity, _pointDownVector);

			//Add movement velocity to momentum;
			_horizontalMomentum += _slopeMovementVelocity * Time.fixedDeltaTime;
		}

		//Apply friction to horizontal momentum based on whether the controller is grounded;
		if (currentControllerState == ControllerState.Grounded)
			_horizontalMomentum = VectorMath.IncrementVectorTowardTargetVector(_horizontalMomentum, groundFriction, Time.deltaTime, Vector3.zero);
		else
			_horizontalMomentum = VectorMath.IncrementVectorTowardTargetVector(_horizontalMomentum, airFriction, Time.deltaTime, Vector3.zero);

		//Add horizontal and vertical momentum back together;
		momentum = _horizontalMomentum + _verticalMomentum;

		//Additional momentum calculations for sliding;
		if (currentControllerState == ControllerState.Sliding)
		{
			//Project the current momentum onto the current ground normal if the controller is sliding down a slope;
			momentum = Vector3.ProjectOnPlane(momentum, mover.GetGroundNormal());

			//Remove any upwards momentum when sliding;
			if (VectorMath.GetDotProduct(momentum, tr.up) > 0f)
				momentum = VectorMath.RemoveDotVector(momentum, tr.up);

			//Apply additional slide gravity;
			Vector3 _slideDirection = Vector3.ProjectOnPlane(-tr.up, mover.GetGroundNormal()).normalized;
			momentum += _slideDirection * slideGravity * Time.deltaTime;
		}

		//If controller is jumping, override vertical velocity with jumpSpeed;
		if (currentControllerState == ControllerState.Jumping)
		{
			momentum = VectorMath.RemoveDotVector(momentum, tr.up);
			momentum += JumpMomentum();
		}

		if (useLocalMomentum)
			momentum = tr.worldToLocalMatrix * momentum;

		//Finally, clamp momentum if it is too big
		momentum = Vector3.ClampMagnitude(momentum, maximumVelocity);
	}

	private Vector3 JumpMomentum() => tr.up * jumpSpeed;

	//Events;

	//This function is called when the player has initiated a jump;
	void OnJumpStart()
	{
		//If local momentum is used, transform momentum into world coordinates first;
		if (useLocalMomentum)
			momentum = tr.localToWorldMatrix * momentum;

		momentum += JumpMomentum();

		//Set jump start time;
		currentJumpStartTime = Time.time;

		//Lock jump input until jump key is released again;
		jumpKeyState.IsKeyLocked = true;

		//Call event;
		if (OnJump != null)
			OnJump(momentum);

		if (useLocalMomentum)
			momentum = tr.worldToLocalMatrix * momentum;
	}

	//This function is called when the controller has lost ground contact, i.e. is either falling or rising, or generally in the air;
	void OnGroundContactLost()
	{
		//If local momentum is used, transform momentum into world coordinates first;
		if (useLocalMomentum)
			momentum = tr.localToWorldMatrix * momentum;

		//Get current movement velocity;
		Vector3 _velocity = GetMovementVelocity();

		//Check if the controller has both momentum and a current movement velocity;
		if (_velocity.sqrMagnitude >= 0f && momentum.sqrMagnitude > 0f)
		{
			//Project momentum onto movement direction;
			Vector3 _projectedMomentum = Vector3.Project(momentum, _velocity.normalized);
			//Calculate dot product to determine whether momentum and movement are aligned;
			float _dot = VectorMath.GetDotProduct(_projectedMomentum.normalized, _velocity.normalized);

			//If current momentum is already pointing in the same direction as movement velocity,
			//Don't add further momentum (or limit movement velocity) to prevent unwanted speed accumulation;
			if (_projectedMomentum.sqrMagnitude >= _velocity.sqrMagnitude && _dot > 0f)
				_velocity = Vector3.zero;
			else if (_dot > 0f)
				_velocity -= _projectedMomentum;
		}

		//Add movement velocity to momentum;
		momentum += _velocity;

		if (useLocalMomentum)
			momentum = tr.worldToLocalMatrix * momentum;
	}

	//This function is called when the controller has landed on a surface after being in the air;
	void OnGroundContactRegained()
	{
		isLateForgivenessJumpPrevented = false;

		//Call 'OnLand' event;
		if (OnLand != null)
		{
			Vector3 _collisionVelocity = momentum;
			//If local momentum is used, transform momentum into world coordinates first;
			if (useLocalMomentum)
				_collisionVelocity = tr.localToWorldMatrix * _collisionVelocity;

			OnLand(_collisionVelocity);
		}

	}

	//This function is called when the controller has collided with a ceiling while jumping or moving upwards;
	void OnCeilingContact()
	{
		//If local momentum is used, transform momentum into world coordinates first;
		if (useLocalMomentum)
			momentum = tr.localToWorldMatrix * momentum;

		//Remove all vertical parts of momentum;
		momentum = VectorMath.RemoveDotVector(momentum, tr.up);

		if (useLocalMomentum)
			momentum = tr.worldToLocalMatrix * momentum;
	}

	//Helper functions;

	//Returns 'true' if vertical momentum is above a small threshold;
	private bool IsRisingOrFalling()
	{
		//Calculate current vertical momentum;
		Vector3 _verticalMomentum = VectorMath.ExtractDotVector(GetMomentum(), tr.up);

		//Setup threshold to check against;
		//For most applications, a value of '0.001f' is recommended;
		float _limit = 0.001f;

		//Return true if vertical momentum is above '_limit';
		return (_verticalMomentum.magnitude > _limit);
	}

	//Returns true if angle between controller and ground normal is too big (> slope limit), i.e. ground is too steep;
	private bool IsGroundTooSteep()
	{
		if (!mover.IsGrounded())
			return true;

		return (Vector3.Angle(mover.GetGroundNormal(), tr.up) > slopeLimit);
	}

	//Getters;

	//Get last frame's velocity;
	public override Vector3 GetVelocity()
	{
		return savedVelocity;
	}

	//Get last frame's movement velocity (momentum is ignored);
	public override Vector3 GetMovementVelocity()
	{
		return savedMovementVelocity;
	}

	//Get current momentum;
	public Vector3 GetMomentum()
	{
		Vector3 _worldMomentum = momentum;
		if (useLocalMomentum)
			_worldMomentum = tr.localToWorldMatrix * momentum;

		return _worldMomentum;
	}

	//Returns 'true' if controller is grounded (or sliding down a slope);
	public override bool IsGrounded()
	{
		return (currentControllerState == ControllerState.Grounded || currentControllerState == ControllerState.Sliding);
	}

	//Returns 'true' if controller is sliding;
	public bool IsSliding()
	{
		return (currentControllerState == ControllerState.Sliding);
	}

	//Add momentum to controller;
	public void AddMomentum(Vector3 _momentum)
	{
		if (useLocalMomentum)
			momentum = tr.localToWorldMatrix * momentum;

		momentum += _momentum;

		if (useLocalMomentum)
			momentum = tr.worldToLocalMatrix * momentum;
	}

	//Set controller momentum directly;
	public void SetMomentum(Vector3 _newMomentum)
	{
		if (useLocalMomentum)
			momentum = tr.worldToLocalMatrix * _newMomentum;
		else
			momentum = _newMomentum;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (Time.fixedDeltaTime > 0f && ((1 << collision.collider.gameObject.layer) & impulseLayerMask) != 0)
		{
			float bounce = 1f;
			float scale = baseImpulseScale * bounce;

			// Sometimes impulse is in the wrong direction because the player hits the object. The normal
			// is in the right direction, but scaled wrong, so we can just combine them
			Vector3 impulse = Vector3.zero;
			for (int i = 0; i < collision.contactCount; i++)
            {
				impulse += collision.GetContact(i).normal;
				Debug.DrawRay(collision.GetContact(i).point, collision.GetContact(i).normal, Color.yellow, 1f);
			}

			impulse = impulse.normalized * collision.impulse.magnitude;

			Debug.Log($"Applying impulse of {impulse} at scale {scale} to runner (used {collision.contactCount} contacts)", this);

			if (scale > 0f && impulse.sqrMagnitude > 0.1f)
            {
				AddMomentum(impulse * scale);
				Debug.DrawRay(transform.position, impulse, Color.green, 1f);
			}
		}
	}
}
