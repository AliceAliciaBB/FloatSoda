← [ガイド](../../Home.md)

# 見た目を整える

## この手順でできること

文字の色や大きさ、背景、角丸、余白は、すべて C# のコードで指定します。見た目は、C# のウィジェットで指定します。シーンやプレハブを編集する手順は要りません。AI に頼むときも、自分で直すときも、同じコードの同じ場所を直せば、見た目が変わります。

この手順では、4つを順に足して、1枚のカードを作ります。終わると、HMD の中に、角が丸く、白い縁取りの付いた青紫色のカードが出ます。カードの中には、大きな太字のタイトルと、説明の文字が並びます。

1. 文字の色と大きさを指定する
2. 背景色を付けて、角を丸める
3. 余白を取る
4. 画面全体の文字書式を1か所で決める

## 前提条件

- [最初のオーバーレイを作る](../../../tutorials/first-overlay.md)を終え、ウィジェットを `Align` や `SizedBox` で組めること
- ソースの先頭に `using SkiaSharp;` があること
- ソースの先頭に、`FloatSoda.Painting`、`FloatSoda.Geometrics`、`FloatSoda.Widgets`、`FloatSoda.Widgets.Layout`、`FloatSoda.Widgets.Paint` の `using` があること

## 手順1: 文字の色と大きさを指定する

> 【未決】この節は判断が要るため、ト書きのまま残しています。オーナーが決めたあとで本文にします(追跡: #219)。

- 読み手が持って来るもの: 文字を出しているコード
- このページが渡すもの: `TextStyle` の指定方法と、`RichText` / `Text` のどちらで書くか
- 読み手がすること: 色、大きさ、太さを指定する
- できた?: HMD の中で文字の色と大きさが変わった
- 次へ進む先: 手順2
- 【未決】`Text` と `RichText` のどちらで書くか。見本の `CounterWidget.cs` は `RichText` を使うが、`Text` のほうが短い

<details>
<summary>仮に書いた本文(判断の材料。採用は未定)</summary>

文字は `Text` ウィジェットで出します。色、大きさ、太さは、`Style` に渡す `TextStyle` で指定します。

```csharp
new Text("タイトル")
{
    Style = new TextStyle
    {
        Color = SKColors.WhiteSmoke,
        FontSize = 56,
        FontWeight = 700,
    },
}
```

`FontWeight` は数値で、太字は 700 です。指定を省くと、大きさは 30、色は黒になります。色は `SKColors` の名前のほか、`new Color(124, 205, 255)` のように、赤、緑、青の値でも指定できます。

日本語の文字が出ないときは、`Font` にフォントの名前を指定します。`SystemFontProvider` は、`FloatSoda.Core.Providers` の名前空間にあります。

```csharp
Style = new TextStyle { Font = new SystemFontProvider("Meiryo") }
```

<!-- ※要確認: 日本語のフォントの指定が必要になる条件と、指定したフォントで日本語が出ること。samples/FloatSoda.Samples.Text で確かめる。実機で確かめる -->

**できた?** HMD の中で、文字の色と大きさが変わっていれば、手順1は完了です。

<!-- ※要確認: HMD の中で文字の色と大きさが変わること。実機で確かめる -->

</details>

## 手順2: 背景色を付けて、角を丸める

> 【未決】この節は判断が要るため、ト書きのまま残しています。オーナーが決めたあとで本文にします(追跡: #219)。

- 読み手が持って来るもの: 手順1のコード
- このページが渡すもの: `ColoredBox` で単色の背景を付ける方法と、角丸の付け方
- 読み手がすること: 文字の外側に背景と角丸を足す
- できた?: 文字の後ろに色が付き、角が丸い
- 次へ進む先: 手順3
- 【未決】角丸を `ClipRoundRect` で作るか、`DecoratedBox` で作るか。どちらか1つに決めて書く

<details>
<summary>仮に書いた本文(判断の材料。採用は未定)</summary>

背景と角丸は、`DecoratedBox` と `BoxDecoration` でまとめて指定します。`BoxDecoration` には、背景の色(`Color`)、角の丸み(`BorderRadius`)、縁取り(`Border`)を渡せます。

```csharp
new DecoratedBox
{
    Decoration = new BoxDecoration
    {
        Color = SKColors.SlateBlue,
        BorderRadius = BorderRadius.All(Radius.Circular(24)),
        Border = Border.All(new BorderSide { Color = SKColors.White, Width = 4 }),
    },
    Child = new Text("タイトル"),
}
```

角丸が要らず、単色の背景だけでよいときは、`ColoredBox { Color = ... }` が短く書けます。子の中身ごと角丸で切り抜きたいときは、`ClipRoundRect` を使います。

**できた?** 文字の後ろに色が付き、角が丸くなっていれば、手順2は完了です。

<!-- ※要確認: DecoratedBox の角丸と縁取りが HMD で期待どおりに描かれること。コードのコンパイルは確かめた。表示は実機で確かめる -->

</details>

## 手順3: 余白を取る

文字と背景の縁のあいだの余白は、`Padding` と `EdgeInsets` で取ります。次のコードの `decoration` には、手順2で作った `BoxDecoration` を渡します。`Padding` の余白は、`Spacing` に渡します。`DecoratedBox` の `Child` を `Padding` で包みます。

```csharp
new DecoratedBox
{
    Decoration = decoration,
    Child = new Padding
    {
        Spacing = EdgeInsets.Symmetric(vertical: 24, horizontal: 48),
        Child = new Text("タイトル"),
    },
}
```

四方向に同じ余白を取るなら `EdgeInsets.All(24)`、縦と横で変えるなら `EdgeInsets.Symmetric(vertical: 24, horizontal: 48)` です。

**できた?** 文字が背景の縁に張り付かず、余白が空いていれば、手順3は完了です。

## 手順4: 画面全体の文字書式を1か所で決める

`DefaultTextStyle` で画面の外側を包むと、子孫の `Text` すべてに、同じ文字書式が渡ります。文字が複数ある画面で、色や大きさを1か所で揃えられます。次のコードの `card` は、手順3までに組んだカードのウィジェットです。

```csharp
new DefaultTextStyle
{
    Style = new TextStyle { Color = SKColors.WhiteSmoke, FontSize = 36 },
    Child = card,
}
```

`Text` の `Style` を個別に指定すると、その指定が優先されます。優先順位は、`Text` に書いた指定、`DefaultTextStyle`、既定値の順です。指定は項目ごとに引き継がれるので、タイトルだけ大きくしたいときは、そのタイトルの `Text` にだけ `FontSize = 56` を書きます。色は `DefaultTextStyle` の値になります。

ここまでを合わせたカード全体は、次のとおりです。

```csharp
public record Card : StatelessWidget
{
    public override Widget Build(IBuildContext context) => new DefaultTextStyle
    {
        Style = new TextStyle { Color = SKColors.WhiteSmoke, FontSize = 36 },
        Child = new Align
        {
            Alignment = Alignment.Center,
            Child = new DecoratedBox
            {
                Decoration = new BoxDecoration
                {
                    Color = SKColors.SlateBlue,
                    BorderRadius = BorderRadius.All(Radius.Circular(24)),
                    Border = Border.All(new BorderSide { Color = SKColors.White, Width = 4 }),
                },
                Child = new Padding
                {
                    Spacing = EdgeInsets.Symmetric(vertical: 24, horizontal: 48),
                    Child = new Column
                    {
                        Children =
                        {
                            new Text("タイトル") { Style = new TextStyle { FontSize = 56, FontWeight = 700 } },
                            new Text("説明の文章"),
                        },
                    },
                },
            },
        },
    };
}
```

組み込みの、見た目をまとめて切り替える仕組み(テーマやデザインシステム)は、今はありません(追跡: #219)。この手順では、色や文字書式を、使う場所のコードで指定します。

起動のコードで、ウィンドウの `Child` を `new Card()` に置き換えます。

**できた?** 複数の文字の書式が、1か所の指定で揃っていれば、手順4は完了です。

## 結果を確かめる

HMD で、画面全体が指定どおりかを見ます。

- カードの背景色、角丸、縁取りが指定どおりに出ている
- タイトルが説明より大きく、太く出ている
- 文字が背景の縁に張り付いていない

HMD を被らずに確かめたいときは、[HMD を被らずに確かめる](../input/test-without-hmd.md)に進みます。見た目を整えたら、次は[アニメーションを付ける](add-animation.md)に進みます。

## うまくいかないとき

見た目が指定と違う、または文字が出ないときは、症状に合うページに進みます。大きさや位置が思ったとおりでない場合は[レイアウトの考え方](../../5-troubleshoot/layout-model.md)、例外が出る場合は[エラーを読んで直す](../../5-troubleshoot/read-errors.md)です。
