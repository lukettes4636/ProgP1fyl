using UnityEngine;
using System.Collections;

public class Resource_Collect : MonoBehaviour
{
    [SerializeField] private int max_Health = 5;
    [SerializeField] private PlayerActionController.EquipType requiredtool = PlayerActionController.EquipType.Hacha;

    private int current_Health;

    [SerializeField] private GameObject resoursedrop_Prefab;
    [SerializeField] private int drop_Amount;

    // Configuración del Temblor
    [Header("Efecto de Temblor")]
    [SerializeField] private float shakeDuration = 0.1f;
    [SerializeField] private float shakeMagnitude = 0.05f;
    private Vector3 originalPosition;

    private void Start()
    {
        current_Health = max_Health;
        originalPosition = transform.position;
    }

    private void DestroyResourse()
    {
        for (int i = 0; i < drop_Amount; i++)
        {
            Instantiate(resoursedrop_Prefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    public bool TakeHit(PlayerActionController.EquipType toolUsed, int damgae)
    {
        if (toolUsed != requiredtool)
        {
            Debug.Log(gameObject.name + ": Herramienta incorrecta. Necesitas: " + requiredtool.ToString());
            return false;
        }

        current_Health -= damgae;
        Debug.Log(gameObject.name + " Golpeado. Vida Restante: " + current_Health);

        
        StartCoroutine(Shake());

        if (current_Health <= 0)
        {
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