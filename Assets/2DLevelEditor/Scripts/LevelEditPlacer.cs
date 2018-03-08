using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LevelEditor2D
{
	public class LevelEditPlacer : MonoBehaviour
	{
		//[SerializeField] GameObject m_currentObject;
		private Dictionary<IntVec2, GameObject> m_currentGrid = new Dictionary<IntVec2, GameObject>();

		[SerializeField] UnityEngine.UI.InputField m_levelNameField;

		void Update()
		{
			if (EditorMenu.m_menuOpen) return;
			if (Input.GetMouseButton(0))
			{
				IntVec2 roundedPos = GetClickPos();
				if (!m_currentGrid.ContainsKey(roundedPos))
				{
					Vector3 spawnPos = roundedPos.ToVec2();
					GameObject newObj = Instantiate(EditorMenu.GetCurrentObject(), spawnPos, Quaternion.identity);
					m_currentGrid.Add(roundedPos, newObj);
				}
			}
			else if (Input.GetMouseButton(1))
			{
				IntVec2 roundedPos = GetClickPos();
				if (m_currentGrid.ContainsKey(roundedPos))
				{
					Destroy(m_currentGrid[roundedPos]);
					m_currentGrid.Remove(roundedPos);
				}
			}
		}
		private IntVec2 GetClickPos()
		{
			Vector3 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			IntVec2 roundedPos;
			roundedPos.x = Mathf.RoundToInt(clickPos.x);
			roundedPos.y = Mathf.RoundToInt(clickPos.y);
			return roundedPos;
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
		public void SaveLevel()
		{
			if (m_levelNameField.text.Length == 0) return;
			string filePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
			LevelData levelData = new LevelData();
			List<IntVec2> positions = new List<IntVec2>(m_currentGrid.Keys);
			levelData.m_grid = new GridObject[positions.Count];
			for(int j = 0; j < positions.Count; j++)
			{
				int objId = m_currentGrid[positions[j]].GetComponent<ObjectInfo>().m_objectID;
				levelData.m_grid[j] = new GridObject
				{
					m_x = positions[j].x, m_y = positions[j].y, m_objID = objId
				};
			}
			filePath += "\\ProjectStratos\\2D\\" + m_levelNameField.text + ".lvl";
			Debug.Log(filePath);
			LevelData.SaveGrid(levelData, filePath);
		}
	}
}