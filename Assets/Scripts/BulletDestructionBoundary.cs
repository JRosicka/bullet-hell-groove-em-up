using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BulletDestructionBoundary : MonoBehaviour {
    private void OnTriggerEnter2D(Collider2D other) {
        if (!other.gameObject.CompareTag("Bullet"))
            return;

        other.gameObject.SendMessage("DestroyBullet");
    }
}
