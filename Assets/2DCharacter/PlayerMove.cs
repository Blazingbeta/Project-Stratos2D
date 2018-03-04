using System.Collections;
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
		[SerializeField] [Range(0.0f, 0.5f)] private float m_groundDrag = 0.05f;
		[SerializeField] [Range(0.0f, 0.5f)] private float m_airDrag = 0.01f;
		[SerializeField] [Range(0.0f, 0.5f)] private float m_idleDrag = 0.01f;
		[SerializeField] [Range(0.0f, 1.0f)] private float m_airControlPercent = 0.8f;
		[SerializeField] private LayerMask m_landMask;

		//Private Storage
		private float m_moveAccel = 0.0f;
		private bool m_inSubState = false;
		private bool m_isSprinting = false;

		//Components
		Rigidbody2D m_rb;
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
			transform.position += Vector3.right * m_moveAccel * Time.deltaTime;
		}

		#region MoveStates
		///The primary states of movement with the logic on when to go to other states.
		///Make sure all of these have a coresponding EMovementState and are added to the dictionary.
		private void IdleState()
		{
			m_moveAccel *= (1 - m_idleDrag);
			if (Input.GetKeyDown(KeyCode.W))
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
			if (Input.GetAxis("Horizontal") == 0)
			{
				StartCoroutine(SubstateMoveStop());
			}
			if (Input.GetKeyDown(KeyCode.W))
			{
				StartCoroutine(SubstateJump());
			}
		}
		private void AirbornState()
		{
			m_moveAccel += Input.GetAxis("Horizontal") * (m_isSprinting ? m_runSpeed : m_moveSpeed)*m_airControlPercent;
			m_moveAccel *= (1 - m_airDrag);
			if (m_rb.velocity.y < 0)
			{
				Debug.DrawLine(transform.position+Vector3.back, transform.position + Vector3.down*1.5f+Vector3.back, Color.green);
				RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.5f, m_landMask);
				if (hit.collider)
				{
					StartCoroutine(SubstateLand());
				}
			}
		}
		private void WallState()
		{

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
		#endregion
	}
}