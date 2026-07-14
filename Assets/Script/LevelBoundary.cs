using UnityEngine;


public class LevelBoundary : MonoBehaviour
{

    void OnDrawGizmos()
    {
        Collider2D[] cols = GetComponentsInChildren<Collider2D>();
        Gizmos.color = new Color(0f, 0.3f, 0.8f, 0.4f);

        foreach (Collider2D col in cols)
        {
            Bounds b = col.bounds;
            Gizmos.DrawCube(b.center, b.size);
        }

        Gizmos.color = new Color(0f, 0.3f, 0.8f, 0.9f);
        foreach (Collider2D col in cols)
        {
            Bounds b = col.bounds;
            Gizmos.DrawWireCube(b.center, b.size);
        }
    }
}