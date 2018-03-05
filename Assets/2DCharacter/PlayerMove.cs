﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// The logic, functionality, and state machine behind the players movement system.
/// Primary states are listed in EMovementState, Substates such as state transitions are their own IENumerable functions that are run on Coroutine
/// It might be advisable to get the controls from another source rather than directly calling Input.Get
/// </summary>
namespace Character2D
{
	[SerializeField] public enum EMovementState { Idle, Moving, Airborn, Wallslide, Dead}
	delegate void PlayerMoveState();
	public class PlayerMove : MonoBehaviour {

		//StateMachine
		[SerializeField] EMovementState m_state = EMovementState.Idle;
		Dictionary<EMovementState, PlayerMoveState> m_moveStates = new Dictionary<EMovementState, PlayerMoveState>();

		//Inspector Values
		[SerializeField] private float m_moveSpeed = 3.0f;
		[SerializeField] private float m_runSpeed = 5.0f;
		[SerializeField] private float m_jumpHeight = 10.0f;
		[SerializeField] private float m_fallingGravityMod = 2.0f;
		[SerializeField] private float m_jumpHoldForce = 0.1f;
		[SerializeField] private float m_walljumpForce = 0.0f;
		[SerializeField] private Vector2 m_walljumpDirection = Vector2.zero;
		[SerializeField] [Range(0.0f, 0.5f)] private float m_groundDrag = 0.05f;
		[SerializeField] [Range(0.0f, 0.5f)] private float m_airDrag = 0.01f;
		[SerializeField] [Range(0.0f, 0.5f)] private float m_idleDrag = 0.01f;
		[SerializeField] [Range(0.0f, 1.0f)] private float m_airControlPercent = 0.8f;
		[SerializeField] [Range(0.0f, 1.0f)] private float m_wallslideGravityMod = 0.2f;
		[SerializeField] [Range(-0.5f, 0.5f)] private float m_raycastExtension = 0.0f;
		[SerializeField] private LayerMask m_landMask = 0, m_wallslideMask = 0;

		//Private Storage
		private Vector3 m_hitboxCenterOffset;
		private float m_moveAccel = 0.0f;
		private bool m_inSubState = false;
		private bool m_isSprinting = false;
		private bool m_holdJump = false;
		private bool m_slideOnRight = false;

		//Components
		Rigidbody2D m_rb;
		BoxCollider2D m_hitbox;
		private void Awake()
		{
			m_moveStates.Add(EMovementState.Idle, IdleState);
			m_moveStates.Add(EMovementState.Moving, MoveState);
			m_moveStates.Add(EMovementState.Airborn, AirbornState);
			m_moveStates.Add(EMovementState.Wallslide, WallState);
			m_moveStates.Add(EMovementState.Dead, DeadState);
		}
		private void Start()
		{
			m_rb = GetComponent<Rigidbody2D>();
			m_hitbox = GetComponent<BoxCollider2D>();
			m_hitboxCenterOffset = m_hitbox.offset;
		}
		private void Update()
		{
			m_isSprinting = Input.GetKey(KeyCode.LeftShift);
			//If not currently doing a coroutine substate, do the regular state action
			if (!m_inSubState)
			{
				m_moveStates[m_state]();
			}
			ApplyXForce();
		}
		private void ApplyXForce()
		{
			//m_rb.AddForce(Vector2.right * m_moveAccel, ForceMode2D.Force);
			m_moveAccel = (float)System.Math.Truncate(m_moveAccel * 1000.0f)/1000.0f;
			transform.position += Vector3.right * m_moveAccel * Time.deltaTime;
		}

		#region MoveStates
		///The primary states of movement with the logic on when to go to other states.
		///Make sure all of these have a coresponding EMovementState and are added to the dictionary.
		private void IdleState()
		{
			m_moveAccel *= (1 - m_idleDrag);
			WallCollide();
			if (Input.GetButtonDown("Jump"))
			{
				StartCoroutine(SubstateJump());
			}
			else if (Input.GetAxis("Horizontal") != 0)
			{
				StartCoroutine(SubstateMoveStartup());
			}
		}
		/// <summary>
		/// Walking or Running
		/// </summary>
		private void MoveState()
		{
			m_moveAccel += Input.GetAxis("Horizontal") * (m_isSprinting ? m_runSpeed : m_moveSpeed);
			m_moveAccel *= (1 - m_groundDrag);
			WallCollide();
			if (Input.GetAxis("Horizontal") == 0)
			{
				StartCoroutine(SubstateMoveStop());
			}
			if (Input.GetButtonDown("Jump"))
			{
				StartCoroutine(SubstateJump());
			}
		}
		/// <summary>
		/// Raycasts for a wall and stop movement if you collide with one
		/// </summary>
		/// <returns>Returns true if movement was stopped.</returns>
		private bool WallCollide()
		{
			int directionMod = (m_moveAccel > 0 ? 1 : m_moveAccel < 0 ? -1 : 0);
			if (directionMod == 0) return false;
			Debug.DrawLine(transform.position + m_hitboxCenterOffset + Vector3.back, (Vector3.right * directionMod * (m_hitbox.size.x + m_raycastExtension)) + transform.position + m_hitboxCenterOffset + Vector3.back, Color.red);
			RaycastHit2D hit = Physics2D.Raycast(transform.position + m_hitboxCenterOffset, Vector2.right * directionMod, m_hitbox.size.x + m_raycastExtension, m_wallslideMask);
			if (hit.collider)
			{
				m_moveAccel = 0.0f;
				return true;
			}
			return false;
		}
		private void AirbornState()
		{
			if (m_holdJump)
			{
				m_rb.AddForce(Vector2.up * m_jumpHoldForce, ForceMode2D.Force);
				m_holdJump = Input.GetButton("Jump");
			}
			m_rb.gravityScale = 1.0f;
			m_moveAccel += Input.GetAxis("Horizontal") * (m_isSprinting ? m_runSpeed : m_moveSpeed)*m_airControlPercent;
			m_moveAccel *= (1 - m_airDrag);
			if (ShouldWallslide())
			{
				m_holdJump = false;
				StartCoroutine(SubstateWallslide());
			}
			else if (m_rb.velocity.y <= 0)
			{
				m_holdJump = false;
				m_rb.gravityScale = m_fallingGravityMod;
				///Does a raycast from the center of your hitbox to the edge of the hitbox + m_raycastExtension. Incrase m_raycastExtension if collisions dont always work
				RaycastHit2D hit = Physics2D.Raycast(transform.position+m_hitboxCenterOffset, Vector2.down, m_hitbox.size.y+m_raycastExtension, m_landMask);
				if (hit.collider)
				{
					m_rb.gravityScale = 1.0f;
					StartCoroutine(SubstateLand());
				}
			}
		}
		private bool ShouldWallslide()
		{
			///Does a raycast from the center of your hitbox to the edge of the hitbox + m_raycastExtension. Incrase m_raycastExtension if collisions dont always work
			int directionMod = (m_moveAccel > 0 ? 1 : m_moveAccel < 0 ? -1 : 0);
			if (directionMod == 0) return false;
			Debug.DrawLine(transform.position + m_hitboxCenterOffset + Vector3.back, (Vector3.right * directionMod * (m_hitbox.size.x + m_raycastExtension)) + transform.position + m_hitboxCenterOffset + Vector3.back, Color.blue);
			RaycastHit2D hit = Physics2D.Raycast(transform.position + m_hitboxCenterOffset, Vector2.right*directionMod, m_hitbox.size.x + m_raycastExtension, m_wallslideMask);
			if (hit.collider)
			{
				return true;
			}
			return false;
		}
		private void WallState()
		{
			//Only affect gravityscale if sliding down the wall not up
			if(m_rb.velocity.y <= 0) m_rb.gravityScale = m_wallslideGravityMod;
			if (!IsHoldingTowardsWall())
			{
				StartCoroutine(SubstateFallOffWall());
			}
			else if (Input.GetButtonDown("Jump"))
			{
				StartCoroutine(SubstateWallJump());
			}
			else
			{
				RaycastHit2D hit = Physics2D.Raycast(transform.position + m_hitboxCenterOffset, Vector2.down, m_hitbox.size.y + m_raycastExtension, m_landMask);
				if (hit.collider)
				{
					m_rb.gravityScale = 1.0f;
					StartCoroutine(SubstateLand());
				}
			}
		}
		private bool IsHoldingTowardsWall()
		{
			if (m_slideOnRight)
			{
				return Input.GetAxis("Horizontal") > 0;
			}
			else
			{
				return Input.GetAxis("Horizontal") < 0;
			}
		}
		private void DeadState()
		{

		}

		#endregion

		#region Substates
		///Make sure all of these set m_inSubState to true at the start and false at the end
		
		///<summary>
		///	Jumping from either an idle or moving state.
		///</summary>
		private IEnumerator SubstateJump()
		{
			m_inSubState = true;
			//Animation here
			yield return null;
			m_rb.AddForce(Vector2.up * m_jumpHeight, ForceMode2D.Impulse);
			m_state = EMovementState.Airborn;
			m_holdJump = true;
			m_inSubState = false;
		}
		/// <summary>
		/// Starting to move after being idle (or landing?)
		/// </summary>
		private IEnumerator SubstateMoveStartup()
		{
			m_inSubState = true;
			//Animation Here
			yield return null;
			m_state = EMovementState.Moving;
			m_inSubState = false;
		}
		/// <summary>
		/// Stopping movement after moving
		/// </summary>
		/// <returns></returns>
		private IEnumerator SubstateMoveStop()
		{
			m_inSubState = true;
			//Animation Here
			yield return null;
			m_state = EMovementState.Idle;
			m_inSubState = false;

		}
		/// <summary>
		/// Lands from a jump and transitions to idle
		/// </summary>
		/// <returns></returns>
		private IEnumerator SubstateLand()
		{
			m_inSubState = true;
			//Landing Animation Logic
			yield return null;
			m_state = EMovementState.Idle;
			m_inSubState = false;
		}
		/// <summary>
		/// Starts a wallslide
		/// </summary>
		/// <returns></returns>
		private IEnumerator SubstateWallslide()
		{
			m_inSubState = true;
			m_slideOnRight = m_moveAccel > 0;
			m_moveAccel = 0;
			Vector2 velocity = m_rb.velocity;
			velocity.x = 0;
			m_rb.velocity = velocity;
			//Wallslide animation logic
			yield return null;
			m_state = EMovementState.Wallslide;
			m_inSubState = false;
		}
		/// <summary>
		/// Player either stops holding against the wall or holds the opposite direction, going back to airborn state
		/// </summary>
		/// <returns></returns>
		private IEnumerator SubstateFallOffWall()
		{
			m_inSubState = true;
			//Landing Animation Logic
			yield return null;
			m_state = EMovementState.Airborn;
			m_inSubState = false;

		}
		/// <summary>
		/// Jumps off the side of a wall, going to Airborn state
		/// </summary>
		/// <returns></returns>
		private IEnumerator SubstateWallJump()
		{
			int jumpDir = m_slideOnRight ? -1 : 1;
			m_inSubState = true;
			//Landing Animation Logic
			yield return null;
			Vector2 dir = m_walljumpDirection;
			dir.x *= jumpDir;
			dir.Normalize();
			m_rb.AddForce(dir*m_walljumpForce, ForceMode2D.Impulse);
			yield return new WaitForSeconds(0.1f);
			m_state = EMovementState.Airborn;
			m_inSubState = false;
		}
		#endregion
	}
}