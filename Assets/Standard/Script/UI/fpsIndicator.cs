using UnityEngine;
using System.Collections;

//fps表示用
public class fpsIndicator : MonoBehaviour {

	public float interval = 0.5f;
	public UILabel indicator;

	//計測用
	protected float min, max, avg;
	protected int count = 0;

	public void Start() {
		StartCoroutine(FpsCoroutine());
	}

	IEnumerator FpsCoroutine() {
		min = max = avg = 1f / Time.deltaTime;
		while (true) {
			count++;
			float fps = 1f / Time.deltaTime;
			//最小
			if(fps < min) {
				min = fps;
			}
			//最大
			if(fps > max) {
				max = fps;
			}
			//平均
			avg += fps;

			string str = "";
			str += "fps : " + fps + "\n";
			str += "max : " + max + "\n";
			str += "min : " + min + "\n";
			str += "avg : " + (avg / count) + "(" + count + ")";
			//ラベルがあるならラベルに表示
			if (indicator) {
				indicator.text = str;
			} else {
				Debug.Log(str);
			}
			yield return new WaitForSeconds(interval);
		}
	}
}