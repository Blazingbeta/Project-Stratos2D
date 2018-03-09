using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour {
	Dropdown m_levelSelector;
	string[] levels;
	public static string levelFilePath = "default.lvl";
	// Use this for initialization
	private void Start()
	{
		m_levelSelector = GetComponent<Dropdown>();
		string filePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\ProjectStratos\\2D\\";
		levels = System.IO.Directory.GetFiles(filePath, "*.lvl");
		List<string> condencedLevelNames = new List<string>();
		for (int j = 0; j < levels.Length; j++)
		{
			condencedLevelNames.Add(levels[j].Substring(filePath.Length));
			print(condencedLevelNames[j]);
		}
		m_levelSelector.AddOptions(condencedLevelNames);
	}
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
	}
	public void RunLevel()
	{
		levelFilePath = levels[m_levelSelector.value];
		SceneManager.LoadScene("PlatformerDebug");
	}
	public void LoadEditor()
	{
		SceneManager.LoadScene("LevelEditor");
	}
}
