using FloatSoda.RenderObjects.Layout;

namespace FloatSoda.Widgets.Layout;

/// <summary>子の自然な最大幅に合わせて幅を決めます。</summary>
/// <remarks>
/// intrinsic測定は追加のレイアウト走査を行い、入れ子では最悪O(N²)になり得ます。
/// スクロール領域や大規模ツリーでは使用せず、可能なら<see cref="SizedBox"/>や<see cref="ConstrainedBox"/>を使用してください。
/// </remarks>
public sealed record IntrinsicWidth : SingleChildRenderObjectWidget<RenderIntrinsicWidth>
{
    /// <summary>自然な幅を切り上げる単位を取得します。</summary>
    /// <value>0以上の有限値。丸めない場合は<see langword="null"/>または0です。</value>
    /// <remarks>0はRenderObjectへ渡す前に<see langword="null"/>へ置き換えます。Flutter の <c>IntrinsicWidth</c> に合わせています。</remarks>
    /// <exception cref="ArgumentOutOfRangeException">値が負数または非有限値です。</exception>
    public double? StepWidth
    {
        get;
        init
        {
            if (value != 0) RenderIntrinsicWidth.ValidateStep(value, nameof(StepWidth));
            field = value;
        }
    }

    private double? EffectiveStepWidth => StepWidth == 0 ? null : StepWidth;

    /// <inheritdoc/>
    public override RenderIntrinsicWidth CreateRenderObject() => new() { StepWidth = EffectiveStepWidth };

    /// <inheritdoc/>
    public override void UpdateRenderObject(RenderIntrinsicWidth renderObject) => renderObject.StepWidth = EffectiveStepWidth;
}
