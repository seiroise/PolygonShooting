using UnityEngine;
using System.Collections;
using GamepadInput;

//UICameraを継承して仮想マウスで動かせるようにしたもの
public class UICustomCamera : UICamera {

	[Header("追加パラメータ")]
	public bool useMousePos = true;	//マウスの座標を使うか
	public Vector3 inputScreenPos;	//仮想入力座標
	public GamePad.Index gamePadIndex;

	//入力
	protected bool padButtonDown, padButtonUp;
	protected bool prevPadButton = false;

	//こいつがすべての元凶
	protected override void FixedUpdate() {
		if (useMouse && Application.isPlaying && handlesEvents) {
			//座標
			if(useMousePos) inputScreenPos = Input.mousePosition;
			//ここのレイキャストが肝心
			hoveredObject = Raycast(inputScreenPos, out lastHit) ? lastHit.collider.gameObject : fallThrough;
			if (hoveredObject == null) hoveredObject = genericEventHandler;
			for (int i = 0; i < 3; ++i) mMouse[i].current = hoveredObject;
		}
	}

	//マウス入力にゲームパッド入力を居候させる
	public override void ProcessMouse() {
		//ゲームパッド状態
		GamepadState state = GamePad.GetState(gamePadIndex);

		bool updateRaycast = (useMouse && Time.timeScale < 0.9f);
		bool input = state.A || state.B || state.X || state.Y;

		//パッドボタンのアップダウン判定
		padButtonDown = padButtonUp = false;
		if(prevPadButton != input) {
			if(prevPadButton) {
				//前回押してたので
				padButtonUp = true;
			} else {
				//前回離してたので
				padButtonDown = true;
			}
		}

		prevPadButton = input;

		//マウス
		if (!updateRaycast) {
			for (int i = 0; i < 3; ++i) {
				if (Input.GetMouseButton(i) || Input.GetMouseButtonUp(i)) {
					updateRaycast = true;
					break;
				}
			}
			//※追記
			//ゲームパッドのボタン入力も加える

			if(input) updateRaycast = true;
		}		
		//座標
		if(useMousePos) inputScreenPos = Input.mousePosition;

		// Update the position and delta
		mMouse[0].pos = inputScreenPos;
		mMouse[0].delta = mMouse[0].pos - lastTouchPosition;

		bool posChanged = (mMouse[0].pos != lastTouchPosition);
		lastTouchPosition = mMouse[0].pos;

		// Update the object under the mouse
		if (updateRaycast) {
			hoveredObject = Raycast(inputScreenPos, out lastHit) ? lastHit.collider.gameObject : fallThrough;
			if (hoveredObject == null) hoveredObject = genericEventHandler;
			mMouse[0].current = hoveredObject;
		}

		//if(mMouse[0].current) Debug.Log(mMouse[0].current.name);	//確認用

		// Propagate the updates to the other mouse buttons
		for (int i = 1; i < 3; ++i) {
			mMouse[i].pos = mMouse[0].pos;
			mMouse[i].delta = mMouse[0].delta;
			mMouse[i].current = mMouse[0].current;
		}

		// Is any button currently pressed?
		bool isPressed = false;

		for (int i = 0; i < 3; ++i) {
			if (Input.GetMouseButton(i) || input) {
				isPressed = true;
				break;
			}
		}

		if (isPressed) {
			// A button was pressed -- cancel the tooltip
			mTooltipTime = 0f;
		} else if (useMouse && posChanged && (!stickyTooltip || mHover != mMouse[0].current))	{
			if (mTooltipTime != 0f) {
				// Delay the tooltip
				mTooltipTime = Time.realtimeSinceStartup + tooltipDelay;
			} else if (mTooltip != null) {
				// Hide the tooltip
				ShowTooltip(false);
			}
		}

		// The button was released over a different object -- remove the highlight from the previous
		if (useMouse && !isPressed && mHover != null && mHover != mMouse[0].current) {
			if (mTooltip != null) ShowTooltip(false);
			//※ここのHighlightでオブジェクトから外れたかを監視
			Highlight(mHover, false);
			mHover = null;
		}

		// Process all 3 mouse buttons as individual touches
		if (useMouse) {
			for (int i = 0; i < 3; ++i) {
				bool pressed = Input.GetMouseButtonDown(i) | padButtonDown;
				bool unpressed = Input.GetMouseButtonUp(i) | padButtonUp;
	
				currentTouch = mMouse[i];
				currentTouchID = -1 - i;
	
				// We don't want to update the last camera while there is a touch happening
				if (pressed) currentTouch.pressedCam = currentCamera;
				else if (currentTouch.pressed != null) currentCamera = currentTouch.pressedCam;
	
				// Process the mouse events
				//※追記この関数でpress判定をしている
				ProcessTouch(pressed, unpressed);
			}
			currentTouch = null;
		}

		// If nothing is pressed and there is an object under the touch, highlight it
		if (useMouse && !isPressed && mHover != mMouse[0].current) {
			mTooltipTime = Time.realtimeSinceStartup + tooltipDelay;
			mHover = mMouse[0].current;
			//※追記この関数からゲームオブジェクトにイベントを送っている
			Highlight(mHover, true);
		}
	}

}