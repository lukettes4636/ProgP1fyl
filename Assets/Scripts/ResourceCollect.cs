using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

public class ResourceCollect : MonoBehaviour
{
    [FormerlySerializedAs("max_Health")]
    [SerializeField] private int maxHealth = 5;
    [FormerlySerializedAs("requiredtool")]
    [SerializeField] private PlayerActionController.EquipType requiredTool = PlayerActionController.EquipType.Axe;

    private int currentHealth;

    [FormerlySerializedAs("resoursedrop_Prefab")]
    [SerializeField] private GameObject lootPrefab;
    [FormerlySerializedAs("drop_Amount")]
    [SerializeField] private int dropAmount;
    [FormerlySerializedAs("drop_Name")]
    [FormerlySerializedAs("dropResourceName")]
    [SerializeField] private string dropResourceName = "Wood";

    [SerializeField] private float shakeDuration = 0.1f;
    [SerializeField] private float shakeMagnitude = 0.05f;
    private Vector3 originalPosition;

    private JoystickVibration joystickVibration; 

    private void Start()
    {
        currentHealth = maxHealth;
        originalPosition = transform.position;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            joystickVibration = player.GetComponent<JoystickVibration>();
        }
    }

    private void DestroyResource()
    {
        for (int i = 0; i < dropAmount; i++)
        {
            GameObject obj = Instantiate(lootPrefab, transform.position, Quaternion.identity);
            LootDrop loot = obj.GetComponent<LootDrop>();
            if (loot != null)
            {
                loot.SetResourceName(dropResourceName);
            }
            CollectableItem col = obj.GetComponent<CollectableItem>();
            if (col != null)
            {
                col.Initialize(dropResourceName, 1, null);
            }
        }

        if (joystickVibration != null)
        {
            joystickVibration.OnMining(); 
        }

        Destroy(gameObject);
    }

    public bool ReceiveHit(PlayerActionController.EquipType toolUsed, int damage)
    {
        if (toolUsed != requiredTool)
        {
            return false;
        }

        currentHealth -= damage;

        StartCoroutine(Shake());

        if (joystickVibration != null)
        {
            joystickVibration.OnMining(); 
        }

        if (currentHealth <= 0)
        {
            DestroyResource();
        }

        return true;
    }

    private IEnumerator Shake()
    {
        float elapsedTime = 0.0f;

        if (transform.position != originalPosition)
        {
            transform.position = originalPosition;
        }

        while (elapsedTime < shakeDuration)
        {
            float x = originalPosition.x + UnityEngine.Random.Range(-1f, 1f) * shakeMagnitude;
            float y = originalPosition.y + UnityEngine.Random.Range(-1f, 1f) * shakeMagnitude;

            transform.position = new Vector3(x, y, originalPosition.z);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        transform.position = originalPosition;
    }
}
