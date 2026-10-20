using System.Reflection;
using System.Runtime.CompilerServices;
using FloatSoda.Widgets.Paint;

namespace FloatSoda.Test.Widgets;

public class ColoredBoxFlutterParityTest
{
    [Fact]
    public void Color_指定を省略_必須プロパティとして拒否する()
    {
        var property = typeof(ColoredBox).GetProperty(nameof(ColoredBox.Color));

        Assert.NotNull(property);
        Assert.True(property.IsDefined(typeof(RequiredMemberAttribute)));
    }
}
