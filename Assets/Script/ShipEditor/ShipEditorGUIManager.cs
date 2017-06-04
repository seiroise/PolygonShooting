using UnityEngine;
using System.Collections;
using System.Linq;

namespace ShipEditor {
	/// <summary>
	/// 機体エディターのGUI管理
	/// </summary>
	public class ShipEditorGUIManager : SingletonMonoBehaviour<ShipEditorGUIManager> {
		//依存クラス
		protected GameManager gm;
		protected ShipEditorManager sem;
		//表示スタック
		protected ClassBox.GameObjectStack indicateStack;
		[Header("表示_Blueprint")]
		public UILabel editModeLabel;
		[Header("表示_ShipData")]
		public UIShipDataIndicator shipDataIndicator;
		[Header("GamepadInput")]
		public GamepadInput_Message gamepadInput;
		[Header("MainMenu")]
		public GameObject mainMenu;
		[Header("Menu")]
		public GameObject edit;
		public GameObject parts;
		[Header("Dialog_Infobox")]
		public Standard.UI.Text.UITextBox infoBox;
		[Header("Dialog_PartsInfo")]
		public GameObject dialog_partsInfo;
		public UIShipPartsDataIndicator shipPartsDataIndicator;
		public GameObject dialog_partsEdit;
		[Header("Dialog_Launcher")]
		public GameObject dialog_launcher;
		public UILauncher launcher;
		public GameObject dialog_bulletSelect;
		[Header("Dialog_Color")]
		public GameObject dialog_color;
		public UIColorSetting colorSetting;
		[Header("Rename")]
		public GameObject dialog_rename;
		public Dialog_Input textInput;
		[Header("Dialog_YesNo")]
		public GameObject dialog_yesNo;
		public Dialog_YesNo yesNo;
		[Header("Dialog_Yes")]
		public GameObject dialog_yes;	//二択ダイアログのNoを消して使用
		public Dialog_YesNo yes;
	#region MonoBehaviourイベント
		protected override void Awake() {
			base.Awake();
		}
		protected void Start() {
			gm = GameManager.Instance;
			sem = ShipEditorManager.Instance;
			//初期化
			Init();
		}
	#endregion
	#region 関数
		/// <summary>
		/// 初期化
		/// </summary>
		protected void Init() {
			//infoBox
			infoBox.Indicate(false);
			//表示スタック
			indicateStack = new ClassBox.GameObjectStack();
			//MainMenuを追加しておく
			indicateStack.Push(mainMenu);
			//Menu
			edit.SetActive(false);
			parts.SetActive(false);
			//Dialog_PartsInfo
			dialog_partsInfo.SetActive(false);
			dialog_partsEdit.SetActive(false);
			//Dialog_launcher
			dialog_launcher.SetActive(false);
			dialog_bulletSelect.SetActive(false);
			//Dialog_Color
			dialog_color.SetActive(false);
			//Dialog_YesNo
			dialog_yesNo.SetActive(false);
			//Dialog_Yesn
			dialog_yes.SetActive(false);
			//Dialog_Rename
			dialog_rename.SetActive(false);
		}
		/// <summary>
		/// スタック格納用
		/// </summary>
		protected void IndicateStackPush(GameObject g) {
			//プッシュ
			indicateStack.Push(g);
			//ゲームパッド入力ターゲットの変更
			gamepadInput.target = g;
		}
	#endregion
	#region UI関連
		/// <summary>
		/// 編集モードラベルを設定
		/// </summary>
		public void SetEditModeLabel(ShipEditorManager.EditState state) {
			switch(state) {
				case ShipEditorManager.EditState.None : 
					editModeLabel.text = "-";
				break;
				case ShipEditorManager.EditState.Dialog_PartsInfo:
				case ShipEditorManager.EditState.Dialog_Launcher:
				case ShipEditorManager.EditState.Dialog_Color:
					editModeLabel.text = "Dialog";
				break;
				default :
					editModeLabel.text = state.ToString();
				break;
			}
		}
		/// <summary>
		/// 表示されているメニューを全て閉じる
		/// </summary>
		public void HideMenu() {
			//スタックがmainMenuにるまでループ
			indicateStack.StackCheck(mainMenu);
			//ゲームパッド入力ターゲットの変更
			gamepadInput.target = mainMenu;
			//機体パーツ情報ダイアログのプレビューカメラを切る
			shipPartsDataIndicator.HideCamera();
			//ランチャーダイアログのプレビューカメラを切る
			launcher.HideCamera();
		}
		/// <summary>
		/// 機体パーツダイアログを表示/非表示
		/// <para>flagは表示/非表示 flagEditはeditメニューを表示するか</para>
		/// </summary>
		public void ShipPartsDialog(ToolBox.ShipPartsData shipPartsData, bool flagEdit = false) {
			//表示してるものを全て消す
			HideMenu();
			//表示
			indicateStack.Push(dialog_partsInfo);
			shipPartsDataIndicator.Set(shipPartsData);
			//editメニューの表示
			if(flagEdit) {
				indicateStack.Push(dialog_partsEdit);
			}
		}
		/// <summary>
		/// ランチャーダイアログの表示/非表示
		/// </summary>
		public void LauncherDialog(ToolBox.Launcher l) {
			//表示してるものを全て消す
			HideMenu();
			//表示
			indicateStack.Push(dialog_bulletSelect);
			indicateStack.Push(dialog_launcher);
			launcher.SetLauncher(l);
		}
		/// <summary>
		/// 色設定ダイアログの表示
		/// </summary>
		public void ColorDialog(ToolBox.ShipPartsData shipPartsData) {
			//表示してるものを全て消す
			HideMenu();
			//表示
			indicateStack.Push(dialog_color);
			colorSetting.SetShipPartsData(shipPartsData);
		}
		/// <summary>
		/// テキスト入力ダイアログの表示
		/// </summary>
		public void InputDialog(string text) {
			//表示してるものを全て消す
			HideMenu();
			//表示
			indicateStack.Push(dialog_rename);
			textInput.SetText(text);
		}
		/// <summary>
		/// 二択ダイアログの表示
		/// </summary>
		public void YesNoDialog(string title, string text, string functionName) {
			//表示してるものを全て消す
			HideMenu();
			//表示
			IndicateStackPush(dialog_yesNo);
			yesNo.SetDialogText(title, text);
			yesNo.SetEvent(gameObject, functionName);
		}
		/// <summary>
		/// 一択(確認)ダイアログの表示
		/// </summary>
		public void YesDialog(string title, string text, string functionName) {
			//表示してるものを全て消す
			HideMenu();
			//表示
			IndicateStackPush(dialog_yes);
			yes.SetDialogText(title, text);
			yes.SetEvent(gameObject, functionName);
		}
	#endregion
	#region UIイベント
		//Button_Menu
		protected void EditButtonClicked() {
			//EditStateをNoneに
			sem.ResetEditState();
			//閉じる
			HideMenu();
			//editをスタック
			IndicateStackPush(edit);
		}
		protected void WeaponButtonClicked() {
			//EditStateをNoneに
			sem.ResetEditState();
			//Weapon状態へ
			sem.ChangeEditState(ShipEditorManager.EditState.Weapon);
			//閉じる
			HideMenu();
			//遅延フラグ
			sem.SetFlagLate();
		}
		protected void PartsButtonClicked() {
			//EditStateをNoneに
			sem.ResetEditState();
			//メニューを閉じる
			HideMenu();
			//partsをスタック
			IndicateStackPush(parts);
		}
		protected void RenameButtonClicked() {
			//EditStateをNoneに
			sem.ResetEditState();
			//入力ダイアログを表示
			InputDialog(sem.shipName);
		}
		protected void TestButtonClicked() {
			//EditStateをNoneに
			sem.ResetEditState();
			if(sem.GetShipPartsNum() >= 1) {
				//二択ダイアログを表示
				YesNoDialog("Test", "テストプレイをします\nセーブしていない場合は\n自動的にセーブされます", "Test_Result");
			} else {
				//確認ダイアログを表示(確認だけなので渡す関数名は適当)
				YesDialog("Test?", "パーツが一つもないので\nテストプレイできません", "HideMenu");
			}
		}
		protected void SaveButtonClicked() {
			//EditStateをNoneに
			sem.ResetEditState();
			if(sem.GetShipPartsNum() > 0) {
				//新規か上書きか
				if(sem.GetFlagNew()) {
					//二択ダイアログを表示
					YesNoDialog("SaveAs", "機体を新規保存します\n\n機体名\n [" + sem.shipName +"]\n\nよろしいですか?", "Save_Result");
				} else {
					//二択ダイアログを表示
					YesNoDialog("Overwrite Save", "機体を上書き保存します\n\n機体名\n [" + sem.shipName +"]\n\nよろしいですか?", "Save_Result");
				}
			} else {
				//確認ダイアログを表示(確認だけなので渡す関数名は適当)
				YesDialog("Save?", "パーツが一つもないので\n保存できません", "HideMenu");
			}
		}
		protected void ReturnButtonClicked() {
			//EditStateをNoneに
			sem.ResetEditState();
			//二択ダイアログを表示
			if(sem.GetShipPartsNum() == 0) {
				Return_Result(true);
			} else {
				YesNoDialog("Return", "メニューに戻ります\n\n機体を保存してない場合\n今までの作業がなくなりますが\n戻りますか?", "Return_Result");
			}
		}
		//Button_Edit
		protected void Edit_CreateButtonClicked() {
			sem.ChangeEditState(ShipEditorManager.EditState.Create);
			//メニューを閉じる
			HideMenu();
			//遅延フラグ
			sem.SetFlagLate();
		}
		protected void Edit_SelectButtonClicked() {
			sem.ChangeEditState(ShipEditorManager.EditState.Select);
			//メニューを閉じる
			HideMenu();
			//遅延フラグ
			sem.SetFlagLate();
		}
		protected void Edit_ResetButtonClicked() {
			//メニューを閉じる
			HideMenu();
			//二択ダイアログを表示
			YesNoDialog("Reset", "パーツを全て削除して白紙に戻します\nよろしいですか?\n※元には戻せません!", "Reset_Result");
		}
		//Button_Parts
		//Dialog_PartsEdit
		protected void PartsEdit_AdjustButtonClicked() {
			sem.ChangeEditState(ShipEditorManager.EditState.Adjust);
		}
		protected void PartsEdit_MoveButtonClicked() {
			sem.ChangeEditState(ShipEditorManager.EditState.Move);
			//遅延フラグ
			sem.SetFlagLate();
		}
		protected void PartsEdit_CopyButtonClicked() {
			sem.SelectShipCopyAndMove();
		}
		protected void PartsEdit_ColorButtonClicked() {
			sem.ChangeEditState(ShipEditorManager.EditState.Dialog_Color);
		}
		protected void PartsEdit_DeleteButtonClicked() {
			//二択ダイアログを表示
			YesNoDialog("Delete", "本当に\n削除しますか?", "Delete_Result");
		}
		//Dialog_Color
		protected void OnChangeColor(Color color) {
			//機体の色を変更
			sem.SetSelectShipPartsColor(color);
			//前の状態へ
			sem.ChangePrevEditState();
		}
		//Dialog_Return
		protected void Return_Result(bool value) {
			if(value) {
				gm.LoadLevel("ShipEditorMenu");
			} else {
				//非表示に
				HideMenu();
			}
		}
		//Dialog_Delete
		protected void Delete_Result(bool value) {
			if(value) {
				//選択機体パーツを削除(辞書からもマーカーも含めて)
				sem.DeleteSelectShipParts(true, true);
				//非表示に
				HideMenu();
				sem.ChangePrevEditState();
			} else {
				//同じ状態には遷移できないのでいったん戻してから
				sem.ChangePrevEditState();
				sem.ChangeEditState(ShipEditorManager.EditState.Dialog_PartsInfo);
			}
		}
		//Dialog_Test
		protected void Test_Result(bool value) {
			if(value) {
				//保存
				if(sem.SaveShipData()) {
					gm.playMode = ToolBox.PlayMode.Test;
					gm.SetEditShipData(sem.GetShipData());
					gm.selectStage = gm.stageDic["Simple"][0];
					gm.LoadLevel("Stage_SingleCamera_Test_1");
				}
			}
			//非表示に
			HideMenu();
		}
		//Dialog_Save
		protected void Save_Result(bool value) {
			if(value) {
				//保存テスト
				if(sem.SaveShipData()) {
					//二択ダイアログを表示
					YesNoDialog("SaveComplete", "保存が完了しました。\nこのままメニューに戻りますか?", "Return_Result");
				}
			} else {
				//非表示に
				HideMenu();
			}
		}
		//Dialog_Input
		protected void Input_Submit(string text) {
			sem.shipName = text;
			shipDataIndicator.SetName(text);
			//確認ダイアログを表示(確認だけなので渡す関数名は適当)
			YesDialog("RenameComplete", "機体名を\n[" + sem.shipName + "]\nに変更しました", "HideMenu");
		}
		//Dialog_Reset
		protected void Reset_Result(bool value) {
			if(value) {
				sem.AllDeleteShipParts();
			}
			//メニューを非表示
			HideMenu();
		}
	#endregion
	}
} 