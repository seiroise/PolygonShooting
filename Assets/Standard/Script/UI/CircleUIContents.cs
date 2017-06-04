using UnityEngine;
using System.Collections;

//円形UIのコンテンツ用
public class CircleUIContents : MonoBehaviour {
	
	public UISprite sprite;
	public UILabel label;
	public ButtonMessageManager messageManager;

	//設定
	public void SetContents(float fillAmount, string labelText) {

		if (sprite) {
			sprite.type = UISprite.Type.Filled;
			sprite.fillDirection = UISprite.FillDirection.Radial360;
			sprite.invert = false;
			sprite.fillAmount = fillAmount;
			sprite.color = new Color(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f));
		}
		if (label) {
			label.text = labelText;
			label.transform.eulerAngles = Vector3.zero;
		}
		if (messageManager) {

		}
	}
}