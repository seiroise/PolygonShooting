using UnityEngine;
using System.Collections;

//文字表示用の小さなウィンドウ(Indicator)の管理
public class UIIndicator : MonoBehaviour {
	
	[Header("UIパーツ")]
	public GameObject Indicator;
	public UILabel label;

	[Header("その他")]
	public Vector3 defaultPos;
	public float lerpT;
	public float returnPosTime;		//デフォルトの座標に戻るまでの時間
	public float time = 0f;

	//その他
	protected Vector3 targetPos;

#region MonoBehaviourイベント
	public void Start() {
		targetPos = Indicator.transform.position = defaultPos;
	}

	public void Update() {
		//Indicatorを動かす
		Vector3 pos = Indicator.transform.position;
		pos = FuncBox.Vector3Lerp(pos, targetPos, lerpT * Time.deltaTime);
		Indicator.transform.position = pos;

		//時間の計測
		if(time <= returnPosTime) {
			time += Time.deltaTime;
			if(time > returnPosTime) {
				targetPos = defaultPos;
			}
		}
	}

#endregion

#region 関数

	public void SetIndicator(string text, Vector3 pos) {
		targetPos = pos;
		label.text = text;
		time = 0f;
	}

	public void SetDefault() {
		targetPos = defaultPos;
	}

#endregion
}