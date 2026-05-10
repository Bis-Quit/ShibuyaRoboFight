using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class RobotUI : MonoBehaviour
{
    [Header("Target Robot")]
    public RobotStats targetRobot;

    [Header("UI Element")]
    public TextMeshProUGUI nameText;
    public Slider hpSlider;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI skillPowerText;

    public void BindRobot(RobotStats newRobot)
    {
        if (targetRobot != null) UnsubscribeEvents();

        targetRobot = newRobot;

        if (targetRobot != null)
        {
            SubscribeEvents();
            if (nameText != null && targetRobot.baseData != null)
                nameText.text = targetRobot.baseData.characterName;
                
            if (hpSlider != null && targetRobot.baseData != null)
            {
                hpSlider.maxValue = targetRobot.baseData.maxHealth;
                hpSlider.value = targetRobot.currentHP; 
            }
            
            UpdateEnergyText(targetRobot.currentEnergy);
            UpdateSkillPowerText(targetRobot.currentSkillPower);
        }
    }

    private void SubscribeEvents()
    {
        targetRobot.OnHPChanged += UpdateHPBar;
        targetRobot.OnEnergyChanged += UpdateEnergyText;
        targetRobot.OnSkillPowerChanged += UpdateSkillPowerText;
    }

    private void UnsubscribeEvents()
    {
        targetRobot.OnHPChanged -= UpdateHPBar;
        targetRobot.OnEnergyChanged -= UpdateEnergyText;
        targetRobot.OnSkillPowerChanged -= UpdateSkillPowerText;
    }

    private void UpdateHPBar(int currentHP, int maxHP)
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP; 
            hpSlider.DOValue(currentHP, 0.5f).SetEase(Ease.OutCubic);
        }
    }

    private void UpdateEnergyText(int currentEnergy)
    {
        if (energyText != null) energyText.text = currentEnergy.ToString();
    }

    private void UpdateSkillPowerText(int currentSkillPower)
    {
        if (skillPowerText != null) skillPowerText.text = currentSkillPower.ToString();
    }
}