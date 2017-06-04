using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// ターゲット(複数)を全て映すカメラ(平面)
/// <para>ターゲットの移動や追加で自動的にサイズを調整する</para>
/// </summary>
public class TargetAreaCamera : MonoBehaviour {
	protected Camera camera;		//対象カメラ
	[Header("ターゲットリスト")]
	public List<GameObject> targets;	//ターゲットリスト
	[Header("パラメータ")]
	public float minSize = 10f;			//最小サイズ
	public float defaultSize = 80f;		//デフォルトのカメラサイズ
	public float spacePar = 1.05f;		//余白割合(1なら余白なし,1以上で余白有)
	public float sizeLerpPar = 10f;		//サイズの線形補間率
	//その他
	[HideInInspector]
	public float targetSize;
#region MonoBehaviourイベント
	private void Start() {
		if(!camera) {
			camera = GetComponent<Camera>();
			if(!camera) {
				camera = gameObject.AddComponent<Camera>();
			}
		}
		camera.orthographic = true;
	}
	//Updateだとガクガクする。Updateが呼ばれる順番はオブジェクトごとに一定ではないため。
	private void FixedUpdate() {
		//ターゲッティング
		Targeting();
		//サイズ補間
		LerpSize();
	}
#endregion
#region 関数
	/// <summary>
	/// ターゲッティング
	/// </summary>
	protected void Targeting() {
		if(targets.Count <= 0) {
			return;
		}

		//ターゲットの作るバウンディングボックスを求める
		Vector3 min, max, pos;
		min = max = pos = targets[0].transform.position;

		for(int i = 1; i < targets.Count; i++) {
			pos = targets[i].transform.position;
			//x
			if(min.x > pos.x) {
				min.x = pos.x;
			} else if(max.x < pos.x) {
				max.x = pos.x;
			}
			//y
			if(min.y > pos.y) {
				min.y = pos.y;
			} else if(max.y < pos.y) {
				max.y = pos.y;
			}
		}
		
		//カメラ位置
		Vector3 cameraPos;
		float width = 0f, height = 0f;
		if(targets.Count == 1) {
			cameraPos = pos;
			width = height = defaultSize;
		} else {
			width = (max.x - min.x) * 0.5f;
			height =(max.y - min.y) * 0.5f;
			cameraPos = new Vector3(min.x + width, min.y + height);
		}		
		cameraPos.z = camera.transform.position.z;
		camera.transform.position = FuncBox.Vector3Lerp(camera.transform.position, cameraPos, 5 * Time.deltaTime);
		
		//カメラサイズ
		targetSize = (width < height ? height : width) * spacePar;
		if(targetSize < minSize) targetSize = minSize;
	}
	/// <summary>
	/// サイズ補間(急激な接近の場合補完しないと酔う
	/// </summary>
	protected void LerpSize() {
		if(targets.Count > 0) {
			if(targetSize < camera.orthographicSize) {
				camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, targetSize, sizeLerpPar * Time.deltaTime);
			} else {
				camera.orthographicSize = Mathf.Lerp(targetSize, camera.orthographicSize, sizeLerpPar * Time.deltaTime);
			}
		}
	}
#endregion
}