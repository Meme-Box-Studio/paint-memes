using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace IndieStudio.DrawingAndColoring.Logic
{
	public class ShapesManager : MonoBehaviour
	{
		public List<Shape> shapes = new List<Shape>();
		[HideInInspector]
		public int lastSelectedShape;
		public static ShapesManager instance;

		void Awake()
		{
			if (instance == null)
			{
				instance = this;
				DontDestroyOnLoad(gameObject);
				lastSelectedShape = 0; // Оригинальное значение
			}
			else
			{
				Destroy(gameObject);
			}
		}

		[System.Serializable]
		public class Shape
		{
			public bool showContents = true;
			public GameObject gamePrefab;
		}
	}
}