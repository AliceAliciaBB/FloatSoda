← [コントリビューター](../Home.md)

# 実験: フレームワーク層

## 問い: 一部の色を変えると、どこが描き直されるか

`SetState` で画面の一部の色を変えたとき、どの再描画境界が描画待ちリストに登録されるかを、ログで確かめる実験です。最終画像の部分再描画を調べるものではありません。[フレームワーク層](framework-layer.md) を読んだあとに進みます。HMD も SteamVR も使いません。

確かめる対象は `RenderPipeline.NodesNeedingPaint` です。次の `FlushPaint` で描き直す RenderObject の待ちリストで、空ならその時点で描き直すものはありません。このリストを、4つのタイミングで出力します。

1. 初回フレームのあと
2. `SetState` の直後
3. `BuildScope` のあと
4. `FlushPaint` のあと

先に予想を立てます。2 と 3 で、リストに何が入るかを考えてください。2つの四角を縦に並べて、片方の色だけを変えます。それぞれを `RepaintBoundary` で包みます。

## 用意するもの

FloatSoda を clone したフォルダーと同じ階層に、コンソールアプリを作ります。`FloatSoda` のプロジェクトへの参照を足します。

```powershell
dotnet new console -o ExpFramework
dotnet add ExpFramework reference ./FloatSoda/src/FloatSoda/FloatSoda.csproj
```

参照先は、clone したフォルダーの名前に合わせて書き換えます。.NET の SDK のバージョンなど、開発の準備は [開発環境を用意する(コントリビューター)](../3-prepare/dev-environment.md) にあります。

**できた?** `dotnet build ExpFramework` が成功した。

<details>
<summary>できなかったら</summary>

SDK が見つからない、またはビルドで例外が出る場合は、[開発環境を用意する(コントリビューター)](../3-prepare/dev-environment.md) で SDK と clone の状態を確かめます。

</details>

## 動かす

`ExpFramework/Program.cs` を、次のコードに置き換えます。パイプラインを自分で組み、`BuildScope`、`FlushLayout`、`FlushPaint` を順に呼びます。

```csharp
using FloatSoda.Core;
using FloatSoda.Elements;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using SkiaSharp;

var useBoundary = args.Length == 0 || args[0] != "no-boundary";

var renderView = new RenderView(400, 300);
var pipeline = new RenderPipeline
{
    OnNeedVisualUpdate = () => { },
    RenderView = renderView
};
var owner = new BuildOwner(() => { });

TogglePanelState? state = null;
new RenderObjectToWidgetAdapter
{
    Container = renderView,
    Child = new TogglePanel
    {
        UseBoundary = useBoundary,
        OnStateCreated = s => state = s
    }
}.AttachToRenderTree(owner, null);

renderView.PrepareInitialFrame();
owner.BuildScope();
pipeline.FlushLayout();
pipeline.FlushPaint();
Show("初回フレーム後", pipeline);

state!.Toggle();
Show("SetState 直後", pipeline);

owner.BuildScope();
Show("BuildScope 後", pipeline);

pipeline.FlushLayout();
pipeline.FlushPaint();
Show("FlushPaint 後", pipeline);

static void Show(string label, RenderPipeline pipeline)
{
    var nodes = pipeline.NodesNeedingPaint
        .Select(n => $"{n.GetType().Name}@depth{n.Depth}");
    Console.WriteLine($"{label}: NodesNeedingPaint = [{string.Join(", ", nodes)}]");
}

record TogglePanel : StatefulWidget<TogglePanel>
{
    public bool UseBoundary { get; init; } = true;
    public Action<TogglePanelState>? OnStateCreated { get; init; }

    public override State<TogglePanel> CreateState() => new TogglePanelState();
}

class TogglePanelState : State<TogglePanel>
{
    private bool _flag;

    // SetState は protected なので、外から呼べるように State を受け渡す
    public override void InitState() => Widget!.OnStateCreated?.Invoke(this);

    public void Toggle() => SetState(() => _flag = !_flag);

    private Widget Panel(SKColor color) => new SizedBox
    {
        Width = 100,
        Height = 100,
        Child = new ColoredBox { Color = color }
    };

    private Widget Wrap(Widget child) =>
        Widget!.UseBoundary ? new RepaintBoundary { Child = child } : child;

    public override Widget Build(IBuildContext context) => new Column
    {
        Children =
        {
            Wrap(Panel(_flag ? SKColors.Tomato : SKColors.CornflowerBlue)),
            Wrap(Panel(SKColors.DarkSeaGreen)),
        }
    };
}
```

コードの流れは次のとおりです。

- `RenderView`、`RenderPipeline`、`BuildOwner` を作り、`RenderObjectToWidgetAdapter` でウィジェットのツリーをつなぐ
- `TogglePanel` は、`Column` に四角を2つ並べ、上の四角の色を `SetState` で切り替える
- `UseBoundary` が真のとき、それぞれの四角を `RepaintBoundary` で包む

次のコマンドで動かします。

```powershell
dotnet run --project ExpFramework
```

**できた?** 「初回フレーム後」「SetState 直後」「BuildScope 後」「FlushPaint 後」の4つの行が出た。

<details>
<summary>できなかったら</summary>

コンパイルが通らない場合は、公開 API が変わった可能性があります。使っている型(`RenderPipeline`、`BuildOwner`、`RenderObjectToWidgetAdapter`、`StatefulWidget<T>`、`State<T>`)の名前と使い方を、[フレームワーク層](framework-layer.md) と、`src/FloatSoda` の現在のソースで照らします。

</details>

## ログを読む

出力は次の4行になります。

```text
初回フレーム後: NodesNeedingPaint = []
SetState 直後: NodesNeedingPaint = []
BuildScope 後: NodesNeedingPaint = [RenderRepaintBoundary@depth2]
FlushPaint 後: NodesNeedingPaint = []
```

見る場所は、リストの中身です。

- `SetState` 直後: 空
- `BuildScope` 後: `RenderRepaintBoundary` が1件
- `FlushPaint` 後: 空

`SetState` が行うのは再ビルドの予約までなので、直後は RenderObject がまだ変わらず、リストは空です。`BuildScope` の後に境界が1件入るのは、色を変えた四角の `RenderColoredBox` が `MarkNeedsPaint` を呼び、直近の境界で要求が止まるためです。`@depth2` は木の深さで、`RenderView`(0)、`Column` の `RenderFlex`(1)、境界(2)の順です。

このログで確認できるのは、深さ2の境界が1件だけ登録されたことです。2つの境界は型も深さも同じなので、どちらの境界かはこの出力から区別できません。

**できた?** 「`BuildScope` のあとだけ、深さ2の `RenderRepaintBoundary` が1件入った」と言える。

<details>
<summary>できなかったら</summary>

`BuildScope` のあとのリストが空の場合は、`Toggle()`、`BuildScope()`、`Show()` の呼び出し順を確かめます。`state!.Toggle()` で例外になる場合は、`OnStateCreated` が呼ばれたかを確かめます。`UseBoundary` が偽のログになっている場合は、実行コマンドに `no-boundary` を付けていないかを確かめます。

</details>

## 壊してみる: RepaintBoundary を外す

境界を外すと、結果がどう変わるかを予想してから実行します。コードの `Wrap` が、`RepaintBoundary` で包まずに、四角をそのまま返すようにします。引数 `no-boundary` を付けると同じ状態になります。

```powershell
dotnet run --project ExpFramework -- no-boundary
```

**できた?** 「`BuildScope` 後」の行が `[RenderView@depth0]` になった。描き直しの範囲が、境界から根の `RenderView` まで広がっている。

<details>
<summary>できなかったら</summary>

行が `[RenderRepaintBoundary@depth2]` のままなら、引数が渡っていません。`--` の位置と綴りを確かめます。

</details>

## なぜそうなるか

この例では、`RenderColoredBox.Color` の値が変わると、`MarkNeedsPaint` が呼ばれます。`MarkNeedsPaint` は、自分が再描画境界ならそこで止まって `NodesNeedingPaint` に登録し、そうでなければ親へ伝わります。`RepaintBoundary` があると、色を変えた `RenderColoredBox` から親へ伝わった要求が、最初の境界で止まります。境界がなければ、要求は根の `RenderView` まで伝わります。`RenderView` も再描画境界なので、そこで登録されます。

つまり、描き直す範囲は、変えたノードの祖先をたどって、最初に見つかる境界までです。自分の変更で境界をどこに置くかは、この範囲の広さで考えます。範囲が根まで広がる場所に、頻繁に変わる部品を置かないようにします。伝播の細部は、[フレームワーク層](framework-layer.md) の「レイアウトと描画は、境界までを描き直す」にあります。

<!-- ※要確認: HMD も SteamVR も要らないことは、OVRApplication を作らないコードで実行して確かめた(この実験の実行は、SteamVR が入った PC だった) / SteamVR が入っていない PC でも同じ出力になるかを確かめる -->

## 次に読むもの

> 【未決】この節は判断が要るため、ト書きのまま残しています。オーナーが決めたあとで本文にします(追跡: #219)。

- 読み手が持って来るもの: 実験の結果
- このページが渡すもの: `framework-layer.md`、`experiment-rendering.md`、`frame-to-steamvr.md` へのリンク
- 読み手がすること: 続きを選ぶ
- 止まる箇所: なし。ここでこのページは終わる
- 【未決】実験のコードを、本文に全文で載せるか、`samples/` に置いてリンクするか

<details>
<summary>仮に書いた本文(判断の材料。採用は未定)</summary>

- [フレームワーク層](framework-layer.md): ツリーの役割と、差分更新の仕組み
- [実験: 描画層](experiment-rendering.md): Layer の合成を png で確かめる
- [1フレームが SteamVR に届くまで](frame-to-steamvr.md): 描き直した結果が、SteamVR に届くまで

</details>
