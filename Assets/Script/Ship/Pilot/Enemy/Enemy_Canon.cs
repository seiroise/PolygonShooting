using UnityEngine;
using System.Collections;
/// <summary>
/// ランダム移動の砲台風
/// </summary>
public class Enemy_Canon : Enemy {
	private int subUpdateCount = 0;
#region 関数
	protected override void Move() {
		if(lockObject) {
			//locktargetへの向きベクトルを取得。
			Vector2 direction = (lockObject.transform.position - transform.position).normalized;
			//directionの方向を向く
			ship.MoveAngle(direction);
		}
	}
#endregion
#region サブアップデート
	protected override void SubUpdate() {
		base.SubUpdate();
		subUpdateCount++;
		//数回に一度行う
		if(Random.Range(2, 5) == subUpdateCount) {
			//ランダムな方向を向いてクイックブースト
			Vector3 randDir = FuncBox.GetRandomVector2(-1, 1);
			ship.MoveAngle(randDir);
			ship.QuickBoost();
			//カウントを0に
			subUpdateCount = 0;
		}		
	}
#endregion
}