using UnityEngine;
using System.Collections;

//UIパーツのcolorを指定時間ごとに変化させる
public class UIPistonColor : MonoBehaviour {

	public Color[] colorList;		//色配列
	protected int index = 0;		//現在のインデックス
	protected Color nextColor;	//次の色

	public float interval;			//線形間隔
	protected float measureTime;	//計測時間
	public UIWidget uiWidget;	//UI

	public bool flagPlay;		//再生フラグ

#region MonoBehaviourイベン
	protected void Start() {
		SetNextColor();
		measureTime = 0f;
	}
	protected void Update() {
		if(!flagPlay) return;
		if(measureTime >= interval) {
			SetNextColor();
			measureTime = 0f;
		}
		uiWidget.color = Color.Lerp(uiWidget.color, nextColor, measureTime * Time.deltaTime);

		measureTime += Time.deltaTime;
	}
#endregion

#region 関数
	//次の色を設定
	protected void SetNextColor() {
		nextColor = colorList[index];
		//インデックスを進める
		index ++;
		//範囲確認
		int l = colorList.Length;
		if(index >= l) index = 0;		
	}
#endregion
}