using UnityEngine;
using UnityEditor;
using System.Collections;

//CustomEditorの引数には拡張したクラスを指定
[CustomEditor(typeof(WireFrame))]
public class Editor_WireFrame : Editor {
	//OnInspectorGUIをoverrideする
	public override void OnInspectorGUI() {
		//デフォルトのInspectorを表示
		DrawDefaultInspector();

		WireFrame t = target as WireFrame;

		//描画モード変更
		if (GUILayout.Button("描画モード更新")) {
			//押したときの処理
			t.SetMT();
		}
	}
}