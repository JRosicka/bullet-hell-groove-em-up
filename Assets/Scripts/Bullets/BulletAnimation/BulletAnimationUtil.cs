using UnityEngine;

/// <summary>
/// Util class for getting various animation and shader resources for bullets
/// </summary>
public static class BulletAnimationUtil {
    private static Shader _shaderGUIText;
    public static Shader ShaderGUIText {
        get {
            if (_shaderGUIText == null)
                _shaderGUIText = Shader.Find("GUI/Text Shader");
            return _shaderGUIText;
        }
    }

    private static Shader _shaderSpritesDefault;
    public static Shader ShaderSpritesDefault {
        get {
            if (_shaderSpritesDefault == null)
                _shaderSpritesDefault = Shader.Find("Sprites/Default");
            return _shaderSpritesDefault;
        }
    }
}
