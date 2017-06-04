using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(ScreenShot))]
public class Editor_ScreenShot : Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector();
		ScreenShot t = target as ScreenShot;

		if (GUILayout.Button("ScreenShot")) {
			//押したときの処理
			t.TakeScreenShot();
		}
	}
}