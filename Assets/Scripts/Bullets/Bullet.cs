using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Bullet : MonoBehaviour {
    public float speed;
    public List<SpriteRenderer> Sprites;
    public bool IsFollowingSpline;
    public bool IsSpeedLogicInterrupted;
    public bool IsRotationInterrupted;

    private List<BulletLogic> bulletLogic;

    public int SortingOrder;

    private void Awake() {
        SortingOrder = GameController.Instance.GetNewSortingOrder();
        foreach (SpriteRenderer spriteObject in Sprites) {
            // TODO: We should probably also assign the sortingLayer to "Bullets" here
            spriteObject.sortingOrder = SortingOrder;
        }
    }

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
