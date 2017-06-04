using UnityEngine;
using System.Collections;

//指定した矩形範囲内だけを映すカメラ
public class RectCamera : MonoBehaviour {
	
	[Header("Camera")]
	public Camera c;
	public float minCameraSize = 1f;		//最小カメラサイズ
	public float minCameraScale = 0.25f;	//最小カメラスケール
	protected float nowScaling = 0f;		//現在の倍率
	[Header("Controll")]
	public float swipeScale = 4f;			//スワイプ量の倍率
	[Header("Rect(-x, -y : w, h)")]
	public Rect rect;
	[Header("UI")]
	public UIRectArea rectArea;
	public UILabel scalingLabel;

#region MonoBehaviourイベント
	protected void Update() {
		//動作テスト_移動
		if(Input.GetKeyDown(KeyCode.RightArrow)) {
			CameraMove(1f, 0f);
		} else if(Input.GetKeyDown(KeyCode.LeftArrow)) {
			CameraMove(-1f, 0f);
		}
		if(Input.GetKeyDown(KeyCode.UpArrow)) {
			CameraMove(0f, 1f);
		} else if(Input.GetKeyDown(KeyCode.DownArrow)) {
			CameraMove(0f, -1f);
		}
		//動作テスト_スワイプ&拡大縮小
		if(Input.GetMouseButton(2)) {
			Vector2 swipe = new Vector2(Input.GetAxis("Mouse_X"), Input.GetAxis("Mouse_Y"));
			if(swipe != Vector2.zero) {
				//拡大率に応じて移動量を変える
				swipe = (swipe / nowScaling) * swipeScale;
				CameraMove(-swipe);
			}
		} else {
			float scroll = Input.GetAxis("Mouse ScrollWheel");
			CameraScaling(scroll);
		}
	}
#endregion
#region 関数
	/// <summary>
	/// カメラを移動させる。引数は移動量
	/// </summary>
	public void CameraMove(float x, float y) {
		c.transform.position += new Vector3(x, y);
		//はみ出し確認
		CheckPushOutMove();
		//範囲はみ出し確認
		CheckPushOutRect();
		//UI
		if(rectArea) {
			rectArea.SetRect(GetCameraRect(), rect);
		}
	}
	public void CameraMove(Vector2 v) {
		CameraMove(v.x, v.y);
	}
	/// <summary>
	/// カメラを拡大縮小させる。負数で縮小、正数で拡大
	/// </summary>
	public void CameraScaling(float scaling) {
		bool flag = false;
		if(scaling < 0f) {
			//拡大&はみ出し確認
			if(!CheckPushOutScaling()) {
				c.orthographicSize *= 2f;
				//範囲はみ出し確認
				CheckPushOutMove();
				CheckPushOutRect();
				flag = true;
			}
		} else if(scaling > 0f) {
			//縮小
			if(c.orthographicSize > minCameraSize) {
				c.orthographicSize *= 0.5f;
				flag = true;
			}
		}
		//更新フラグ
		if(flag) {
			//拡大率の取得
			nowScaling = GetNowScaling();
			UIUpdate();
		}
	}
	/// <summary>
	/// 移動したときの矩形範囲のはみ出し確認。
	/// </summary>
	protected void CheckPushOutMove() {
		//カメラの映している矩形範囲を求める(ワールド座標)
		Rect cameraRect = GetCameraRect();
		Vector3 cPos = c.transform.localPosition;
		//範囲からはみ出てないか確認
		//x
		if(cameraRect.xMin < rect.xMin) {
			cPos.x += (rect.xMin - cameraRect.xMin);
		} else if(cameraRect.xMax > rect.xMax) {
			cPos.x -= (cameraRect.xMax - rect.xMax);
		}
		//y
		if(cameraRect.yMin < rect.yMin) {
			cPos.y += (rect.yMin - cameraRect.yMin);
		} else if(cameraRect.yMax > rect.yMax) {
			cPos.y -= (cameraRect.yMax - rect.yMax);
		}
		c.transform.localPosition = cPos;
	}
	/// <summary>
	/// 拡大縮小したときの矩形範囲のはみ出し確認
	/// </summary>
	protected bool CheckPushOutScaling() {
		//カメラの映している矩形範囲を求める(ワールド座標)
		Rect cameraRect = GetCameraRect();
		if(cameraRect.width > rect.width) {
			if(cameraRect.height > rect.height) {
				return true;
			}
		}		
		return false;
	}
	/// <summary>
	/// 縦横の矩形範囲がはみ出してないか確認
	/// </summary>
	protected void CheckPushOutRect() {
		//カメラの映している矩形範囲を求める(ワールド座標)
		Rect cameraRect = GetCameraRect();
		Vector3 cPos = c.transform.localPosition;
		//範囲からはみ出てないか確認
		//x
		if(cameraRect.width > rect.width) {
			cPos.x = rect.xMin + (rect.width * 0.5f);
		}
		//y
		if(cameraRect.height > rect.height) {
			cPos.y = rect.yMin + (rect.height * 0.5f);
		}
		c.transform.localPosition = cPos;
	}
	/// <summary>
	/// カメラの表示している矩形範囲を返す(ワールド座標)
	/// </summary>
	protected Rect GetCameraRect() {
		Vector3 cPos = c.transform.localPosition;
		float hWidth, hHeight;		//harf(半分)
		hHeight = c.orthographicSize;
		hWidth = hHeight * c.aspect;
		return new Rect(cPos.x - hWidth, cPos.y - hHeight, hWidth * 2f, hHeight * 2f);
	}
	/// <summary>
	/// 現在の拡大率を返す
	/// </summary>
	protected float GetNowScaling() {
		return (minCameraSize / c.orthographicSize) / minCameraScale * 100f;
	}
	/// <summary>
	/// UIを更新
	/// </summary>
	protected void UIUpdate() {
		//UI更新
		scalingLabel.text = nowScaling + " %";
		if(rectArea) {
			rectArea.SetRect(GetCameraRect(), rect);
		}
	}
	/// <summary>
	/// カメラの大きさを設定
	/// </summary>
	public void SetCameraSize(float size) {
		c.orthographicSize = size;
		//拡大率の取得
		nowScaling = GetNowScaling();
		//UI
		UIUpdate();
	}
#endregion
}