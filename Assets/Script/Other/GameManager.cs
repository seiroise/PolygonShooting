using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Text;
/// <summary>
/// ゲーム管理クラス
/// </summary>
public class GameManager : SingletonMonoBehaviour<GameManager> {
	[Header("管理クラス")]
	public string fadeManagerPath;	//フェード管理クラスのパス
	public string audioManagerPath;	//音声管理クラスのパス
	//管理クラスインスタンス
	protected FadeManager fade;	
	protected AudioManager audio;
	//シーン管理
	protected Stack<string> sceneStack;	//今までのシーンのスタック
	[Header("空オブジェクト")]
	public GameObject emptyObject;
	[Header("データ")]
	//色
	public Color[] playerColor;
	public Color enemyColor;
	//機体
	public List<ToolBox.ShipSize> shipSizeList;
	private Dictionary<string, Dictionary<string, ToolBox.ShipData>> shipDataDic;		//機体データ辞書
	public string defaultShipCategory = "Player";
	public Material shipMaterial;					//機体用マテリアル
	public Material shipPartsMaterial;				//機体パーツ用マテリアル
	public PhysicMaterial shipPhysicsMaterial;		//機体用物理マテリアル
	public Aiming playerBackground;				//プレイヤーのバックグラウンドオブジェクト
	//機体エディター
	public ToolBox.ShipEditMode shipEditMode;		//機体編集モード
	public bool flagNewShip = true;				//機体新規作成
	private ToolBox.ShipData editShipData;			//編集機体
	//弾
	public Dictionary<string, Dictionary<string, ToolBox.Bullet>> bulletDic;	//弾辞書
	[Header("エフェクト")]
	public List<string> effectPrefabPaths;
	public Dictionary<string, GameObject> effectDic;	//エフェクト辞書
	[Header("機体作成関連_Test")]
	public ToolBox.ShipSize editShipSize;	//編集機体サイズ
	[Header("ステージ")]
	public Material stageObjectMaterial;
	public Dictionary<string, List<ToolBox.Stage>> stageDic;	//ステージ辞書
	[Header("プレイ用パラメータ")]
	public ToolBox.PlayMode playMode = ToolBox.PlayMode.None;			//プレイモード
	public ToolBox.BattleMode battleMode = ToolBox.BattleMode.Normal;	//対戦モード
	private ToolBox.Stage _selectStage;			//選択ステージ
	public ToolBox.Stage selectStage {
		get{return _selectStage;}
		set{_selectStage = value;}
	}
	private ToolBox.ShipData[] playerSelectShips;	//プレイヤーが選んだ機体情報
	private ToolBox.PlayerScore[] _playerScores;		//プレイヤーのスコア
	[Header("コンフィグ用パラメータ")]
	public int battleTime = 90;		//対戦の時間
	public float stageScale = 150f;	//ステージのサイズ
	public float gameSpeed = 1f;		//全体的な速度
	public float playerHPScale = 1f;	//プレイヤーのHP倍率
	public int maxEnergy = 3000;		//エネルギー容量
	public int energyOutput = 500;	//秒間エネルギー出力

	[Header("ランキング")]
	private List<ToolBox.RankingRecord> _rankingRecords;	//ランキングレコードのリスト
	public List<ToolBox.RankingRecord> rankingRecords {
		get {return _rankingRecords;}
	}
	public int yourRank;
	[Header("音声")]
	public string titleBGM;
	public string menuBGM;
	public string editorBGM;
	public string stageBGM;
	public string hoverSE;
	public string clickSE;
	//読み込み
	[HideInInspector]
	public string bulletFilePath;	//弾情報ファイル
	[HideInInspector]
	public string stageFolderPath;	//ステージ情報フォルダ
	[HideInInspector]
	public string shipFolderPath;	//機体情報フォルダ
	[HideInInspector]
	public string configFilePath;	//コンフィグファイルパス
	[HideInInspector]
	public string rankingFilePath;	//ランキングファイル
#region MonoBehaviourイベント
	protected override void Awake() {
		base.Awake();
		//初期設定
		DontDestroyOnLoad(gameObject);
		//初期化
		bulletDic = new Dictionary<string, Dictionary<string, ToolBox.Bullet>>();
		shipDataDic = new Dictionary<string, Dictionary<string, ToolBox.ShipData>>();
		//管理クラス生成
		fade = (Instantiate(Resources.Load(fadeManagerPath)) as GameObject).GetComponent<FadeManager>();
		audio = (Instantiate(Resources.Load(audioManagerPath)) as GameObject).GetComponent<AudioManager>();
		//シーン管理
		sceneStack = new Stack<string>();
		//弾情報
		bulletFilePath = "/Bullet/Bullet.txt";
		bulletFilePath = Application.streamingAssetsPath + bulletFilePath;
		bulletDic = ReadBullet(FuncBox.GetTextReader(bulletFilePath));
		//ステージ情報
		stageFolderPath = "/Stage";
		stageFolderPath = Application.streamingAssetsPath + stageFolderPath;
		stageDic = ReadStageDic(stageFolderPath);
		//機体情報
		shipFolderPath = "/Ship";
		shipFolderPath = Application.streamingAssetsPath + shipFolderPath;
		shipDataDic = ReadShipDic(shipFolderPath);
		//コンフィグ
		configFilePath = "/Config.txt";
		configFilePath = Application.streamingAssetsPath + configFilePath;
		ReadConfig(configFilePath);
		//ランキング
		rankingFilePath = "/Ranking.txt";
		rankingFilePath = Application.streamingAssetsPath + rankingFilePath;
		ReadRankingRecords(rankingFilePath);
		//エフェクト
		effectDic = ReadResourcesDic(effectPrefabPaths);
		//デフォルト編集機体サイズ
		editShipSize = shipSizeList[0];
		//プレイヤー選択機体
		playerSelectShips = new ToolBox.ShipData[4];
		for(int i = 0; i < playerSelectShips.Length; i++) {
			playerSelectShips[i] = null;
		}
	}
	protected void OnDestroy() {
		//機体のデータを書き込む
		WriteShipDic(shipFolderPath);
		//コンフィグデータを書き込む
		WriteConfig(configFilePath);
		//ランキングデータを書き込む
		WriteRankingRecords(rankingFilePath);
	}
#endregion
#region アクセサ
	public ToolBox.PlayerScore[] playerScores {
		get {return _playerScores;}
		set {_playerScores = value;}
	}
#endregion
#region ステージ関連
	/// <summary>
	/// 選択ステージを設定
	/// </summary>
	public void SetSelectStage(string category, int num) {
		selectStage = stageDic[category][num];
	}
#endregion
#region シーン関連
	/// <summary>
	/// シーン遷移
	/// </summary>
	public void LoadLevel(string sceneLevel) {
		//読み込みシーンをプッシュ
		sceneStack.Push(sceneLevel);
		//遷移
		fade.LoadLevel(sceneLevel);
		//コルーチン停止
		StopMeshCreating();
	}
	/// <summary>
	/// 1つ前のシーンへ遷移
	/// </summary>
	public void LoadPrevLevel() {
		if(sceneStack.Count >= 1) {
			sceneStack.Pop();
		}
		if(sceneStack.Count <= 0) {
			//タイトルへ
			fade.LoadLevel("Title");
		} else {
			//前回のシーンへ
			fade.LoadLevel(sceneStack.Peek());
		}
		foreach(string level in sceneStack) {
			Debug.Log("確認 : " + level);
		}
	}
#endregion
#region オーディオ関連
	//BGM
	public void StopBGM() {
		audio.StopBGM();
	}
	public void PlayTitleBGM() {
		audio.PlayBGM(titleBGM);
	}
	public void PlayMenuBGM() {
		audio.PlayBGM(menuBGM);
	}
	public void PlayStageBGM() {
		audio.PlayBGM(stageBGM);
	}
	public void PlayEditorBGM() {
		audio.PlayBGM(editorBGM);
	}
	//SE
	public void PlayHoverSE() {
		audio.PlaySE(hoverSE);
	}
	public void PlayClickSE() {
		audio.PlaySE(clickSE);
	}
#endregion
#region 機体関連
	/// <summary>
	/// カテゴリ、IDを指定して機体を生成
	/// </summary>
	public Ship InstantiateShip(string category, string shipID, Color color) {
		ToolBox.ShipData shipData = ShipDic_Select(category, shipID);
		if (shipData == null) {
			return null;
		}
		return InstantiateShip(shipData, color);
	}
	/// <summary>
	/// 機体を指定して機体を作成
	/// </summary>
	public Ship InstantiateShip(ToolBox.ShipData shipData, Color color) {
		//機体を実体化
		GameObject g = (GameObject)Instantiate(emptyObject);
		Ship ship = g.AddComponent<Ship>();
		//有効化
		ship.Activate(shipData, this, color);
		return ship;
	}
	/// <summary>
	/// プレイヤー用機体を作成
	/// </summary>
	public Player InstantiatePlayer(ToolBox.ShipData shipData, int playerNum = 1) {
		//色
		Color c = playerColor[playerNum - 1];
		//機体を生成
		Ship ship = InstantiateShip(shipData, c);
		//プレイヤー用コンポーネントをアタッチする
		Player player = ship.gameObject.AddComponent<Player>();
		//有効化
		player.Activate(this, "Player " + playerNum, "Player");
		//プレイヤーゲームパッド設定
		player.SetGamePad(playerNum, ToolBox.InputType.GamePad, false);
		return player;
	}
	/// <summary>
	/// エネミー用機体を作成(ジェネリック)
	/// </summary>
	public T InstantiateEnemy<T>(ToolBox.ShipData shipData) where T : Enemy{
		//色
		Color c = enemyColor;
		//機体を生成
		Ship ship = InstantiateShip(shipData, c);
		//エネミー用コンポーネントをアタッチする
		T enemy = ship.gameObject.AddComponent<T>();
		//有効化
		enemy.Activate(this, "Enemy", "Enemy");
		return enemy;
	}
	/// <summary>
	/// エネミー用機体を作成(文字列から)
	/// </summary>
	public Enemy InstantiateEnemy(string componentName, ToolBox.ShipData shipData) {
		//色
		Color c = enemyColor;
		//機体を生成
		Ship ship = InstantiateShip(shipData, c);
		//エネミー用コンポーネントをアタッチする
		Enemy enemy = (Enemy)ship.gameObject.AddComponent(Type.GetType(componentName));
		//有効化
		enemy.Activate(this, "Enemy", "Enemy");
		return enemy;
	}
	/// <summary>
	/// メッシュデータから立体をコルーチンで作成
	/// </summary>
	public void StartMeshCreating(Mesh mesh, MeshFilter mf,
		GameObject iventRecever = null, string iventName = "OnMeshCreated", float time = 2f) {
		StartCoroutine(FuncBox.MeshCreatingCoroutine(mesh,
			mf, shipMaterial,iventRecever, iventName, time));
	}
	/// <summary>
	/// メッシュ作成コルーチンを停止
	/// </summary>
	public void StopMeshCreating() {
		StopAllCoroutines();
	}
	/// <summary>
	/// サイズ名を指定して機体サイズ情報を取得
	/// </summary>
	public ToolBox.ShipSize GetShipSize(string sizeName) {
		ToolBox.ShipSize size = null;
		foreach(ToolBox.ShipSize s in shipSizeList) {
			if(sizeName.Equals(s.name)) {
				size = s;
				break;
			}
		}
		return size;
	}
	/// <summary>
	/// 機体パーツリストから機体データを作成する(辞書に登録はしない)
	/// </summary>
	public ToolBox.ShipData CreateShipData(List<ToolBox.ShipPartsData > partsList, string name) {	
		ToolBox.ShipData shipData = new ToolBox.ShipData(partsList);
		//カテゴリー
		shipData.category = defaultShipCategory;
		//一意なIDを取得する
		if(!shipDataDic.ContainsKey(shipData.category)) {
			shipDataDic.Add(shipData.category, new Dictionary<string,ToolBox.ShipData>());
		}
		string guid;
		do {
			 guid = Guid.NewGuid().ToString();
		} while(shipDataDic[shipData.category].ContainsKey(guid));
		shipData.id = guid;
		//名前
		shipData.name = name;
		//機体サイズ
		shipData.shipSize = editShipSize.name;
		//日時の設定
		shipData.created = shipData.updated = DateTime.Now.ToString();
		shipData.updateNum = 1;
		//フラグ設定
		shipData.flagNew = true;
		shipData.flagUpdate = false;
		return shipData;
	}
	/// <summary>
	/// 編集機体データを取得(ディープコピーせずに)
	/// </summary>
	public ToolBox.ShipData GetEditShipData() {
		return editShipData;
	}
	/// <summary>
	/// 編集機体データを設定(ディープコピーせずに)
	/// </summary>
	public void SetEditShipData(ToolBox.ShipData shipData) {
		editShipData = shipData;
	}
	/// <summary>
	/// プレイヤーの選択した機体データを取得
	/// </summary>
	public ToolBox.ShipData GetPlayerSelectShipData(int index) {
		if(playerSelectShips == null) return null;
		if(index < 0 && playerSelectShips.Length <= index) return null;
		return playerSelectShips[index];
	}
	/// <summary>
	/// プレイヤーの選択機体データを設定
	/// </summary>
	public void SetPlayerSelectShipData(ToolBox.ShipData[] selectShips) {
		playerSelectShips = selectShips;
	}
#endregion
#region 機体辞書関連
	/// <summary>
	/// カテゴリー毎の機体辞書を取得する
	/// </summary>
	public Dictionary<string, ToolBox.ShipData> ShipDic_Select(string category) {
		if (shipDataDic.ContainsKey(category)) {
			return shipDataDic[category];
		} else {
			return null;
		}
	}
	/// <summary>
	/// 指定カテゴリーから指定したIDの機体データを取得する
	/// </summary>
	public ToolBox.ShipData ShipDic_Select(string category, string id) {
		//包括確認
		if(!shipDataDic.ContainsKey(category)) return null;
		//カテゴリーの辞書を取得
		var categoryDic = ShipDic_Select(category);
		//辞書にidが含まれるか確認
		if (!categoryDic.ContainsKey(id)) {
			return null;
		}
		//機体を返す
		return categoryDic[id];
	}
	/// <summary>
	/// 指定したカテゴリーの内容をソートしたリストで出力
	/// </summary>
	public List<ToolBox.ShipData> ShipDic_Sort(string category) {
		//とりあえず作成順にして出力
		if (!shipDataDic.ContainsKey(category)) {
			return null;
		}
		List<ToolBox.ShipData> shipDataList = new List<ToolBox.ShipData>(shipDataDic[category].Values);
		//リストの内容を全てコピー
		shipDataList = new List<ToolBox.ShipData>(shipDataList.Select(elem => elem.DeepCopy()));
		//ソート
		shipDataList.Sort(
			(a, b) => {
				return String.Compare(a.updated, b.updated);
			}
		);
		return shipDataList;
	}
	/// <summary>
	/// 指定したカテゴリーに新規に機体を保存する
	/// </summary>
	public bool ShipDic_SaveAs(string category, ToolBox.ShipData ship) {
		//カテゴリーが存在するか
		if (!shipDataDic.ContainsKey(category)) {
			//新しくカテゴリーを作成
			shipDataDic.Add(category, new Dictionary<string,ToolBox.ShipData>());
		}

		//一意なIDを取得する
		string guid;
		do {
			 guid = Guid.NewGuid().ToString();
		} while(shipDataDic[category].ContainsKey(guid));
		
		//パラメータを設定
		ship.id = guid;
		ship.category = category;

		//フラグ設定
		ship.flagNew = true;
		ship.flagUpdate = false;

		//日時の設定
		ship.created = ship.updated = DateTime.Now.ToString();
		ship.updateNum = 1;

		//辞書に追加
		shipDataDic[category].Add(guid, ship);

		return true;
	}
	/// <summary>
	/// 指定したカテゴリーの指定データを更新する
	/// </summary>
	public bool ShipDic_Update(ToolBox.ShipData ship) {
		//カテゴリー確認
		if (!shipDataDic.ContainsKey(ship.category)) {
			return false;
		}
		//機体が含まれているかを確認
		if (!shipDataDic[ship.category].ContainsKey(ship.id)) {
			return false;
		}

		//フラグ
		ship.flagNew = false;
		ship.flagUpdate = true;

		//日時の設定
		ship.updated = DateTime.Now.ToString();
		ship.updateNum++;

		//上書き
		shipDataDic[ship.category][ship.id] = ship;
		return true;
	} 
	/// <summary>
	/// いくつかのカテゴリーからランダムに一つ機体データを選択
	/// </summary>
	public ToolBox.ShipData ShipDic_RandomSelect() {
		List<string> stringList = new List<string>(shipDataDic.Keys);
		string category = stringList[UnityEngine.Random.Range(0, stringList.Count)];
		stringList = new List<string>(shipDataDic[category].Keys);
		string shipID = stringList[UnityEngine.Random.Range(0, stringList.Count)];
		return shipDataDic[category][shipID];
	}
#endregion
#region エフェクト関連
	/// <summary>
	/// 名前を指定してエフェクトを取得する
	/// </summary>
	public GameObject GetEffect(string effectName) {
		//範囲確認
		if(!effectDic.ContainsKey(effectName)) return null;
		return effectDic[effectName];
	}
#endregion
#region ランキング関連
	/// <summary>
	/// 新しいランキングレコードを追加する
	/// </summary>
	public int AddRankingRecord(ToolBox.RankingRecord record) {
		if(_rankingRecords.Count < 1) {
			_rankingRecords.Add(record);
			return 1;
		} else {
			for(int i = 0; i < _rankingRecords.Count; i++) {
				if(_rankingRecords[i].wave <= record.wave) {
					_rankingRecords.Insert(i, record);
					return i + 1;
				}
			}
		}
		return 0;
	}
	/// <summary>
	/// 指定したデータ数だけウェーブ情報を文字列で返す
	/// </summary>
	public string Ranking_Wave(int num) {
		StringBuilder sb = new StringBuilder();
		for(int i = 0; i < num; i++) {
			if(_rankingRecords.Count > i) {
				sb.AppendLine(_rankingRecords[i].wave.ToString());
			} else {
				sb.AppendLine("-");
			}
		}
		return sb.ToString();
	}
	/// <summary>
	/// 指定したデータ数だけウプレイヤー情報を文字列で返す
	/// </summary>
	public string Ranking_Player(int num) {
		StringBuilder sb = new StringBuilder();
		for(int i = 0; i < num; i++) {
			if(_rankingRecords.Count > i) {
				Debug.Log(_rankingRecords[i].player);
				sb.AppendLine(_rankingRecords[i].player.ToString());
			} else {
				sb.AppendLine("-");
			}
		}
		return sb.ToString();
	}
	/// <summary>
	/// 指定したデータ数だけ日付情報を文字列で返す
	/// </summary>
	public string Ranking_Date(int num) {
		StringBuilder sb = new StringBuilder();
		for(int i = 0; i < num; i++) {
			if(_rankingRecords.Count > i) {
				sb.AppendLine(_rankingRecords[i].date.ToString());
			} else {
				sb.AppendLine("-");
			}
		}
		return sb.ToString();
	}
#endregion
#region IO関数
	/// <summary>
	/// 弾情報を読み込む
	/// </summary>
	protected Dictionary<string, Dictionary<string, ToolBox.Bullet>> ReadBullet(TextReader reader) {
		var dic = new Dictionary<string, Dictionary<string, ToolBox.Bullet>>();

		string line = "";
		string[] split;

		int i = 0;
		string category, name, caption, atlasName, path;
		GameObject instance;
		ToolBox.LauncherPerformance  per;

		while((line = reader.ReadLine()) != null) {
			i = 0;
			split = line.Split(',');
			//名前ID
			name = split[i];	i++;
			//カテゴリー
			category = split[i];	i++;
			//説明文
			caption = split[i];	i++;
			//改行チェック
			caption = caption.Replace("[改行]", "\n");
			//アトラス名
			atlasName = split[i];	i++;
			//インスタンス
			path = split[i];		i++;
			instance = (GameObject)Instantiate(Resources.Load(path));
			if(instance != null) {
				instance.transform.parent = transform;
				instance.name = name;
				instance.SetActive(false);
				//性能
				per = new ToolBox.LauncherPerformance();
				per.SetReadString(split, i);
				//カテゴリー登録の確認
				if(!dic.ContainsKey(category)) {
					Debug.Log("追加 : " + category);
					dic.Add(category, new Dictionary<string,ToolBox.Bullet>());
				}
				//追加
				dic[category].Add(name, new ToolBox.Bullet(category, name, caption, atlasName, instance, per));			
			}
		}
		return dic;
	}
	/// <summary>
	/// ステージ情報を読み込む
	/// </summary>
	protected Dictionary<string, List<ToolBox.Stage>> ReadStageDic(string folderPath) {
		if(!Directory.Exists(folderPath)) {
			return null;
		}
		Dictionary<string, List<ToolBox.Stage>> dic = new Dictionary<string, List<ToolBox.Stage>>();
		List<ToolBox.Stage> list;
		string category = "";
		string[] folderList;
		folderList = Directory.GetDirectories(folderPath);
		for(int i = 0; i < folderList.Length; i++) {
			category = Path.GetFileName(folderList[i]);
			folderList[i] = folderPath + "/" + category;
			list = ReadStageList(folderList[i]);
			//読み込み
			dic.Add(category, list);
		}
		return dic;
	}
	protected List<ToolBox.Stage> ReadStageList(string folderPath) {
		if(!Directory.Exists(folderPath)) {
			return null;
		}
		//読み込み
		var list = new List<ToolBox.Stage>();
		string[] fileList;	//folderPath以下にあるファイルリスト(メタデータは抜く)
		string fileName;
		fileList = Directory.GetFiles(folderPath);
		for(int i = 0; i< fileList.Length; i++) {
			fileName = Path.GetFileName(fileList[i]);
			//拡張子が3つ = メタデータ
			if(fileName.Split('.').Length != 3) {
				fileList[i] = folderPath + "/" + fileName;
				Debug.Log(folderPath + "/" + fileName);
				list.Add(ToolBox.Stage.Read(FuncBox.GetTextReader(fileList[i])));
			}
		}
		return list;
	}
	/// <summary>
	/// 機体情報を読み込む
	/// </summary>
	protected Dictionary<string, Dictionary<string, ToolBox.ShipData>> ReadShipDic(string folderPath) {
		string[] folderList;	//機体データルート以下にあるフォルダリスト
		if(!Directory.Exists(folderPath)) {
			return null;
		}
		//読み込み
		var dic = new Dictionary<string, Dictionary<string, ToolBox.ShipData>>();
		string category = "";
		folderList = Directory.GetDirectories(folderPath);
		for(int i = 0; i < folderList.Length; i++) {
			category = Path.GetFileName(folderList[i]);
			folderList[i] = folderPath + "/" + category;
			Debug.Log("カテゴリー : " + category);
			//読み込み
			dic.Add(category, ReadShipDic(folderList[i], category));
		}
		return dic;
	}
	protected Dictionary<string, ToolBox.ShipData> ReadShipDic(string folderPath, string category) {
		string[] folderList;	//機体データルート以下にあるフォルダリスト
		if(!Directory.Exists(folderPath)) {
			return null;
		}

		//読み込み
		var dic = new Dictionary<string, ToolBox.ShipData>();
		ToolBox.ShipData ship;
		folderList = Directory.GetDirectories(folderPath);

		for(int i = 0; i < folderList.Length; i++) {
			folderList[i] = folderPath + "/" + Path.GetFileName(folderList[i]);
			//読み込み&追加
			ship = ToolBox.ShipData.Read(folderList[i]);
			if (ship != null) {
				//カテゴリー
				ship.category = category;
				//キー: ID, 値 : 機体データ
				dic.Add(ship.id, ship);
				Debug.Log("機体 : " + ship.name + " : 読み込み");
			}
		}

		return dic;
	}
	/// <summary>
	/// 機体情報を書き込む
	/// </summary>
	public void WriteShipDic(string folderPath) {
		//新規、更新フラグの立っているもののみを処理する
		foreach (string category in shipDataDic.Keys) {
			foreach(ToolBox.ShipData shipData in shipDataDic[category].Values) {
				if (shipData.flagNew) {
					//新規

					//保存
					shipData.Write(folderPath + "/" + category);
					//フラグを下す
					shipData.flagNew = false;
				} else if (shipData.flagUpdate) {
					//更新

					//フォルダを一旦削除してから
					string path = folderPath + "/" + category + "/" + shipData.id;
					if (Directory.Exists(path)) {
						Directory.Delete(path, true);
					}
					//保存
					shipData.Write(folderPath + "/" + category);
					//フラグを下す
					shipData.flagUpdate = false;
				}
			}
		}
	}
	/// <summary>
	/// リソース読み込んで辞書で返す
	/// </summary>
	public Dictionary<string, GameObject> ReadResourcesDic(List<string> resourcesPaths) {
		Dictionary<string, GameObject> dic = new Dictionary<string,GameObject>();
		GameObject g;
		for(int i = 0; i < resourcesPaths.Count; i++) {
			g = Resources.Load<GameObject>(resourcesPaths[i]);
			dic.Add(g.name, g);
		}
		return dic;
	}
	/// <summary>
	/// ランキングレコードを書き込む
	/// </summary>
	public void WriteRankingRecords(string filePath) {
		using(TextWriter w = FuncBox.GetTextWriter(filePath)) {
			for(int i = 0; i < _rankingRecords.Count; i++) {
				w.WriteLine(_rankingRecords[i].GetWriteString());
			}
		}
	}
	/// <summary>
	/// ランキングレコードを読み込み
	/// </summary>
	public void ReadRankingRecords(string filePath) {
		_rankingRecords = new List<ToolBox.RankingRecord>();
		if(!File.Exists(filePath)) return;
		//読み込み
		using(TextReader r = FuncBox.GetTextReader(filePath)) {
			string line = "";
			string[] split;
			while((line = r.ReadLine()) != null) {
				split = line.Split(',');
				_rankingRecords.Add(ToolBox.RankingRecord.ReadString(split));
			}
		}
	}
	/// <summary>
	/// コンフィグファイルを読み込む
	/// </summary>
	public void ReadConfig(string filePath) {
		if(!File.Exists(filePath)) return;
		//読み込み
		using(TextReader r = FuncBox.GetTextReader(filePath)) {
			string line = "";
			while((line = r.ReadLine()) != null) {
				switch (line) {
					case "<BattleTime>":
						line = r.ReadLine();
						battleTime = int.Parse(line);
					break;
					case "<StageScale>":
						line = r.ReadLine();
						stageScale = float.Parse(line);
					break;
					case "<GameSpeed>":
						line = r.ReadLine();
						gameSpeed = float.Parse(line);
					break;
					case "<PlayerHPScale>":
						line = r.ReadLine();
						playerHPScale = float.Parse(line);
					break;
					case "<MaxEnergy>":
						line = r.ReadLine();
						maxEnergy = int.Parse(line);
					break;
					case "<EnergyOutput>":
						line = r.ReadLine();
						energyOutput = int.Parse(line);
					break;
				}
			}
		}
	}
	/// <summary>
	/// コンフィグファイルを書き込み
	/// </summary>
	public void WriteConfig(string filePath) {
		using(TextWriter w = FuncBox.GetTextWriter(filePath)) {
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("<BattleTime>");
			sb.AppendLine(battleTime.ToString());
			sb.AppendLine("<StageScale>");
			sb.AppendLine(stageScale.ToString());
			sb.AppendLine("<GameSpeed>");
			sb.AppendLine(gameSpeed.ToString());
			sb.AppendLine("<PlayerHPScale>");
			sb.AppendLine(playerHPScale.ToString());
			sb.AppendLine("<MaxEnergy>");
			sb.AppendLine(maxEnergy.ToString());
			sb.AppendLine("<EnergyOutput>");
			sb.AppendLine(energyOutput.ToString());
			//書き込み
			w.Write(sb.ToString());
		}
	}
#endregion
}