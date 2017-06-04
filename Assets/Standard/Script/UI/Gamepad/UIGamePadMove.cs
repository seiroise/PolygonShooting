using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// ボタン(ゲームオブジェクト)をゲームパッドで移動するための
/// </summary>
public class UIGamePadMove : MonoBehaviour{
	[Header("ターゲット")]
	public GameObject startTarget;		//初期ターゲット
	public GameObject targetParent;		//ターゲットの親オブジェクト
	protected GameObject nowTarget;		//現在のターゲット
	[Header("ボタンオブジェクト")]
	public List<GameObject> buttons;		//ボタンオブジェクト(デフォルトでは子オブジェクトのリスト)
#region 関数
	/// <summary>
	/// 指定角度内で一番近くの子オブジェクトを取得する
	/// </summary>
	protected GameObject GetNearChildren(float minAngle, float maxAngle) {
		if(buttons.Count == 0) {
			SetChildren();
		}
		if(!nowTarget) {
			SetNewTarget(startTarget);
			return null;
		}
		//二次元平面(xy)で一番近いオブジェクトを探す
		float disX = float.MaxValue;
		float disY = 0;
		GameObject near = null;
		for(int i = 0; i < buttons.Count; i++) {
			if(nowTarget == buttons[i]) continue;
			if(!buttons[i].activeInHierarchy) continue;
			//角度計測
			float angle = FuncBox.TwoPointAngleD(nowTarget.transform.position, buttons[i].transform.position);
			Debug.Log(angle);
			if(minAngle <= angle && angle <= maxAngle) {
				//距離計測
				disY = Vector3.Distance(nowTarget.transform.position, buttons[i].transform.position);
				if(disX > disY) {
					near = buttons[i];
					disX = disY;
				}
			}
		}
		return near;
	}
	/// <summary>
	/// 新しいターゲットの設定
	/// </summary>
	protected void SetNewTarget(GameObject newTarget) {
		//ターゲットにイベント送信
		FuncBox.Notify(nowTarget, "OnHover", false);
		//ターゲット切り替え
		if(newTarget)
		nowTarget = newTarget;
		//ターゲットにイベント送信
		FuncBox.Notify(nowTarget, "OnHover", true);
	}
	/// <summary>
	/// 子オブジェクトを設定
	/// </summary>
	protected void SetChildren() {
		//親オブジェクトの設定
		if(!targetParent) {
			targetParent = gameObject;
		}
		//リスト初期化
		if(buttons == null) {
			buttons = new List<GameObject>();
		} else {
			buttons.Clear();
		}
		//子を取得
		foreach(Transform c in targetParent.transform) {
			buttons.Add(c.gameObject);
		}
	}
	/// <summary>
	/// 現在のターゲットにイベント通知
	/// </summary>
	protected void NotifyNowTarget(string functionName, object value) {
		FuncBox.Notify(nowTarget, functionName, value);
	}
#endregion
#region 入力イベント
	protected void AButtonDown() {
		NotifyNowTarget("OnPress", true);
	}
	protected void AButtonUp() {
		NotifyNowTarget("OnClick", null);
	}
	//上下入力
	protected void Right() {
		GameObject newTarget = GetNearChildren(0, 45);
		if(!newTarget) {
			newTarget = GetNearChildren(315, 360);
		}
		//ターゲット設定
		if(newTarget) {
			SetNewTarget(newTarget);
		}
	}
	protected void Left() {
		GameObject newTarget = GetNearChildren(135, 225);
		//ターゲット設定
		if(newTarget) {
			SetNewTarget(newTarget);
		}
	}
	protected void Up() {
		GameObject newTarget = GetNearChildren(45, 135);
		//ターゲット設定
		if(newTarget) {
			SetNewTarget(newTarget);
		}
	}
	protected void Down() {
		GameObject newTarget = GetNearChildren(225, 315);
		//ターゲット設定
		if(newTarget) {
			SetNewTarget(newTarget);
		}
	}
#endregion
}