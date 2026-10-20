← [ガイド](../../Home.md)

# 自分の部品を作る

画面のコードで同じ組み合わせを何度も書いているなら、その組み合わせに名前を付けて、自分の部品にまとめられます。Unity でよく使う形をプレハブにしておくのと同じ使い方を、C# のコードだけで書けます。このページでは、ラベルの付いた札を例に、状態を持たない部品、外から値を渡す部品、押すと開閉する部品の順に作ります。

## どちらで作るかを決める

自分の部品は、`StatelessWidget` か `StatefulWidget<T>` を継承して作ります。どちらにするかは、部品の中で値が変わるかどうかで決めます。

| 部品の見た目を決めるもの | 継承する型 | 例 |
| :-- | :-- | :-- |
| 外から渡された値だけ | `StatelessWidget` | ラベルと色の付いた札 |
| 部品の中で変わる値(押すと開く、数が増える) | `StatefulWidget<T>` | 押すたびに開閉する札 |

迷ったら `StatelessWidget` で作り始め、部品の中で値を変えたくなったら `StatefulWidget<T>` に書き換えます。値が変わると画面が変わる仕組みは、[値が変わると、画面が変わる仕組み](../mechanics/state-and-rebuild.md)で説明しています。

このページで作るのは、今ある部品を組み合わせた部品です。自分で絵を描く部品は、Flutter の `CustomPaint` に当たる仕組みがまだないため、作れません(追跡: #171)。できないことの一覧は、リファレンスの[できないこと・まだ使えないもの](../../../reference/limitations.md)にあります。

## 前提条件

次の2つを済ませてください。

- [最初のオーバーレイを作る](../../../tutorials/first-overlay.md)を終え、ダッシュボードにオーバーレイを出せること
- 部品を置くファイルを、1つの部品につき1つ作ること(例: `NameTag.cs`)

このページの例は、プロジェクトの名前空間を `MyOverlay` としています。自分のプロジェクトの名前空間に読み替えてください。部品のファイルの先頭には、次の `using` が要ります。

```csharp
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using SkiaSharp;
```

押す操作を受け取る部品(手順3から)は、`using FloatSoda.Widgets.Gesture;` も足します。

## 手順1: 状態を持たない部品を作る

何度も書いている組み合わせを、`StatelessWidget` を継承した `record` に移します。組み合わせは、`Build` から返します。

次の例は、背景の色と余白と文字を組み合わせた札です。

```csharp
namespace MyOverlay;

public sealed record NameTag : StatelessWidget
{
    public override Widget Build(IBuildContext context)
    {
        return new ColoredBox
        {
            Color = SKColors.SeaGreen,
            Child = new Padding
            {
                Spacing = EdgeInsets.All(16),
                Child = new Text("VRChat")
                {
                    Style = new TextStyle { Color = SKColors.White, FontSize = 40 }
                }
            }
        };
    }
}
```

元の画面では、組み合わせを書いていた場所を `new NameTag()` に置き換えます。

```csharp
app.CreateWindow(new DashboardWindow
{
    Title = "MyOverlay",
    Child = new NameTag(),
});
```

**できた?** 部品に置き換える前と同じ見た目で、ダッシュボードに札が出る。

<details>
<summary>できなかったら</summary>

何も出ないときや、見た目が変わったときは、[反応しないときに切り分ける](../../5-troubleshoot/not-responding.md#文字や画像が出ない)の「文字や画像が出ない」を開きます。

</details>

## 手順2: 外から値を渡す

手順1の札は、文字と色が決め打ちです。部品を使う側から値を渡せるように、値を `init` のプロパティにします。必ず渡してほしい値には `required` を付けます。

```csharp
public sealed record NameTag : StatelessWidget
{
    /// <summary>札に出す文字。</summary>
    public required string Label { get; init; }

    /// <summary>札の背景の色。</summary>
    public SKColor Color { get; init; } = SKColors.SeaGreen;

    public override Widget Build(IBuildContext context)
    {
        return new ColoredBox
        {
            Color = Color,
            Child = new Padding
            {
                Spacing = EdgeInsets.All(16),
                Child = new Text(Label)
                {
                    Style = new TextStyle { Color = SKColors.White, FontSize = 40 }
                }
            }
        };
    }
}
```

使う側は、オブジェクト初期化子で値を渡します。FloatSoda の部品は、どれもコンストラクタの引数ではなく、プロパティで値を受け取ります。自分の部品も同じ形にしておくと、FloatSoda の部品と同じ書き方で並べられます。

```csharp
new Row
{
    Children =
    {
        new NameTag { Label = "VRChat" },
        new NameTag { Label = "Booth", Color = SKColors.Tomato },
    },
}
```

`List<T>` のような参照型の値をプロパティで渡すときは、値を変えるたびに新しいリストを作って渡します。部品は `record` なので、作り直しのときに前と同じ値の部品は、等しいとみなされて作り直されません。リストは中身ではなく参照で比べられるため、同じリストの中身だけを書き換えて渡すと、部品の見た目は変わりません。

```csharp
// 変わらない: 同じリストの中身だけを書き換えている
SetState(() => _items.Add("Booth"));

// 変わる: 新しいリストを作って渡している
SetState(() => _items = [.. _items, "Booth"]);
```

**できた?** 文字と色の違う札が、2つ並んで出る。

## 手順3: 状態を持つ部品を作る

押すと開閉する札のように、部品の中で値が変わる部品は、`StatefulWidget<T>` で作ります。`StatefulWidget<T>` を継承した `record` には外から渡す値を置き、`State<T>` を継承した `class` には部品の中で変わる値を置きます。状態の中から自分のプロパティを読むときは、`Widget` を使います。

```csharp
public sealed record FoldableTag : StatefulWidget<FoldableTag>
{
    /// <summary>札に出す見出し。</summary>
    public required string Title { get; init; }

    /// <summary>開いたときに出す説明。</summary>
    public required string Detail { get; init; }

    public override State<FoldableTag> CreateState() => new FoldableTagState();
}

public sealed class FoldableTagState : State<FoldableTag>
{
    private bool _open;

    public override Widget Build(IBuildContext context)
    {
        var children = new List<Widget>
        {
            new NameTag { Label = Widget!.Title },
        };
        if (_open)
        {
            children.Add(new Text(Widget.Detail)
            {
                Style = new TextStyle { Color = SKColors.White, FontSize = 30 }
            });
        }

        return new GestureDetector
        {
            OnTap = () => SetState(() => _open = !_open),
            Child = new Column { Children = children },
        };
    }
}
```

値を変えるときは、`SetState()` の中で変えます。`SetState()` の外で変えた値は、画面に出ません。

状態を作ったときに一度だけ行う準備は `InitState()` に、部品が画面から外れるときの後片付けは `Dispose()` に書きます。どちらも `State<T>` を継承した `class` で上書きします。

押せるのは、ダッシュボードに出したオーバーレイだけです(追跡: #182)。押せる範囲の決め方は、[ボタンやトグルを組み立てる](build-buttons-toggles.md)で説明しています。

**できた?** 札を押すたびに、説明が出たり隠れたりする。

<!-- ※要確認: FoldableTag を押したときの開閉を、HMD のダッシュボードで確かめていない。コードは公開 API だけでビルドが通ることを確かめた -->

<details>
<summary>できなかったら</summary>

押しても変わらないときは、[反応しないときに切り分ける](../../5-troubleshoot/not-responding.md#数字や文字が変わらない)の「数字や文字が変わらない」を開きます。

</details>

## 手順4: 押されたことを外へ知らせる

部品の中で起きたことを、部品を使う側で受け取るには、`On` で始まる名前のプロパティを持たせます。値を渡さないときは `Action?`、値を渡すときは `Action<T>?` にします。

手順3の `FoldableTag` に、開閉を知らせるプロパティを足します。開いたら `true`、閉じたら `false` を渡します。

```csharp
public sealed record FoldableTag : StatefulWidget<FoldableTag>
{
    public required string Title { get; init; }
    public required string Detail { get; init; }

    /// <summary>開いたり閉じたりしたときに呼ばれる。開いたら true を渡す。</summary>
    public Action<bool>? OnToggled { get; init; }

    public override State<FoldableTag> CreateState() => new FoldableTagState();
}
```

状態の側では、押したときに値を変えたあとで、`Widget!.OnToggled` を呼びます。使う側が何も渡していないときに備えて、`?.Invoke` で呼びます。

```csharp
private void Toggle()
{
    SetState(() => _open = !_open);
    Widget!.OnToggled?.Invoke(_open);
}
```

`Build` の `GestureDetector` は、`OnTap = Toggle` に書き換えます。使う側では、受け取った値で自分の状態を変えます。次の例は、札を開いた回数を数える画面です。

```csharp
public sealed record TagScreen : StatefulWidget<TagScreen>
{
    public override State<TagScreen> CreateState() => new TagScreenState();
}

public sealed class TagScreenState : State<TagScreen>
{
    private int _opened;

    public override Widget Build(IBuildContext context)
    {
        return new Column
        {
            Children =
            {
                new FoldableTag
                {
                    Title = "FaceEmo",
                    Detail = "表情を切り替える",
                    OnToggled = open =>
                    {
                        if (open) SetState(() => _opened++);
                    },
                },
                new Text($"開いた回数: {_opened}")
                {
                    Style = new TextStyle { Color = SKColors.White, FontSize = 30 }
                },
            },
        };
    }
}
```

**できた?** 札を開くたびに、「開いた回数」の数字が1つ増える。

<!-- ※要確認: TagScreen の数字が増えることを、HMD のダッシュボードで確かめていない。コードは公開 API だけでビルドが通ることを確かめた -->

## 結果を確かめる

作った部品について、次の3点を確かめます。

1. 別の画面でも、同じ部品を `new NameTag { ... }` のように呼んで使える
2. 値の違う部品を並べると、それぞれの値で出る
3. 部品を押したことが、`On` で始まるプロパティを通して、使う側に届く

見た目だけなら、HMD を被らずに確かめられます。方法は、[HMD を被らずに確かめる](../input/test-without-hmd.md)にあります。

## うまくいかないとき

部品にしたら動かなくなったときは、症状に合わせて次を確かめます。

| 症状 | 確かめること |
| :-- | :-- |
| 値を渡しても見た目が変わらない | 前と同じ値を渡していないか。`List<T>` なら、新しいリストを作って渡しているか(手順2) |
| 同じ種類の部品を並べ替えると、開いた状態が別の部品に移る | 部品に `Key` を付けて、どの部品かを区別できるようにする |
| 押しても反応しない | [反応しないときに切り分ける](../../5-troubleshoot/not-responding.md#ボタンを押しても反応しない)の「ボタンを押しても反応しない」 |

<!-- ※要確認: Key を付けないと並べ替えで状態が取り違えられる例を、今の実装で再現していない -->

自分で絵を描く部品を作りたいときは、`CustomPaint` に当たる仕組みがまだないため、作れません(追跡: #171)。ここまでで解決しないときは、ガイドの[docs で見つからないとき](../../Home.md#docs-で見つからないとき)を開いてください。
