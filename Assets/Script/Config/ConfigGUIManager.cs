using UnityEngine;
using System.Collections;
/// <summary>
/// コンフィグ画面のGUI管理
/// </summary>
public class ConfigGUIManager : SingletonMonoBehaviour<ConfigGUIManager> {
	[Header("UIパーツ")]
	public UILabel battleTimeLabel;
	public UISlider battleTimeSlider;
	public int battleTimeBase = 30;
	
	public UILabel stageScaleLabel;
	public UISlider stageScaleSlider;
	public float stageScaleBase = 50f;

	public UILabel gameSpeedLabel;
	public UISlider gameSpeedSlider;
	public float gameSpeedBase = 0.25f;

	public UILabel playerHPScaleLabel;
	public UISlider playerHPScaleSlider;
	public float playerHPScaleBase = 1f;

	public UILabel maxEnergyLabel;
	public UISlider maxEnergySlider;
	public int maxEnergyBase = 1000;

	public UILabel energyOutputLabel;
	public UISlider energyOutputSlider;
	public int energyOutputBase = 250;
#region MonoBehaviourイベント
	private void Start() {
		float value;
		value = (GameManager.Instance.battleTime / battleTimeBase) - 1;
		battleTimeSlider.sliderValue = value * 0.1f;
		value = (GameManager.Instance.stageScale / stageScaleBase) - 1;
		stageScaleSlider.sliderValue = value * 0.1f;
		value = (GameManager.Instance.gameSpeed / gameSpeedBase) - 1;
		gameSpeedSlider.sliderValue = value * 0.1f;
		value = (GameManager.Instance.playerHPScale / playerHPScaleBase) - 1;
		playerHPScaleSlider.sliderValue = value * 0.1f;
		value = (GameManager.Instance.maxEnergy / maxEnergyBase) - 1;
		maxEnergySlider.sliderValue = value * 0.1f;
		value = (GameManager.Instance.energyOutput / energyOutputBase) - 1;
		energyOutputSlider.sliderValue = value * 0.1f;
	}
#endregion
#region 関数
	private void SetBattleTime(int value) {
		GameManager.Instance.battleTime = value * battleTimeBase;
		battleTimeLabel.text = GameManager.Instance.battleTime.ToString();
	}
	private void SetStageScale(int value) {
		GameManager.Instance.stageScale = value * stageScaleBase;
		stageScaleLabel.text = GameManager.Instance.stageScale.ToString();
	}
	private void SetGameSpeed(int value) {
		GameManager.Instance.gameSpeed = value * gameSpeedBase;
		gameSpeedLabel.text = GameManager.Instance.gameSpeed.ToString();
	}
	private void SetPlayerHPScale(int value) {
		GameManager.Instance.playerHPScale = value * playerHPScaleBase;
		playerHPScaleLabel.text = GameManager.Instance.playerHPScale.ToString();
	}
	private void SetMaxEnergy(int value) {
		GameManager.Instance.maxEnergy = value * maxEnergyBase;
		maxEnergyLabel.text = GameManager.Instance.maxEnergy.ToString();
	}
	private void SetEnergyOutput(int value) {
		GameManager.Instance.energyOutput = value * energyOutputBase;
		energyOutputLabel.text = GameManager.Instance.energyOutput.ToString();
	}
#endregion
#region UIイベント
	//ボタン
	private void TitleButtonClicked() {
		FadeManager.Instance.LoadLevel("MainMenu");
	}
	private void ResetButtonClicked() {
		battleTimeSlider.sliderValue = 0.2f;
		stageScaleSlider.sliderValue = 0.2f;
		gameSpeedSlider.sliderValue = 0.1f;
		playerHPScaleSlider.sliderValue = 0f;
		maxEnergySlider.sliderValue = 0.4f;
		energyOutputSlider.sliderValue = 0.2f;
	}
	//スライダー
	private void BattleTimeSliderChange(float value) {
		int intValue = Mathf.RoundToInt(value * 10f) + 1;
		SetBattleTime(intValue);
	}
	private void StageScaleSliderChange(float value) {
		int intValue = Mathf.RoundToInt(value * 10f) + 1;
		SetStageScale(intValue);
	}
	private void GameSpeedSliderChange(float value) {
		int intValue = Mathf.RoundToInt(value * 10f) + 1;
		SetGameSpeed(intValue);
	}
	private void PlayerHPScaleSliderChange(float value) {
		int intValue = Mathf.RoundToInt(value * 10f) + 1;
		SetPlayerHPScale(intValue);
	}
	private void MaxEnergySliderChange(float value) {
		int intValue = Mathf.RoundToInt(value * 10f) + 1;
		SetMaxEnergy(intValue);
	}
	private void EnergyOutputSliderChange(float value) {
		int intValue = Mathf.RoundToInt(value * 10f) + 1;
		SetEnergyOutput(intValue);
	}
#endregion
}