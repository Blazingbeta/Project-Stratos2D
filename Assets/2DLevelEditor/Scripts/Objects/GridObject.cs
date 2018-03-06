using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace LevelEditor2D
{
	[System.Serializable]
	public struct GridObject
	{
		public int m_x, m_y, m_objID;
	}
	[System.Serializable]
	public struct LevelData
	{
		public GridObject[] m_grid;

		public static LevelData LoadGrid(string path)
		{
			IFormatter formatter = new BinaryFormatter();
			Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
			LevelData grid = (LevelData)formatter.Deserialize(stream);
			stream.Close();
			return grid;
		}
		public static void SaveGrid(LevelData levelToSave, string path)
		{
			IFormatter formatter = new BinaryFormatter();
			Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
			formatter.Serialize(stream, levelToSave);
			stream.Close();
		}
	}
}