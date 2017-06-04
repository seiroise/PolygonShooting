using UnityEngine;
using System.Collections;
/// <summary>
/// スクリーンショットをとる
/// </summary>
public class ScreenShot : MonoBehaviour {
	[Header("設定")]
	public string directory;
	public string fileName;
	public int scale = 4;
#region MonoBehaviourイベント
	private void Start() {
		directory = Application.dataPath;
	}
#endregion
#region 関数
	public void TakeScreenShot() {
		Application.CaptureScreenshot(directory + "/" + fileName, scale);
	}
#endregion
}