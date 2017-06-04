using UnityEngine;
using System.Collections;

//NGUIのUI用
public class UIAudioMessage : MonoBehaviour {

	public string audioName;
	public UIButtonMessage.Trigger trigger = UIButtonMessage.Trigger.OnClick;
	protected AudioManager audio;
	protected bool mHighlighted = false;
#region MonoBehaviourイベント
	protected void Start() {
		audio = AudioManager.Instance;
	}
#endregion
#region 関数
	protected void Send() {
		if(!audio) return;
		audio.PlaySE(audioName);
	}
#endregion
#region UIイベント
	void OnHover (bool isOver) {
		if (enabled) {
			if (((isOver && trigger == UIButtonMessage.Trigger.OnMouseOver) ||
				(!isOver && trigger == UIButtonMessage.Trigger.OnMouseOut))) {
				Send();
			}
			mHighlighted = isOver;
		}
	}
	void OnPress (bool isPressed)	{
		if (enabled) {
			if (((isPressed && trigger == UIButtonMessage.Trigger.OnPress) ||
				(!isPressed && trigger == UIButtonMessage.Trigger.OnRelease))) {
				Send();
			}
		}
	}
	void OnClick ()	{
		if (enabled && trigger == UIButtonMessage.Trigger.OnClick) {
			Send();
		}
	}
	void OnDoubleClick () {
		if (enabled && trigger ==UIButtonMessage.Trigger.OnDoubleClick) {
			Send();
		}
	}
#endregion
}