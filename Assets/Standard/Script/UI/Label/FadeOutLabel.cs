using UnityEngine;
using System.Collections;
/// <summary>
/// LabelManager用
/// </summary>
public class FadeOutLabel : MonoBehaviour {
	[Header("Label")]
	public UILabel label;
	private bool flagFade = false;	//実行中
	private string text;			//文字列
	[Header("物理風挙動")]
	public bool flagPhysics = true;
	public float yForce = 40f;	//上向きの力
	public float xMove = 20f;	//x軸移動
	public float gravity = 98.1f;	//重力
#region MonoBehaviourイベント
	private void Awake() {
		if(!label) {
			label = GetComponent<UILabel>();
			if(!label) {
				label = gameObject.AddComponent<UILabel>();
			}
		}
	}
#endregion
#region 関数
	/// <summary>
	/// テキストを設定してフェードアウト開始(色を設定)
	/// </summary>
	public void StartFadeLabel(string text, Color c, float fadeTime = 1f) {
		if(flagFade) {
			StopCoroutine("FadeOutCoroutine");
		}
		//有効にする
		gameObject.SetActive(true);
		//テキスト、色を設定
		label.text = text;
		label.color = c;
		StartCoroutine("FadeOutCoroutine", fadeTime);
	}
	/// <summary>
	/// テキストを設定してフェードアウト開始
	/// </summary>
	public void StartFadeLabel(string text, float fadeTime) {
		StartFadeLabel(text, label.color, fadeTime);
	}
	/// <summary>
	/// 物理挙動の設定
	/// </summary>
	public void SetPhysicsParameter(float yForceMin, float yForceMax, float xMoveMin, float xMoveMax) {
		//物理風挙動(テスト)
		yForce = Random.Range(yForceMin, yForceMax);
		xMove = Random.Range(xMoveMin, xMoveMax);
	}
	/// <summary>
	/// フェードアウト用コルーチン
	/// </summary>
	protected IEnumerator FadeOutCoroutine(float fadeTime) {
		if(label) {
			flagFade = true;

			float measureTime = fadeTime;
			while(true) {
				yield return 0;
				measureTime -= Time.deltaTime;
				float alpha = measureTime / fadeTime;
				//透明度設定
				label.color = FuncBox.SetColorAlpha(label.color, alpha);
				if(measureTime < 0) {
					break;
				}
				//物理風挙動
				if(flagPhysics) {
					Vector3 translate = new Vector3(xMove, yForce, 0f);
					transform.position += translate * Time.deltaTime;
					yForce -= gravity * Time.deltaTime;
				}
			}
			label.color = FuncBox.SetColorAlpha(label.color, 0f);

			flagFade = false;
			//無効にする
			gameObject.SetActive(false);
		}
	}
#endregion
}