/// <summary>
/// A single piece of a <see cref="BackgroundScroller"/>, like a single image or single particle system
/// </summary>
public interface IBackgroundElement {
    public void FadeOut(float fadeOutTime);
    public void FadeIn(float fadeInTime);
}