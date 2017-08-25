using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SerializationTest.Common;

namespace SerializationTest.Test6
{

	[Serializable]
	public class Zoo : ISerializationCallbackReceiver
	{
		[NonSerialized]
		public List<Animal> animals = new List<Animal>();

		[SerializeField]
		private List<Cat> catList;
		[SerializeField]
		private List<Dog> dogList;
		[SerializeField]
		private List<Giraffe> giraffeList;
		[SerializeField]
		private List<int> indexList;

		public void OnBeforeSerialize() {
			catList = new List<Cat>();
			dogList = new List<Dog>();
			giraffeList = new List<Giraffe>();
			indexList = new List<int>();

			for(int i = 0; i < animals.Count; i++) {
				var type = animals[i].GetType();
				if(type == typeof(Cat)) {
					indexList.Add(0);
					indexList.Add(catList.Count);
					catList.Add((Cat)animals[i]);
				}
				if(type == typeof(Dog)) {
					indexList.Add(1);
					indexList.Add(dogList.Count);
					dogList.Add((Dog)animals[i]);
				}
				if(type == typeof(Giraffe)) {
					indexList.Add(2);
					indexList.Add(giraffeList.Count);
					giraffeList.Add((Giraffe)animals[i]);
				}
			}
		}

		public void OnAfterDeserialize() {
			animals.Clear();

			for(int i = 0; i < indexList.Count; i += 2) {
				switch(indexList[i]) {
				case 0:
					animals.Add(catList[indexList[i + 1]]);
					break;
				case 1:
					animals.Add(dogList[indexList[i + 1]]);
					break;
				case 2:
					animals.Add(giraffeList[indexList[i + 1]]);
					break;
				}
			}

			indexList = null;
			catList = null;
			dogList = null;
			giraffeList = null;
		}
	}

	public class TestWindow6 : EditorWindow {
		private Zoo zoo;

		void OnEnable() {
			titleContent.text = "TestWindow6";

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

		[MenuItem ("Window/Serialization Test/Test Window 6")]
		public static void  ShowWindow()
		{
			EditorWindow.GetWindow<TestWindow6>();
		}

	} // class TestWindow6

} // namespace SerializationTest.Test6