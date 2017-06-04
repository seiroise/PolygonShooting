using UnityEngine;
using System.Collections;

//画面遷移用のフェードアウト/イン
public class FadeManager : SingletonMonoBehaviour<FadeManager> {
	
	[Header("基本要素")]
	public float fadeSpeed = 1f;			//フェード速度
	public Texture2D blackTexture = null;		//テクスチャ
	protected float fadeAlpha = 0f;			//フェード中の透明度
	protected bool isFading = false;			//フェードしているか

#region MonoBehaviour

	protected override void Awake() {
		base.Awake();
		DontDestroyOnLoad(gameObject);
		//黒いテクスチャを作る
		blackTexture = CreateBlackTexture();
	}

	protected void OnGUI() {
		if (!this.isFading) return;

		//透明度を更新して黒テクスチャを描画
		GUI.color = new Color(0, 0, 0, fadeAlpha);
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTexture);
	}

#endregion

#region 関数

	protected Texture2D CreateBlackTexture() {
		Texture2D texture;

		texture = new Texture2D(32, 32, TextureFormat.RGB24, false);
		texture.Apply();

		return texture;
	}

	//シーン遷移
	public void LoadLevel(string sceneName) {
		if (!isFading) {
			StartCoroutine(FadeCoroutine(sceneName, fadeSpeed));
		}
	}

	//遷移用コルーチン
	protected IEnumerator FadeCoroutine(string sceneName, float fadeSpeed) {
		isFading = true;
		float fadeTime = 0f;

		//フェードアウト(徐々にアルファを1に)
		while (fadeTime <= fadeSpeed) {
			fadeTime += Time.deltaTime;
			//透明度の計算
			fadeAlpha = fadeTime / fadeSpeed;
			yield return 0;
		}
		//アルファを1に
		fadeAlpha = 1f;
		yield return 0;

		//シーンの切り替え
		Application.LoadLevel(sceneName);

		//フェードイン(徐々にアルファを0に)
		fadeTime = fadeSpeed;
		while (fadeTime >= 0) {
			fadeTime -= Time.deltaTime;
			//透明度の計算
			fadeAlpha = fadeTime / fadeSpeed;
			yield return 0;
		}
		//アルファを0に
		fadeAlpha = 0f;
		yield return 0;

		isFading = false;
	}

#endregion
}