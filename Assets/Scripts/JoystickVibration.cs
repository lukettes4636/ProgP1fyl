using UnityEngine;
using UnityEngine.InputSystem;

public class JoystickVibration : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] private bool enableVibration = true;

    [Header("Combat Vibrations")]
    [SerializeField] private float attackTreeLowFreq = 0.6f;
    [SerializeField] private float attackTreeHighFreq = 0.8f;
    [SerializeField] private float attackTreeDuration = 0.3f;

    [SerializeField] private float attackEnemyLowFreq = 0.9f;
    [SerializeField] private float attackEnemyHighFreq = 1.0f;
    [SerializeField] private float attackEnemyDuration = 0.4f;

    [SerializeField] private float miningLowFreq = 0.8f;
    [SerializeField] private float miningHighFreq = 0.6f;
    [SerializeField] private float miningDuration = 0.36f;

    [Header("Damage Vibrations")]
    [SerializeField] private float takeDamageLowFreq = 1.0f;
    [SerializeField] private float takeDamageHighFreq = 1.0f;
    [SerializeField] private float takeDamageDuration = 0.6f;

    [SerializeField] private float lowHealthLowFreq = 1.0f;
    [SerializeField] private float lowHealthHighFreq = 0.8f;
    [SerializeField] private float lowHealthDuration = 0.5f;

    [Header("Movement Vibrations")]
    [SerializeField] private float dashLowFreq = 0.4f;
    [SerializeField] private float dashHighFreq = 0.6f;
    [SerializeField] private float dashDuration = 0.2f;

    [SerializeField] private float jumpLowFreq = 0.6f;
    [SerializeField] private float jumpHighFreq = 0.4f;
    [SerializeField] private float jumpDuration = 0.24f;

    [Header("Collection Vibrations")]
    [SerializeField] private float collectItemLowFreq = 0.2f;
    [SerializeField] private float collectItemHighFreq = 0.4f;
    [SerializeField] private float collectItemDuration = 0.16f;

    [Header("UI Vibrations")]
    [SerializeField] private float menuSelectLowFreq = 0.2f;
    [SerializeField] private float menuSelectHighFreq = 0.2f;
    [SerializeField] private float menuSelectDuration = 0.1f;

    [Header("Custom Settings")]
    [SerializeField] private float customLowFreq = 1.0f;
    [SerializeField] private float customHighFreq = 1.0f;
    [SerializeField] private float customDuration = 0.4f;

    private Gamepad gamepad;

    private void Start()
    {
        gamepad = Gamepad.current;
    }

    public void OnAttackTree()
    {
        if (enableVibration)
            Vibrate(attackTreeLowFreq, attackTreeHighFreq, attackTreeDuration);
    }

    public void OnAttackEnemy()
    {
        if (enableVibration)
            Vibrate(attackEnemyLowFreq, attackEnemyHighFreq, attackEnemyDuration);
    }

    public void OnMining()
    {
        if (enableVibration)
            Vibrate(miningLowFreq, miningHighFreq, miningDuration);
    }

    public void OnTakeDamage()
    {
        if (enableVibration)
            Vibrate(takeDamageLowFreq, takeDamageHighFreq, takeDamageDuration);
    }

    public void OnLowHealth()
    {
        if (enableVibration)
            Vibrate(lowHealthLowFreq, lowHealthHighFreq, lowHealthDuration);
    }

    public void OnDash()
    {
        if (enableVibration)
            Vibrate(dashLowFreq, dashHighFreq, dashDuration);
    }

    public void OnJump()
    {
        if (enableVibration)
            Vibrate(jumpLowFreq, jumpHighFreq, jumpDuration);
    }

    public void OnCollectItem()
    {
        if (enableVibration)
            Vibrate(collectItemLowFreq, collectItemHighFreq, collectItemDuration);
    }

    public void OnMenuSelect()
    {
        if (enableVibration)
            Vibrate(menuSelectLowFreq, menuSelectHighFreq, menuSelectDuration);
    }

    public void VibrateLight()
    {
        if (enableVibration)
            Vibrate(0.2f, 0.2f, 0.1f);
    }

    public void VibrateMedium()
    {
        if (enableVibration)
            Vibrate(0.5f, 0.5f, 0.2f);
    }

    public void VibrateStrong()
    {
        if (enableVibration)
            Vibrate(1.0f, 1.0f, 0.3f);
    }

    public void VibrateCustom()
    {
        if (enableVibration)
            Vibrate(customLowFreq, customHighFreq, customDuration);
    }

    public void Vibrate(float lowFreq, float highFreq, float time)
    {
        if (gamepad != null && enableVibration)
        {
            gamepad.SetMotorSpeeds(lowFreq, highFreq);
            Invoke(nameof(StopVibration), time);
        }
    }

    public void StopVibration()
    {
        if (gamepad != null)
        {
            gamepad.SetMotorSpeeds(0f, 0f);
        }
    }

    public void SetVibrationEnabled(bool enabled)
    {
        enableVibration = enabled;
        if (!enabled)
        {
            StopVibration();
        }
    }

    private void OnDestroy()
    {
        StopVibration();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            StopVibration();
        }
    }
}