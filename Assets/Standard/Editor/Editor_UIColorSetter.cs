using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(UIColorSetter))]
public class Editor_UIColorSetter : Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector();
		UIColorSetter t = target as UIColorSetter;
		//色を設定
		if (GUILayout.Button("Set Color")) {
			//押したときの処理
			t.SetColor(t.setColor);
		}
	}
}