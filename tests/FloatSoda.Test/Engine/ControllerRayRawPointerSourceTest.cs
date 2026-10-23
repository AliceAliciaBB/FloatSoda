using FloatSoda.Abstractions.Geometries;
using FloatSoda.Abstractions.Input;
using FloatSoda.Engine;
using FloatSoda.OVR;
using FloatSoda.OVR.Overlay;
using SkiaSharp;

namespace FloatSoda.Test.Engine;

public class ControllerRayRawPointerSourceTest
{
    private static readonly SKSizeI TextureSize = new(800, 600);

    private static (ControllerRayRawPointerSource Source, List<RawPointerEvent> Events) CreateSource()
    {
        var source = new ControllerRayRawPointerSource(TrackedDevice.LeftController);
        var events = new List<RawPointerEvent>();
        source.OnPointerEvent += events.Add;
        return (source, events);
    }

    [Fact]
    public void ToLocalPosition_UVが原点_左上端を返す()
    {
        var position = ControllerRayRawPointerSource.ToLocalPosition(new PointerData(0f, 0f, PointerChange.Hover), TextureSize);

        Assert.Equal(new Offset(0, 0), position);
    }

    [Fact]
    public void ToLocalPosition_UVが右下_右下端を返す()
    {
        var position = ControllerRayRawPointerSource.ToLocalPosition(new PointerData(1f, 1f, PointerChange.Hover), TextureSize);

        Assert.Equal(new Offset(800, 600), position);
    }

    [Fact]
    public void ToLocalPosition_UVが中央_テクスチャ中央を返す()
    {
        var position = ControllerRayRawPointerSource.ToLocalPosition(new PointerData(0.5f, 0.5f, PointerChange.Hover), TextureSize);

        Assert.Equal(new Offset(400, 300), position);
    }

    [Fact]
    public void Process_外れたまま_イベントを出さない()
    {
        var (source, events) = CreateSource();

        source.Process(null, TextureSize);

        Assert.Empty(events);
    }

    [Fact]
    public void Process_初めて当たった_Enterだけを出す()
    {
        var (source, events) = CreateSource();

        source.Process(new PointerData(0.5f, 0.5f, PointerChange.Hover), TextureSize);

        Assert.Equal([new RawPointerEvent(RawPointerKind.Enter, new Offset(400, 300))], events);
    }

    [Fact]
    public void Process_当たったまま位置が変わった_Moveを出す()
    {
        var (source, events) = CreateSource();
        source.Process(new PointerData(0.5f, 0.5f, PointerChange.Hover), TextureSize);
        events.Clear();

        source.Process(new PointerData(0.25f, 0.5f, PointerChange.Hover), TextureSize);

        Assert.Equal([new RawPointerEvent(RawPointerKind.Move, new Offset(200, 300))], events);
    }

    [Fact]
    public void Process_当たったまま位置が同じ_イベントを出さない()
    {
        var (source, events) = CreateSource();
        source.Process(new PointerData(0.5f, 0.5f, PointerChange.Hover), TextureSize);
        events.Clear();

        source.Process(new PointerData(0.5f, 0.5f, PointerChange.Hover), TextureSize);

        Assert.Empty(events);
    }

    [Fact]
    public void Process_トリガーを押して離した_ButtonDownとButtonUpを出す()
    {
        var (source, events) = CreateSource();
        source.Process(new PointerData(0.5f, 0.5f, PointerChange.Hover), TextureSize);
        events.Clear();

        source.Process(new PointerData(0.5f, 0.5f, PointerChange.Down), TextureSize);
        source.Process(new PointerData(0.5f, 0.5f, PointerChange.Up), TextureSize);

        Assert.Equal(
            [
                new RawPointerEvent(RawPointerKind.ButtonDown, new Offset(400, 300)),
                new RawPointerEvent(RawPointerKind.ButtonUp, new Offset(400, 300)),
            ],
            events);
    }

    [Fact]
    public void Process_押していないのにUp_ButtonUpを出さない()
    {
        var (source, events) = CreateSource();
        source.Process(new PointerData(0.5f, 0.5f, PointerChange.Hover), TextureSize);
        events.Clear();

        source.Process(new PointerData(0.5f, 0.5f, PointerChange.Up), TextureSize);

        Assert.Empty(events);
    }

    [Fact]
    public void Process_当たっていた状態から外れた_最後の位置でLeaveを出す()
    {
        var (source, events) = CreateSource();
        source.Process(new PointerData(0.5f, 0.5f, PointerChange.Hover), TextureSize);
        events.Clear();

        source.Process(null, TextureSize);

        Assert.Equal([new RawPointerEvent(RawPointerKind.Leave, new Offset(400, 300))], events);
    }

    [Fact]
    public void Process_押したまま外れて再び当たった_押下状態を引き継がずにEnterからやり直す()
    {
        var (source, events) = CreateSource();
        source.Process(new PointerData(0.5f, 0.5f, PointerChange.Hover), TextureSize);
        source.Process(new PointerData(0.5f, 0.5f, PointerChange.Down), TextureSize);
        source.Process(null, TextureSize);
        events.Clear();

        source.Process(new PointerData(0.5f, 0.5f, PointerChange.Up), TextureSize);

        Assert.Equal([new RawPointerEvent(RawPointerKind.Enter, new Offset(400, 300))], events);
    }
}
