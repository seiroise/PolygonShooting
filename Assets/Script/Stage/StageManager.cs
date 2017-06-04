using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// ステージの管理
/// </summary>
public class StageManager : SingletonMonoBehaviour<StageManager> {
	//管理クラス
	protected GameManager gm;
	protected StageGUIManager gui;
	private AudioManager am;
	[Header("ステージ")]
	public GameObject stageParent;
	private float scale;
	[Header("背景")]
	public GameObject backgroundParent;
	public MeshRenderer backgroundRenderer;
	private Color targetColor;
	[Header("プレイヤー関連")]
	public float playerRebornTime = 2f;		//プレイヤーの再生成時間
	private ToolBox.PlayerScore[] playerScores;	//プレイヤーのスコア
	private int playerNum = 0;				//参加「プレイヤー数
	private int breakPlayerNum = 0;			//戦闘不能プレイヤーの数
	//エネミー関連
	private EnemySpawner enemySpawner;
	[Header("カメラ")]
	public TargetAreaCamera mainCamera;
	[Header("エフェクト")]
	public ParticleSystem destroyEffect;
	[Header("アイテム")]
	public GameObject[] dropItemList;
	public GameObject lifeItem;
	public GameObject bigLifeItm;
	[Header("ウェーブ")]
	public WaveManager waveManager;		//ウェーブの敵管理
	private int totalWaveNum = 1;			//総ウェーブ数
	private int waveNum = 1;				//読み込むウェーブ数
	private int waveLoopNum = 0;			//ウェーブのループ回数
	private bool flagIndicateComment	 = false;	//コメントを表示しているか
	private int enemyNum;					//生成される敵の数
	//データベース
	protected class StageObjectInfo {
		//メンバ
		public Pilot pilot;				//ステージオブジェクトの操縦者
		public List<Pilot> thisLockList;	//自身(this)をロックしている操縦者リスト
		public LinkedListNode<StageObjectInfo> node;	//双方向リストの格納ノード
		//コンストラクタ
		public StageObjectInfo(Pilot pilot) {
			this.pilot = pilot;
			thisLockList = new List<Pilot>();
			node = null;
		}
	}
	protected LinkedList<StageObjectInfo> stageObjectLinkedList;	//ロック用の双方向リスト
	protected Dictionary<Pilot, StageObjectInfo> stageObjectDic;	//ステージオブジェクト全体の辞書
	protected Dictionary<Player, StageObjectInfo> playerDic;		//プレイヤー辞書
	protected Dictionary<Enemy, StageObjectInfo> enemyDic;		//エネミー辞書
	//フラグ
	private bool flagGameTimerEnd = false;
#region MonoBehaviourイベント
	protected override void Awake() {
		base.Awake();
		//初期化
		enemySpawner = gameObject.AddComponent<EnemySpawner>();
		playerScores = new ToolBox.PlayerScore[4];
		stageObjectLinkedList = new LinkedList<StageObjectInfo>();
		stageObjectDic = new Dictionary<Pilot,StageObjectInfo>();
		playerDic = new Dictionary<Player,StageObjectInfo>();
		enemyDic = new Dictionary<Enemy,StageObjectInfo>();
	}
	protected void Start() {
		gm = GameManager.Instance;
		gui = StageGUIManager.Instance;
		am = AudioManager.Instance;
		//ステージサイズ
		scale = gm.stageScale;
		//カメラ
		mainCamera.minSize = scale * 0.5f;
		//ステージ作成
		InstantiateStage();
		//プレイヤー初期化
		PlayerInit();
		//諸々の初期化
		Init();
		//BGM再生
		gm.PlayStageBGM();
	}
	protected void Update() {
		if(Input.GetKeyDown(KeyCode.Escape)) {
			ReturnLevel();
		}
		if(gm.playMode != ToolBox.PlayMode.Test) {
			//背景の色をスコアが一位の人色に変える
			ChangeBackgroundColor();
		}
	}
#endregion
#region 初期化
	/// <summary>
	/// 初期化
	/// </summary>
	protected void Init() {
		if(gm.playMode != ToolBox.PlayMode.Test) {
			//ステージオブジェクトの操作フラグをオフに
				StageObjectControll(false);
				//プレイヤーUIの非表示
				gui.IndicatePlayerUI(false);
				//ゲーム開始タイマーの開始
				gui.PlayGameStartTimer();
		}
		switch(gm.playMode) {
			case ToolBox.PlayMode.VsEnemy:
			break;
			case ToolBox.PlayMode.Test:
				//ゲーム開始タイマーの停止
				gui.gameStartTimerAnimation.speed = 0f;
			break;
		}
	}
	/// <summary>
	/// プレイヤーの初期化
	/// </summary>
	protected void PlayerInit() {
		switch(gm.playMode) {
		case ToolBox.PlayMode.Test:
			if(gm.GetEditShipData() == null) return;
			//パッドの刺さっている数だけ
			playerNum = Input.GetJoystickNames().Length;
			if(playerNum <= 0) playerNum = 1;
			//プレイヤーの作成
			Player[] players = new Player[playerNum];
			for(int i = 0; i < playerNum; i++) {
				players[i] = InstantiatePlayer(gm.GetEditShipData(), i + 1);
			}
			//プレイヤーUIの有効化
			gui.ActivatePlayerUI(players);
			//プレイヤー数
			
			break;
		case ToolBox.PlayMode.Battle:
		case ToolBox.PlayMode.VsEnemy:
			List<Player> playerList = new List<Player>();
			ToolBox.ShipData shipData;
			Player player;
			for(int i = 0; i < 4; i++) {
				shipData = gm.GetPlayerSelectShipData(i);
				if(shipData != null) {
					player = InstantiatePlayer(shipData, i + 1);
					//リストに追加
					playerList.Add(player);
					//スコア
					playerScores[i] = new ToolBox.PlayerScore();
					playerScores[i].playerNo = i;
					//カメラの対象に追加
					mainCamera.targets.Add(player.gameObject);
				}
			}
			//プレイヤーUIの有効化
			gui.ActivatePlayerUI(playerList.ToArray());
			//プレイヤー数の設定
			playerNum = playerList.Count;
			break;
		}
	}
#endregion
#region 関数
	/// <summary>
	/// ステージ生成
	/// </summary>
	private void InstantiateStage() {
		if(gm.selectStage == null) {
			gm.selectStage = gm.stageDic["Simple"][0];
		}
		//ステージと背景のサイズ
		SetStageScale(scale);
		GameObject stage;
		List<KeyValuePair<GameObject, Color>> stageObjectList = gm.selectStage.stageObjectList;
		for(int i = 0; i < stageObjectList.Count; i++) {
			stage = (GameObject)Instantiate(stageObjectList[i].Key);
			stage.name = gm.selectStage.name;
			stage.transform.parent = stageParent.transform;
			stage.transform.localEulerAngles = Vector3.zero;
			stage.transform.localPosition = Vector3.zero;
			stage.transform.localScale = Vector3.one;			
			//色
			MeshRenderer mr = stage.GetComponent<MeshRenderer>();
			mr.material.SetColor("_Color", stageObjectList[i].Value);
			//タグ
			stage.tag = "Stage";
		}
	}
	/// <summary>
	/// ステージのサイズを変更する
	/// </summary>
	private void SetStageScale(float scale) {
		Vector3 scaleVec = Vector3.one * scale;
		scaleVec.y = 10f;
		stageParent.transform.localScale = scaleVec;
		scaleVec.y = scale;
		scaleVec.z = 1f;
		backgroundParent.transform.localScale = scaleVec;
	}
	/// <summary>
	/// プレイヤーを生成
	/// </summary>
	private Player InstantiatePlayer(ToolBox.ShipData shipData, int playerNo) {
		Player p = gm.InstantiatePlayer(shipData, playerNo);
		//VsEnemyの場合はnameの調整
		if(gm.playMode == ToolBox.PlayMode.VsEnemy) {
			p.ship.SetTagAndName("Player", "Player", "Player");
			p.notCollisionName = "Player";
		}
		//位置調整
		if(gm.selectStage != null) {
			p.transform.position = gm.selectStage.playerRespawnPos[playerNo - 1] * scale;
		}
		//データベースに追加
		AddPlayer(p);
		return p;
	}
	/// <summary>
	/// ステージオブジェクトの操作フラグを一括でオンオフ
	/// </summary>
	public void StageObjectControll(bool flag) {
		foreach(Pilot p in stageObjectDic.Keys) {
			p.flagControl = flag;
		}
	}
	/// <summary>
	/// 前のシーンに戻る
	/// </summary>
	public void ReturnLevel() {
		switch(gm.playMode) {
			case ToolBox.PlayMode.Test:
				gm.LoadLevel("ShipEditor");
			break;
			case ToolBox.PlayMode.Battle:
			case ToolBox.PlayMode.VsEnemy:
				gm.LoadLevel("ShipSelect");
			break;
		}
	}
	/// <summary>
	/// 死亡エフェクト
	/// </summary>
	public void InstantiateDestroyEffect(Ship ship) {
		Vector3 pos = ship.transform.position;
		GameObject g = (GameObject)Instantiate(destroyEffect.gameObject, pos, Quaternion.identity);
		ParticleSystem p = g.GetComponent<ParticleSystem>();
		p.startColor = ship.symbolColor;
		p.startSize = ship.collisionRadius;
		g.AddComponent<ParticleEndWithDestroy>();
	}
	/// <summary>
	/// 背景の色を変更する
	/// </summary>
	public void ChangeBackgroundColor() {
		gui.SetPlayerScore(playerScores);
		int maxScore = 0;
		int playerNo = -1;
		for(int i = 0; i < playerScores.Length; i++) {
			if(playerScores[i] != null) {
				if(playerScores[i].score > maxScore) {
					maxScore = playerScores[i].score;
					playerNo = playerScores[i].playerNo;
				}
			}
		}
		if(playerNo == -1) return;
		Color fromColor = backgroundRenderer.material.GetColor("_Color");
		targetColor = gm.playerColor[playerNo];
		targetColor.a = fromColor.a;
		backgroundRenderer.material.SetColor("_Color", Color.Lerp(fromColor, targetColor, 0.5f));
	}
	/// <summary>
	/// アイテムをランダムに複数生成する
	/// </summary>
	public void InstantiateRandomItem(Vector3 pos, int num) {
		float angle = 360f / (float)num;
		for(int i = 0; i < num; i++) {
			int randNum = Random.Range(0, dropItemList.Length);
			GameObject g = Instantiate(dropItemList[randNum]);
			g.transform.parent = transform;
			g.transform.position = pos;
			Rigidbody r = g.GetComponent<Rigidbody>();
			r.AddForce(FuncBox.DegreeToVector3(angle * i) * 1000f);
		}
	}
	/// <summary>
	/// 回復アイテムをランダムに複数生成する
	/// </summary>
	public void InstantiateRandomLifeItem(Vector3 pos, int num) {
		float angle = 360f / (float)num;
		GameObject inst;
		for(int i = 0; i < num; i++) {
			if(Random.Range(0, 20) > 1) {
				inst = lifeItem;
			} else {
				inst = bigLifeItm;
			}
			GameObject g = Instantiate(lifeItem);
			g.transform.parent = transform;
			g.transform.position = pos;
			//Rigidbody r = g.GetComponent<Rigidbody>();
			//r.AddForce(FuncBox.DegreeToVector3(angle * i) * 750f);
		}
	}
	/// <summary>
	/// プレイヤー復活
	/// </summary>
	public void PlayerReborn(Player p) {
		Vector3 resPos = gm.selectStage.playerRespawnPos[(int)p.playerNo - 1] * scale;
		p.Reborn(resPos);
		//データベースに再追加
		AddPlayer(p);
	}
#endregion
#region データベース関連
	/// <summary>
	/// データベースにステージオブジェクトを追加
	/// </summary>
	protected StageObjectInfo AddStageObject(Pilot pilot) {
		StageObjectInfo sInfo = new StageObjectInfo(pilot);
		//追加
		sInfo.node = stageObjectLinkedList.AddFirst(sInfo);
		stageObjectDic.Add(pilot, sInfo);
		return sInfo;
	}
	/// <summary>
	/// データベースからステージオブジェクトを削除
	/// </summary>
	protected void RemoveStageObject(Pilot pilot) {
		if(!stageObjectDic.ContainsKey(pilot)) return;
		StageObjectInfo sInfo = stageObjectDic[pilot];
		//削除対象をロックしているオブジェクトがいればそれらのロック状態を解除する
		if(sInfo.thisLockList.Count > 0) {
			for(int i = 0; i < sInfo.thisLockList.Count; i++) {
				sInfo.thisLockList[i].LockClear();
			}
			sInfo.thisLockList.Clear();
		}
		//削除
		stageObjectLinkedList.Remove(sInfo.node);
		stageObjectDic.Remove(pilot);
		//カメラの対象から削除
		mainCamera.targets.Remove(pilot.gameObject);
	}
	/// <summary>
	/// データベースにプレイヤーを追加
	/// </summary>
	protected void AddPlayer(Player player) {
		StageObjectInfo sInfo = AddStageObject(player);
		playerDic.Add(player, sInfo);
		//カメラの対象に追加
		mainCamera.targets.Add(player.gameObject);
	}
	/// <summary>
	/// データベースからプレイヤーを削除
	/// </summary>
	protected void RemovePlayer(Player player) {
		playerDic.Remove(player);
		RemoveStageObject(player);
		//カメラの対象から削除
		mainCamera.targets.Remove(player.gameObject);
	}
	/// <summary>
	/// データベースにエネミーを追加
	/// </summary>
	protected void AddEnemy(Enemy enemy) {
		StageObjectInfo sInfo = AddStageObject(enemy);
		enemyDic.Add(enemy, sInfo);
	}
	/// <summary>
	/// データベースからエネミーを削除
	/// </summary>
	protected void RemoveEnemy(Enemy enemy) {
		enemyDic.Remove(enemy);
		RemoveStageObject(enemy);
	}
	/// <summary>
	/// 次にロックするオブジェクトを取得する
	/// </summary>
	public Pilot GetNextLockObject(Pilot me, Pilot nowLockObject) {
		if(stageObjectLinkedList.Count <= 1) return null;
		//辞書確認
		if(!stageObjectDic.ContainsKey(nowLockObject)) return null;
		//現在のロック対象のノードを取得
		StageObjectInfo sInfo = stageObjectDic[nowLockObject];
		LinkedListNode<StageObjectInfo> node = sInfo.node;
		//ロック対象リストから削除
		sInfo.thisLockList.Remove(me);
		//次のノードを取得(nodeに格納)
		if(node== stageObjectLinkedList.Last) {
			node = stageObjectLinkedList.First;
		} else {
			node = node.Next;
		}
		//ロックリストに追加
		sInfo = node.Value;
		sInfo.thisLockList.Add(me);
		return sInfo.pilot;
	}
	/// <summary>
	/// ロック処理
	/// </summary>
	public Pilot Lock(Pilot me, Pilot target) {
		if(stageObjectLinkedList.Count <= 1) return null;
		//現在のロック対象への処理
		StageObjectInfo sInfo;
		if(me.lockObject) {
			sInfo = stageObjectDic[me.lockObject];
			//ロック対象リストから削除
			sInfo.thisLockList.Remove(me);
		}
		//次のロック対象への処理
		sInfo = stageObjectDic[target];
		//ロック対象リストに追加
		sInfo.thisLockList.Add(me);
		return target;
	}
	/// <summary>
	/// 一番近くのプレイヤーをロックする
	/// </summary>
	public Player GetNearPlayer(Pilot me) {
		if( playerDic.Count < 1) return null;
		float distance = float.MaxValue;
		float measureDistance;
		Player nearPlayer = null;
		Vector3 pos = me.transform.position;
		foreach(Player p in playerDic.Keys) {
			measureDistance = Vector3.Distance(pos, p.transform.position);
			if(distance > measureDistance) {
				nearPlayer = p;
				distance = measureDistance;
			}
		}
		//ロック処理
		Lock(me, nearPlayer);
		return nearPlayer;
	}
	/// <summary>
	/// ランダムにプレイヤーをロックする
	/// </summary>
	public Pilot GetRandomPlayer(Pilot me) {
		if(playerDic.Count < 1) return null;
		List<Player> pList = new List<Player>(playerDic.Keys);
		Player p = pList[Random.Range(0, pList.Count)];
		return  Lock(me, p);
	}
	/// <summary>
	/// 一番近くのエネミーをロックする
	/// </summary>
	public Enemy GetNearEnemy(Pilot me) {
		if(enemyDic.Count < 1) return null;
		float distance = float.MaxValue;
		float measureDistance;
		Enemy nearEnemy = null;
		Vector3 pos = me.transform.position;
		foreach(Enemy e in enemyDic.Keys) {
			measureDistance = Vector3.Distance(pos, e.transform.position);
			if(distance > measureDistance) {
				nearEnemy = e;
				distance = measureDistance;
			}
		}
		//ロック処理
		Lock(me, nearEnemy);
		return nearEnemy;
	}
#endregion
#region ステージオブジェクトイベント
	/// <summary>
	/// ステージオブジェクトのHPが0になったときに呼ばれる
	/// </summary>
	public void OnStageObjectBreak(Pilot me, Pilot rival) {
		//エフェクト生成
		InstantiateDestroyEffect(me.ship);
		//SE
		am.PlaySE("Destroy_1", 0, true);
		//データベースから削除
		switch(me.tag) {
		case "Player":
			RemovePlayer((Player)me);
			break;
		case "Enemy":
			RemoveEnemy((Enemy)me);
			me.ship.DestroyShip();
			if(gm.playMode == ToolBox.PlayMode.VsEnemy) {
				enemyNum--;
			}
			//アイテムをドロップ
			int num = ((Enemy)me).dropItemNum;
			InstantiateRandomItem(me.transform.position, num);
			InstantiateRandomLifeItem(me.transform.position, 1);
			break;
		}
		//スコア
		switch(gm.playMode) {
			case ToolBox.PlayMode.Battle:
				if(me.tag == "Player" && rival.tag == "Player") {
					//スコアの受け渡し(player対playerの場合)
					Player meP = (Player)me;
					//meのスコアを半分に
					int score = meP.indicator.scoreLabel.GetTargetNum();
					score /= 2;
					meP.indicator.scoreLabel.SetInt(score);
					//rivaleにそのスコアを与える
					Player rivalP = (Player)rival;
					rivalP.indicator.scoreLabel.AddInt(score);
					//killとdeadの増加
					playerScores[((int)meP.playerNo) - 1].dead++;
					playerScores[((int)rivalP.playerNo) - 1].kill++;
				}
			break;
			case ToolBox.PlayMode.VsEnemy:
				if(me.tag == "Enemy" && rival.tag == "Player") {
					//playerのkill数を増加させる
					Player rivalP = (Player)rival;
					rivalP.indicator.scoreLabel.AddInt(1);
					playerScores[((int)rivalP.playerNo) - 1].kill++;
				}
			break;
		}
	}
	/// <summary>
	/// プレイヤーが破壊されたとき
	/// </summary>
	public void OnPlayerBreak(Player p) {
		//VsEnemyの場合は復活させない
		if(gm.playMode == ToolBox.PlayMode.VsEnemy) {
			//戦闘不能プレイヤー数
			breakPlayerNum++;
			if(playerNum <= breakPlayerNum) {
				//プレイヤーのスコア
				gui.SetPlayerScore(playerScores);
				gm.playerScores = playerScores;
				//新しいランキングレコードを追加
				ToolBox.RankingRecord record = new ToolBox.RankingRecord(0, waveNum, playerNum);
				gm.yourRank = gm.AddRankingRecord(record);
				//ゲーム終了
				gm.LoadLevel("Result_VsEnemy");
			}
		} else {
			//コルーチン
			StartCoroutine(Coroutine_PLayerReborn(p));
		}		
	}
#endregion
#region ゲームパッドイベント(GamepadInput_Message)
	protected void BackButtonDown() {
		if(!flagGameTimerEnd) {
			//前のシーンへ
			ReturnLevel();
		}
	}
	protected void StartButtonDown() {
		switch(gm.playMode) {
			case ToolBox.PlayMode.VsEnemy:
				if(flagIndicateComment) {
					//コメントを非表示に
					gui.HideComment();
					flagIndicateComment = false;
				}
			break;
		}
	}
#endregion
#region タイマーイベント
	/// <summary>
	/// ゲーム開始タイマーの終了
	/// </summary>
	private void OnGameStartTimerEnd() {
		//全てのオブジェクトの操作を可能に
		StageObjectControll(true);
		//プレイヤーUIの表示
		gui.IndicatePlayerUI(true);
		//プレイモード固有のUIを表示
		gui.IndicatePlayModeUI(gm.playMode);
		//プレイモード毎に実行
		switch(gm.playMode) {
			case ToolBox.PlayMode.Battle:
				//ゲームタイマーの開始
				gui.PlayBattleTimer(gm.battleTime);
			break;
			case ToolBox.PlayMode.VsEnemy:
				//ウェーブコルーチン
				StartCoroutine(Coroutine_Wave());
			break;
		}
	}
	/// <summary>
	/// ゲームタイマーの時間ごとイベント
	/// </summary>
	private void OnTimeEvent(float time) {
		if(time == 10) {
			gui.battleTimerAnimation.SetBool("Last10", true);	
		}
	}
	/// <summary>
	/// ゲームタイマーの終了
	/// </summary>
	private void OnGameTimerEnd() {
		//フラグ
		flagGameTimerEnd = true;
		//プレイヤーのスコア
		gui.SetPlayerScore(playerScores);
		gm.playerScores = playerScores;
		//プレイヤーUIを非表示に
		gui.IndicatePlayerUI(false);
		//TimeScaleを遅く
		Time.timeScale = 0.4f;
		//コルーチン
		StartCoroutine(Coroutine_GameSet());
	}
#endregion
#region コルーチン
	/// <summary>
	/// ゲームセットコルーチン
	/// </summary>
	private IEnumerator Coroutine_GameSet() {
		yield return new WaitForSeconds(0.7f);
		Time.timeScale = 1f;
		gm.LoadLevel("Result_Battle");
	}
	/// <summary>
	/// プレイヤー復活コルーチン
	/// </summary>
	private IEnumerator Coroutine_PLayerReborn(Player p) {
		yield return new WaitForSeconds(playerRebornTime);
		PlayerReborn(p);
	}
	/// <summary>
	/// ウェーブ管理用コルーチン
	/// </summary>
	private IEnumerator Coroutine_Wave() {
		while(true) {
			//少し休憩
			yield return new WaitForSeconds(4f);
			//UI変更
			gui.waveNumLabel.text = totalWaveNum.ToString();
			//ウェーブ開始のウェーブ数表示
			gui.SetWaveIndicator("Wave" + totalWaveNum);
			//敵を生成
			Enemy[] enemys = waveManager.InstantiateWaveEnemys(waveNum - 1, scale, waveLoopNum + 1, (waveLoopNum * 2.5f) + 1f);
			for(int i = 0; i < enemys.Length; i++) {
				AddEnemy(enemys[i]);
			}
			enemyNum = enemys.Length;
			//敵の全滅を確認するまでウェイト
			while(enemyNum > 0) {
				yield return 0;
			}
			Debug.Log("敵全滅!");
			//ウェーブ数増加
			totalWaveNum++;
			waveNum++;
			//ループの確認
			if(!waveManager.CheckWaveIndex(waveNum - 1)) {
				Debug.Log("ループ++");
				waveLoopNum++;
				waveNum = 1;
			}
			//一定数毎にステージを大きくする
			if(totalWaveNum % 5 == 0) {
				scale += 20f;
				SetStageScale(scale);
			}
		}
	}
#endregion
}