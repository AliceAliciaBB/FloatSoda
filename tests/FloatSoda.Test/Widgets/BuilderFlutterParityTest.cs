using FloatSoda.Elements;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;

namespace FloatSoda.Test.Widgets;

public class BuilderFlutterParityTest
{
    [Fact]
    public void Build_値が等しい新しいBuilderへ更新_もう一度呼び出す()
    {
        var buildCount = 0;
        Widget Build(IBuildContext context)
        {
            buildCount++;
            return new SizedBox();
        }

        var owner = new BuildOwner(() => { });
        var view = new RenderView();
        var root = new RenderObjectToWidgetAdapter
        {
            Container = view,
            Child = new Builder { ChildBuilder = Build }
        }.AttachToRenderTree(owner, null);
        Assert.Equal(1, buildCount);

        _ = new RenderObjectToWidgetAdapter
        {
            Container = view,
            Child = new Builder { ChildBuilder = Build }
        }.AttachToRenderTree(owner, root);
        owner.BuildScope();

        Assert.Equal(2, buildCount);
    }
}
