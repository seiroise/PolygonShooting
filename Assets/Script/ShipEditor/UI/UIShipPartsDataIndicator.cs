using UnityEngine;
using System.Collections;
/// <summary>
/// 機体パーツデータの表示
/// </summary>
public class UIShipPartsDataIndicator : MonoBehaviour {
	[Header("Preview")]
	public MeshFilter previewMesh;
	public Camera previewCamera;
	[Header("UIParts")]
	public UILabel launcher;
	public UILabel booster;
	public UILabel width;
	public UILabel height;
#region 関数
	/// <summary>
	/// 機体パーツデータを受け取り表示を設定
	/// </summary>
	public void Set(ToolBox.ShipPartsData shipPartsData) {
		//基本パラメータ
		if(launcher) {
			launcher.text = shipPartsData.launcher.Count.ToString();
		}
		if(booster) {
			booster.text = shipPartsData.booster.Count.ToString();
		}
		//プレビュー
		Mesh mesh = shipPartsData.figureData.GetMesh();
		if(previewMesh) {
			previewMesh.mesh = mesh;
		}
		//最小矩形範囲
		Bounds b = mesh.bounds;
		if(width) {
			width.text = b.size.x.ToString();
		}
		if(height) {
			height.text = b.size.y.ToString();
		}
		//プレビューカメラ
		if(previewCamera) {
			//表示
			previewCamera.enabled = true;
			//カメラ位置
			Vector3 pos = b.center;
			pos.z = previewCamera.transform.position.z;
			previewCamera.transform.localPosition = pos;
			//カメラサイズ
			previewCamera.orthographicSize = (b.size.x > b.size.y ? b.size.x : b.size.y) * 0.5f;
		}
	}
	/// <summary>
	/// カメラを非表示に
	/// </summary>
	public void HideCamera() {
		if(previewCamera.enabled) {
			previewCamera.enabled = false;
		}
	}
#endregion
}