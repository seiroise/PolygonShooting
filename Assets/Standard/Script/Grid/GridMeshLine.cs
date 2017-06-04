using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// グリッド上にメッシュ線を表示
/// <para>AddPosition</para>
/// </summary>
public class GridMeshLine : MeshLine {
	
	[Header("Grid")]
	public Grid grid;
	public Camera targetCamera;
	[Header("Event")]
	public GameObject target;

#region 関数(override)
	/// <summary>
	/// 座標を追加
	/// </summary>
	public override void AddPosition(Vector3 pos) {
		//グリッド内の点か確認
		if(!grid.WorldToGridCrossPosition(out pos, pos)) return;
		//追加
		base.AddPosition(pos);
	}
	/// <summary>
	/// 座標の挿入
	/// </summary>
	public override void InsertPosition(int index, Vector3 pos) {
		if(!grid.WorldToGridCrossPosition(out pos, pos)) return;
		//挿入
		base.InsertPosition(index, pos);
	}
	/// <summary>
	/// 座標を削除
	/// </summary>
	public override void RemovePosition(int index) {
		//描画フラグが立っていないときはそもそも削除できない
		base.RemovePosition(index);
		//要素数が0以下になった場合
		if(positions.Count <= 0) {
			//リセット
			Reset();
		}
	}
	/// <summary>
	/// インデックスを指定して座標の変更する
	/// </summary>
	public override void ChangePosition(int index, Vector3 pos) {
		//グリッド内の点か確認
		if(!grid.WorldToGridCrossPosition(out pos, pos)) return;
		base.ChangePosition(index, pos);
	}
	/// <summary>
	/// 全体のリセット
	/// </summary>
	public override void Reset() {
		base.Reset();
	}
	/// <summary>
	/// 一時的に座標を追加して線を描画
	/// </summary>
	public override void FlashPosition(Vector3 pos) {
		if(!grid.WorldToGridCrossPosition(out pos, pos)) return;
		base.FlashPosition(pos);
	}
#endregion
#region 関数(その他)
	/// <summary>
	/// targetにイベントを送信
	/// </summary>
	protected void Notify(string functionName, object value) {
		if(target) {
			target.SendMessage(functionName, value, SendMessageOptions.DontRequireReceiver);
		}
	}
#endregion
#region 座標関連_Last
	/// <summary>
	///座標を最後の要素に挿入
	/// </summary>
	public void InsertEndPosition(Vector3 pos) {
		int index = positions.Count - 1;
		//挿入
		InsertPosition(index, pos);
	}
	/// <summary>
	/// 最後の座標を削除
	/// </summary>
	public void RemoveEndPosition() {
		int index = positions.Count - 1;
		RemovePosition(index);
	}
	/// <summary>
	/// 最後の座標の変更する
	/// </summary>
	public void ChangeEndPosition(Vector3 pos) {
		int index = positions.Count - 1;
		ChangePosition(index, pos);
	}
#endregion
#region 座標関連_マウス
	/// <summary>
	/// マウスの座標を追加する
	/// </summary>
	public void AddPosition_Mouse() {
		Vector3 pos = FuncBox.GetMousePoint(targetCamera);
		AddPosition(pos);
	}
	/// <summary>
	/// マウス座標の挿入
	/// </summary>
	public void InsertPosition_Mouse(int index) {
		Vector3 pos = FuncBox.GetMousePoint(targetCamera);
		InsertPosition(index, pos);
	}
	/// <summary>
	/// インデックスを指定して座標の変更をマウス座標で変更する
	/// </summary>
	public void ChangePosition_Mouse(int index) {
		Vector3 pos = FuncBox.GetMousePoint(targetCamera);
		ChangePosition(index, pos);
	}
	/// <summary>
	/// 一時的にマウス座標を追加して線を描画する
	/// </summary>
	public void FlashPosition_Mouse() {
		Vector3 pos = FuncBox.GetMousePoint(targetCamera);
		FlashPosition(pos);
	}
	/// <summary>
	/// マウスの座標をグリッド座標に直して取得する。戻り値はグリッド内の座標か
	/// </summary>
	public bool GetMouseGridPos(out Vector3 mPos) {
		mPos = FuncBox.GetMousePoint(targetCamera);
		return grid.WorldToGridCrossPosition(out mPos, mPos);
	}
#endregion
#region 座標関連_マウス_Last
	/// <summary>
	/// マウス座標を最後の要素に挿入
	/// </summary>
	public void InsertEndPosition_Mouse() {
		Vector3 pos = FuncBox.GetMousePoint(targetCamera);
		InsertEndPosition(pos);
	}
	/// <summary>
	/// 最後の要素の座標をマウス座標をで変更する
	/// </summary>
	public void ChangeEndPosition_Mouse() {
		Vector3 pos = FuncBox.GetMousePoint(targetCamera);
		ChangeEndPosition(pos);
	}
#endregion
}