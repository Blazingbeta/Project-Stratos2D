using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LevelEditor2D
{
	public class LevelEditPlacer : MonoBehaviour
	{
		//[SerializeField] GameObject m_currentObject;
		private Dictionary<IntVec2, GameObject> m_currentGrid = new Dictionary<IntVec2, GameObject>();
		void Update()
		{
			if (EditorMenu.m_menuOpen) return;
			if (Input.GetMouseButton(0))
			{
				Vector3 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				IntVec2 roundedPos;
				roundedPos.x = Mathf.RoundToInt(clickPos.x);
				roundedPos.y = Mathf.RoundToInt(clickPos.y);
				if (!m_currentGrid.ContainsKey(roundedPos))
				{
					Vector3 spawnPos = roundedPos.ToVec2();
					GameObject newObj = Instantiate(EditorMenu.GetCurrentObject(), spawnPos, Quaternion.identity);
					m_currentGrid.Add(roundedPos, newObj);
				}
			}
		}
		[System.Serializable]
		public struct IntVec2
		{
			public int x;
			public int y;
			public Vector2 ToVec2()
			{
				Vector2 output = Vector2.zero;
				output.x = x;
				output.y = y;
				return output;
			}
		}
	}
}