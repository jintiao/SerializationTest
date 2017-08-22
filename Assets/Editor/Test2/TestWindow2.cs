using System;
using UnityEngine;
using UnityEditor;

namespace SerializationTest.Test2
{

public class TestWindow2 : EditorWindow {
	public MyScriptableObject2 m;

	void OnEnable() {
		if(m == null) 
			m = CreateInstance<MyScriptableObject2>();

		titleContent.text = "TestWindow2";
	}

	void OnGUI() {
		EditorGUILayout.Space();
		m.OnGUI();
		EditorGUILayout.Space();
		if(GUILayout.Button("roll")) {
			m.Roll();
		}
	}

	[MenuItem ("Window/Serialization Test/Test Window 2")]
	public static void  ShowWindow() {
		EditorWindow.GetWindow<TestWindow2>();
	}

} // class TestWindow2

} // namespace SerializationTest.Test2