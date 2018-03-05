using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace LevelEditor2D
{
	public class EditorCameraControl : MonoBehaviour
	{
		[SerializeField] private float m_moveSpeed = 1.0f, m_sprintMod = 1.25f;
		[SerializeField] private Vector2 m_minBound = Vector2.zero, m_maxBound = Vector2.zero;
		void Update()
		{
			if (EditorMenu.m_menuOpen) return;
			Vector2 dir = Vector2.zero;
			dir.x = Input.GetAxis("Horizontal");
			dir.y = Input.GetAxis("Vertical");
			
			float sprintMod = (Input.GetKey(KeyCode.LeftShift) ? m_sprintMod : 1.0f);
			transform.position += (Vector3)dir * m_moveSpeed * sprintMod * Time.deltaTime;

			Vector3 clampedPos = transform.position;
			clampedPos.x = Mathf.Clamp(clampedPos.x, m_minBound.x, m_maxBound.x);
			clampedPos.y = Mathf.Clamp(clampedPos.y, m_minBound.y, m_maxBound.y);
			transform.position = clampedPos;
		}
	}
}