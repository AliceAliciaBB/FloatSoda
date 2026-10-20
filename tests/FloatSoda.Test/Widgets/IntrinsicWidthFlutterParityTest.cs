using FloatSoda.Widgets.Layout;

namespace FloatSoda.Test.Widgets;

public class IntrinsicWidthFlutterParityTest
{
    [Fact]
    public void StepWidth_0を指定_丸めを無効にする()
    {
        var widget = new IntrinsicWidth { StepWidth = 0 };

        Assert.Null(widget.CreateRenderObject().StepWidth);
    }
}
