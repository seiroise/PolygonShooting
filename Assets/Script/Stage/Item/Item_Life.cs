using UnityEngine;
using System.Collections;
/// <summary>
/// 回復アイテム
/// </summary>
public class Item_Life : StageItem {
	[Header("回復量")]
	public int cureHP = 200;
#region 衝突処理
	protected override void OnHit(Pilot p) {
		//プレイヤーか確認
		if(p.tag == "Player") {
			((Player)p).ship.CureHP(cureHP);
			Destroy(gameObject);
		}
	}
#endregion
}