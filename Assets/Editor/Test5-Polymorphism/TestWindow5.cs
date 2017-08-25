using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SerializationTest.Common;

namespace SerializationTest.Test5
{

	[Serializable]
	public class Zoo
	{
		public List<Animal> animals = new List<Animal>();
	}
		
	public class TestWindow5 : EditorWindow {
		private Zoo zoo;

		void OnEnable() {
			titleContent.text = "TestWindow5";

			if(zoo == null) {
				zoo = new Zoo();
			}
		}

		void OnGUI() {
			EditorGUILayout.LabelField("animal count", zoo.animals.Count.ToString());
			for(int i = 0; i < zoo.animals.Count; i++) {
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(i.ToString(), zoo.animals[i].Species);
				if(GUILayout.Button("remove", GUILayout.Width(70))) {
					zoo.animals.RemoveAt(i);
				}
				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button("add Cat")) {
				zoo.animals.Add(new Cat());
			}
			if(GUILayout.Button("add Dog")) {
				zoo.animals.Add(new Dog());
			}
			if(GUILayout.Button("add Giraffe")) {
				zoo.animals.Add(new Giraffe());
			}
			EditorGUILayout.EndHorizontal();
		}

		[MenuItem ("Window/Serialization Test/Test Window 5")]
		public static void  ShowWindow()
		{
			EditorWindow.GetWindow<TestWindow5>();
		}

	} // class TestWindow5

} // namespace SerializationTest.Test5