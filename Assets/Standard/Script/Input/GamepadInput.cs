using UnityEngine;
using System.Collections;
using GamepadInput;

namespace Standard.Input {
/// <summary>
/// ゲームパッドの入力を処理
/// </summary>
public class GamepadInput : MonoBehaviour {
	[Header("コントローラ番号")]
	public GamePad.Index index = GamePad.Index.Any;
	[Header("入力設定")]
	public bool flagGetABXY = true;
	public bool flagGetShoulder = true;
	public bool flagGetStartBack = true;
	public bool flagGetLStick = true;
	public bool flagGetRStick = true;
	public bool flagGetDPad = true;
	protected Vector2 prevRStickVec = Vector2.zero;
	protected Vector2 prevLStickVec = Vector2.zero;
	protected Vector2 prevDpadVec = Vector2.zero;
#region MonoBehaviourイベント
	protected void Update() {
		//ボタン
		ButtonInput();
		//軸
		AxisInput();
	}
#endregion
#region 関数
	/// <summary>
	/// ボタン入力
	/// </summary>
	protected void ButtonInput() {
		if(flagGetABXY) {
			//A
			if(GamePad.GetButtonDown(GamePad.Button.A, index)) {
				AButtonDown();
			} else if(GamePad.GetButtonUp(GamePad.Button.A, index)){
				AButtonUp();
			}
			//B
			if(GamePad.GetButtonDown(GamePad.Button.B, index)) {
				BButtonDown();
			} else if(GamePad.GetButtonUp(GamePad.Button.B, index)){
				BButtonUp();
			}
			//X
			if(GamePad.GetButtonDown(GamePad.Button.X, index)) {
				XButtonDown();
			} else if(GamePad.GetButtonUp(GamePad.Button.X, index)){
				XButtonUp();
			}
			//Y
			if(GamePad.GetButtonDown(GamePad.Button.Y, index)) {
				YButtonDown();
			} else if(GamePad.GetButtonUp(GamePad.Button.Y, index)){
				YButtonUp();
			}
		}
		if(flagGetShoulder) {
			//RShoulder
			if(GamePad.GetButtonDown(GamePad.Button.RightShoulder, index)) {
				RShoulderButtonDown();
			} else if(GamePad.GetButtonUp(GamePad.Button.RightShoulder, index)){
				RShoulderButtonUp();
			}
			//LShoulder
			if(GamePad.GetButtonDown(GamePad.Button.LeftShoulder, index)) {
				LShoulderButtonDown();
			} else if(GamePad.GetButtonUp(GamePad.Button.LeftShoulder, index)){
				LShoulderButtonUp();
			}
		}
		if(flagGetStartBack) {
			//Start
			if(GamePad.GetButtonDown(GamePad.Button.Start, index)) {
				StartButtonDown();
			} else if(GamePad.GetButtonUp(GamePad.Button.Start, index)) {
				StartButtonUp();
			}
			//Back
			if(GamePad.GetButtonDown(GamePad.Button.Back, index)) {
				BackButtonDown();
			} else if(GamePad.GetButtonUp(GamePad.Button.Back, index)) {
				BackButtonUp();
			}
		}
	}
	/// <summary>
	/// 軸入力
	/// </summary>
	protected void AxisInput() {
		//軸入力
		if(flagGetRStick) {
			//Right
			Vector2 rightVec = GamePad.GetAxis(GamePad.Axis.RightStick, index);
			RightStickAxis(rightVec);
			//Cross
			CrossKeyInput(prevRStickVec, rightVec);
			prevRStickVec = rightVec;
		}
		if(flagGetLStick) {
			//Left
			Vector2 leftVec = GamePad.GetAxis(GamePad.Axis.LeftStick, index);
			LeftStickAxis(leftVec);
			//Cross
			CrossKeyInput(prevLStickVec, leftVec);
			prevLStickVec = leftVec;
		}
		if(flagGetDPad) {
			//Dpad
			Vector2 dPadVec = GamePad.GetAxis(GamePad.Axis.Dpad, index);
			DPadAxis(dPadVec);
			//Cross
			CrossKeyInput(prevDpadVec, dPadVec);
			prevDpadVec = dPadVec;
		}		
	}
	/// <summary>
	/// 十字キー入力
	/// </summary>
	protected void CrossKeyInput(Vector2 prev, Vector2 now) {
		if(prev == Vector2.zero) {
			if(now.x > 0) {
				Right();
			} else if(now.x < 0) {
				Left();
			}
			if(now.y > 0) {
				Up();
			} else if(now.y < 0) {
				Down();
			}
		}
	}
#endregion
#region 入力関数(仮想)
	public virtual void AButtonDown(){}
	public virtual void AButtonUp(){}

	public virtual void BButtonDown(){}
	public virtual void BButtonUp(){}
	
	public virtual void XButtonDown(){}
	public virtual void XButtonUp(){}

	public virtual void YButtonDown(){}
	public virtual void YButtonUp(){}

	public virtual void RShoulderButtonDown(){}
	public virtual void RShoulderButtonUp(){}

	public virtual void LShoulderButtonDown(){}
	public virtual void LShoulderButtonUp(){}

	public virtual void StartButtonDown(){}
	public virtual void StartButtonUp(){}

	public virtual void BackButtonDown(){}
	public virtual void BackButtonUp(){}

	public virtual void LeftStickAxis(Vector2 vec){}
	public virtual void RightStickAxis(Vector2 vec){}
	public virtual void DPadAxis(Vector2 vec){}

	public virtual void Up(){}
	public virtual void Down(){}
	public virtual void Left(){}
	public virtual void Right(){}
#endregion
}
}