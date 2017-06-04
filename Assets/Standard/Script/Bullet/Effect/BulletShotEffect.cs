using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//弾を撃つときのエフェクト
public class BulletShotEffect : MonoBehaviour {
	
	//エフェクト情報内部クラス
	[Serializable]
	public class EffectInfo {
		[Header("タイムライン")]
		public float timeLine;

		[Header("大きさ")]
		public float scaleMin;
		public float scaleMax;

		[Header("距離")]
		public float rangeMin;
		public float rangeMax;
	}

	[Header("パラメータ")]
	public bool flagEndDestroy = false;	//終了時に削除
	public GameObject startEffect;		//開始用エフェクト
	public GameObject effect;			//生成エフェクト
	public List<EffectInfo> effectTimeLine;	//エフェクト生成のタイムライン

	protected float measureTime;		//計測時間
	protected int timeLineIndex = 0;	//タイムラインインデックス
	protected Vector3 direction;		//向いている方向
	protected float effectRange = 0f;	//エフェクトの生成距離
	protected bool flagStart = false;	//開始

#region MonoBehaviourイベント
	protected virtual void Start() {
		measureTime = 0f;
		direction = transform.right;	//向いてる方向
	}

	protected virtual void Update() {
		if (flagStart) {
			measureTime += Time.deltaTime;

			//タイムラインの最後まで来たら削除する
			if(effectTimeLine.Count <= timeLineIndex) {
				//削除
				if (flagEndDestroy) {
					Destroy(gameObject);
				} else {
					Destroy(this);
				}
			} else if (effectTimeLine[timeLineIndex].timeLine < measureTime) {			
				//生成
				var g = (GameObject)Instantiate(effect);
				g.transform.parent = transform;
				//大きさ
				var s = UnityEngine.Random.Range(effectTimeLine[timeLineIndex].scaleMin, effectTimeLine[timeLineIndex].scaleMax);
				g.transform.localScale = new Vector3(s, s, s);
				//位置(距離)
				effectRange += UnityEngine.Random.Range(effectTimeLine[timeLineIndex].rangeMin, effectTimeLine[timeLineIndex].rangeMax);
				g.transform.position = transform.position + (direction * effectRange);
				//角度
				g.transform.eulerAngles = transform.eulerAngles;

				//インデックスを進める
				timeLineIndex++;
			}
		}
	}
#endregion

#region 関数
	public void StartEffect() {
		flagStart = true;

		//開始用エフェクトには生成
		if (startEffect) {
			//生成
			var g = (GameObject)Instantiate(startEffect);
			g.transform.parent = transform;
			g.transform.localPosition = Vector3.zero;
			g.transform.eulerAngles = transform.eulerAngles;

		}
	}
#endregion
}