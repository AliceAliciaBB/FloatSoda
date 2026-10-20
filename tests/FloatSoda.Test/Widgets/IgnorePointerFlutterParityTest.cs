using FloatSoda.Widgets.Gesture;

namespace FloatSoda.Test.Widgets;

public class IgnorePointerFlutterParityTest
{
    [Fact]
    public void Ignoring_指定を省略_ポインター入力を無視する()
    {
        var renderObject = new IgnorePointer().CreateRenderObject();

        Assert.True(renderObject.Ignoring);
    }
}
