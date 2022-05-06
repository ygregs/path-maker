using UnityEngine;

public class TTL : MonoBehaviour
{
    public float AgeLimit;
    float age;

    void Reset()
    {
        AgeLimit = 2.0f;
    }

    void Update()
    {
        if (age > AgeLimit)
        {
            Destroy(gameObject);
            return;
        }

        age += Time.deltaTime;
    }
}