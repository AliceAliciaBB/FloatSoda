← [ガイド](../../Home.md)

# ボタンやトグルを組み立てる

## ボタンが効くオーバーレイの種類

今、コントローラーで押せるのは、ダッシュボードのオーバーレイだけです。`WorldSpaceWindow` と `DeviceTrackedWindow` は表示専用で、`GestureDetector` のコールバックが呼ばれません(追跡: #182)。ボタンやトグルを組み立てるときは、`DashboardWindow` に出します。

`Button` ウィジェットは、まだありません(追跡: #219)。この手順では、`GestureDetector` と見た目のウィジェットを組み合わせて、押せるボタンと、押すたびに切り替わるトグルを作ります。

自分のオーバーレイの種類は、`CreateWindow` に渡している型で確かめます。ダッシュボード以外に出したい場合は、[オーバーレイの種類と選び方](../mechanics/overlay-types.md)で、種類ごとの違いを確かめます。

## 前提条件

- [最初のオーバーレイを作る](../../../tutorials/first-overlay.md)を終え、ダッシュボードにオーバーレイが出ること
- `SetState()` で値を変えると画面が変わることを知っていること
- ソースの先頭に、`FloatSoda.Geometrics`、`FloatSoda.Widgets.Gesture`、`FloatSoda.Widgets.Layout`、`FloatSoda.Widgets.Paint`、`FloatSoda.Painting`、`SkiaSharp` の `using` があること

`SetState()` の仕組みが分からない場合は、[値が変わると、画面が変わる仕組み](../mechanics/state-and-rebuild.md)を先に読みます。

## 手順1: 押せるボタンを作る

見た目の四角を `GestureDetector` で包み、`OnTap` で `SetState()` を呼びます。`OnTap` は、押して離した時点で呼ばれます。押しただけでは呼ばれません。

```csharp
public record CountButton : StatefulWidget<CountButton>
{
    public override State<CountButton> CreateState() => new CountButtonState();
}

public class CountButtonState : State<CountButton>
{
    private int _count;

    public override Widget Build(IBuildContext context) => new GestureDetector
    {
        OnTap = () => SetState(() => _count++),
        Child = new ClipRoundRect
        {
            BorderRadius = BorderRadius.All(Radius.Circular(15)),
            Child = new SizedBox
            {
                Width = 300,
                Height = 100,
                Child = new ColoredBox
                {
                    Color = SKColors.CornflowerBlue,
                    Child = new Center
                    {
                        Child = new Text($"押した回数: {_count}")
                        {
                            Style = new TextStyle { Color = SKColors.WhiteSmoke, FontSize = 40, FontWeight = 700 },
                        },
                    },
                },
            },
        },
    };
}
```

起動のコードで、`DashboardWindow` の `Child` を `new CountButton()` に置き換えます。

**できた?** ダッシュボードで、コントローラーのレーザーを四角に当てて押し、離したときに押した回数が増えれば、手順1は完了です。

<!-- ※要確認: GestureDetector のボタンが、ダッシュボードのオーバーレイで実機で反応するか。HMD で確かめる -->

<details>
<summary>できなかったら</summary>

押しても反応しないときは、ウィンドウが `DashboardWindow` であることを確かめます。`WorldSpaceWindow` と `DeviceTrackedWindow` では、コントローラーのレーザー操作を受け取れません(追跡: #182)。それでも直らないときは、[反応しないときに切り分ける](../../5-troubleshoot/not-responding.md)に進みます。

</details>

## 手順2: 押すたびに切り替わるトグルを作る

トグルは、ボタンと同じ仕組みで作れます。`bool` の状態を `State` に持ち、`OnTap` で反転させ、状態に合わせて見た目を変えます。

```csharp
public record ToggleButton : StatefulWidget<ToggleButton>
{
    public override State<ToggleButton> CreateState() => new ToggleButtonState();
}

public class ToggleButtonState : State<ToggleButton>
{
    private bool _isOn;

    public override Widget Build(IBuildContext context) => new GestureDetector
    {
        OnTap = () => SetState(() => _isOn = !_isOn),
        Child = new ClipRoundRect
        {
            BorderRadius = BorderRadius.All(Radius.Circular(15)),
            Child = new SizedBox
            {
                Width = 300,
                Height = 100,
                Child = new ColoredBox
                {
                    Color = _isOn ? SKColors.SeaGreen : SKColors.Gray,
                    Child = new Center
                    {
                        Child = new Text(_isOn ? "ON" : "OFF")
                        {
                            Style = new TextStyle { Color = SKColors.White, FontSize = 40, FontWeight = 700 },
                        },
                    },
                },
            },
        },
    };
}
```

手順1の確認のあとは、`DashboardWindow` の `Child` を `new ToggleButton()` に置き換えます。

**できた?** 押すたびに、緑の `ON` と灰色の `OFF` が切り替われば、手順2は完了です。

<!-- ※要確認: トグルの表示が HMD で切り替わること。コードのコンパイルは確かめた。表示は HMD で確かめる -->

## 手順3: 押せる範囲を決める

> 【未決】この節は判断が要るため、ト書きのまま残しています。オーナーが決めたあとで本文にします(追跡: #219)。

- 読み手が持って来るもの: 余白が多いボタン
- このページが渡すもの: `Behaviour = HitTestBehaviour.Opaque` の意味と、押せない範囲ができる理由
- 読み手がすること: 余白の部分を押しても反応するように指定する
- できた?: ボタンの余白を押しても反応する
- 次へ進む先: 「結果を確かめる」
- 【未決】ドラッグ(`OnPanUpdate`)もこのページで扱うか。見本の `DragBoxWidget.cs` はあるが、読み手の目的は「ボタンやトグル」

<details>
<summary>仮に書いた本文(判断の材料。採用は未定)</summary>

`GestureDetector` は、既定では、色や文字のように描かれている部分だけが押せます。余白だけの部分は、押しても反応しません。余白を含む領域全体で反応させるには、`Behaviour = HitTestBehaviour.Opaque` を指定します。

```csharp
new GestureDetector
{
    Behaviour = HitTestBehaviour.Opaque,
    OnTap = () => SetState(() => _count++),
    Child = new Padding
    {
        Spacing = EdgeInsets.All(40),
        Child = new Text($"押した回数: {_count}"),
    },
}
```

`HitTestBehaviour` は、`FloatSoda.Gesture` の名前空間にあります。

**できた?** 文字の周りの余白を押しても、押した回数が増えれば、手順3は完了です。

ボタンを押したまま動かすドラッグは、`OnPanUpdate` で受け取ります。コードは[サンプルの DragBoxWidget.cs](../../../../samples/FloatSoda.Samples.OverlayApp/DragBoxWidget.cs) にあります。ホバーや生のポインターイベントが要るときは、`Listener` と `PointerRegion` を使います。

</details>

## 結果を確かめる

ダッシュボードのタブを開き、コントローラーで次の3点を試します。

- ボタンを押すと、値が変わる
- トグルを押すたびに、見た目が切り替わる
- 余白を含む範囲を押しても、反応する

HMD を被らずに確かめたいときは、[HMD を被らずに確かめる](../input/test-without-hmd.md)に進みます。ただし、デスクトップで動いても、ダッシュボードで動く確認の代わりにはなりません(追跡: #182)。最後は、ダッシュボードで押して確かめます。

組み立てた部品を使った、通しの例は[表情切り替えパネルを作る](expression-switcher.md)です。

## うまくいかないとき

押しても反応しないときは、[反応しないときに切り分ける](../../5-troubleshoot/not-responding.md)で原因を切り分けます。
