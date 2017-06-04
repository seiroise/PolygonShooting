using UnityEngine;
using System.Collections.Generic;
using GamepadInput;
using System;

/// <summary>
/// ゲームパッドの入力処理を行う
/// <para>上下左右入力イベント</para>
/// <para>OnInputUp OnInputDown OnInputLeft OnInputRight</para>
/// <para>ボタン入力イベント</para>
/// <para>OnInputA OnInputB OnInputX OnInputY</para>
/// <para>引数 object[]</para>
/// </summary>
public class GamePadInputManager : MonoBehaviour {
	[Header("入力関連")]
	public GamePad.Index inputIndex;		//入力パッド番号
	protected Vector2 prevStickInput;		//前回のスティック入力
	protected bool prevButtonInput;		//前回のボタン入力
	[Serializable]
	public class Target {
		public string key;
		public GameObject target;
	}
	[Header("イベント関連")]
	public string defaultTarget = "Default";
	public Target[] targets;		//送信相手
	protected GameObject nowTarget;

#region MonoBehviourイベント
	protected void Start() {
		SwitchTarget(defaultTarget);
	}
	protected void Update() {
		//スティック入力
		Vector2 stickInput = GamePad.GetAxis(GamePad.Axis.LeftStick, inputIndex, true);
		if(prevStickInput == Vector2.zero) {
			string functionName = "";
			//x
			if(stickInput.x > 0f) {
				//右入力
				functionName = "OnInputRight";
			} else if(stickInput.x < 0f){
				//左入力
				functionName = "OnInputLeft";
			}
			//y
			if(stickInput.y > 0f) {
				//上入力
				functionName = "OnInputUp";
			} else if(stickInput.y < 0f){
				//下入力
				functionName = "OnInputDown";
			}
			//送信
			if(functionName != "") {
				Notify(functionName, null);
			}
		}
		prevStickInput = stickInput;

		//ボタン入力
		if(GamePad.GetButtonDown(GamePad.Button.A, inputIndex)) {
			Notify("OnInputA", true);
		} else if(GamePad.GetButtonUp(GamePad.Button.A, inputIndex)) {
			Notify("OnInputA", false);
		}
		if(GamePad.GetButtonDown(GamePad.Button.B, inputIndex)) {
			Notify("OnInputB", true);
		} else if(GamePad.GetButtonUp(GamePad.Button.B, inputIndex)) {
			Notify("OnInputB", false);
		}
		if(GamePad.GetButtonDown(GamePad.Button.X, inputIndex)) {
			Notify("OnInputX", true);
		} else if(GamePad.GetButtonUp(GamePad.Button.X, inputIndex)) {
			Notify("OnInputX", false);
		}
		if(GamePad.GetButtonDown(GamePad.Button.Y, inputIndex)) {
			Notify("OnInputY", true);
		} else if(GamePad.GetButtonUp(GamePad.Button.Y, inputIndex)) {
			Notify("OnInputY", false);
		}
		//if(GamePad.GetButtonDown(GamePad.Button.))
	}
#endregion
#region 関数
	/// <summary>
	/// ターゲット切り替え
	/// </summary>
	public void SwitchTarget(string key) {
		for(int i = 0; i < targets.Length; i++) {
			if(targets[i].key == key) {
				nowTarget = targets[i].target;
				break;
			}
		}
	}
	/// <summary>
	/// イベント送信
	/// </summary>
	protected void Notify(string functionName, object value = null) {
		object[] objs = new object[] {(int)inputIndex, value};
		if(nowTarget) {
			nowTarget.SendMessage(functionName, objs, SendMessageOptions.DontRequireReceiver);
		}
	}
#endregion
}