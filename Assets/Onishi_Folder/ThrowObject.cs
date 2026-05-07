using UnityEngine;
using UnityEngine.InputSystem;

public class ThrowObject : MonoBehaviour
{
    public GameObject boomerangPrefab;
    public Transform throwPoint;

    private bool hasBoomerang = true;

    void Update()
    {
        if (Keyboard.current.fKey.wasPressedThisFrame && hasBoomerang)
        {
            Throw();
        }
    }

    void Throw()
    {
        GameObject obj = Instantiate(
            boomerangPrefab,
            throwPoint.position,
            throwPoint.rotation
        );

        Boomerang boom = obj.GetComponent<Boomerang>();

        // ƒvƒŒƒCƒ„[î•ñ‚ð“n‚·
        boom.owner = transform;

        hasBoomerang = false;

        // –ß‚Á‚Ä‚«‚½‚çÄ‚Ñ“Š‚°‚ç‚ê‚é
        boom.onReturn += () =>
        {
            hasBoomerang = true;
        };
    }
}

