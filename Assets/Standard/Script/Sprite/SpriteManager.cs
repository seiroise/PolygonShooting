using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// 複数のスプライトを名前の辞書で管理
/// </summary>
public class SpriteManager : MonoBehaviour {
	//スプライトを管理する辞書
	protected Dictionary<string, List<SpriteRenderer>> spriteDic = null;
	//アクティブなマーカーの辞書
	protected Dictionary<GameObject, SpriteRenderer> activeSprites;
#region MonoBehaviourイベント
	protected void Awake() {
		spriteDic = new Dictionary<string,List<SpriteRenderer>>();
		activeSprites = new Dictionary<GameObject,SpriteRenderer>();
	}
#endregion
#region 関数
	/// <summary>
	/// 登録確認 登録されていればtrue
	/// </summary>
	public bool Contain(string name) {
		//確認
		if(spriteDic.ContainsKey(name)) {
			return true;
		} else {
			return false;
		}
	}
	/// <summary>
	/// SpriteRendererを持ったプレハブを生成。activeはfalse
	/// </summary>
	public void InstantiatePrefabs(string name, SpriteRenderer prefab, int num) {
		//登録確認
		bool flag = Contain(name);
		//登録
		List<SpriteRenderer> spriteList = new List<SpriteRenderer>();
		if(flag) {
			spriteList = spriteDic[name];
		}
		for(int i = 0; i < num; i++) {
			GameObject g = Instantiate(prefab.gameObject);
			g.transform.parent = transform;
			g.transform.localPosition = Vector3.zero;
			g.name = name;
			SpriteRenderer s = g.GetComponent<SpriteRenderer>();
			g.SetActive(false);
			spriteList.Add(s);
		}
		if(flag) {
			spriteDic[name] = spriteList;
		} else {
			spriteDic.Add(name, spriteList);
		}
	}
	/// <summary>
	/// パラメータを設定してスプライトを使用
	/// <para>戻り値は設定したオブジェクト</para>
	/// </summary>
	public GameObject SetSpriteParameter(string name, Vector3 pos, Vector3 angles, Color c) {
		//登録確認
		if(!Contain(name)) return null;
		//非アクティブなSpriteRendererを取得
		SpriteRenderer s = GetNonActiveSprite(name);
		if(s == null) return null;
		s.gameObject.SetActive(true);
		//値を設定
		s.transform.position = pos;
		s.transform.eulerAngles = angles;
		s.color = c;
		//アクティブなスプライト辞書に追加
		activeSprites.Add(s.gameObject, s);
		return s.gameObject;
	}
	/// <summary>
	/// 使用していたスプライトを元に戻す
	/// </summary>
	public void ResetSprite(GameObject s) {
		s.SetActive(false);
		if(activeSprites.ContainsKey(s))  {
			activeSprites.Remove(s);
		}
	}
	/// <summary>
	/// 名前を指定して非アクティブなSpriteRendererを返す
	/// </summary>
	protected SpriteRenderer GetNonActiveSprite(string name) {
		//登録確認
		if(!Contain(name)) return null;
		//アクティブ確認
		SpriteRenderer s = null;
		foreach(SpriteRenderer sr in spriteDic[name]) {
			if(!sr.gameObject.activeInHierarchy) {
				s = sr;
				break;
			}
		}
		//nullだった場合は新規追加
		if(s == null) {
			int c = spriteDic[name].Count;
			InstantiatePrefabs(name, spriteDic[name][0], spriteDic[name].Count);
			s = spriteDic[name][c + 1];
		}
		return s;
	}
	/// <summary>
	/// アクティブなスプライトの色を変更する
	/// </summary>
	public void ChangeActiveSpriteColor(GameObject g, Color c) {
		if(!activeSprites.ContainsKey(g)) return;
		//アクティブ確認
		if(g.activeInHierarchy) {
			SpriteRenderer s = activeSprites[g];
			s.color = c;
		} else {
			activeSprites.Remove(g);
		}
	}
#endregion
}