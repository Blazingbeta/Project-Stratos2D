using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Character2D
{
	public class CameraFollow : MonoBehaviour
	{
		[SerializeField] private Transform target;
		[SerializeField] private Vector3 offset;
		[SerializeField] private float smoothTime = 0.3F;
		private Vector3 velocity = Vector3.zero;
		void Update()
		{
			Vector3 targetPosition = target.position + offset;
			transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
		}
	}
}