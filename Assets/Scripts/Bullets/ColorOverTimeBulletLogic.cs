using UnityEngine;

public class ColorOverTimeBulletLogic : BulletLogic {
    private Color originalColor;
    private Color destinationColor;
    private AnimationCurve colorCurve;
    private float elapsedTime;
    private float colorAdjustmentDuration;
    private bool onlyAdjustAlpha;
    
    public ColorOverTimeBulletLogic(Color originalColor, Color destinationColor, AnimationCurve colorCurve, 
            float colorAdjustmentDuration, bool onlyAdjustAlpha) {
        this.originalColor = originalColor;
        this.destinationColor = destinationColor;
        this.colorCurve = colorCurve;
        this.colorAdjustmentDuration = colorAdjustmentDuration;
        this.onlyAdjustAlpha = onlyAdjustAlpha;
    }
    
    public override void OnBulletSpawned(Bullet bullet) {
        UpdateColor(bullet, originalColor);

        elapsedTime = 0;
    }

    public override void BulletLogicUpdate(Bullet bullet, float deltaTime) {
        elapsedTime += deltaTime;
        Color currentColor = Color.Lerp(originalColor, destinationColor,
            colorCurve.Evaluate(elapsedTime / colorAdjustmentDuration));
        
        UpdateColor(bullet, currentColor);
    }
    
    public void UpdateColor(Bullet bullet, Color newColor) {
        foreach (GameObject spriteObject in bullet.Sprites) {
            SpriteRenderer renderer = spriteObject.GetComponent<SpriteRenderer>();
            if (onlyAdjustAlpha) {
                float alpha = newColor.a;
                newColor = renderer.color;
                newColor.a = alpha;
            }

            renderer.color = newColor;
        }
    }

    public override void OnBulletDestroyed(Bullet bullet) {
        // Nothing to do here
    }
}
