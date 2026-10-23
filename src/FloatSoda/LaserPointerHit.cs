using FloatSoda.OVR.Overlay;

namespace FloatSoda;

/// <summary><see cref="FloatSodaApp.OnLaserPointer"/> で通知される、レーザーの交差判定の結果です。</summary>
/// <param name="WindowTitle">判定したウィンドウの <see cref="Widgets.WindowWidget.Title"/>。</param>
/// <param name="Device">レイを飛ばしたコントローラー。</param>
/// <param name="Hit">交差結果。外れた場合は <see langword="null"/>。<see cref="OverlayHit.Distance"/> はコントローラーから交点までの距離（メートル）です。</param>
public readonly record struct LaserPointerHit(string WindowTitle, TrackedDevice Device, OverlayHit? Hit);
