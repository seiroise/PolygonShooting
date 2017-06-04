using UnityEngine;
using System.Collections;

//指定時間後に弾(主にミサイル)を生成する弾
public class ContainerBullet : Bullet {
	[Header("生成関連(LifeTimeは使用しない)")]
	public AdvancedMissile instantMissile;	//生成ミサイル
	public int instantNum;				//生成数
	protected int nowInstantNum = 0;		//現在の生成数
	public float instantTime;				//生成開始時間
	public float instantInterval;			//生成間隔
	public enum InstantMode {
		Vertical_Right,		//右側面から垂直に
		Vertical_Left,		//左側面から垂直に
	}
	public InstantMode instantMode;		//生成モード

#region 関数
	protected override void MeasureTime() {
		//基底は呼ばない
		measureLifeTime += Time.deltaTime;
		
		//時間が経過したら
		while(measureLifeTime >= instantTime + (instantInterval * nowInstantNum)) {
			//生成
			Bullet b = InstantiateBullet(instantMissile.gameObject);
			//とりえず向きは交互に
			if(nowInstantNum % 2 == 1) {
				b.transform.eulerAngles += new Vector3(0f, 0f, 90f);
			} else {
				b.transform.eulerAngles += new Vector3(0f, 0f, -90f);
			}
			//インクリ
			nowInstantNum++;
		}
		if(instantNum <= nowInstantNum) {
			//削除
			OnDestroyer();
		}
	}
#endregion
}