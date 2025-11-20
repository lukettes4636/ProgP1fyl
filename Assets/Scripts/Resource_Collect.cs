using UnityEngine;
using System.Collections;

public class Resource_Collect : MonoBehaviour
{
    [SerializeField] private int max_Health = 5;
    [SerializeField] private PlayerActionController.EquipType requiredtool = PlayerActionController.EquipType.Hacha;

    private int current_Health;

    [SerializeField] private GameObject resoursedrop_Prefab;
    [SerializeField] private int drop_Amount;
    [SerializeField] private string drop_Name = "Madera";

    [SerializeField] private float shakeDuration = 0.1f;
    [SerializeField] private float shakeMagnitude = 0.05f;
    private Vector3 originalPosition;

    private JoystickVibration joystickVibration; 

    private void Start()
    {
        current_Health = max_Health;
        originalPosition = transform.position;

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
            GameObject obj = Instantiate(resoursedrop_Prefab, transform.position, Quaternion.identity);
            LootDrop loot = obj.GetComponent<LootDrop>();
            if (loot != null)
            {
                loot.SetResourceName(drop_Name);
            }
            CollectableItem col = obj.GetComponent<CollectableItem>();
            if (col != null)
            {
                col.Initialize(drop_Name, 1, null);
            }
        }

        if (joystickVibration != null)
        {
            joystickVibration.OnMining(); 
        }

        Destroy(gameObject);
    }

    public bool TakeHit(PlayerActionController.EquipType toolUsed, int damage)
    {
        if (toolUsed != requiredtool)
        {
            return false;
        }

        current_Health -= damage;

        StartCoroutine(Shake());

        if (joystickVibration != null)
        {
            joystickVibration.OnMining(); 
        }

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
