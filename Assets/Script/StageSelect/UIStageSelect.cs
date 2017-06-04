using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//ステージ選択
public class UIStageSelect : MonoBehaviour {
	//依存クラス
	protected GameManager gm;

	[Header("ゲームパッド関連")]
	public UIGamePadMap gamePadMap;

	[Header("選択関連")]
	public ToolBox.Stage selectStage;				//選択ステージ
	public MeshFilter previewMeshFilter;
	protected List<ToolBox.Stage> selectStageList;	//選択ステージリスト

	[Header("UIパーツ")]
	public List<UIButtonComponents> buttonList;	//ボタンリスト

#region MonoBehaviourイベント
	protected void Start() {
		gm = GameManager.Instance;
		//ゲームパッドマップ
		gamePadMap.flagControll = false;
		//テスト
		SetCategoryStageList("Simple");
	}
#endregion

#region 関数
	//ステージリストを設定
	protected void SetCategoryStageList(string category) {
		selectStageList = null;
		//選択ステージリストに設定
		if(gm.stageDic.ContainsKey(category)) {
			selectStageList = gm.stageDic[category];
		}
		//ステージ選択ボタン設定
		SetButtonList(selectStageList);
	}
	//ボタンを設定
	protected void SetButtonList(List<ToolBox.Stage> list) {
		//ボタンの数だけループ
		for(int i = 0; i < buttonList.Count; i++) {
			//範囲確認
			if(list.Count <= i) {
				//範囲外。ボタンは非表示
				buttonList[i].gameObject.SetActive(false);
			} else {
				//範囲内。ボタンは表示
				buttonList[i].gameObject.SetActive(true);
				//機体の名前を設定
				buttonList[i].label.text = list[i].name;
			}
		}
	}
#endregion

#region UIイベント
	//最後のtweenの終了イベント
	protected void OnTweenFinished() {
		//ゲームパッドマップ
		gamePadMap.flagControll = true;
	}
	//ステージボタン
	protected void StageButtonHover(GameObject g) {
		//インデックス取得
		if(selectStageList == null) return;
		int index = int.Parse(g.name);
		ToolBox.Stage stage = selectStageList[index];
		if(selectStage == stage) return;
		selectStage = stage;
		//表示
		GameObject obj = (GameObject)Instantiate(selectStage.stageObjectList[0].Key);
		Mesh mesh = obj.GetComponent<MeshFilter>().mesh;
		gm.StopMeshCreating();
		gm.StartMeshCreating(mesh, previewMeshFilter);
		Destroy(obj);
	}
	protected void StageButtonClicked() {
		gm.selectStage = selectStage;
		gm.LoadLevel("Stage");
	}
#endregion
}