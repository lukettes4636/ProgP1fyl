using UnityEngine;
using System.Collections;

public class Resource_Collect : MonoBehaviour
{
    [SerializeField] private int max_Health = 5;
    [SerializeField] private PlayerActionController.EquipType requiredtool = PlayerActionController.EquipType.Hacha;

    private int current_Health;

    [SerializeField] private GameObject resoursedrop_Prefab;
    [SerializeField] private int drop_Amount;

    [Header("Efecto de Temblor")]
    [SerializeField] private float shakeDuration = 0.1f;
    [SerializeField] private float shakeMagnitude = 0.05f;
    private Vector3 originalPosition;

    private JoystickVibration joystickVibration; //  referencia al sistema de vibración

    private void Start()
    {
        current_Health = max_Health;
        originalPosition = transform.position;

        // Busca automáticamente el componente de vibración en el jugador
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            joystickVibration = player.GetComponent<JoystickVibration>();
        }
    }

    private void DestroyResourse()
    {
        for (int i = 0; i < drop_Amount; i++)
        {
            Instantiate(resoursedrop_Prefab, transform.position, Quaternion.identity);
        }

        //  Vibración al destruir completamente el recurso
        if (joystickVibration != null)
        {
            joystickVibration.OnMining(); // puedes cambiar a OnAttackTree() o lo que prefieras
        }

        Destroy(gameObject);
    }

    public bool TakeHit(PlayerActionController.EquipType toolUsed, int damage)
    {
        if (toolUsed != requiredtool)
        {
            Debug.Log(gameObject.name + ": Incorrect tool. Required: " + requiredtool.ToString());
            return false;
        }

        current_Health -= damage;
        Debug.Log(gameObject.name + ": Hit. Remaining Health: " + current_Health);

        StartCoroutine(Shake());

        //  Vibración leve al golpear aunque no se destruya
        if (joystickVibration != null)
        {
            joystickVibration.OnMining(); // usa vibración de minado / corte
        }

        if (current_Health <= 0)
        {
            Debug.Log($"{gameObject.name} has been destroyed");
            DestroyResourse();
        }

        return true;
    }

    private IEnumerator Shake()
    {
        float elapsed = 0.0f;

        if (transform.position != originalPosition)
        {
            transform.position = originalPosition;
        }

        while (elapsed < shakeDuration)
        {
            float x = originalPosition.x + UnityEngine.Random.Range(-1f, 1f) * shakeMagnitude;
            float y = originalPosition.y + UnityEngine.Random.Range(-1f, 1f) * shakeMagnitude;

            transform.position = new Vector3(x, y, originalPosition.z);
            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.position = originalPosition;
    }
}
