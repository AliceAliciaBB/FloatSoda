using FloatSoda.Widgets.Layout;

namespace FloatSoda.Test.Widgets;

public class AlignFlutterParityTest
{
    [Fact]
    public void WidthFactor_負数を指定_ArgumentOutOfRangeExceptionを投げる()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Align { WidthFactor = -1 });
    }
}
