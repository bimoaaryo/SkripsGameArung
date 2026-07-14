using UnityEngine;


public class CloudDrift : MonoBehaviour
{
    [Tooltip("Kecepatan gerak awan ke kiri (unit per detik)")]
    public float speed = 0.2f;

    [Tooltip("Posisi X — saat transform.position.x kurang dari ini, GameObject dihapus")]
    public float destroyAtX = -15f;

    void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;

        if (transform.position.x < destroyAtX)
            Destroy(gameObject);
    }
}