using UnityEngine;

public class JoystickVibration : MonoBehaviour
{
    [SerializeField] private bool enableVibration = true;

    [SerializeField] private float attackTreeLowFreq = 0.6f;
    [SerializeField] private float attackTreeHighFreq = 0.8f;
    [SerializeField] private float attackTreeDuration = 0.3f;

    [SerializeField] private float attackEnemyLowFreq = 0.9f;
    [SerializeField] private float attackEnemyHighFreq = 1.0f;
    [SerializeField] private float attackEnemyDuration = 0.4f;

    [SerializeField] private float miningLowFreq = 0.8f;
    [SerializeField] private float miningHighFreq = 0.6f;
    [SerializeField] private float miningDuration = 0.36f;

    [SerializeField] private float dashLowFreq = 0.4f;
    [SerializeField] private float dashHighFreq = 0.6f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float runLowFreq = 0.2f;
    [SerializeField] private float runHighFreq = 0.3f;
    [SerializeField] private float runDuration = 0.08f;

    [SerializeField] private float collectItemLowFreq = 0.2f;
    [SerializeField] private float collectItemHighFreq = 0.4f;
    [SerializeField] private float collectItemDuration = 0.16f;
}

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

    public void OnAttackTree() => Vibrate(attackTreeLowFreq, attackTreeHighFreq, attackTreeDuration);
    public void OnAttackEnemy() => Vibrate(attackEnemyLowFreq, attackEnemyHighFreq, attackEnemyDuration);
    public void OnMining() => Vibrate(miningLowFreq, miningHighFreq, miningDuration);
    public void OnDash() => Vibrate(dashLowFreq, dashHighFreq, dashDuration);
    public void OnRun() => Vibrate(runLowFreq, runHighFreq, runDuration);
    public void OnCollectItem() => Vibrate(collectItemLowFreq, collectItemHighFreq, collectItemDuration);

    public void Vibrate(float lowFreq, float highFreq, float duration)
    {
        if (!enableVibration) return;

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
        enableVibration = enabled;
        if (!enabled)
        {
            StopVibration();
        }
    }
}
