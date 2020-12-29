using UnityEngine;

/// <summary>
/// <see cref="BulletLogic"/> for animating the fade-in effect of a bullet as it is spawning. This instantiates a copy
/// of the passed-in <see cref="AnimateInBulletView"/> and attaches it to the bullet. The animator of the passed-in
/// <see cref="AnimateInBulletView"/> is used as the fade-in animation. 
/// </summary>
public class AnimateInBulletLogic : BulletLogic {
    private AnimateInBulletView view;
    private AnimateInBulletView viewPrefab;
    private bool useWhiteShader;
    
    public AnimateInBulletLogic(AnimateInBulletView viewPrefab, bool useWhiteShader) {
        this.viewPrefab = viewPrefab;
        this.useWhiteShader = useWhiteShader;
    }
    
    public override void OnBulletSpawned(Bullet bullet) {
        // Spawn the view
        view = Object.Instantiate(viewPrefab, bullet.transform);
        
        // Set the view's sprite local scale to match that of the bullet's sprite
        view.Sprite.transform.localScale = bullet.Sprite.transform.localScale;
        
        // Set the view's sprite to match that of the bullet's sprite
        SpriteRenderer spawnedRenderer = view.Sprite.GetComponent<SpriteRenderer>();
        spawnedRenderer.sprite = bullet.Sprite.GetComponent<SpriteRenderer>().sprite;

        if (useWhiteShader) {
            // Set the view's sprite's shader to be white
            spawnedRenderer.material.shader = BulletAnimationUtil.ShaderGUIText;
            spawnedRenderer.color = Color.white;
        }
    }
}
