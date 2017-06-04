using UnityEngine;
using System.Collections;
/// <summary>
/// ランチャーアイテム
/// </summary>
public class Item_Launcher : StageItem {
#region 衝突処理
	protected override void OnHit(Pilot p) {
		base.OnHit(p);
	}
#endregion
}