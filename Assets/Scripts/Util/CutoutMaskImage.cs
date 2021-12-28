using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

/// <summary>
/// Intended to invert the normal behavior of an image with regard to masking behavior. A use case for this is to
/// attach this component to a gameObject that is a child of a mask in order to invert the mask's effects on that child.
/// </summary>
public class CutoutMaskImage : Image {
    private static readonly int StencilComp = Shader.PropertyToID("_StencilComp");
    public override Material materialForRendering {
        get {
            Material newMaterial = new Material(base.materialForRendering);
            newMaterial.SetInt(StencilComp, (int) CompareFunction.NotEqual);
            return newMaterial;
        } 
    }
}
