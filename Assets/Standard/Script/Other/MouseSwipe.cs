using UnityEngine;
using System.Collections;

//マウスでのスワイプ操作に関する基本的な座標情報と変換関数を提供する
public class MouseSwipe : MonoBehaviour {

	//入力方法
	public enum InputInterface {
		Mouse,
		Touch
	}

	[Header("操作関連")]
	public InputInterface inputInterface = InputInterface.Mouse;
	public int mouseButtonID = 0;
	public bool flagActive = true;				//情報を取得するか
	public bool flagSwipe = false;				//スワイプ中か
	public bool flagMomentumActive = false;	//慣性が働いているか

	[Header("座標関連")]
	public Vector3 startPosition;		//始点
	public Vector3 prevPosition;		//一つ前の点
	public Vector3 currentPosition;		//現在の座標
	public Vector3 offset;				//前回点とのオフセット

	[Header("慣性")]
	public bool flagMomentum;		//慣性を働かせるか
	public float momentum = 10;		//慣性の強さ

#region MonoBehaviourイベント

	protected void Update() {
		//スワイプ管理を行う
		SwipeManage();
	}

#endregion

#region スワイプ管理関数
	
	//スワイプ状態管理
	protected void SwipeManage() {
		if (flagActive) {
			//スワイプ処理
			if (flagSwipe) {
				OnSwipeMove();
			} else if (flagMomentum) {
				OnMomentum();
			}

			//入力処理
			InputManage();		
		}
	}

	//入力処理
	protected void InputManage() {
		switch (inputInterface) {
			case InputInterface.Mouse:
				//マウス
				if (Input.GetMouseButtonDown(mouseButtonID)) {
					//ダウン//スワイプ開始
					OnSwipeSart();
				} else if (Input.GetMouseButtonUp(mouseButtonID)) {
					//アップ//スワイプ終了
					OnSwipeEnd();
				}
				break;	
			case InputInterface.Touch:
				//タッチ
				if (Input.touchCount > 0) {
					Touch touch;
					touch = Input.GetTouch(0);
					//入力検出
					if (touch.phase == TouchPhase.Began) {
						//ダウン//スワイプ開始
						OnSwipeSart();
					} else if (touch.phase == TouchPhase.Ended) {
						//アップ//スワイプ終了
						OnSwipeEnd();
					}
				}
				break;
			default:
				break;
		}

	}

	//スワイプ開始
	protected void OnSwipeSart() {
		startPosition = Input.mousePosition;
		prevPosition = startPosition;
		flagSwipe = true;
	}

	//スワイプ移動
	protected void OnSwipeMove() {
		currentPosition = Input.mousePosition;
		//一つ前の点とのオフセットを測る
		offset = currentPosition - prevPosition;

		//現在の座標を一つ前の座標に設定
		prevPosition = currentPosition;
	}

	//スワイプ終了
	protected void OnSwipeEnd() {
		flagSwipe = false;
		//慣性を働かせない場合はオフセットを0にしておく
		if (!flagMomentum) {
			offset = Vector3.zero;
		}
	}

	//慣性処理
	protected void OnMomentum() {
		if(offset != Vector3.zero) {
			offset = Vector3.Lerp(offset, Vector3.zero, momentum * Time.deltaTime);
			//オフセットの残り値が小さい場合は0とする
			if (Mathf.Abs((offset.x + offset.y + offset.z) / 3f) <= 0.1f) {
				offset = Vector3.zero;
			}
		}
	}

#endregion

#region その他関数
	
	//offsetを指定カメラの見ている座標系に変換する
	public Vector3 TransformTargetCamera(Camera targetCamera) {
		Vector3 pos = offset;
		pos += new Vector3(Screen.width * 0.5f, Screen.height *0.5f);
		pos.z = -targetCamera.transform.position.z;
		pos = targetCamera.ScreenToWorldPoint(pos);
		return pos;
	}

	public Vector3 GetNormalizedOffset() {
		return new Vector3(offset.x / Screen.width, offset.y / Screen.height);
	}

	//始点からの距離を求める(スワイプ中のみ)
	public float GetDistance() {
		if (flagSwipe) {
			return Vector3.Distance(startPosition, currentPosition);
		} else {
			return -1f;
		}
	}

	//始点からの角度を求める(スワイプ中のみ)
	public float GetAngle() {
		if (flagSwipe) {
			return FuncBox.TwoPointAngleD(currentPosition, startPosition);
		} else {
			return -1f;
		}
	}

#endregion
}