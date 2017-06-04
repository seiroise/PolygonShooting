using UnityEngine;
using System.Collections;
/// <summary>
/// ステージ上に表示するアイテム
/// </summary>
public class StageItem : MonoBehaviour {
	[Header("見た目")]
	public UISprite sprite;		//UISpriteで見た目を表現
	[Header("表示時間")]
	public float time;
	private float measureTime;
	[Header("アイテム名")]
	public string itemName;
#region MonoBehaviourイベント {
	protected void Start() {
		if(!sprite) {
			sprite = GetComponent<UISprite>();
		}
		measureTime = time;
	}
	protected void Update() {
		if(measureTime > 0) {
			measureTime -= Time.deltaTime;
			//徐々に透明に
			sprite.color = FuncBox.SetColorAlpha(sprite.color, measureTime / time);
			if(measureTime <= 0) {
				Destroy(gameObject);
				return;
			}
		}
	}
	protected void OnTriggerEnter(Collider c) {
		Pilot p = c.gameObject.GetComponent<Pilot>();
		if(p == null) return;
		//衝突処理に任せる
		OnHit(p);
	}
#endregion
#region 衝突処理
	/// <summary>
	/// 衝突時の処理
	/// </summary>
	protected virtual void OnHit(Pilot p) {
		//プレイヤーか確認
		if(p.tag == "Player") {
			((Player)p).GetItem(sprite, itemName);
			Destroy(gameObject);
		}
	}
#endregion
}