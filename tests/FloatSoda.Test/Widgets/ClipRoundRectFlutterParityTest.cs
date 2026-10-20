using FloatSoda.Core;
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Rendering;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using SkiaSharp;

namespace FloatSoda.Test.Widgets;

public class ClipRoundRectFlutterParityTest
{
    [Fact]
    public void UpdateRenderObject_描画後にBorderRadiusを変更_角丸クリップを再描画する()
    {
        var view = new RenderView();
        var pipeline = new RenderPipeline { OnNeedVisualUpdate = () => { }, RenderView = view };
        var owner = new BuildOwner(() => { });
        var initial = new SizedBox
        {
            Width = 20,
            Height = 20,
            Child = new ClipRoundRect
            {
                BorderRadius = BorderRadius.Zero,
                Child = new ColoredBox { Color = new Color(255, 0, 0) }
            }
        };
        var root = new RenderObjectToWidgetAdapter { Container = view, Child = initial }
            .AttachToRenderTree(owner, null);
        view.PrepareInitialFrame();
        pipeline.FlushLayout();
        pipeline.FlushPaint();
        var layerRenderer = new LayerBitmapRenderer();
        using var before = layerRenderer.Render(view.Layer!.Clone(), new SKSizeI(20, 20));

        new RenderObjectToWidgetAdapter
        {
            Container = view,
            Child = initial with
            {
                Child = new ClipRoundRect
                {
                    BorderRadius = BorderRadius.Circular(8),
                    Child = new ColoredBox { Color = new Color(255, 0, 0) }
                }
            }
        }.AttachToRenderTree(owner, root);
        owner.BuildScope();
        pipeline.FlushPaint();
        using var after = layerRenderer.Render(view.Layer!.Clone(), new SKSizeI(20, 20));

        Assert.Equal(SKColors.Red, before.GetPixel(0, 0));
        Assert.NotEqual(before.GetPixel(0, 0), after.GetPixel(0, 0));
    }
}
