using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShipEditor {
	//機体エディターの管理
	public class ShipEditorManager : SingletonMonoBehaviour<ShipEditorManager> {
		//編集状態
		public enum EditState {
			None,
			Create,
			Select,
			Adjust,
			Move,
			Weapon,
			Dialog_PartsInfo,
			Dialog_Launcher,
			Dialog_Color,
		}
		//依存クラス
		protected GameManager gm;
		protected ShipEditorGUIManager seGUI;
		//Other
		protected bool flagLate = false;	//入力を1フレーム遅らせる
		[Header("Alpha")]
		public float shipPartsAlpha;				//アルファ値
		protected string colorPropertyName = "_Color";
		[Header("Grid")]
		public Grid mainGrid;			//メインのグリッド
		public RectCamera rectCamera;	//矩形範囲移動カメラ
		public Camera shipCamera;		//機体の全体を常に映すカメラ
		[Header("Edit")]
		public GridMeshLine line;
		public string shipName = "名無しの機体";
		public GameObjectLayerManager layerManager;	//重なり管理
		protected Stack<EditState> editStateStack;		//エディット状態のスタック
		protected GameObject hoverObject;			//マウスに重なっているオブジェクト
		[Header("Launcher")]
		public bool flagAutoLauncherSetting = true;		//自動的に弾を設定するかどうか
		public string defaultBullet = "通常弾Lv1";
		//機体パーツ管理
		protected Dictionary<GameObject, ToolBox.ShipPartsData> shipPartsDic;		//検索用
		protected float zSpace;	//機体パーツ同士のz軸での間隔
		//保存
		protected bool flagNew;		//新規か
		protected string shipSaveID;	//新規でない場合の保存ID
		//Create
		protected GameObject createLineStartPointMarker;
		//Select
		protected GameObject selectShipParts;				//選択機体パーツ
		protected ToolBox.ShipPartsData selectShipPartsData;	//選択機体パーツデータ
		protected ToolBox.Launcher selectLauncher;			//選択ランチャー
		protected GameObject selectLauncherMarker;		//選択ランチャーマーカー
		//Adjust
		protected int[] selectVertexIndex;		//選択頂点インデックス
		protected bool flagEditVertex;		//頂点編集フラグ
		//Move
		protected bool flagMoveShipParts;		//機体パーツ移動フラグ
		protected Vector3 moveOffset;		//機体パーツの原点からのずれ
		[Header("Marker")]
		public SpriteManager marker;
		public SpriteRenderer vertexMarker;
		public SpriteRenderer newVertexMarker;
		public SpriteRenderer launcherMarker;
		public SpriteRenderer boosterMarker;
		public SpriteRenderer startPointMarker;
		public Color defaultMarkerColor = Color.white;			//標準マーカーカラー
		public Color usingLauncherMarkerColor = Color.green;	//使用中ランチャーマーカーカラー
		public Color vartexMarkerColor = Color.blue;			//頂点マーカーカラー
		public Color newVartexMarkerColor = Color.red;			//新規頂点マーカーカラー
		//マーカー管理
		protected Dictionary<GameObject, List<GameObject>> shipMarkerDic;		//機体パーツからマーカーにアクセス
		protected Dictionary<GameObject, GameObject> markerShipDic;			//マーカーから機体パーツにアクセス
		protected Dictionary<GameObject, ToolBox.Launcher> markerLauncherDic;	//マーカーからランチャーにアクセス
		protected Dictionary<GameObject, ToolBox.Booster> markerBoosterDic;		//マーカーからブースターにアクセス
		protected Dictionary<GameObject, int[]> markerEditrDic;					//マーカーから頂点インデックスにアクセス
	#region MonoBehaviourイベント
		protected override void Awake() {
			base.Awake();
			//メンバ初期化
			editStateStack = new Stack<EditState>();
			editStateStack.Push(EditState.None);
			//機体パーツ管理
			shipPartsDic = new Dictionary<GameObject,ToolBox.ShipPartsData>();
			//マーカー管理
			shipMarkerDic  = new Dictionary<GameObject,List<GameObject>>();
			markerShipDic = new Dictionary<GameObject,GameObject>();
			markerLauncherDic = new Dictionary<GameObject,ToolBox.Launcher>();
			markerBoosterDic = new Dictionary<GameObject,ToolBox.Booster>();
			markerEditrDic = new Dictionary<GameObject,int[]>();
		}
		protected void Start() {
			gm = GameManager.Instance;
			seGUI = ShipEditorGUIManager.Instance;
			//グリッド
			GridInit();
			//マーカー
			marker.InstantiatePrefabs("Vertex", vertexMarker, 64);
			marker.InstantiatePrefabs("NewVertex", newVertexMarker, 64);
			marker.InstantiatePrefabs("Launcher", launcherMarker, 32);
			marker.InstantiatePrefabs("Booster", boosterMarker, 32);
			marker.InstantiatePrefabs("StartPoint", startPointMarker, 4);
			//既存機体の読み込み
			if(gm.GetEditShipData() != null) {
				ToolBox.ShipData shipData = gm.GetEditShipData();
				SetShipData(shipData);
				shipSaveID = shipData.id;
				flagNew = false;
			} else {
				flagNew = true;
			}
			//表示
			seGUI.shipDataIndicator.SetShipData(GetShipData());
		}
		protected void Update() {
			UpdateEditState();
		}
	#endregion
	#region 状態遷移関数
		/// <summary>
		/// 編集状態を遷移させる
		/// </summary>
		public void ChangeEditState(EditState state) {
			//同値判定
			if(editStateStack.Count > 0) {
				if(editStateStack.Peek() == state) return;
			}
			switch(state) {
				case EditState.None:
					ChangeEditState_None();
				break;
				case EditState.Create:
					ChangeEditState_Create();
				break;
				case EditState.Select:
					ChangeEditState_Select();
				break;
				case EditState.Adjust:
					ChangeEditState_Adjust();
				break;
				case EditState.Move:
					ChangeEditState_Move();
				break;
				case EditState.Weapon:
					ChangeEditState_Weapon();
				break;
				case EditState.Dialog_PartsInfo:
					ChangeEditState_DialogPartsInfo();
				break;
				case EditState.Dialog_Launcher:
					ChangeEditState_DialogLauncher();
				break;
				case EditState.Dialog_Color:
					ChangeEditState_DialogColor();
				break;
			}
			editStateStack.Push(state);
			//UI変更
			seGUI.SetEditModeLabel(state);
		}
		/// <summary>
		/// 前回の状態に遷移
		/// </summary>
		public void ChangePrevEditState() {
			if(editStateStack.Count <= 0) return;
			editStateStack.Pop();
			ChangeEditState(editStateStack.Pop());
		}
		/// <summary>
		/// 強制的にNoneに戻す
		/// </summary>
		public void ResetEditState() {
			while(editStateStack.Peek() != EditState.None) {
				//ひたすらキャンセル
				Cancel(true);
			}
		}
	#endregion
	#region キャンセル処理
		/// <summary>
		/// キャンセル処理
		/// <para>フラグは強いキャンセル</para>
		/// </summary>
		public void Cancel(bool flagStrong = false) {
			switch(editStateStack.Peek()) {
				case EditState.None:
					Cancel_None(flagStrong);
				break;
				case EditState.Dialog_PartsInfo:
					Cancel_Dialog(flagStrong);
				break;
				case EditState.Dialog_Launcher:
					Cancel_DialogLauncher(flagStrong);
				break;
				case EditState.Dialog_Color:
					Cancel_Dialog(flagStrong);
				break;
				case EditState.Create:
					Cancel_Create(flagStrong);
				break;
				case EditState.Select:
					Cancel_Select(flagStrong);
				break;
				case EditState.Adjust:
					Cancel_Adjust(flagStrong);
				break;
				case EditState.Move:
					Cancel_Move(flagStrong);
				break;
				case EditState.Weapon:
					Cancel_Weapon(flagStrong);
				break;
			}
		}
	#endregion
	#region 状態別更新処理
		/// <summary>
		/// 状態別更新処理
		/// </summary>
		protected void UpdateEditState() {
			if(flagLate) {
				flagLate = false;
				return;
			}
			if(editStateStack.Count <= 0) return;
			switch(editStateStack.Peek()) {
				case EditState.None:
					Update_None();
				break;
				case EditState.Create: 
					Update_Create();
				break;
				case EditState.Select:
					Update_Select();
				break;
				case EditState.Dialog_PartsInfo:
					Update_Dialog();
				break;
				case EditState.Dialog_Launcher:
					Update_Dialog();
				break;
				case EditState.Dialog_Color:
					Update_Dialog();
				break;
				case EditState.Adjust:
					Update_Adjust();
				break;
				case EditState.Move:
					Update_Move();
				break;
				case EditState.Weapon:
					Update_Weapon();
				break;
			}
		}
	#endregion
	#region None関連
		//変更時
		protected void ChangeEditState_None() {
			//表示
			seGUI.infoBox.Indicate(false);
		}
		//キャンセル時
		protected void Cancel_None(bool flagStrong) {
			seGUI.HideMenu();
		}
		//更新
		protected void Update_None() {
			if(Input.GetMouseButtonDown(1)) {
				Cancel();
			}
		}
	#endregion
	#region Create関連
		//変更時
		protected void ChangeEditState_Create() {
			//infoBoxに表示
			Create_IndicateInfo();
		}
		//キャンセル時
		protected void Cancel_Create(bool flagStrong) {
			if(flagStrong) {
				//線のリセット
				line.Reset();
				//前に戻る
				ChangePrevEditState();
			} else {
				if(line.PositionCount() <= 0) {
					//前に戻る
					ChangePrevEditState();
				}
			}
		}
		//更新
		protected void Update_Create() {
			bool flagClick = false;
			//入力
			if(Input.GetMouseButtonDown(0)) {
				Create_Click();
				flagClick = true;
			} else if(Input.GetMouseButtonDown(1)) {
				//処理の順番上Cancelを先に呼ぶ
				Cancel();
				line.RemoveEndPosition();
				flagClick = true;
				if(line.PositionCount() == 0) {
					//始点の非表示
					Create_IndicateStartPointMarker(false);
				}
			}
			//出力
			if(editStateStack.Peek() == EditState.Create) {
				//予告線の描画
				line.FlashPosition_Mouse();
				//情報表示
				if(flagClick) {
					Create_IndicateInfo();
				}
			}
		}

		/// <summary>
		/// クリック処理
		/// </summary>
		protected void Create_Click() {
			line.AddPosition_Mouse();
			if(line.PositionCount() == 1) {
				//始点の表示
				Create_IndicateStartPointMarker(true);
			}
			//始点終点重なり確認
			if(line.PositionCount() >= 3) {
				if(line.CheckEqualPos(0, line.PositionCount() - 1)) {
					//機体を作成
					List<Vector3> positions = line.GetPositions();
					positions.RemoveAt(positions.Count - 1);
					CreateShipParts(positions);
					//座標リストを初期化
					line.Reset();
					//始点の非表示
					Create_IndicateStartPointMarker(false);
				}
			}
		}
		/// <summary>
		/// 情報表示
		/// </summary>
		protected void Create_IndicateInfo() {
			StringBuilder sb = new StringBuilder();
			int count = line.PositionCount();
			sb.Append("頂点数 : ");
			sb.AppendLine(count.ToString());
			if(count == 0) {
				sb.AppendLine("グリッドのどこかを左クリックしよう");
			} else if(2 >= count && count > 0) {
				sb.AppendLine("頂点を追加して形を作ろう");
			} else if(count > 2) {
				sb.AppendLine("形ができたら始点を左クリックしてパーツの完成");
			}
			sb.Append("左クリック : 頂点の追加\n右クリック : 戻る");
			//出力
			seGUI.infoBox.SetText(sb.ToString());
		}
		/// <summary>
		/// スタートポイントマーカーの表示/非表示
		/// </summary>
		/// <param name="flag"></param>
		protected void Create_IndicateStartPointMarker(bool flag) {
			if(flag) {
				if(createLineStartPointMarker) {
					createLineStartPointMarker.SetActive(false);
				}
				createLineStartPointMarker = marker.SetSpriteParameter("StartPoint", line.GetPositions()[0], Vector3.zero, defaultMarkerColor);
			} else {
				marker.ResetSprite(createLineStartPointMarker);
			}
		}
	#endregion
	#region Select関連
		//変更時
		protected void ChangeEditState_Select() {
			hoverObject = null;
			string text = "左クリック : 選択\n右クリック : 戻る";
			seGUI.infoBox.SetText(text);
		}
		//キャンセル時
		protected void Cancel_Select(bool flagStrong) {
			//前(None)に戻る
			ChangePrevEditState();
		}
		//更新
		protected void Update_Select() {
			//レイキャスト
			Select_Raycast();
			//クリック
			Select_Click();
		}
		
		/// <summary>
		/// クリック処理
		/// </summary>
		protected void Select_Click() {
			if(Input.GetMouseButtonDown(0)) {
				if(hoverObject == null) return;
				switch(hoverObject.name) {
				case "ShipParts":
					//データを取得
					selectShipParts = hoverObject;
					selectShipPartsData = shipPartsDic[selectShipParts];
					//状態遷移
					ChangeEditState(EditState.Dialog_PartsInfo);
					break;
				}
			} else if(Input.GetMouseButtonDown(1)) {
				Cancel();
			}
		}
		/// <summary>
		/// レイキャスト処理
		/// </summary>
		protected void Select_Raycast() {
			Vector3 pos = FuncBox.GetMousePoint(rectCamera.c);
			pos.z = -10f;
			GameObject g = RaycastInput(pos);
			string text = "左クリック : 選択\n右クリック : 戻る";
			//nullチェック
			if(g == null) {
				 if(hoverObject != null) {
					seGUI.infoBox.SetText(text);
					hoverObject = g;
				 }
			} else {
				if(hoverObject == null || hoverObject != g) {
					switch(g.name) {
					case "ShipParts":
						//データを取得
						seGUI.infoBox.SetText(shipPartsDic[g].GetStringInfo() + "\n" + text);
						break;
					case "Launcher":
						//データを取得
						seGUI.infoBox.SetText(markerLauncherDic[g].GetStringInfo() + "\n右クリック : 戻る");
						break;
					case "Booster":
						//データを取得
						seGUI.infoBox.SetText(markerBoosterDic[g].GetStringInfo() + "\n右クリック : 戻る");
						break;
					}
					hoverObject = g;
				}
			}
		}
	#endregion
	#region Adjust関連
		//変更時
		protected void ChangeEditState_Adjust() {
			hoverObject = null;
			//メニューを非表示
			seGUI.HideMenu();
			//マーカーを非表示
			HideSelectShipPartsMarker();
			//選択機体パーツの線を表示する
			IndicateSelectShipPartsLine();
			//マーカーを表示
			IndicateSelectShipPartsMarker_Edit(true);
			//選択機体パーツを見えなくする
			selectShipParts.SetActive(false);
			//infoBoxに表示
			Adjust_IndicateInfo();
		}
		//キャンセル時
		protected void Cancel_Adjust(bool flagStrong) {
			//機体の更新
			List<Vector3> positions = line.GetPositions();
			positions.RemoveAt(positions.Count - 1);
			UpdateShipParts(selectShipParts, positions);
			//機体を見えるようにする
			selectShipParts.SetActive(true);
			//線を初期化
			line.Reset();
			//配置の修正
			layerManager.Replace();
			//前(ダイアログ)に戻る
			ChangePrevEditState();
		} 
		//更新
		protected void Update_Adjust() {
			if(flagEditVertex) {
				//マウス座標を取得する
				Vector3 pos;
				if(line.GetMouseGridPos(out pos)) {
					for(int i = 0; i < selectVertexIndex.Length; i++) {
						line.ChangePosition(selectVertexIndex[i], pos);
					}
				}
				//入力
				if(Input.GetMouseButtonDown(0)) {
					Adjust_Click();
				}
			} else {
				//レイキャスト処理
				Adjust_Raycast();
				//入力
				if(Input.GetMouseButtonDown(0)) {
					Adjust_Click();
				}				
			}
			 if(Input.GetMouseButtonDown(1)) {
				//頂点編集確認
				if(flagEditVertex) {
					Adjust_Click();
				} else {
					Cancel();
				}
			}
		}

		/// <summary>
		/// クリック処理
		/// </summary>
		protected void Adjust_Click() {
			if(flagEditVertex) {
				List<Vector3> posList = line.GetPositions();
				IndicateShipPartsMarker_Edit(selectShipParts, posList, selectShipPartsData);
				flagEditVertex = false;
				//表示更新
				Adjust_IndicateInfo();
			} else {
				if(hoverObject == null) return;
				if(!markerEditrDic.ContainsKey(hoverObject)) return;
				int[] index = markerEditrDic[hoverObject];
				switch(hoverObject.name) {
				case "Vertex":
					//指定キーが押されていたら削除
					if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftControl)) {
						//頂点の数が5未満(始点終点含めて)なら何もしない
						if(line.positions.Count < 5) break;
						//頂点を削除
						for(int i = 0; i < index.Length; i++) {
							line.RemovePosition(index[i]);
						}
						//始点なら
						if(index.Length >= 2) {
							line.AddPosition(line.positions[0]);
						}
						//マーカーの描画更新
						HideSelectShipPartsMarker();
						List<Vector3> posList = line.GetPositions();
						IndicateShipPartsMarker_Edit(selectShipParts, posList, selectShipPartsData);
					} else {
						flagEditVertex = true;
					}
					break;
				case "NewVertex":
					flagEditVertex = true;
					line.InsertPosition_Mouse(index[0]);
					break;
				}
				//頂点編集フラグが立っていたら
				if(flagEditVertex) {
					selectVertexIndex = index;
					//マーカーを非表示に
					HideSelectShipPartsMarker();
					//表示更新
					Adjust_IndicateInfo();
				}
			}
		}
		/// <summary>
		/// レイキャスト処理
		/// </summary>
		protected void Adjust_Raycast() {
			Vector3 pos = FuncBox.GetMousePoint(rectCamera.c);
			pos.z = -10f;
			GameObject g = RaycastInput(pos);
			bool flag = false;
			//nullチェック
			if(g == null) {
				 if(hoverObject != null) {
					hoverObject = g;
					flag = true;
				 }
			} else {
				if(hoverObject == null || hoverObject != g) {
					hoverObject = g;
					flag = true;
				}
			}
			//表示更新
			if(flag) {
				Adjust_IndicateInfo();
			}
		}
		/// <summary>
		/// 情報表示
		/// </summary>
		protected void Adjust_IndicateInfo() {
			StringBuilder sb = new StringBuilder();
			int count = line.PositionCount() - 1;
			sb.Append("頂点数 : ");
			sb.AppendLine(count.ToString());
			if(flagEditVertex) {
				sb.AppendLine("位置の決定 : 左クリック");
			} else {
				if(hoverObject == null) {
					//頂点移動
					sb.Append("頂点移動 : [");
					sb.Append(FuncBox.ColorToHexString(vartexMarkerColor));
					sb.AppendLine("]■[-]マーカー左クリック");
					//頂点削除
					sb.Append("頂点削除 : Ctrlキー + [");
					sb.Append(FuncBox.ColorToHexString(vartexMarkerColor));
					sb.AppendLine("]■[-]マーカー左クリック");
					//新しい頂点を作成
					sb.Append("新しい頂点を作成 : [");
					sb.Append(FuncBox.ColorToHexString(newVartexMarkerColor));
					sb.AppendLine("]◆[-]マーカー左クリック");
				} else {
					switch(hoverObject.name) {
					case "Vertex":
						sb.AppendLine("頂点マーカー");
						sb.AppendLine("移動 : 左クリック");
						sb.AppendLine("削除 : Ctrlキー + 左クリック");
						break;
					case "NewVertex":
						sb.AppendLine("新規頂点マーカー");
						sb.AppendLine("新しい頂点を作成 : 左クリック");
						break;
					}
				}
			}
			//その他
			sb.Append("戻る : 右クリック");
			//出力
			seGUI.infoBox.SetText(sb.ToString());
		}
	#endregion
	#region Move関連
		//変更時
		protected void ChangeEditState_Move() {
			//メニューを非表示
			seGUI.HideMenu();
			//マーカーを非表示
			HideSelectShipPartsMarker();
			//表示更新
			Move_IndicateInfo();
		}
		//キャンセル時
		protected void Cancel_Move(bool flagStrong) {
			//マーカーを表示
			IndicateSelectShipPartsMarker_Special();
			//座標のずれを修正
			layerManager.Replace();
			//前(ダイアログ)に戻る
			ChangePrevEditState();
		}
		//更新
		protected void Update_Move() {
			//移動処理
			if(flagMoveShipParts) {
				Move_Translate();
			}
			//入力
			if(Input.GetMouseButtonDown(0)) {
				Move_Click();
			} else if(Input.GetMouseButtonUp(0)) {
				Move_Click();
			}
			if(Input.GetMouseButtonDown(1)) {
				if(flagMoveShipParts) {
					Move_Click();
				} else {
					Cancel();
				}
			}
		}

		/// <summary>
		/// クリック処理
		/// </summary>
		protected void Move_Click() {
			//操作フラグを立てる
			flagMoveShipParts = flagMoveShipParts ? false : true;
			if(flagMoveShipParts) {
				//オフセットは左下に合わせる
				moveOffset = selectShipPartsData.bounds.center - selectShipPartsData.bounds.size / 2f;
			} else {
				selectShipPartsData.offset = selectShipParts.transform.localPosition;
			}
			//表示更新
			Move_IndicateInfo();
		}
		/// <summary>
		/// 動かす処理
		/// </summary>
		protected void Move_Translate() {
			//マウス座標を取得する
			Vector3 mPos = FuncBox.GetMousePoint(rectCamera.c);
			//グリッド座標に変換
			mPos = mainGrid.WorldToGridCrossPosition(mPos);
			Vector3 setPos = selectShipParts.transform.localPosition;	//最終的な座標
			//はみ出し確認(x, yについて)
			Vector3 checkPos = mPos + selectShipPartsData.bounds.size;
			//x
			if(mainGrid.CheckGridX(checkPos.x)) {
				if(mainGrid.CheckGridX(mPos.x)) {
					setPos.x = mPos.x - moveOffset.x;
				}
			}
			//y
			if(mainGrid.CheckGridY(checkPos.y)) {
				if(mainGrid.CheckGridY(mPos.y)) {
					setPos.y = mPos.y - moveOffset.y;
				}
			}
			selectShipParts.transform.localPosition = setPos;
		}
		/// <summary>
		/// 情報表示
		/// </summary>
		protected void Move_IndicateInfo() {
			StringBuilder sb = new StringBuilder();
			if(flagMoveShipParts) {
				sb.AppendLine("位置の決定 : 左クリック");
			} else {
				sb.AppendLine("移動開始 : 左クリック");
			}
			sb.Append("戻る : 右クリック");
			seGUI.infoBox.SetText(sb.ToString());
		}
	#endregion
	#region Weapon関連
		//変更時
		protected void ChangeEditState_Weapon() {
			hoverObject = null;
			string text = "左クリック : 選択\n右クリック : 戻る";
			seGUI.infoBox.SetText(text);
		}
		//キャンセル時
		protected void Cancel_Weapon(bool flagStrong) {
			//前(None)に戻る
			ChangePrevEditState();
		}
		//更新
		protected void Update_Weapon() {
			//レイキャスト
			Weapon_Raycast();
			//クリック
			Weapon_Click();
		}

		/// <summary>
		/// クリック処理
		/// </summary>
		protected void Weapon_Click() {
			if(Input.GetMouseButtonDown(0)) {
				if(hoverObject == null) return;
				switch(hoverObject.name) {
				case "Launcher":
					//データを取得
					selectLauncherMarker = hoverObject;
					selectLauncher = markerLauncherDic[selectLauncherMarker];
					//状態遷移
					ChangeEditState(EditState.Dialog_Launcher);			
					break;
				case "Booster":
					//データを取得
					//ToolBox.Booster b  = markerBoosterDic[hoverObject];
					break;
				}
			} else if(Input.GetMouseButtonDown(1)) {
				Cancel();
			}
		}
		/// <summary>
		/// レイキャスト処理
		/// </summary>
		protected void Weapon_Raycast() {
			Vector3 pos = FuncBox.GetMousePoint(rectCamera.c);
			pos.z = -10f;
			GameObject g = RaycastInput(pos);
			string text = "左クリック : 選択\n右クリック : 戻る";
			//nullチェック
			if(g == null) {
				 if(hoverObject != null) {
					seGUI.infoBox.SetText(text);
					hoverObject = g;
				 }
			} else {
				if(hoverObject == null || hoverObject != g) {
					switch(g.name) {
					case "ShipParts":
						//データを取得
						seGUI.infoBox.SetText(shipPartsDic[g].GetStringInfo() + "\n右クリック : 戻る");
						break;
					case "Launcher":
						//データを取得
						seGUI.infoBox.SetText(markerLauncherDic[g].GetStringInfo() + "\n" + text);
						break;
					case "Booster":
						//データを取得
						seGUI.infoBox.SetText(markerBoosterDic[g].GetStringInfo() + "\n右クリック : 戻る");
						break;
					}
					hoverObject = g;
				}
			}
		}
	#endregion
	#region Dialog関連
		//キャンセル時
		protected void Cancel_Dialog(bool flagStrong) {
			//前に戻る
			seGUI.HideMenu();
			ChangePrevEditState();
		}
		//更新
		protected void Update_Dialog() {
			if(Input.GetMouseButtonDown(1)) {
				Cancel();
			}
		}
	#endregion
	#region Dialog_PartsInfo関連
		protected void ChangeEditState_DialogPartsInfo() {
			//ダイアログを表示
			seGUI.ShipPartsDialog(selectShipPartsData, true);
			//infoBoxを非表示
			seGUI.infoBox.Indicate(false);
		}
	#endregion
	#region Dialog_Launcher関連
		//変更時
		protected void ChangeEditState_DialogLauncher() {
			//ダイアログを表示
			seGUI.LauncherDialog(selectLauncher);
			//infoBoxを非表示
			seGUI.infoBox.Indicate(false);
		}
		//キャンセル
		protected void Cancel_DialogLauncher(bool flagStrong) {
			//ランチャーマーカーの色を変更する
			if(markerLauncherDic.ContainsKey(selectLauncherMarker)) {
				marker.ChangeActiveSpriteColor(selectLauncherMarker, selectLauncher.flagUse ? usingLauncherMarkerColor : defaultMarkerColor);
			}
			//前に戻る
			seGUI.HideMenu();
			ChangePrevEditState();
		}
	#endregion
	#region Dialog_Color関連
		//変更時
		protected void ChangeEditState_DialogColor() {
			//ダイアログを表示
			seGUI.ColorDialog(selectShipPartsData);
			//infoBoxを非表示
			seGUI.infoBox.Indicate(false);
		}
	#endregion
	#region 関数
		/// <summary>
		/// グリッド周りの初期化
		/// </summary>
		protected void GridInit() {
			//グリッド
			ToolBox.ShipSize size = gm.editShipSize;
			mainGrid.InstantiateGrid(size.hLineNum, size.vLineNum, Vector2.one * 0.125f);
			//矩形範囲カメラ
			Rect rect = mainGrid.GetGridRect();
			rectCamera.rect = rect;
			rectCamera.SetCameraSize(rect.height * 0.5f);
			//機体カメラ
			shipCamera.orthographicSize = (rect.height * 0.5f);
		}
		/// <summary>
		/// 入力遅延フラグの設定
		/// </summary>
		public void SetFlagLate() {
			flagLate = true;
		}
		/// <summary>
		/// 新規作成か
		/// </summary>
		public bool GetFlagNew() {
			return flagNew;
		}
	#endregion
	#region 機体関連
		/// <summary>
		/// 機体データの保存
		/// </summary>
		public bool SaveShipData() {
			ToolBox.ShipData shipData = GetShipData();
			if(shipData == null) return false;
			//保存
			if(flagNew) {
				//新規
				if(!gm.ShipDic_SaveAs(gm.defaultShipCategory, shipData)) return false;
				shipSaveID = shipData.id;
				flagNew = false;
			} else {
				//上書き
				gm.ShipDic_Update(shipData);
			}			
			//書き込み
			gm.WriteShipDic(gm.shipFolderPath);
			return true;
		}
		/// <summary>
		/// 機体パーツをまとめて機体データを取得する。
		/// <para>データはディープコピーしたもの</para>
		/// </summary>
		public ToolBox.ShipData GetShipData() {
			//機体パーツデータのリストを取得する(ディープコピーしたもの)
			List<GameObject> shipPartsList = layerManager.GetObjectList();
			List<ToolBox.ShipPartsData> dataList = new List<ToolBox.ShipPartsData>();
			foreach(GameObject parts in shipPartsList) {
				if(shipPartsDic.ContainsKey(parts)) {
					dataList.Add(shipPartsDic[parts]);
				}
			}
			ToolBox.ShipData shipData =gm.CreateShipData(dataList, shipName);
			//新規ではない場合はidを引き継ぎ
			if(!flagNew) {
				shipData.id = shipSaveID;
			}
			return shipData.DeepCopy();
		}
		/// <summary>
		/// 機体データからパーツを配置する
		/// </summary>
		public void SetShipData(ToolBox.ShipData shipData) {
			foreach(ToolBox.ShipPartsData parts in shipData.shipPartsList) {
				Debug.Log("確認");
				CreateShipParts(parts.DeepCopy());
			}
			shipName = shipData.name;
		}
	#endregion
	#region 機体パーツ作成
		/// <summary>
		/// 空の機体パーツオブジェクトを作成する
		/// </summary>
		protected GameObject CreateEmptyShipParts() {
			//機体パーツを作成する
			GameObject shipParts = (GameObject)Instantiate(gm.emptyObject);
			shipParts.name = "ShipParts";
			shipParts.transform.position = Vector3.zero;
			return shipParts;
		}
		/// <summary>
		/// 空の機体パーツオブジェクトを作成し図形データを付加する
		/// </summary>
		protected GameObject CreateEmptyShipParts(ClassBox.Figure figure) {
			GameObject shipParts = CreateEmptyShipParts();
			figure.SetMeshToGameObject(shipParts, gm.shipPartsMaterial, figure.GetColor(), flagAddCollision: true);
			return shipParts;
		}
		/// <summary>
		/// 頂点リストと色を指定して機体パーツを作成し辞書に追加する。
		/// <para>戻り値は生成パーツ。失敗したらnull</para>
		/// </summary>
		public GameObject CreateShipParts(List<Vector3> posList, Color color) {
			
			//図形データを作成
			ClassBox.Figure f = ClassBox.Figure.CreateNewFigure(posList);
			if(f == null) return null;
			//空の機体パーツを作成
			GameObject shipParts = CreateEmptyShipParts(f);
			//機体パーツデータの作成
			ToolBox.ShipPartsData shipPartsData = new ToolBox.ShipPartsData(f);
			//ランチャーにデフォルトバレットを設定
			if(flagAutoLauncherSetting) {
				foreach(ToolBox.Launcher l in shipPartsData.launcher) {
					l.SetBullet(gm.bulletDic["Normal"][defaultBullet]);
				}
			}
			shipPartsData.instance = shipParts;
			//マーカーの表示
			IndicateShipPartsMarker_Special(shipParts, shipPartsData);
			//追加
			AddShipParts(shipParts, shipPartsData);
			//アルファを設定
			SetShipPartsAlpha(shipParts);
			//UI更新
			seGUI.shipDataIndicator.SetShipData(GetShipData());
			return shipParts;
		}
		/// <summary>
		/// 頂点リストを指定して機体パーツを作成し辞書に追加する。
		/// <para>戻り値は生成パーツ。失敗したらnull</para>
		/// </summary>
		public GameObject CreateShipParts(List<Vector3> posList) {
			return CreateShipParts(posList, Color.black);
		}
		/// <summary>
		/// 機体パーツデータを指定して機体パーツを作成し辞書に追加する
		/// </summary>
		public GameObject CreateShipParts(ToolBox.ShipPartsData shipPartsData) {
			//空の機体パーツを作成
			GameObject shipParts = CreateEmptyShipParts(shipPartsData.figureData);
			//位置
			shipParts.transform.localPosition = shipPartsData.offset;
			//辞書に追加
			AddShipParts(shipParts, shipPartsData);
			//アルファを設定
			SetShipPartsAlpha(shipParts);
			//マーカーの表示
			IndicateShipPartsMarker_Special(shipParts, shipPartsData);
			return shipParts;
		}
	#endregion
	#region 機体パーツ関連
		/// <summary>
		/// 既に存在する機体パーツの頂点を更新する
		/// </summary>
		public void UpdateShipParts(GameObject shipParts, List<Vector3> posList) {
			//辞書確認
			if(!shipPartsDic.ContainsKey(shipParts)) return;
			//図形データの作成
			ClassBox.Figure f = ClassBox.Figure.CreateNewFigure(posList);
			if(f == null) return;
			//更新(見た目)
			f.SetMeshToGameObject(shipParts, gm.shipPartsMaterial, selectShipPartsData.figureData.GetColor());
			//更新(面積)
			f.GetTotalSize();
			//更新(当たり判定)
			shipParts.GetComponent<MeshCollider>().sharedMesh = f.GetMesh();
			//座標を0に設定する
			shipParts.transform.localPosition = selectShipPartsData.offset = Vector3.zero;
			//更新(データ)
			shipPartsDic[shipParts].SetFigureData(f);
			//マーカーの更新
			HideShipPartsMarker(shipParts);
			IndicateShipPartsMarker_Special(shipParts, shipPartsDic[shipParts]);
			//アルファを設定
			SetShipPartsAlpha(shipParts);
			//UI更新
			seGUI.shipDataIndicator.SetShipData(GetShipData());
		}
		/// <summary>
		/// 機体パーツをコピーする
		/// </summary>
		protected GameObject CopyShipParts(ToolBox.ShipPartsData source) {
			//機体パーツデータを先に複製しておく
			ToolBox.ShipPartsData shipPartsData = source.DeepCopy();
			//図形データを作成
			ClassBox.Figure f = ClassBox.Figure.CreateNewFigure(source.figureData.GetVector3List());
			if(f == null) return null;
			//図形データを設定(直接)
			shipPartsData.figureData = f;
			//空の機体パーツを作成
			GameObject shipParts = CreateEmptyShipParts();
			//メッシュを設定
			f.SetMeshToGameObject(shipParts, gm.shipPartsMaterial, source.figureData.GetColor(), flagAddCollision: true);
			//機体インスタンス
			shipPartsData.instance = shipParts;
			//マーカーの表示
			IndicateShipPartsMarker_Special(shipParts, shipPartsData);
			//追加
			AddShipParts(shipParts, shipPartsData);
			//アルファを設定
			SetShipPartsAlpha(shipParts);
			//UI更新
			seGUI.shipDataIndicator.SetShipData(GetShipData());
			return shipParts;
		}
		/// <summary>
		/// 機体パーツをリスト、辞書に追加
		/// </summary>
		protected void AddShipParts(GameObject parts, ToolBox.ShipPartsData shipPartsData) {
			if(shipPartsDic.ContainsKey(parts)) {
				//登録済み
				Debug.Log("機体パーツリスト、辞書に登録済みです");
				return;
			}
			//登録
			shipPartsDic.Add(parts, shipPartsData);
			layerManager.Add(parts);
		}
		/// <summary>
		/// 機体パーツをリスト、辞書から削除
		/// </summary>
		protected void RemoveShipParts(GameObject parts) {
			if(!shipPartsDic.ContainsKey(parts)) {
				//登録されていない
				Debug.Log("機体パーツリスト、辞書に登録されていません");
				return;
			}
			//削除
			shipPartsDic.Remove(parts);
			layerManager.Remove(parts);
		}
		/// <summary>
		/// 機体パーツを指定して機体パーツデータを取得する
		/// </summary>
		protected ToolBox.ShipPartsData GetShipPartsData(GameObject shipParts) {
			if(shipPartsDic.ContainsKey(shipParts)) {
				return shipPartsDic[shipParts];
			} else {
				return null;
			}
		}
		/// <summary>
		/// 機体パーツを隠す。(activeをfalseに)
		/// </summary>
		protected void HideShipParts(GameObject shipParts) {
			shipParts.SetActive(false);
		}
		/// <summary>
		/// 指定した機体パーツを削除する。
		/// <para>flagRemoveDicは辞書から削除するか</para>
		/// <para>flagHideMarkerはマーカーを非表示にするか</para>
		/// </summary>
		protected void DeleteShipParts(GameObject shipParts, bool flagRemoveDic = true, bool flagHideMarker = false) {
			if(!shipParts) return;
			//リスト、辞書から削除
			if(flagRemoveDic) {
				RemoveShipParts(shipParts);
			}
			//マーカーを非表示
			if(flagHideMarker) {
				HideShipPartsMarker(shipParts);
			}
			//オブジェクトを削除
			Destroy(shipParts);
			//UI更新
			seGUI.shipDataIndicator.SetShipData(GetShipData());
		}
		/// <summary>
		/// 選択機体パーツを削除する。flagHideMarkerはマーカーを非表示にするか
		/// </summary>
		public void DeleteSelectShipParts(bool flagRemoveDic = true, bool flagHideMarker = false) {
			DeleteShipParts(selectShipParts, flagRemoveDic, flagHideMarker);
		}
		/// <summary>
		/// 選択機体パーツの線を表示する。flagMarkerはmarkerを表示するか
		/// </summary>
		public void IndicateSelectShipPartsLine() {
			//機体パーツデータ
			List<Vector3> posList = selectShipPartsData.figureData.GetVector3List();
			//オフセット分ずらす
			for(int i = 0; i < posList.Count; i++) {
				posList[i] += selectShipPartsData.offset;
			}
			//線を設定
			line.SetPositions(posList, true);
		}
		/// <summary>
		/// 機体パーツのマテリアルのアルファ値だけ設定する
		/// </summary>
		public void SetShipPartsAlpha(GameObject shipParts) {
			//辞書確認
			if(!shipPartsDic.ContainsKey(shipParts)) return;
			ToolBox.ShipPartsData s = shipPartsDic[shipParts];
			Color color = s.figureData.GetColor();
			color.a = shipPartsAlpha;
			s.figureData.mr.material.SetColor(colorPropertyName, color);
		}
		/// <summary>
		/// 選択機体パーツのマテリアルのアルファ値だけ設定する
		/// </summary>
		public void SetSelectShipPartsAlpha() {
			SetShipPartsAlpha(selectShipParts);
		}
		/// <summary>
		/// 機体パーツの色を設定する
		/// </summary>
		public void SetShipPartsColor(GameObject shipParts, Color c) {
			//辞書確認
			if(!shipPartsDic.ContainsKey(shipParts)) return;
			ToolBox.ShipPartsData s = shipPartsDic[shipParts];
			//色設定
			s.figureData.SetColor(c, colorPropertyName);
			//アルファ設定
			SetShipPartsAlpha(shipParts);
		}
		/// <summary>
		/// 選択機体パーツの色を設定する
		/// </summary>
		public void SetSelectShipPartsColor(Color c) {
			SetShipPartsColor(selectShipParts, c);
		}
		/// <summary>
		/// 選択中の機体をコピーしてMove状態に遷移
		/// </summary>
		public void SelectShipCopyAndMove() {
			GameObject shipParts = CopyShipParts(selectShipPartsData);
			selectShipParts = shipParts;
			selectShipPartsData = shipPartsDic[shipParts];
			ChangeEditState(EditState.Move);
			//操作フラグを立てる
			flagMoveShipParts = true;
			//オフセットは左下に合わせる
			moveOffset = selectShipPartsData.bounds.center - selectShipPartsData.bounds.size / 2f;
		}
		/// <summary>
		/// 機体パーツの数を取得する
		/// </summary>
		public int GetShipPartsNum() {
			return shipPartsDic.Count;
		}
		/// <summary>
		/// 全ての機体パーツを削除する
		/// </summary>
		public void AllDeleteShipParts() {
			List<GameObject> shipPartsList = new List<GameObject>(shipPartsDic.Keys);
			for(int i = shipPartsList.Count - 1; i >= 0; i--) {
				DeleteShipParts(shipPartsList[i], true, true);
			}
		}
	#endregion
	#region マーカー関連
		/// <summary>
		/// 機体パーツデータを指定して特殊マーカーを表示
		/// </summary>
		protected void IndicateShipPartsMarker_Special(GameObject shipParts, ToolBox.ShipPartsData shipPartsData) {
			GameObject m;
			List<GameObject> mList = new List<GameObject>();
			bool flag = false;
			//既に機体パーツが追加されているか確認
			if(shipMarkerDic.ContainsKey(shipParts)) {
				mList = shipMarkerDic[shipParts];
				flag = true;
			}
			Vector3 pos;	
			float z = -1f;	//マーカーのz座標
			foreach(ToolBox.Launcher l in shipPartsData.launcher) {
				pos = l.point + shipParts.transform.localPosition;
				pos.z = z;
				m = marker.SetSpriteParameter("Launcher", pos, l.angle, l.flagUse ? usingLauncherMarkerColor : defaultMarkerColor);
				//辞書追加
				markerShipDic.Add(m, shipParts);
				mList.Add(m);
				markerLauncherDic.Add(m, l);
			}
			foreach(ToolBox.Booster b in shipPartsData.booster) {
				pos = b.point + shipParts.transform.localPosition;
				pos.z = z;
				m = marker.SetSpriteParameter("Booster", pos, b.angle, defaultMarkerColor);
				//辞書追加
				markerShipDic.Add(m, shipParts);
				mList.Add(m);
				markerBoosterDic.Add(m, b);
			}
			//辞書追加
			if(flag) {
				shipMarkerDic[shipParts] = mList;
			} else {
				shipMarkerDic.Add(shipParts, mList);
			}
		}
		/// <summary>
		/// 選択機体パーツデータの特殊マーカーを表示
		/// </summary>
		protected void IndicateSelectShipPartsMarker_Special() {
			//nullチェック
			if(selectShipParts == null) return;
			//選択機体パーツデータを取得
			ToolBox.ShipPartsData selectShipPartsData = shipPartsDic[selectShipParts];
			//表示
			IndicateShipPartsMarker_Special(selectShipParts, selectShipPartsData);
		}
		/// <summary>
		/// 任意の座標データを指定して編集マーカーを表示
		/// <para>座標リストは始点が最後に追加されていることが前提</para>
		/// </summary>
		protected void IndicateShipPartsMarker_Edit(GameObject shipParts, List<Vector3> posList, ToolBox.ShipPartsData shipPartsData, bool flagUseShipPartsOffset = false) {
			GameObject m;
			List<GameObject> mList = new List<GameObject>();
			bool flag = false;
			//既に機体パーツが追加されているか確認
			if(shipMarkerDic.ContainsKey(shipParts)) {
				mList = shipMarkerDic[shipParts];
				flag = true;
			}
			Vector3 offset = Vector3.zero;
			if(flagUseShipPartsOffset) {
				offset = shipPartsData.offset;
			}
			//頂点
			Vector3 pos;
			float z = -1f;	//マーカーのz座標
			for(int i = 0; i < posList.Count - 1; i++) {
				pos = posList[i] + offset;
				pos.z = z;
				m = marker.SetSpriteParameter("Vertex", pos, Vector3.zero, vartexMarkerColor);
				//辞書追加
				markerShipDic.Add(m, shipParts);
				if(i == 0) {
					markerEditrDic.Add(m, new int[]{i, posList.Count - 1});
				} else {
					markerEditrDic.Add(m, new int[]{i});
				}
				mList.Add(m);
			}
			//中点
			for(int i = 0; i < posList.Count - 1; i++) {
				pos = ((posList[i + 1] + posList[i]) / 2f) + offset;
				pos.z = z;
				m = marker.SetSpriteParameter("NewVertex", pos, Vector3.zero, newVartexMarkerColor);
				//辞書追加
				markerShipDic.Add(m, shipParts);
				markerEditrDic.Add(m, new int[]{i + 1});
				mList.Add(m);
			}
			//辞書追加
			if(flag) {
				shipMarkerDic[shipParts] = mList;
			} else {
				shipMarkerDic.Add(shipParts, mList);
			}
		}
		/// <summary>
		/// 機体パーツデータを指定して編集マーカーを表示
		/// </summary>
		protected void IndicateShipPartsMarker_Edit(GameObject shipParts, ToolBox.ShipPartsData shipPartsData, bool flagUseShipPartsOffset = false) {
			//座標リスト
			List<Vector3> posList = shipPartsData.figureData.GetVector3List();
			posList.Add(posList[0]);
			//マーカーを表示
			IndicateShipPartsMarker_Edit(shipParts, posList, shipPartsData, flagUseShipPartsOffset);
		}
		/// <summary>
		/// 選択機体パーツデータからマーカーを生成
		/// </summary>
		protected void IndicateSelectShipPartsMarker_Edit(bool flagUseShipPartsOffset = false) {
			if(selectShipParts == null) return;
			IndicateShipPartsMarker_Edit(selectShipParts, selectShipPartsData, flagUseShipPartsOffset);
		}
		/// <summary>
		/// 機体パーツを指定してそれに関係のあるマーカーを全て非表示に
		/// </summary>
		protected void HideShipPartsMarker(GameObject shipParts) {
			//含み確認
			if(!shipMarkerDic.ContainsKey(shipParts)) return;
			//マーカーの非アクティブ化
			List<GameObject> markers = shipMarkerDic[shipParts];
			foreach(GameObject g in markers) {
				marker.ResetSprite(g);
				//辞書から削除
				markerShipDic.Remove(g);
				if(markerLauncherDic.ContainsKey(g)) {
					markerLauncherDic.Remove(g);
				} else if(markerBoosterDic.ContainsKey(g)) {
					markerBoosterDic.Remove(g);
				} else if(markerEditrDic.ContainsKey(g)) {
					markerEditrDic.Remove(g);
				}
			}
			//辞書から削除
			shipMarkerDic.Remove(shipParts);
		}
		/// <summary>
		/// 選択機体パーツに関係のあるマーカーを全て非表示に
		/// </summary>
		protected void HideSelectShipPartsMarker() {
			HideShipPartsMarker(selectShipParts);
		}
	#endregion
	#region レイキャスト関連
		/// <summary>
		/// 座標を指定してレイキャスト処理。(0f, 0f, 1f)方向にレイを撃つ
		/// <para>戻り値はヒットしたGameObject</para>
		/// </summary>
		protected GameObject RaycastInput(Vector3 pos) {
			//レイキャスト
			RaycastHit hitInfo;
			if(!Physics.Raycast(pos, Vector3.forward, out hitInfo, 100f)) return null;
			GameObject g = hitInfo.collider.gameObject;
			return g;
		}
		/// <summary>
		/// マウス座標からレイキャスト
		/// </summary>
		protected GameObject RaycastInput_Mouse() {
			Vector3 mPos = FuncBox.GetMousePoint(rectCamera.c);
			mPos.z = -10f;
			return RaycastInput(mPos);
		}
	#endregion
	#region ゲームパッドイベント(GamepadInput_Message)
		protected void BButtonDown() {
			Cancel();
		}
	#endregion
	}
}