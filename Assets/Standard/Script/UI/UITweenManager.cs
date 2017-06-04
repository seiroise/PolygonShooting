using UnityEngine;
using System.Collections;

//Tweenを配列で管理
public class UITweenManager : MonoBehaviour {

	[Header("パラメータ")]
	public UITweener[] tweener;
	public float interval;
	protected bool flag = false; 

	protected int index = 0;
	protected float time = 0f;

	protected void Start() {
		for(int i = 0; i < tweener.Length; i++) {
			tweener[i].Play(false);
		}
	}

	protected void Update() {

	}

#region 関数

	public void Play(bool flag) {
		//フラグの値が違うなら停止して実行
		if(this.flag != flag) {
			StopCoroutine("PlayCoroutine");
			StartCoroutine("PlayCoroutine", flag);
		}
	}

#endregion
}