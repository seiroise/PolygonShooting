using UnityEngine;
using System.Collections;

//アニメーションを管理するにくいやつ
public class AnimationController : MonoBehaviour {
	[Header("Animator")]
	public Animator anime;
	[Header("パラメータ")]
	public SpriteRenderer sprite;
	public float speed = 1f;

	protected void Start () {
		anime.speed = speed;
	}

#region アニメ－ションイベント
	//自身を削除
	public void AnimationEvent_Destroy() {
		Destroy(gameObject);
	}
#endregion
}