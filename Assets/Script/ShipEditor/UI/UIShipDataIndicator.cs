using UnityEngine;
using System.Collections;
/// <summary>
/// 機体パーツデータを表示する
/// </summary>
public class UIShipDataIndicator : MonoBehaviour {
	[Header("プレビュー")]
	public MeshFilter previewMf;		//プレビューメッシュ表示
	public Camera previewCamera;	//プレビュー表示カメラ
	[Header("UIパーツ")]
	public UILabel name;
	public UILabel hp;
	public UILabel cost;
	public UILabel launcherTotal;
	public UILabel launcherNormal;
	public UILabel launcherMissile;
	public UILabel launcherOptical;
	public UILabel boostTotal;
	public UILabel boostOutput;
	public UILabel size;
#region 関数
	/// <summary>
	/// 表示/非標示
	/// </summary>
	/// <param name="flag"></param>
	public void Indicate(bool flag) {
		gameObject.SetActive(flag);
		if(previewCamera) {
			previewCamera.gameObject.SetActive(flag);
		}
	}
#endregion
#region 設定関数
	/// <summary>
	/// 機体データを一括で表示に設定
	/// </summary>
	public void SetShipData(ToolBox.ShipData shipData) {
		if(shipData == null) return;
		SetPreview(shipData);
		SetName(shipData.name);
		SetHP(shipData.GetHP());
		//SetCost(shipData.GetCost());
		int total, normal, missile, optical;
		shipData.GetLauncherNum(out total, out normal, out missile, out optical);
		SetLauncher(total, normal, missile, optical);
		float output;
		shipData.GetBoosterInfo(out total, out output);
		SetBoost(total, (int)output);
		SetSize(shipData.shipSize);
	}
	//それぞれの設定用関数
	public void SetPreview(ToolBox.ShipData shipData) {
		if(!previewMf) return;
		Mesh m = shipData.GetConnectedMesh();
		previewMf.mesh = m;
		if(!previewCamera) return;
		Vector3 size = m.bounds.size / 2f;
		Vector3 center = FuncBox.Vector3Abs(m.bounds.center);
		previewCamera .orthographicSize = Vector3.Distance(Vector3.zero, center + size);
	}
	public void SetName(string name) {
		if(this.name) this.name.text = name;
	}
	public void SetHP(int hp) {
		if(this.hp)	this.hp.text = hp.ToString();
	}
	public void SetCost(int now, int max) {
		if(cost) cost.text = now + "/" + max;
	}
	public void SetLauncher(int total, int normal, int missile, int optical) {
		if(launcherTotal) launcherTotal.text = total.ToString();
		if(launcherNormal) launcherNormal.text = normal.ToString();
		if(launcherMissile) launcherMissile.text = missile.ToString();
		if(launcherOptical) launcherOptical.text = optical.ToString();
	}
	public void SetBoost(int total, int outPut) {
		if(boostTotal) boostTotal.text = total.ToString();
		if(boostOutput) boostOutput.text = outPut.ToString();
	}
	public void SetSize(string size) {
		if(this.size) this.size.text = size;
	}
#endregion
}