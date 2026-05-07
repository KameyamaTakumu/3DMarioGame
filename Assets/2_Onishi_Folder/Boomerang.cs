using UnityEngine;
using System;

public class Boomerang : MonoBehaviour
{
    [Header("‘¬“x")]
    public float speed = 15f;

    [Header("–ß‚é‚Ü‚Å‚ÌŠÔ")]
    public float returnTime = 1.5f;

    [Header("‰ñ“]‘¬“x")]
    public float rotateSpeed = 720f;

    public Transform owner;

    private bool isReturning = false;
    private bool hitEnemy = false;

    public Action onReturn;

    void Start()
    {
        Invoke(nameof(StartReturn), returnTime);
    }

    void Update()
    {
        // ‰ñ“]
        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);

        // “G‚É“–‚½‚Á‚Ä‚¢‚½‚ç’â~
        if (hitEnemy) return;

        // s‚«
        if (!isReturning)
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
        // –ß‚è
        else
        {
            if (owner == null) return;

            Vector3 dir =
                (owner.position - transform.position).normalized;

            transform.position += dir * speed * Time.deltaTime;

            // ƒvƒŒƒCƒ„[‚É–ß‚Á‚½
            if (Vector3.Distance(transform.position, owner.position) < 1f)
            {
                onReturn?.Invoke();
                Destroy(gameObject);
            }
        }
    }

    void StartReturn()
    {
        if (!hitEnemy)
        {
            isReturning = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // “G‚É“–‚½‚Á‚½
        if (other.CompareTag("Enemy"))
        {
            hitEnemy = true;

            // ’n–Ê‚É—‚Æ‚·
            Rigidbody rb = GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }
        }
    }
}