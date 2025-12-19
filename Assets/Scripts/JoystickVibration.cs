using UnityEngine;

public class JoystickVibration : MonoBehaviour
{
    [SerializeField] private bool vibrationEnabled = true;

    [SerializeField] private float lowFreqTreeAttack = 0.6f;
    [SerializeField] private float highFreqTreeAttack = 0.8f;
    [SerializeField] private float durationTreeAttack = 0.3f;

    [SerializeField] private float lowFreqEnemyAttack = 0.9f;
    [SerializeField] private float highFreqEnemyAttack = 1.0f;
    [SerializeField] private float durationEnemyAttack = 0.4f;

    [SerializeField] private float lowFreqMining = 0.8f;
    [SerializeField] private float highFreqMining = 0.6f;
    [SerializeField] private float durationMining = 0.36f;

    [SerializeField] private float lowFreqDash = 0.4f;
    [SerializeField] private float highFreqDash = 0.6f;
    [SerializeField] private float durationDash = 0.2f;
    [SerializeField] private float lowFreqRun = 0.2f;
    [SerializeField] private float highFreqRun = 0.3f;
    [SerializeField] private float durationRun = 0.08f;

    [SerializeField] private float lowFreqItemPickup = 0.2f;
    [SerializeField] private float highFreqItemPickup = 0.4f;
    [SerializeField] private float durationItemPickup = 0.16f;

    private bool isVibrating = false;
    private float vibrationTimer = 0f;

    private void Update()
    {
        if (isVibrating)
        {
            vibrationTimer -= Time.deltaTime;
            if (vibrationTimer <= 0)
            {
                StopVibration();
            }
        }
    }

    public void OnTreeAttack() => Vibrate(lowFreqTreeAttack, highFreqTreeAttack, durationTreeAttack);
    public void OnEnemyAttack() => Vibrate(lowFreqEnemyAttack, highFreqEnemyAttack, durationEnemyAttack);
    public void OnMining() => Vibrate(lowFreqMining, highFreqMining, durationMining);
    public void OnDash() => Vibrate(lowFreqDash, highFreqDash, durationDash);
    public void OnRun()
    {
        Vibrate(lowFreqRun, highFreqRun, durationRun);
    }
    public void OnItemPickup() => Vibrate(lowFreqItemPickup, highFreqItemPickup, durationItemPickup);

    public void Vibrate(float lowFreq, float highFreq, float duration)
    {
        if (!vibrationEnabled) return;

        isVibrating = true;
        vibrationTimer = duration;
    }

    public void StopVibration()
    {
        isVibrating = false;
        vibrationTimer = 0f;
    }

    public void SetVibrationEnabled(bool enabled)
    {
        vibrationEnabled = enabled;
        if (!enabled)
        {
            StopVibration();
        }
    }
}