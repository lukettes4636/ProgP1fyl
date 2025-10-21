using UnityEngine;

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

    [Header("Movement Vibrations")]
    [SerializeField] private float dashLowFreq = 0.4f;
    [SerializeField] private float dashHighFreq = 0.6f;
    [SerializeField] private float dashDuration = 0.2f;

    [Header("Collection Vibrations")]
    [SerializeField] private float collectItemLowFreq = 0.2f;
    [SerializeField] private float collectItemHighFreq = 0.4f;
    [SerializeField] private float collectItemDuration = 0.16f;

    private bool isVibrating = false;
    private float vibrationTimer = 0f;

    private void Update()
    {
        // Este bloque es opcional — en caso de querer detener vibraciones manualmente por tiempo
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
    public void OnCollectItem() => Vibrate(collectItemLowFreq, collectItemHighFreq, collectItemDuration);

    public void Vibrate(float lowFreq, float highFreq, float duration)
    {
        if (!enableVibration) return;

        // Nota: Unity no tiene vibración nativa sin InputSystem,
        // pero se puede integrar fácilmente con XInputDotNet si lo usas.
        // Aquí simplemente marcamos un estado de vibración activo.
        isVibrating = true;
        vibrationTimer = duration;

        //  Si más adelante agregas XInputDotNet, aquí podrías hacer:
        // XInputDotNetPure.GamePad.SetVibration(0, lowFreq, highFreq);

        Debug.Log($"[VIBRATION] Low: {lowFreq} | High: {highFreq} | Duration: {duration}");
    }

    public void StopVibration()
    {
        isVibrating = false;
        vibrationTimer = 0f;

        // Si usas XInputDotNet:
        // XInputDotNetPure.GamePad.SetVibration(0, 0, 0);
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
