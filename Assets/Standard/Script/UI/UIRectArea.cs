using UnityEngine;
using System.Collections;
//矩形範囲を表示する
public class UIRectArea : MonoBehaviour {
	
	[Header("UI")]
	public UISprite forground;
	public UISprite background;

#region MonoBehaviourイベント
	protected void Start() {
		forground.pivot = UIWidget.Pivot.Center;
		background.pivot = UIWidget.Pivot.Center;
	}
#endregion
#region 関数
	/// <summary>
	/// 手前矩形の大きさと位置を変更
	/// </summary>
	public void SetRect(Rect forRect, Rect backRect) {
		SetRectScale(forRect, backRect);
		SetRectPos(forRect, backRect);
	}
	/// <summary>
	/// 手前矩形の大きさを設定する
	/// </summary>
	protected void SetRectScale(Rect forRect, Rect backRect) {
		//手前のスプライトの大きさ
		Vector3 forScale = Vector3.zero;
		forScale.x = forRect.width / backRect.width;
		forScale.y = forRect.height / backRect.height;
		//1より大きい場合は1に
		if(forScale.x > 1f) forScale.x = 1f;
		if(forScale.y > 1f) forScale.y = 1f;
		//大きさ
		forScale.x *= background.transform.localScale.x;
		forScale.y *= background.transform.localScale.y;
		forground.transform.localScale = forScale;
	}
	/// <summary>
	/// 手前矩形の位置を設定する
	/// </summary>
	protected void SetRectPos(Rect forRect, Rect backRect) {
		//forRectの中心座標
		Vector3 forPos = Vector3.zero;
		forPos.x = forRect.xMin + (forRect.width * 0.5f);
		forPos.y = forRect.yMin + (forRect.height * 0.5f);
		forPos.x /= backRect.width;
		forPos.y /= backRect.height;

		Transform trans = background.transform;
		forPos.x *= trans.localScale.x;
		forPos.y *= trans.localScale.y;
		//forPos.x += (trans.localPosition.x - trans.localScale.x);
		//forPos.y += (trans.localPosition.y - trans.localScale.y);
		forground.transform.localPosition = forPos;
	}
#endregion
}