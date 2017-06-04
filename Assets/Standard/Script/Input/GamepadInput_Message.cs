using UnityEngine;
using System.Collections;
/// <summary>
/// ゲームパッドの入力を通知する
/// </summary>
public class GamepadInput_Message : Standard.Input.GamepadInput {
	[Header("イベント")]
	public GameObject target;
	public GameObject subTarget;
#region 関数
	/// <summary>
	/// ターゲットにイベント通知
	/// </summary>
	protected void NotifyTarget(string functionName, object value) {
		FuncBox.Notify(target, functionName, value);
		FuncBox.Notify(subTarget, functionName, value);
	}
#endregion
#region 入力関数(オーバーライド)
	public override void AButtonDown() {
		NotifyTarget("AButtonDown", gameObject);
	}
	public override void AButtonUp() {
		NotifyTarget("AButtonUp", gameObject);
	}

	public override void BButtonDown() {
		NotifyTarget("BButtonDown", gameObject);
	}
	public override void BButtonUp() {
		NotifyTarget("BButtonUp", gameObject);
	}

	public override void XButtonDown() {
		NotifyTarget("XButtonDown", gameObject);
	}
	public override void XButtonUp() {
		NotifyTarget("XButtonUp", gameObject);
	}

	public override void YButtonDown() {
		NotifyTarget("YButtonDown", gameObject);
	}
	public override void YButtonUp() {
		NotifyTarget("YButtonUp", gameObject);
	}

	public override void RShoulderButtonDown() {
		NotifyTarget("RShoulderButtonDown", gameObject);
	}
	public override void RShoulderButtonUp() {
		NotifyTarget("RShoulderButtonUp", gameObject);
	}

	public override void LShoulderButtonDown() {
		NotifyTarget("LShoulderButtonDown", gameObject);
	}
	public override void LShoulderButtonUp() {
		NotifyTarget("LShoulderButtonUp", gameObject);
	}

	public override void StartButtonDown() {
		NotifyTarget("StartButtonDown", gameObject);
	}
	public override void StartButtonUp() {
		NotifyTarget("StartButtonUp", gameObject);
	}

	public override void BackButtonDown() {
		NotifyTarget("BackButtonDown", gameObject);
	}
	public override void BackButtonUp() {
		NotifyTarget("BackButtonUp", gameObject);
	}

	public override void LeftStickAxis(Vector2 vec) {
		NotifyTarget("LeftStickAxis", vec);
	}
	public override void RightStickAxis(Vector2 vec) {
		NotifyTarget("RightStickAxis", vec);
	}
	public override void DPadAxis(Vector2 vec) {
		NotifyTarget("DPadAxis", vec);
	}

	public override void Up() {
		NotifyTarget("Up", gameObject);
	}
	public override void Down() {
		NotifyTarget("Down", gameObject);
	}
	public override void Left() {
		NotifyTarget("Left", gameObject);
	}
	public override void Right() {
		NotifyTarget("Right", gameObject);
	}
#endregion
}