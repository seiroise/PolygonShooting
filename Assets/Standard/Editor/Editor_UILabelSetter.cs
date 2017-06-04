using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(UILabelSetter))]
public class Editor_UIFontSetter : Editor {
//OnInspectorGUIをoverrideする
	public override void OnInspectorGUI() {
		//デフォルトのInspectorを表示
		DrawDefaultInspector();

		UILabelSetter t = target as UILabelSetter;

		//描画モード変更
		if (GUILayout.Button("フォント設定")) {
			//押したときの処理
			t.FontSet();
		}
	}
}