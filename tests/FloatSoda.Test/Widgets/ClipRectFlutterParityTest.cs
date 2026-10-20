using FloatSoda.Geometrics;
using FloatSoda.Rendering.Layers;
using FloatSoda.RenderObjects.Painting;
using FloatSoda.Testing;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using SkiaSharp;

namespace FloatSoda.Test.Widgets;

public class ClipRectFlutterParityTest
{
    private static readonly WidgetBitmapRenderer Renderer = new();

    [Fact]
    public void ClipBehavior_ClipRectで指定を省略_HardEdgeを使用する()
    {
        var renderObject = new ClipRect().CreateRenderObject();

        Assert.Equal(Clip.HardEdge, renderObject.ClipBehavior);
    }

    [Fact]
    public void ClipBehavior_Noneを指定_子を切り抜かず描画する()
    {
        using var bitmap = Renderer.Render(
            new SizedBox
            {
                Width = 20,
                Height = 20,
                Child = new ClipRect
                {
                    ClipBehavior = Clip.None,
                    Clipper = new TenPixelClipper(),
                    Child = new ColoredBox { Color = new Color(255, 0, 0) }
                }
            },
            new SKSizeI(20, 20));

        Assert.Equal(SKColors.Red, bitmap.GetPixel(15, 15));
    }

    private sealed class TenPixelClipper : CustomClipper<SKRect>
    {
        public override SKRect GetClip(SKSize size) => SKRect.Create(0, 0, 10, 10);

        public override bool ShouldReclip(CustomClipper<SKRect> oldClipper) => false;
    }
}
