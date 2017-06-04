using UnityEngine;
using System.Collections;

//捉えたら回転率が落ちるようなふるまいをする
public class LookAtMissile : AdvancedMissile {
	
	protected bool flagOverBorder = false;	//閾値を超えたか
	protected float originalRotAnglePer;	//元々の回転倍率
	protected float originalDecelerationScale;	//元々の減速倍率

#region MonoBehaviourイベント

	protected override void Start() {
		base.Start();

		//元々の値を記憶しておく
		originalDecelerationScale = decelerationScale;
		originalRotAnglePer = rotAnglePer;
	}
	
#endregion

#region 関数

	protected override void Move() {
		base.Move();

		//閾値確認
		if(!flagOverBorder && !flagWarm) {
			if(rotPer < acceleDeccleBorder) {
				OnOverBorder();
			}
		}
	}

	//閾値を超えたときに呼ばれる
	protected virtual void OnOverBorder() {
		flagOverBorder = true;
		//減速倍率をマイナスに
		decelerationScale = -accelerationScale;
		//回転倍率を下げる
		rotAnglePer *= Random.Range(0.01f, 0.5f);
	}

#endregion
}