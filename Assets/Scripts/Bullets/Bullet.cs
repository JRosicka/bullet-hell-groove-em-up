using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Bullet : MonoBehaviour {
    public float speed;
    public List<GameObject> Sprites;
    public bool IsFollowingSpline;
    public bool IsSpeedLogicInterrupted;

    // TODO: How do we apply this? Do we get any update info from each entry on Update(), or do we do something to apply
    // the logic right when it is assigned?
    private List<BulletLogic> bulletLogic;
    
    void Update() {
        float deltaTime = Time.deltaTime;
        foreach (BulletLogic logicEntry in bulletLogic) {
            logicEntry.BulletLogicUpdate(this, deltaTime);
        }
    }

    public void AssignBulletLogic(List<BulletLogic> logic) {
        bulletLogic = logic;
        foreach (BulletLogic log in bulletLogic) {
            log.OnBulletSpawned(this);
        }
    }

    public void DestroyBullet() {
        foreach (BulletLogic log in bulletLogic) {
            log.OnBulletDestroyed(this);
        }
        Destroy(gameObject);
    }
}
