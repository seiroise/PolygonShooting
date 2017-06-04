using UnityEngine;
using System.Collections;

//HUDバー管理クラス
public class HUDBehaviour : MonoBehaviour {

	public SpriteRenderer hudBase;		//基準バー
	public SpriteRenderer hudCurrent;		//値バー

	public GameObject hudScale;		//大きさを管理しているオブジェクト

	protected float scale;			//バーの大きさ

	public bool active;			//表示されているか

	//HUD初期化
	public void InitHUD(float scale, float y, GameObject  parent) {
		//親子関係
		transform.parent = parent.transform;
		//メインの位置決め
		transform.localPosition = new Vector3(0f, y, 0f);
		//スケール調整用オブジェクトの位置決め
		this.scale = scale * hudScale.transform.localScale.x;
		Vector3 pos = new Vector3( -(this.scale / 2), 0f, -4f);
		hudScale.transform.localPosition = pos;
		transform.eulerAngles = Vector3.zero;
		
		this.scale = scale;
		
		//サイズ反映
		if(hudCurrent) {
			hudCurrent.transform.localScale = new Vector3(scale, 1f, 0f);
		}
		if(hudBase) {
			hudBase.transform.localScale = new Vector3(scale, 1f, 0f);
		}

		active = true;
	}

	//基準値と現在値を指定してHUDを操作する
	public void OnHUD(float baseValue, float currentValue) {
		//現在の値を割り出す
		float div = currentValue / baseValue;
		div *= scale;
		//値バーの倍率に反映させる
		if(hudCurrent) {
			hudCurrent.transform.localScale = new Vector3(div, 1f, 0f);
		}
	}

	//HUDを表示(scaleを1に)
	public void SetHUDScaleOne() {
		if (hudCurrent) {
			hudCurrent.gameObject.transform.localScale = Vector3.one;
		}
		if (hudBase) {
			hudBase.gameObject.transform.localScale = Vector3.one;
		}

		active = true;
	}

	//HUDを隠す(scaleを0に)
	public void SetHUDScaleZero() {
		if(hudCurrent) {
			hudCurrent.gameObject.transform.localScale = Vector3.zero;
		}
		if (hudBase) {
			hudBase.gameObject.transform.localScale = Vector3.zero;
		}

		active = false;
	}
}