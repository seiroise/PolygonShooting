using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
/// <summary>
/// 音の管理
/// </summary>
public class AudioManager : SingletonMonoBehaviour<AudioManager> {
	//オーディオソース追加情報
	public class AudioSourceInfo {
		public AudioSource audio;
		public float startTime;		//なり始め
		public float length;			//長さ

		public AudioSourceInfo(AudioSource audio) {
			this.audio = audio;
			startTime = 0f;
			length = 0f;
		}
	}
	//BGMがフェードするのにかかる時間
	[SerializeField, Range(0, 20)]
	private float bgmFadeSpeed = 20f;
	//次に流すBGM,SE名
	protected string nextBGMName;
	//音量
	public float bgmVolume = 0.75f;
	public float seVolume = 0.75f;
	//BGMをフェードアウト中か
	protected bool flagFadeOut = false;
	//BGM.SE用のAudioSource
	protected AudioSource bgmSource;
	protected AudioSourceInfo[] seSources;
	public const int SE_SOURCE_NUM = 20;
	//全てのAudio
	protected Dictionary<string, AudioClip> bgmDic, seDic;
	//書き込みファイル
	protected string settingFile = "Audio.txt";
#region MonoBehaviourイベント
	protected override void Awake() {
		base.Awake();
		DontDestroyOnLoad(this.gameObject);
		//設定ファイル読み込み
		ReadSettingFile();

		//AudioSourceの追加
		bgmSource = gameObject.AddComponent<AudioSource>();
		bgmSource.loop = true;
		bgmSource.priority = 0;
		bgmSource.volume = bgmVolume;

		seSources = new AudioSourceInfo[SE_SOURCE_NUM];
		for(int i = 0; i < SE_SOURCE_NUM; i++) {
			AudioSource seSource = gameObject.AddComponent<AudioSource>();
			seSource.volume = seVolume;
			seSource.priority = 255;
			seSources[i] = new AudioSourceInfo(seSource);
		}
		//ResourcesフォルダからBGM,SEを取得。辞書に登録
		bgmDic = new Dictionary<string,AudioClip>();
		seDic = new Dictionary<string,AudioClip>();

		object[] bgmList = Resources.LoadAll("Audio/BGM");
		object[] seList = Resources.LoadAll("Audio/SE");

		foreach(AudioClip bgm in bgmList) {
			bgmDic.Add(bgm.name, bgm);
			Debug.Log("Add BGM " + bgm.name);
		}
		foreach(AudioClip se in seList) {
			seDic.Add(se.name, se);
			Debug.Log("Add SE " + se.name);
		}
	}
	protected void Update() {
		if(!flagFadeOut) return;
		bgmSource.volume -= Time.deltaTime * bgmFadeSpeed;
		if(bgmSource.volume <= 0f) {
			bgmSource.Stop();
			bgmSource.volume = bgmVolume;
			flagFadeOut = false;
			if(!string.IsNullOrEmpty(nextBGMName)) {
				PlayBGM(nextBGMName);
			}			
		}
	}
	protected void OnDestroy() {
		//設定ファイル書き込み
		WriteSettingFile();
	}
#endregion
#region 関数
	/// <summary>
	/// ファイル名を指定してBGMを再生する
	/// <para>priorityは優先度デフォルト0</para>
	/// </summary>
	public void PlayBGM(string bgmName, int priority = 0) {
		//辞書確認
		if(!bgmDic.ContainsKey(bgmName)) {
			return;
		}
		
		if(!bgmSource.isPlaying) {
			//BGMが流れていない場合はそのまま流す
			Debug.Log("PlayBGM : " + bgmName);
			nextBGMName = "";
			bgmSource.clip = bgmDic[bgmName];
			bgmSource.Play();
		} else if(bgmSource.clip.name != bgmName) {
			//違うBGMが流れている場合は流れているBGMをフェードアウトさせる
			Debug.Log("PlayBGM : " + bgmName);
			nextBGMName = bgmName;
			FadeOutBGM();
		} else {
			Debug.Log("同じBGMを再生しようとしました : " + bgmName);
		}
		bgmSource.priority = priority;
	}
	/// <summary>
	/// 現在流れている曲をフェードアウトさせる
	/// <para>fadeSpeedに指定した速度でフェードアウトする</para>
	/// </summary>
	public void FadeOutBGM() {
		flagFadeOut = true;
	}
	/// <summary>
	/// 再生中ならBGMを停止
	/// </summary>
	public void StopBGM() {
		if(!bgmSource.isPlaying) return;
		bgmSource.Stop();
	}
	/// <summary>
	/// 指定したファイル名のSEを流す。
	/// <para>priorityは優先度デフォルト255</para>
	/// </summary>
	public void PlaySE(string seName , int priority = 255, bool flagMostPriority = false) {
		if(!seDic.ContainsKey(seName)) {
			return;
		}
		//一番再生時間の長いsourceを見つけてそこで流す
		//再生してないsourceがあればそこで流す
		int i = 0, j = 0;
		float maxPlayPer = 0f, playPer = 0f;
		for(i = 0; i < seSources.Length; i++) {
			if(!seSources[i].audio.isPlaying) {
				j = i;
				break;
			} else {
				playPer  = (Time.time - seSources[i].startTime) / seSources[i].length;
				if(maxPlayPer < playPer) {
					maxPlayPer = playPer;
					j = i;
				}
			}
		}
		//同じ名前のSEが再生されている場合はpriorityを下げる
		for(i = 0; i < seSources.Length; i++) {
			if(seSources[i].audio.clip) {
				if(seSources[i].audio.clip.name == seName && seSources[i].audio.isPlaying) {
					seSources[i].audio.priority = (int)(priority * 2f);
				}
				if(flagMostPriority) {
					seSources[i].audio.priority = 255;
					seSources[i].audio.volume = seVolume * 0.25f;
				}
			}
		}
		i = j;
		seSources[i].audio.clip = seDic[seName];
		seSources[i].audio.priority = priority;
		if(flagMostPriority) {
			seSources[i].audio.volume = seVolume * 2f;
		} else {
			seSources[i].audio.volume = seVolume;
		}
		seSources[i].audio.Play();
		seSources[i].startTime = Time.time;
		seSources[i].length = seDic[seName].length;
	}
	/// <summary>
	/// BGMの音量を変更。0~1の範囲
	/// </summary>
	public void BGMVolumeChange(float volume) {
		bgmVolume = volume;
		bgmSource.volume = volume;
	}
	/// <summary>
	/// SEの音量を変更。0~1の範囲
	/// </summary>
	public void SEVolumeChange(float volume) {
		seVolume = volume;
		foreach(AudioSourceInfo seSource in seSources) {
			seSource.audio.volume = volume;
		}
	}
#endregion
#region 設定ファイル関連
	/// <summary>
	/// 設定ファイル読み込み
	/// </summary>
	protected void ReadSettingFile() {
		string path = Application.streamingAssetsPath + "/" + settingFile;
		//ファイル確認
		if(!File.Exists(path)) return;
		string line;
		using(TextReader r = FuncBox.GetTextReader(path)) {
			while((line = r.ReadLine()) != null) {
				switch(line) {
					case "<BGMVolume>":
					bgmVolume = float.Parse(r.ReadLine());
					break;
					case "<SEVolume>":
					seVolume = float.Parse(r.ReadLine());
					break;
					case "<End>":
					return;
				}
			}			
		}
	}
	/// <summary>
	/// 設定ファイル書き込み
	/// </summary>
	protected void WriteSettingFile() {
		string path = Application.streamingAssetsPath + "/" + settingFile;
		using(TextWriter w = FuncBox.GetTextWriter(path)) {
			//書き込み文字列の生成
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("<BGMVolume>");
			sb.AppendLine(bgmVolume.ToString());
			sb.AppendLine("<SEVolume>");
			sb.AppendLine(seVolume.ToString());
			sb.AppendLine("<End>");
			//書き込み
			w.Write(sb.ToString());
		}
	}
#endregion
}