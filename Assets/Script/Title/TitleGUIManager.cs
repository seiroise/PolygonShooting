using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//タイトル画面のUI管理
public class TitleGUIManager : SingletonMonoBehaviour<TitleGUIManager> {
	protected GameManager gm;
	[Header("メッシュ")]
	public MeshFilter titleMeshFilter;	//タイトル表示
	public MeshFilter[] shipMeshFilter;	//機体表示
#region MonoBehaviourイベント
	protected override void Awake() {
		base.Awake();
	}
	protected void Start() {
		gm = GameManager.Instance;
		//BGM再生
		gm.PlayTitleBGM();
	}
#endregion
#region UIイベント
	//Start
	protected void StartButtonClicked() {
		gm.LoadLevel("MainMenu");
	}
	//Quit
	protected void QuitButtonClicked() {
		Application.Quit();
	}
#endregion
}