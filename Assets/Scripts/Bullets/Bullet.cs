using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Bullet : MonoBehaviour {
    public float speed;
    public GameObject Sprite;

    // TODO: How do we apply this? Do we get any update info from each entry on Update(), or do we do something to apply
    // the logic right when it is assigned?
    private List<BulletLogic> bulletLogic;
    
    void Update() {
        transform.Translate(Vector3.right * (speed * Time.deltaTime));
    }

    public void AssignBulletLogic(List<BulletLogic> logic) {
        bulletLogic = logic;
        foreach (BulletLogic log in bulletLogic) {
            log.OnBulletSpawned(this);
        }
    }
}
