//using UnityEngine;
//using UnityEditor;
//using System.Collections;

////CustomEditorの引数には拡張したクラスを指定
//[CustomEditor(typeof(AudioManager))]
//public class Editor_AudioManager : Editor {
//	//OnInspectorGUIをoverrideする
//	public override void OnInspectorGUI() {
//		AudioManager t = target as AudioManager;
//		t.BGMVolumeChange(GUILayout.HorizontalSlider(t.bgmVolume, 0f, 1f));
//		t.SEVolumeChange(GUILayout.HorizontalSlider(t.seVolume, 0f, 1f));
//	}
//}