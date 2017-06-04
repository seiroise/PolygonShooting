using UnityEngine;
using System.Collections;

//RGBの値を表示
public class UIRGBIndicator : MonoBehaviour {

	[Header("UIパーツ")]
	public UIRGBSlider rgbSlider;
	public UIRGBInput rgbInput;
	public UISprite dSprite, sSprite;
	public UIButtonMessage okButton;

	[Header("カラープリセット")]
	public GameObject[] colorPresetButton;		//ボタン
	public Color[] colorPreset;					//色
	protected UISprite[] presetSprite;			//表示用のスプライト

	[Header("イベント")]
	public GameObject target;
	public string okFunctionName = "OnPopupOK";
	public string changeColorFunctionName = "OnColorChange";
	public string changePresetFunctionName = "OnPresetChange";

	protected Color dColor;						//元の色
	protected Color indicateColor = Color.white;		//表示色

	//ポップアップは有効になった時にAwakeが走るので、防止用に
	protected bool flagColorPresetYet = false;			//既にカラープリセットが設定されているか

#region MonoBehaviourイベント

	protected void Awake() {
		Init();
	}

#endregion

#region 関数
	
	//初期化
	public void Init() {
		rgbSlider.SetIventHandler(gameObject, "OnChangeValue");
		rgbInput.SetIventHandler(gameObject, "OnSubmit");

		okButton.target = target;
		okButton.functionName = okFunctionName;

		//プリセット設定
		if(!flagColorPresetYet) {
			colorPreset = new Color[colorPresetButton.Length];
			for(int i = 0; i < colorPreset.Length; i++) {
				colorPreset[i] = Color.white;
			}
		}
		presetSprite = new UISprite[colorPresetButton.Length];

		for (int i = 0; i < colorPresetButton.Length; i++) {
			colorPresetButton[i].name = i.ToString();

			//メッセージの設定
			var bm = colorPresetButton[i].GetComponent<UIButtonMessage>();
			if (bm) {
				bm.target = gameObject;
				bm.functionName = "OnPresetClicked";
			}

			//スプライトの設定
			var sprite = colorPresetButton[i].GetComponentInChildren<UISprite>();
			if (sprite) {
				presetSprite[i] = sprite;
			}

			//色設定
			presetSprite[i].color = colorPreset[i];
		}
	}

	//色を設定
	public void SetColor(Color color) {
		dSprite.color = indicateColor = dColor = color;

		ChangeColor();
	}
	public void SetColor(Color color, Color[] presetColor) {
		for (int i = 0; i < presetSprite.Length; i++) {
			if (presetColor.Length > i) {
				//要素がある場合はそのまま色を移す
				presetSprite[i].color = this.colorPreset[i] = presetColor[i];
			} else {
				//デフォルトでは白
				presetSprite[i].color = this.colorPreset[i] = Color.white;
			}
		}

		SetColor(color);
	}

	//カラープリセットを設定
	public void SetColorPreset(Color[] array) {
		colorPreset = array;
		//カラープリセットを登録したことをフラグとして残しておく
		flagColorPresetYet = true;
	}

	//色変更
	protected void ChangeColor() {
		//スプライトの色変更
		sSprite.color = indicateColor;
		//入力数値変更
		rgbInput.SetColor(indicateColor);
		//スラいだー変更
		rgbSlider.SetColor(indicateColor);
	}

	//イベント送信
	protected void SendChangeColor() {
		if (target) {
			target.SendMessage(changeColorFunctionName, indicateColor);
		}
	}
	protected void SendChangePreset() {
		if (target) {
			target.SendMessage(changePresetFunctionName, colorPreset);
		}
	}

#endregion

#region UIイベント
	
	//スライダー
	protected void OnChangeValueR(float v) {
		indicateColor.r = v;
		ChangeColor();
		SendChangeColor();
	}
	protected void OnChangeValueG(float v) {
		indicateColor.g = v;
		ChangeColor();
		SendChangeColor();
	}
	protected void OnChangeValueB(float v) {
		indicateColor.b = v;
		ChangeColor();
		SendChangeColor();
	}

	//入力
	protected void OnSubmitR(string v) {
		//入力が正しいか確認する
		int n;
		if (int.TryParse(v, out n)) {
			indicateColor.r = n / 255f;
			ChangeColor();
			SendChangeColor();
		}
	}
	protected void OnSubmitG(string v) {
		//入力が正しいか確認する
		int n;
		if (int.TryParse(v, out n)) {
			indicateColor.g = n / 255f;
			ChangeColor();
			SendChangeColor();
		}
	}
	protected void OnSubmitB(string v) {
		//入力が正しいか確認する
		int n;
		if (int.TryParse(v, out n)) {
			indicateColor.b = n / 255f;
			ChangeColor();
			SendChangeColor();
		}
	}

	//元の色に戻す
	protected void OnChangeDColor() {
		indicateColor = dColor;
		ChangeColor();
		SendChangeColor();
	}

	//プリセットの色に変更
	protected void OnPresetClicked(GameObject g) {
		int index;
		if (int.TryParse(g.name, out index)) {
			//Ctrlキーを押しているかで処理を変える
			if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
				//押している場合はプリセットの色を表示している色に変更する
				colorPreset[index] = indicateColor;
				presetSprite[index].color = indicateColor;
			} else {
				//色を取得
				indicateColor = colorPreset[index];
				//表示を変更
				ChangeColor();
			}
		}			
	}

#endregion
}