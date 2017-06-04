using UnityEngine;
using System;
using System.Collections.Generic;
using GamepadInput;

//ゲームパッドの入力をNGUIのボタンに伝える
public class UIGamePadMap : MonoBehaviour {

	//マップセル
	[Serializable]
	public class MapCell {
		public GameObject target;
		[Space(10)]
		public int right;
		public int left;
		public int up;
		public int down;

		public MapCell() {
			target = null;
			right = left = up = down = -1;
		}
	}
	public int defaultForcusCell = 0;
	public int startCell = 0;
	public bool flagControll = true;
	public List<MapCell> mapCellList;
	protected MapCell nowCell = null;
	protected int nowIndex;
	protected bool flagPress = false;	//ボタンが押されているか
	//連続入力防止用
	protected bool flagZero = true;	//入力が一旦ゼロになったか
#region MonoBehaviourイベント
	protected void Start() {
		//スタートにフォーカスを充てる
		SetForcus(startCell);
	}
#endregion

#region 関数
	//セル移動
	protected void MoveCell(string direction) {
		if(!flagControll) return;
		if(nowCell == null) {
			DefaultForcus();
			return;
		}

		int targetIndex = 0;
		switch(direction) {
			case "Right" :
				targetIndex = nowCell.right;
			break;
			case "Left" :
				targetIndex = nowCell.left;
			break;
			case "Up" :
				targetIndex = nowCell.up;
			break;
			case "Down" :
				targetIndex = nowCell.down;
			break;
			default:
				//以外はreturn;
			return;
		}
		SetForcus(targetIndex);
	}
	//セルにフォーカス
	protected void SetForcus(int index) {
		//移動先セルの取得
		if( index < 0 || mapCellList.Count <= index) return;
		MapCell cell = mapCellList[index];
		//移動先セルのターゲットのアクティブがfalseの場合
		if(cell.target) {
			if(!cell.target.activeInHierarchy) {
				//デフォルトフォーカスセルを使う
				cell = mapCellList[defaultForcusCell];
			}
		}

		//セルにイベント送信
		if(nowCell != null) if(nowCell.target != null) nowCell.target.SendMessage("OnHover", false, SendMessageOptions.DontRequireReceiver);
		if(cell.target)cell.target.SendMessage("OnHover", true, SendMessageOptions.DontRequireReceiver);

		nowCell = cell;
		nowIndex = index;

		//フラグを下して連続入力させないようにする
		flagZero = false;
		//押してるフラグは下す
		flagPress = false;
	}
	//デフォルトセルにもう一度フォーカスを充てる
	public void DefaultForcus() {
		SetForcus(defaultForcusCell);
	}
	//ボタン入力
	protected void Click(bool input) {
		if(!flagControll) return;
		if(input) {
			if(nowCell != null) {
				nowCell.target.SendMessage("OnPress", true, SendMessageOptions.DontRequireReceiver);
				flagPress = true;
			}
		} else {
			if(nowCell != null) {
				if(flagPress) {
					nowCell.target.SendMessage("OnClick", null, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
	}
#endregion

#region ゲームパッドイベント
	//十字入力
	protected void OnInputLeft() {
		MoveCell("Left");
	}
	protected void OnInputRight() {
		MoveCell("Right");
	}
	protected void OnInputUp() {
		MoveCell("Up");
	}
	protected void OnInputDown() {
		MoveCell("Down");
	}
	//決定
	protected void OnInputButton(bool input) {
		Click(input);
	}
	protected void OnInputA(object[] objs) {
		Click((bool)objs[1]);
	}
	protected void OnInputB(object[] objs) {
		Click((bool)objs[1]);
	}
	protected void OnInputX(object[] objs) {
		Click((bool)objs[1]);
	}
	protected void OnInputY(object[] objs) {
		Click((bool)objs[1]);
	}
#endregion
}