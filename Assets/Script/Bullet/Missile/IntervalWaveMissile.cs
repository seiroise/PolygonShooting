using UnityEngine;
using System.Collections;

public class IntervalWaveMissile : LookAtMissile {	
	
	public float interval;		//間隔

#region 関数

	protected override void OnOverBorder() {
		base.OnOverBorder();
		//コルーチン開始
		StartCoroutine("Coroutine");
	}

#endregion

#region コルーチン

	protected IEnumerator Coroutine() {
		yield return new WaitForSeconds(interval);

		//値を戻す
		flagOverBorder = false;
		rotAnglePer = originalRotAnglePer;
		decelerationScale = originalDecelerationScale;
	}

#endregion
}