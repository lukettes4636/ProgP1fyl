using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerSummonController : MonoBehaviour
{
    [SerializeField] private GameObject angelPrefab;
    [SerializeField] private GameObject demonPrefab;
    [SerializeField] private float summonDistance = 3f;
    [SerializeField] private KeyCode angelKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode demonKey = KeyCode.Alpha3;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip summonClip;

    private GameObject currentAngel;
    private GameObject currentDemon;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

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

        Vector3 spawnPos = GetSummonPosition();
        currentAngel = Instantiate(angelPrefab, spawnPos, Quaternion.identity);
        PlaySummonSound();
    }

    private void SummonDemon()
    {
        if (demonPrefab == null) return;

        if (currentDemon != null)
        {
            Destroy(currentDemon);
        }

        Vector3 spawnPos = GetSummonPosition();
        currentDemon = Instantiate(demonPrefab, spawnPos, Quaternion.identity);
        PlaySummonSound();
    }

    private Vector3 GetSummonPosition()
    {
        PlayerMovement movement = GetComponent<PlayerMovement>();
        Vector2 direction = Vector2.down; 

        if (movement != null)
        {
            direction = movement.GetLastDirection();
        }

        return transform.position + (Vector3)(direction * summonDistance);
    }

    private void PlaySummonSound()
    {
        if (audioSource != null && summonClip != null)
        {
            audioSource.PlayOneShot(summonClip);
        }
    }
}
