using UnityEngine;

public class PlayerSummonController : MonoBehaviour
{
    [SerializeField] private GameObject angelPrefab;
    [SerializeField] private GameObject demonPrefab;
    [SerializeField] private float summonDistance = 3f;
    [SerializeField] private KeyCode angelKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode demonKey = KeyCode.Alpha2;

    private GameObject currentAngel;
    private GameObject currentDemon;

    private void Update()
    {
        if (Input.GetKeyDown(angelKey))
        {
            SummonAngel();
        }
        if (Input.GetKeyDown(demonKey))
        {
            SummonDemon();
        }
    }

    private void SummonAngel()
    {
        if (angelPrefab == null) return;
        
        if (currentAngel != null)
        {
            Destroy(currentAngel);
        }

        Vector3 spawnPos = transform.position + transform.forward * summonDistance;
        currentAngel = Instantiate(angelPrefab, spawnPos, Quaternion.identity);
    }

    private void SummonDemon()
    {
        if (demonPrefab == null) return;

        if (currentDemon != null)
        {
            Destroy(currentDemon);
        }

        Vector3 spawnPos = transform.position + transform.forward * summonDistance;
        currentDemon = Instantiate(demonPrefab, spawnPos, Quaternion.identity);
    }
}
