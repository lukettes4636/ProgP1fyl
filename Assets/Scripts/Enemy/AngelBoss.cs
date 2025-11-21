using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class AngelBoss : MonoBehaviour
{
    [SerializeField] private int maxHealth = 200;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private float attackCooldown = 1.0f;

    [SerializeField] private Slider healthBar;
    [SerializeField] private string victorySceneName = "Victory";

    private int currentHealth;
    private Transform target;
    private Animator animator;
    private float nextAttackTime = 0f;
    [SerializeField] private Transform attackHitbox;
    [SerializeField] private float hitboxDistance = 0.6f;
    private BossAttackHitbox bossHitbox;
    private Vector2 facing;

    private void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) target = playerObj.transform;
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
        if (attackHitbox != null)
        {
            bossHitbox = attackHitbox.GetComponent<BossAttackHitbox>();
            attackHitbox.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (target == null) return;
        if (currentHealth <= 0) return;

        float dist = (target.position - transform.position).magnitude;
        Vector2 dir = target.position - transform.position;
        Vector2 ax = new Vector2(dir.x, 0f);
        Vector2 ay = new Vector2(0f, dir.y);
        if (ax.sqrMagnitude >= ay.sqrMagnitude)
        {
            facing.x = dir.x >= 0f ? 1f : -1f;
            facing.y = 0f;
        }
        else
        {
            facing.y = dir.y >= 0f ? 1f : -1f;
            facing.x = 0f;
        }
        if (animator != null)
        {
            animator.SetFloat("MoveX", facing.x);
            animator.SetFloat("MoveY", facing.y);
        }

        if (dist > attackRange)
        {
            if (animator != null)
            {
                animator.SetBool("Walk", true);
                animator.SetBool("Idle", false);
            }
            Vector3 nd = dir.normalized;
            Vector3 step = new Vector3(nd.x, nd.y, 0f) * moveSpeed * Time.deltaTime;
            transform.position += step;
        }
        else
        {
            if (animator != null)
            {
                animator.SetBool("Walk", false);
                animator.SetBool("Idle", true);
            }
            if (Time.time >= nextAttackTime)
            {
                if (animator != null) animator.SetTrigger("Attack");
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;
        if (healthBar != null) healthBar.value = currentHealth;
        if (animator != null) animator.SetTrigger("Hit");
        if (currentHealth <= 0)
        {
            if (animator != null) animator.SetTrigger("Dead");
            enabled = false;
            if (!string.IsNullOrEmpty(victorySceneName))
            {
                SceneManager.LoadScene(victorySceneName);
            }
            else
            {
                SceneManager.LoadScene(3);
            }
        }
    }

    public void AttackHitboxOn()
    {
        if (attackHitbox == null) return;
        Vector3 offset = new Vector3(facing.x, facing.y, 0f) * hitboxDistance;
        attackHitbox.localPosition = offset;
        if (bossHitbox != null) bossHitbox.damage = attackDamage;
        attackHitbox.gameObject.SetActive(true);
    }

    public void AttackHitboxOff()
    {
        if (attackHitbox == null) return;
        attackHitbox.gameObject.SetActive(false);
        attackHitbox.localPosition = Vector3.zero;
    }
}