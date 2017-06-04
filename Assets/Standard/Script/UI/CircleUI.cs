using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

//円形のUIを作成する
public class CircleUI : MonoBehaviour {
	
	[Header("プレハブ")]
	public CircleUIContents prefab;				//プレハブ
	protected List<CircleUIContents> contentsList;	//リスト

	[Serializable]
	public class CircleValue {
		public float per;
		public string text;
	}
	[Header("円の使用割合")]
	public List<CircleValue> circleValue;	//使用割合
	protected List<float> normalizedPer;	//使用割合を0~1の範囲に変換したもの
	[Header("要素毎の間隔")]
	public float vecScale;

#region MonoBehaviourイベント

	protected void Awake() {
		//生成
		InstantiateCircle();
	}

#endregion

#region 関数

	//サークルUIの生成
	public void InstantiateCircle() {
		//割合計算と生成
		contentsList = new List<CircleUIContents>();;
		float t = 0f;
		GameObject s;
		foreach (CircleValue p in circleValue) {
			//生成
			s = (GameObject)Instantiate(prefab.gameObject);
			s.transform.parent = transform;
			s.transform.localPosition = Vector3.zero;
			s.transform.localScale = Vector3.one;
			contentsList.Add(s.GetComponent<CircleUIContents>());

			//割合の母数を計算
			t += p.per;
		}

		//正規化
		float per = 0f;	
		float addPer = 0f;
		float angle;
		float direction;
		Vector3 vec;
		normalizedPer = new List<float>();
		for(int i = 0; i < circleValue.Count; i++) {
			//正規
			per = circleValue[i].per / t;

			//角度
			angle = 360 * addPer;
			addPer += per;
			normalizedPer.Add(addPer);	//正規化リストに追加

			//ずらす角度
			direction = angle + (per * 360 / 2) + 90f;
			//座標をずらす
			vec = FuncBox.DegreeToVector3(direction);
			contentsList[i].transform.localPosition = vec * vecScale;

			//角度と割合
			contentsList[i].transform.eulerAngles = new Vector3(0f, 0f, angle);
			contentsList[i].SetContents(per, circleValue[i].text);

			//名前
			contentsList[i].name = "[" + i + "]";
		}
	}

	//角度と距離をもとにどこのUIに属しているかを確認する
	public string GetCircleValue(float angle, float distance) {
		//上から始まりの反時計回りなのでずらす
		angle -= 90f;
		if (angle <= 0) {
			angle += 360;
		}
		
		float normalized = angle / 360;	//角度を360で割る
		int i = 0;
		for (; i < normalizedPer.Count; i++) {
			if (normalizedPer[i] >= normalized) {
				break;
			}
		}
		return circleValue[i].text;
	}
#endregion
}