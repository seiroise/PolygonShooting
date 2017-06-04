using UnityEngine;
using System.Collections;

namespace Standard.UI.Text {
/// <summary>
/// テキストボックス表示用
/// </summary>
public class UITextBox : MonoBehaviour {
	[Header("UIParts")]
	public UILabel label;
	public UISprite background;
	[Header("Config")]
	public float spacing = 2f;
	[Header("Move")]
	public bool flagHoverMove = false;		//マウスが被ったときに
	protected bool flagMove = false;			//移動しているか
	public Vector3 hoverMovePos;			//マウスが被ったときの移動位置
	public UIWidget.Pivot hoverMovePivot ;	//マウスが被ったときのピボット
	//元の
	protected Vector3 originPos;				//元の位置
	protected UIWidget.Pivot originPivot;		//元のピボット
	//Other
	protected BoxCollider coll;		//当たり判定
	protected UIWidget.Pivot pivot;	//現在のピボット
	//Lerp
	protected Vector3 targetSize;		//目標サイズ
	protected Vector3 targetPos;		//目標座標
#region MonoBehaviourイベント
	protected void Awake() {
		coll = GetComponent<BoxCollider>();
		if(!coll) {
			coll = gameObject.AddComponent<BoxCollider>();
		}
		originPos = transform.localPosition;
		originPivot = label.pivot;
		SetPivot(originPivot);
		targetSize = Vector3.zero;
		targetPos = transform.localPosition;
	}
	protected void Update() {
		background.transform.localScale = FuncBox.Vector3Lerp(background.transform.localScale, targetSize, 0.5f);
		transform.localPosition = FuncBox.Vector3Lerp(transform.localPosition, targetPos, 0.5f);
	}
#endregion
#region 関数
	/// <summary>
	/// テキストを設定
	/// </summary>
	public void SetText(string text) {
		//表示
		Indicate(true);
		//色々設定
		label.text = text;
		Vector2 size = label.relativeSize;
		Vector2 scale = label.transform.localScale;
		size.x = size.x * scale.x + spacing * 2f;
		size.y = size.y * scale.y + spacing * 2f;
		//大きさ
		targetSize = size;
		//background.transform.localScale = size;
		//背景位置
		background.transform.localPosition = Vector3.zero;
		//テキスト位置
		Vector3 pos = Vector3.zero;
		Vector3 collCenter = Vector3.zero;
		//横
		switch(pivot) {
			case UIWidget.Pivot.Left:
			case UIWidget.Pivot.TopLeft:
			case UIWidget.Pivot.BottomLeft:
				pos.x = spacing;
				collCenter.x = size.x * 0.5f;
			break;
			case UIWidget.Pivot.Right:
			case UIWidget.Pivot.TopRight:
			case UIWidget.Pivot.BottomRight:
				pos.x = -spacing;
				collCenter.x = -(size.x * 0.5f);
			break;
		}
		//縦
		switch(pivot) {
			case UIWidget.Pivot.Top:
			case UIWidget.Pivot.TopLeft:
			case UIWidget.Pivot.TopRight:
				pos.y = -spacing;
				collCenter.y = -(size.y * 0.5f);
			break;
			case UIWidget.Pivot.Bottom:
			case UIWidget.Pivot.BottomLeft:
			case UIWidget.Pivot.BottomRight:
				pos.y = spacing;
				collCenter.y = size.y * 0.5f;
			break;
		}
		label.transform.localPosition = pos;
		//あたり判定
		coll.size = size;
		coll.center = collCenter;
	}
	/// <summary>
	/// ピボット設定
	/// </summary>
	public void SetPivot(UIWidget.Pivot pivot) {
		this.pivot = pivot;
		background.pivot = pivot;
		label.pivot = pivot;
		SetText(label.text);
	}
	/// <summary>
	/// 表示/非表示
	/// </summary>
	public void Indicate(bool flag) {
		gameObject.SetActive(flag);
		label.enabled = flag;
		background.enabled = flag;
	}
	/// <summary>
	/// 元の位置へ
	/// </summary>
	public void SetOriginePos() {
		//transform.localPosition = originPos;
		targetPos = originPos;
		SetPivot(originPivot);
		flagMove = false;
	}
	/// <summary>
	/// 移動先の位置へ
	/// </summary>
	public void SetMovePos() {
		//transform.localPosition = hoverMovePos;
		targetPos = hoverMovePos;
		SetPivot(hoverMovePivot);
		flagMove = true;
	}
#endregion
#region UIイベント
	public void OnHover(bool value) {
		if(value && flagHoverMove) {
			if(flagMove) {
				SetOriginePos();
			} else {
				SetMovePos();
			}
		}
	}
#endregion
}
}