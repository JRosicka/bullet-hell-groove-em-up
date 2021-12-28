using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BulletDestructionBoundary : MonoBehaviour {
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Bullet"))
            other.gameObject.SendMessage("DestroyBullet");
        else if (other.gameObject.CompareTag("BulletTrail"))
            Destroy(other.gameObject);
    }
}
