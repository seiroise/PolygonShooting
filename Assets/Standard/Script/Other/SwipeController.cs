using UnityEngine;
using System.Collections;

//スワイプで何かを移動させる
public class SwipeController : MonoBehaviour {

	[Header("操作関連")]
	public bool flagControl = false;		//操作させるか
	public bool flagCameraMove = true;	//カメラを移動させるか
	[HideInInspector]
	public bool flagSwipe;				//スワイプしているか

	[Header("対象カメラとオブジェクト")]
	public Camera targetCamera;			//対象カメラ
	public GameObject target;				//対象
	public Rect area;					//対象の移動範囲

	[Header("表示スプライト")]
	public UISprite sprite;			//スプライト
	protected bool reverse = false;	//反転表示
	protected Vector3 distance;	//距離
	protected float angle;			//角度

	[Header("出力値")]
	public Vector3 startPos;			//マウス移動の開始点
	public Vector3 prevPos;			//マウスの前回の点
	public Vector3 offset;				//前回座標と今回の座標の差
	public float amount;				//慣性

	[Header("円形UIとの連動")]
	public CircleUI circleUI;
	public float activeDistance;					//有効になる距離
	public GameObject circleUIIventHandler;		//イベント受け手
	public string circleUIIventName = "OnCircleUI";	//イベント名

#region MonoBehaviourイベント
	
	protected void Update() {
		if (flagControl) {
			Swipe();
		}
	}

#endregion

#region 関数
	
	//スワイプ操作
	protected void Swipe(int input = 0) {
		if (Input.GetMouseButton(input)) {
			if (flagSwipe) {
				//マウスがどれだけ移動しているかを取得する
				prevPos = InverseMousePoint();
				offset = startPos - prevPos;

				//カメラの移動
				if (flagCameraMove) {
					Vector3 cPos = targetCamera.transform.position;
					bool isOut;
					Vector3 p = FuncBox.MoveArea(cPos, offset, area, out isOut);
					targetCamera.transform.position = p;
					if (isOut) {
						offset = Vector3.zero;
					}
				}

				//スプライト
				if (sprite) {
					//長さを求める
					distance = sprite.transform.localScale;
					distance.x = Vector3.Distance(prevPos, startPos);
					sprite.transform.localScale = distance;
					//角度を求める
					angle = FuncBox.TwoPointAngleD(startPos, prevPos);
					sprite.transform.eulerAngles = new Vector3(0f, 0f, angle);
				}
			} else {
				//マウスの位置を記憶しておく
				startPos = InverseMousePoint();
				flagSwipe = true;

				//スプライト
				if (sprite) {
					//sprite.transform.localPosition = startPos;
					sprite.gameObject.SetActive(true);
				}
			}
		} else {
			//直前までスワイプしていた場合
			if (flagSwipe) {
				Debug.Log("スワイプ終了");
				if (sprite) {
					sprite.gameObject.SetActive(false);
				}
				//円形UIとの連携
				if (circleUI) {
					string str = circleUI.GetCircleValue(angle, distance.x);
					if(activeDistance <= distance.x) {
						if (circleUIIventHandler) {
							circleUIIventHandler.SendMessage(circleUIIventName + str, angle);
						}
					}
				}
			}
			flagSwipe = false;
		}
	}

	//マウス座標をローカル座標に変換して取得
	protected Vector3 InverseMousePoint() {
		return transform.InverseTransformPoint(FuncBox.GetMousePoint(targetCamera));
	}

	//カメラの移動(慣性付き) {
	protected void CameraMove(Vector3 offset) {
		if(offset != Vector3.zero) {
			//慣性を働かせる
			offset /= amount;
			Vector3 cPos = targetCamera.transform.position;
			bool isOut;
			Vector3 p = FuncBox.MoveArea(cPos, offset, area, out isOut);
			targetCamera.transform.position = p;
			//エリアからはみ出ているので移動量は0に
			if (isOut) {
				offset = Vector3.zero;
			}
		}		
	}
#endregion
}