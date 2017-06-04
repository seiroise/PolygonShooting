using UnityEngine;
using System.Collections;
/// <summary>
/// グリッドを縦スクロールさせる
/// </summary>
public class GridScroll : MonoBehaviour {
	public GameObject scrollObject;		//上側
	public float speed = 2f;
#region MonoBehaviourイベント
	private void Update() {
		scrollObject.transform.position += Vector3.down * speed * Time.deltaTime;
	}
#endregion
}