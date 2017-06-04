using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// 複数のラベルのインスタンスを管理する
/// </summary>
public class LabelManager : SingletonMonoBehaviour<LabelManager> {
	[Header("ラベル")]
	public FadeOutLabel labelPrefab;
	protected int instantNum = 128;
	private List<FadeOutLabel> labelList;
	public Color defaultColor = Color.white;
	private Vector3 defaulutScale;
	[Header("座標変換用")]
	public Camera uiCamera;	//ターゲットカメラ
	[Header("物理挙動")]
	public float yForceMin = 20f;
	public float yForceMax = 40f;
	public float xMoveMin = -20f;
	public float xMoveMax = 20f;

#region MonoBehaviourイベント
	protected override void Awake() {
		base.Awake();
	}
	protected void Start() {
		labelList = new List<FadeOutLabel>();
		defaulutScale = labelPrefab.transform.localScale;
		//ラベル作成
		GameObject l;
		for (int i = 0; i < instantNum; i++) {
			l = (GameObject)Instantiate(labelPrefab.gameObject);
			l.transform.parent = transform;
			l.transform.localScale = labelPrefab.transform.localScale;
			labelList.Add(l.GetComponent<FadeOutLabel>());
		}
	}
#endregion
#region 関数
	/// <summary>
	/// 使用されていないラベルを取得する
	/// </summary>
	public FadeOutLabel GetLabel() {
		foreach (FadeOutLabel label in labelList) {
			if (!label.gameObject.activeInHierarchy) {
				return label;
			}
		}
		return null;
	}
	/// <summary>
	/// ラベルを設定(色と大きさも)
	/// </summary>
	public void SetLabel(string text, float time, Vector3 pos, Color color, Vector3 scale) {
		//ラベルを取得
		FadeOutLabel f = GetLabel();
		if (f != null) {
			//座標変換
			//pos = FuncBox.ViewPointTransform(Camera.main, pos, uiCamera);
			f.transform.position = pos;
			f.transform.localScale = scale;
			f.StartFadeLabel(text, color, time);
			f.SetPhysicsParameter(yForceMin, yForceMax, xMoveMin, xMoveMax);
		}
	}
	/// <summary>
	/// ラベルを設定
	/// </summary>
	public void SetLabel(string text, float time, Vector3 pos) {
		SetLabel(text, time, pos, defaultColor, defaulutScale);
	}	
#endregion
}