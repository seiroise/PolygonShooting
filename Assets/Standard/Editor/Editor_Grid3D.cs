using UnityEngine;
using UnityEditor;
using System.Collections;

//CustomEditorの引数には拡張したクラスを指定
[CustomEditor(typeof(Grid))]
public class Editor_Grid3D : Editor{
	
	//OnInspectorGUIをoverrideする
	public override void OnInspectorGUI() {
		//デフォルトのInspectorを表示
		DrawDefaultInspector();
		
		var t = target as Grid;

		//グリッド生成
		if (GUILayout.Button("グリッド生成")) {
			//押したときの処理
			t.InstantiateGrid();
		}

		//グリッド削除
		if (GUILayout.Button("グリッド削除")) {
			//押したときの処理
			t.DestroyGrid();
		}
	}
}