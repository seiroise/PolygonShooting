using UnityEngine;
using System.Collections;
/// <summary>
/// 優柔不断
/// </summary>
public class Enemy_Indecision : Enemy {
	protected override void SubUpdate() {
		ship.QuickBoost();
		//ロック対象をランダムに探す
		lockObject = sm.GetRandomPlayer(this);
		Attack();
	}
}