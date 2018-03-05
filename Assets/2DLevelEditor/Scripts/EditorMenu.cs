using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace LevelEditor2D
{
	public class EditorMenu : MonoBehaviour
	{
		public static bool m_menuOpen = false;
		[SerializeField] GameObject m_tabUI = null, m_otherUI = null;

		//Selection Menu Stuff
		[SerializeField] Transform m_selectionButtons;
		[SerializeField] Image m_selectionImage;
		private Transform m_currentSelectedButton;


		static GameObject[] m_placeables;
		private static int m_currentSelectedIndex;
		private void Start()
		{
			Object[] resourceObjs = Resources.LoadAll("LevelObjects/", typeof(GameObject));
			m_placeables = new GameObject[resourceObjs.Length];
			for (int j = 0; j < m_placeables.Length; j++)
			{
				m_placeables[j] = (GameObject)resourceObjs[j];
			}
			SelectionButtonPressed("Floor");
		}
		void Update()
		{
			if (Input.GetKeyDown(KeyCode.Tab))
			{
				m_menuOpen = !m_menuOpen;
				m_tabUI.SetActive(m_menuOpen);
				m_otherUI.SetActive(!m_menuOpen);
			}
		}
		public void SelectionButtonPressed(string buttonName)
		{
			if (m_currentSelectedButton)
			{
				m_currentSelectedButton.GetChild(0).gameObject.SetActive(false);
			}
			m_currentSelectedButton = m_selectionButtons.Find(buttonName);
			m_currentSelectedButton.GetChild(0).gameObject.SetActive(true);
			m_currentSelectedIndex = FindIndexOfObject(buttonName);
			Image buttonImage = m_currentSelectedButton.GetChild(1).GetComponent<Image>();
			m_selectionImage.sprite = buttonImage.sprite;
			m_selectionImage.color = buttonImage.color;
		}
		private int FindIndexOfObject(string objectName)
		{
			for(int j = 0; j < m_placeables.Length; j++)
			{
				if (m_placeables[j].name == objectName) return j;
			}
			Debug.LogError("Warning: was not able to find index of object " + objectName);
			return 0;
		}
		public static GameObject GetCurrentObject()
		{
			return m_placeables[m_currentSelectedIndex];
		}
	}
}