using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// ターゲットを中心に映すカメラ(平面)
/// <para>その他に映したいオブジェクトを追加可能</para>
/// </summary>
public class TargetCenterCamera : MonoBehaviour {
	[Header("カメラ")]
	public Camera camera;	//対象カメラ
	[Header("被写体")]
	public GameObject mainTarget;
	public List<GameObject> otherTargets;
	[Header("パラメータ")]
	public float minSize = 10f;		//最小サイズ
	public float spacePer = 1.05f;		//余白割合
	[Header("補完値")]
	public float aimingLerp = 10f;
	public float sizeLerp = 5f;
	//その他
	public float targetSize;
#region MonoBehaviourイベント
	protected void Start() {
		if(!camera) {
			camera = GetComponent<Camera>();
			if(!camera) {
				camera = gameObject.AddComponent<Camera>();
			}
		}
		camera.orthographic = true;
		targetSize = minSize;
	}
	//Updateだとガクガクする。Updateが呼ばれる順番はオブジェクトごとに一定ではないため。
	protected void FixedUpdate() {
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
		if(mainTarget == null) return;
		//カメラ位置
		Vector3 myPos = mainTarget.transform.position;
		Vector3 cameraPos = new Vector3(myPos.x, myPos.y, camera.transform.position.z);
		camera.transform.position = FuncBox.Vector3Lerp(camera.transform.position, cameraPos, aimingLerp * Time.deltaTime);
		if(otherTargets.Count <= 0) {
			targetSize = minSize;
			return;
		}
		//メインから一番遠いターゲットを求める
		float dis, maxDis = 0f;
		int index = 0;
		Vector2 pos;
		for(int i = 0; i < otherTargets.Count; i++) {
			pos = otherTargets[i].transform.position;
			dis = Vector2.Distance(myPos, pos);
			if(maxDis < dis) {
				maxDis = dis;
				index = i;
			}
		}
		//カメラサイズ
		Vector2 size = otherTargets[index].transform.position - myPos;
		size = FuncBox.Vector3Abs(size);
		targetSize = (size.x < size.y ? size.y :size.x);
		//最小サイズ確認
		targetSize = (targetSize < minSize ? minSize : targetSize) * spacePer;
	}
	/// <summary>
	/// サイズ補間(急激な接近の場合補完しないと酔う
	/// </summary>
	protected void LerpSize() {
		if(targetSize < camera.orthographicSize) {
			camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, targetSize, sizeLerp * Time.deltaTime);
		} else {
			camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, targetSize, sizeLerp * Time.deltaTime);
		}
	}
#endregion
}