using FloatSoda.Abstractions.Geometries;
using FloatSoda.Abstractions.Input;
using FloatSoda.OVR;
using FloatSoda.OVR.Overlay;

namespace FloatSoda.Engine;

/// <summary>
/// コントローラーのレイとオーバーレイの交差を <see cref="ControllerPointerSystem"/> で毎フレーム計算し、
/// <see cref="RawPointerEvent"/> に変換する入力源。
/// SteamVR の入力横取り（<c>MakeOverlaysInteractiveIfVisible</c>）を使わないため、ダッシュボード以外の
/// ワールド固定・デバイス追従オーバーレイで使います。トリガーは <see cref="PointerButton.Left"/> として扱います。
/// イベントは <see cref="Poll"/> を呼んだスレッドで発火します。
/// </summary>
public sealed class ControllerRayRawPointerSource : IRawPointerSource
{
    private readonly ControllerPointerSystem? _pointerSystem;
    private bool _inside;
    private bool _pressed;
    private Offset _position;

    /// <inheritdoc />
    public event Action<RawPointerEvent>? OnPointerEvent;

    /// <summary>
    /// <see cref="Poll"/> のたびに、交差判定の結果を通知します。外れた場合は <see langword="null"/> です。
    /// レーザーの見た目（長さなど）を交差距離に合わせる用途を想定しています。
    /// </summary>
    public event Action<OverlayHit?>? OnRayCast;

    /// <summary>判定に使うコントローラーを取得します。</summary>
    public TrackedDevice Device { get; }

    /// <summary>指定したオーバーレイを、指定コントローラーのレイで判定する入力源を作成します。</summary>
    /// <param name="intersection">判定対象オーバーレイの交差判定。</param>
    /// <param name="device">レイを飛ばすコントローラー。</param>
    /// <param name="aimRotation">コントローラー基準でレイの向きを回転させる補正。<see cref="ControllerPointerSystem.AimRotation"/> を参照。</param>
    public ControllerRayRawPointerSource(OverlayIntersection intersection, TrackedDevice device,
        System.Numerics.Quaternion aimRotation = default)
    {
        _pointerSystem = new ControllerPointerSystem(intersection)
        {
            AimRotation = aimRotation == default ? System.Numerics.Quaternion.Identity : aimRotation
        };
        Device = device;
    }

    /// <summary>OpenVR を使わずに状態遷移だけを確かめるための入力源を作成します。</summary>
    internal ControllerRayRawPointerSource(TrackedDevice device)
    {
        Device = device;
    }

    /// <summary>レイの交差判定を行い、変化をイベントとして流します。メインスレッドから毎フレーム呼びます。</summary>
    /// <param name="textureSize">オーバーレイのテクスチャのピクセルサイズ。UV をピクセル座標へ換算するのに使います。</param>
    public void Poll(SkiaSharp.SKSizeI textureSize)
    {
        if (_pointerSystem is null) return;

        var data = _pointerSystem.Poll(Device);
        var hit = _pointerSystem.LastHit;
        OnRayCast?.Invoke(hit);
        Process(data, textureSize);
    }

    /// <summary>1フレーム分の判定結果から Enter / Move / ButtonDown / ButtonUp / Leave を生成します。</summary>
    /// <param name="data">判定結果。外れた場合は <see langword="null"/>。</param>
    /// <param name="textureSize">オーバーレイのテクスチャのピクセルサイズ。</param>
    internal void Process(PointerData? data, SkiaSharp.SKSizeI textureSize)
    {
        if (data is not { } hit)
        {
            if (!_inside) return;

            _inside = false;
            _pressed = false;
            Raise(RawPointerKind.Leave, _position);
            return;
        }

        var position = ToLocalPosition(hit, textureSize);

        if (!_inside)
        {
            _inside = true;
            _position = position;
            Raise(RawPointerKind.Enter, position);
        }

        if (position != _position)
        {
            _position = position;
            Raise(RawPointerKind.Move, position);
        }

        switch (hit.Change)
        {
            case PointerChange.Down when !_pressed:
                _pressed = true;
                Raise(RawPointerKind.ButtonDown, position);
                break;
            case PointerChange.Up when _pressed:
                _pressed = false;
                Raise(RawPointerKind.ButtonUp, position);
                break;
        }
    }

    /// <summary>
    /// UV（0〜1）を描画先ローカル座標（原点左上のピクセル）へ換算します。
    /// 本プロジェクトの OpenGL テクスチャ（ボトムアップ格納）では SteamVR の表示時 v 軸反転と相殺され、
    /// V は上端が 0 で届くことを実機で確認済みのため反転しません（反転すると上下鏡映になる）。
    /// <see cref="OverlayMouseRawPointerSource"/> のマウス座標と同じ事情です。
    /// </summary>
    internal static Offset ToLocalPosition(PointerData data, SkiaSharp.SKSizeI textureSize) =>
        new(data.X * textureSize.Width, data.Y * textureSize.Height);

    private void Raise(RawPointerKind kind, Offset position) =>
        OnPointerEvent?.Invoke(new RawPointerEvent(kind, position));

    /// <inheritdoc />
    public void Dispose()
    {
        OnPointerEvent = null;
        OnRayCast = null;
    }
}
