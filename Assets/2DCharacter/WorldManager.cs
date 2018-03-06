using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Character2D
{
	public class WorldManager : MonoBehaviour
	{
		GameObject m_enviroment;
		GameObject[] m_worldObjects;
		GameObject[] m_placeables;
		private void Start()
		{
			LoadMap("Test.lvl");
		}
		private void LoadMap(string path)
		{
			ImportPlaceables();

			m_enviroment = GameObject.Find("Enviroment");
			LevelEditor2D.LevelData levelData = LevelEditor2D.LevelData.LoadGrid(path);
			m_worldObjects = new GameObject[levelData.m_grid.Length];
			for(int j = 0; j < levelData.m_grid.Length; j++)
			{
				Vector3 position = Vector3.zero;
				position.x = levelData.m_grid[j].m_x;
				position.y = levelData.m_grid[j].m_y;
				m_worldObjects[j] = Instantiate(m_placeables[levelData.m_grid[j].m_objID], position, Quaternion.identity, m_enviroment.transform);
			}
			GameObject player = GameObject.FindGameObjectWithTag("Player");
			GameObject startFlag = GameObject.FindGameObjectWithTag("StartFlag");
			if (startFlag)
			{
				player.transform.position = startFlag.transform.position;
			}
		}
		private void ImportPlaceables()
		{
			Object[] resourceObjs = Resources.LoadAll("LevelObjects/", typeof(GameObject));
			GameObject[] gameObjs = new GameObject[resourceObjs.Length];
			m_placeables = new GameObject[resourceObjs.Length];
			for (int j = 0; j < m_placeables.Length; j++)
			{
				gameObjs[j] = (GameObject)resourceObjs[j];
				//sort placeables by their object id's
				m_placeables[gameObjs[j].GetComponent<ObjectInfo>().m_objectID] = gameObjs[j];
			}
		}

	}
}