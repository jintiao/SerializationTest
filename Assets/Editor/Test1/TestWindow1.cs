using System;
using UnityEngine;
using UnityEditor;

namespace SerializationTest.Test1
{

public class MyClass {
	public string s;

	public void OnGUI() {
		s = EditorGUILayout.TextField("public string s", s);
	}
}

[Serializable]
public class MyClassSerializable {
	public float f1;
	[NonSerialized]public float f2;
	private int i1;
	[SerializeField]private int i2;

	public void OnGUI() {
		f1 = EditorGUILayout.Slider("public float f1", f1, 0, 100);
		f2 = EditorGUILayout.Slider("[NonSerialized]public float f2", f2, 0, 100);
		i1 = EditorGUILayout.IntSlider("private int i1", i1, 0, 100);
		i2 = EditorGUILayout.IntSlider("[SerializeField]private int i2", i2, 0, 100);
	}
}

public class TestWindow1 : EditorWindow {
	public MyClass m1;
	public MyClassSerializable s1;
	private MyClassSerializable s2;

	void OnEnable() {
		if(m1 == null)
			m1 = new MyClass();
		if(s1 == null)
			s1 = new MyClassSerializable();
		if(s2 == null)
			s2 = new MyClassSerializable();
			
		titleContent.text = "TestWindow1";
	}

	void OnGUI() {
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("public MyClass m1");
		m1.OnGUI();
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("public MyClassSerializable s1");
		s1.OnGUI();
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("private MyClassSerializable s2");
		s2.OnGUI();
	}

	[MenuItem ("Window/Serialization Test/Test Window 1")]
	public static void  ShowWindow()
	{
		EditorWindow.GetWindow<TestWindow1>();
	}

} // class TestWindow1

} // namespace SerializationTest.Test1