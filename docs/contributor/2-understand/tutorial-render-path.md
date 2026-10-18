← [コントリビューター](../Home.md)

# 描画が届くまで(4段)

## このチュートリアルで作るもの

層を1つずつ足しながら、描画が SteamVR のオーバーレイに届くまでを、手を動かして確かめます。OVR 層だけでオーバーレイを出すところから始め、描画層、エンジン層、フレームワーク層の順に足します。OVR 層と描画層は互いを参照しないので、それぞれを試してから、両方を使うエンジン層へ進みます。最後に、フレームワーク層で同じ描画の流れを組みます。

| 段 | 足す層 | 目に見える結果 |
| :-- | :-- | :-- |
| 1 | OVR 層 | HMD の 1 m 前のオーバーレイに、png の絵が出る |
| 2 | 描画層 | 自分で組んだ Layer ツリーが、png の画像になる |
| 3 | エンジン層 | 同じ Layer ツリーが、GL テクスチャ経由でオーバーレイに出る |
| 4 | フレームワーク層 | ウィジェットで書いた色の四角が、ダッシュボードに出る |

最後の段で、フレームワークの `FloatSodaApp` が、段3で自分が書いた仕事と同じことをしているのが分かります。

```text
段1  OVR            OpenVR を呼ぶ                         参照: なし
段2  Rendering      Layer ツリーを絵にする                 参照: なし(SkiaSharp のみ)
段3  Engine         GL テクスチャで SteamVR に出す         参照: Abstractions、OVR、Rendering
段4  FloatSoda      ウィジェットから Layer ツリーを作る     参照: Abstractions、Engine、Rendering
```

各段の終わりに、確認の項目と、選択式のクイズが1問あります。仕組みの説明は、層ごとの解説に分けています。

## 始める前に用意するもの

次の3つを用意します。

- .NET の SDK
- FloatSoda を clone したフォルダー
- SteamVR と HMD(段2だけは、HMD なしで進められる)

コンソールアプリは、clone したフォルダーと同じ階層に作ります。参照先は、clone したフォルダーの名前に合わせて書き換えます。準備の詳細は [開発環境を用意する(コントリビューター)](../3-prepare/dev-environment.md) にあります。SteamVR が起動できない、HMD がつながらない場合の直し方は、ここでは扱いません。

## 1. OVR 層だけでオーバーレイを出す

> 【未決】この節は判断が要るため、ト書きのまま残しています。オーナーが決めたあとで本文にします(追跡: #219)。

- 読み手が持って来るもの: 空のコンソールアプリと、`FloatSoda.OVR` へのプロジェクト参照
- このページが渡すもの: `OVRApplication`、`OverlayIdentity`、`DeviceTrackedOverlay`、`Texture.FromFile`、`Visibility.Show` の最小のコード。png を1枚用意する方法
- 読み手がすること: コードを貼って `dotnet run` し、HMD の中でオーバーレイを探す
- できた?: HMD の 1 m 前に付いたオーバーレイに、png の絵が出た(`Target` の既定が HMD のため。※要確認)
- できなかったら: `Init_HmdNotFound` が出る、オーバーレイが見つからない → OVR 層の解説の「落とし穴」の節、SteamVR の状態の確認
- クイズ: 段1のオーバーレイの絵は、だれが描いたか。(a) Skia (b) OpenVR が png を読んで表示した (c) OpenGL。正解は (b)。`overlay.Texture.FromFile` が `SetOverlayFromFile` を呼ぶため(`src/FloatSoda.OVR/Overlay/Capability.cs` の 190 行目)
- 次へ進む先: 段2。OVR 層の詳細は `ovr-layer.md` へリンクする
- 【未決】段1の png を、段2を先に済ませて作る順にするか、固定の画像を `docs/assets/` に置いて使うか。段の順(依存の向き)は変えない

<details>
<summary>仮に書いた本文(判断の材料。採用は未定)</summary>

段1は、FloatSoda のほかの層を使わず、`FloatSoda.OVR` だけで、png の画像をオーバーレイとして HMD の前に出します。

まず、オーバーレイに出す png を用意します。サンプルの `PaintingSample` を1回動かすと、デスクトップに `layer_tree_output.png` ができます。

```powershell
dotnet run --project FloatSoda/samples/FloatSoda.Samples.PaintingSample
```

次に、コンソールアプリを作り、`FloatSoda.OVR` を参照します。

```powershell
dotnet new console -o RenderPath1
dotnet add RenderPath1 reference ./FloatSoda/src/FloatSoda.OVR/FloatSoda.OVR.csproj
```

`RenderPath1/Program.cs` を、次のコードに置き換えます。

```csharp
using System.Numerics;
using FloatSoda.OVR;
using FloatSoda.OVR.Overlay;

// 表示する png のパス。省略するとデスクトップの layer_tree_output.png を使う
var imagePath = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "layer_tree_output.png");

using var app = new OVRApplication(new OVRAppInfo(new AppKey("com.example.render_path")));

var identity = new OverlayIdentity($"com.example.render_path.step1.{Guid.NewGuid()}", "RenderPath step 1");
using var overlay = new DeviceTrackedOverlay(identity);

overlay.Transform.Position = new Vector3(0, 0, -1); // 追従先(既定は HMD)の 1 m 前
overlay.Texture.FromFile(imagePath);
overlay.Visibility.Show();

Console.WriteLine("HMD の中でオーバーレイを探してください。Enter キーで終了します。");
Console.ReadLine();
```

SteamVR を起動してから、動かします。HMD の中で、オーバーレイを探します。

```powershell
dotnet run --project RenderPath1
```

**できた?** HMD の 1 m 前に付いたオーバーレイに、png の絵が出た。

<details>
<summary>できなかったら</summary>

`Init_HmdNotFound` の例外が出る場合や、オーバーレイが見つからない場合は、SteamVR の状態と HMD の接続を確かめます。原因の見分け方は、[OVR 層](ovr-layer.md) の「つまずきやすい点」にあります。

</details>

<!-- ※要確認: オーバーレイが HMD の 1 m 前に出ること / DeviceTrackedOverlay の追従先の既定は HMD(ソースで確認)で、位置 (0, 0, -1) の見え方は実機で確かめていない。HMD で確かめる -->
<!-- ※要確認: 段1のコードが実機で動くか / コードのコンパイルは確かめたが、HMD のある環境で実行していない -->

**クイズ** 段1のオーバーレイの絵は、だれが描いたでしょうか。

- (a) Skia
- (b) OpenVR が png を読んで表示した
- (c) OpenGL

<details>
<summary>答え</summary>

(b) です。`overlay.Texture.FromFile` が、OpenVR の `SetOverlayFromFile` を呼びます。FloatSoda は画像のパスを渡すだけで、描いているのは OpenVR です。

</details>

OVR 層の型の詳細は、[OVR 層](ovr-layer.md) にあります。

</details>

## 2. Layer ツリーを組んで png に描く

> 【未決】この節は判断が要るため、ト書きのまま残しています。オーナーが決めたあとで本文にします(追跡: #219)。

- 読み手が持って来るもの: 段1で動いたコード(png を読んで出す)
- このページが渡すもの: `ContainerLayer`・`PictureLayer`・`SKPictureRecorder` で Layer ツリーを組み、`LayerBitmapRenderer.Render` で `SKBitmap` にして png で保存するコード。`Layout` のあとに `Paint` が走ること
- 読み手がすること: 別のコンソールアプリでコードを動かし、できた png を段1のオーバーレイで表示する
- できた?: 自分で組んだ Layer ツリーの絵が、HMD の中のオーバーレイに出た
- できなかったら: png が空、真っ黒 → 描画層の解説の「Layer の種類」の節。オーバーレイが出ない → 段1に戻る
- クイズ: `LayerBitmapRenderer.Render` が `root` に対して最初に呼ぶのはどれか。(a) `Paint` (b) `Layout` (c) `Clone`。正解は (b)(`src/FloatSoda.Rendering/LayerRenderer.cs` の 33〜34 行目)
- 次へ進む先: 段3。Layer の詳細は `rendering-layer.md` へリンクする
- 【未決】`samples/FloatSoda.Samples.PaintingSample` と段2が同じことをしている。段2の本文を書くときに、PaintingSample を案内するか、段2で別のコードを書かせるかを決める(docs 再編の決定、2026-10-17)

<details>
<summary>仮に書いた本文(判断の材料。採用は未定)</summary>

段2は、HMD も SteamVR も使わずに、自分で組んだ Layer ツリーを png にします。`PaintingSample` が行っていることと同じ仕組みを、自分のコードで書きます。

別のコンソールアプリを作り、`FloatSoda.Rendering` を参照します。

```powershell
dotnet new console -o RenderPath2
dotnet add RenderPath2 reference ./FloatSoda/src/FloatSoda.Rendering/FloatSoda.Rendering.csproj
```

`RenderPath2/Program.cs` を、次のコードに置き換えます。

```csharp
using FloatSoda.Rendering;
using FloatSoda.Rendering.Layers;
using SkiaSharp;

var size = new SKSizeI(400, 300);

using var recorder = new SKPictureRecorder();
var canvas = recorder.BeginRecording(new SKRect(0, 0, size.Width, size.Height));
canvas.Clear(SKColors.WhiteSmoke);
using var paint = new SKPaint { Color = SKColors.DarkSeaGreen };
canvas.DrawRect(new SKRect(100, 75, 300, 225), paint);

var root = new ContainerLayer
{
    Children = { new PictureLayer { Picture = recorder.EndRecording() } }
};

using var bitmap = new LayerBitmapRenderer().Render(root, size);
bitmap.Save("my_layer_tree.png");

Console.WriteLine($"保存しました: {Path.GetFullPath("my_layer_tree.png")}");
```

コードは、`SKPictureRecorder` で描画を記録して `PictureLayer` に入れ、`ContainerLayer` の子にして Layer ツリーを組みます。`LayerBitmapRenderer.Render` は、ツリーに `Layout` を呼んでから `Paint` を呼び、`SKBitmap` に描きます。

```powershell
dotnet run --project RenderPath2
```

コンソールに、png の保存先のパスが出ます。そのパスを、段1のアプリに渡して、自分の絵をオーバーレイに出します。

```powershell
dotnet run --project RenderPath1 -- <保存先のパス>
```

**できた?** 自分で組んだ Layer ツリーの絵(白っぽい地に緑の四角)が、HMD の中のオーバーレイに出た。

<details>
<summary>できなかったら</summary>

png が空になる、真っ黒になる場合は、`PictureLayer` に `Picture` を設定したか、描画の範囲(`BeginRecording` に渡した矩形)がサイズと合っているかを確かめます。仕組みは [描画層](rendering-layer.md) の「CPU の Bitmap に描く」にあります。オーバーレイが出ない場合は、段1に戻ります。HMD がなくても、保存された png を画像ビューアーで開けば、段2の結果は確かめられます。

</details>

**クイズ** `LayerBitmapRenderer.Render` が、`root` に対して最初に呼ぶのは、どれでしょうか。

- (a) `Paint`
- (b) `Layout`
- (c) `Clone`

<details>
<summary>答え</summary>

(b) です。内部の `LayerRenderer.Render` が、`root.Layout(context)` を呼んでから `root.Paint(context)` を呼びます。

</details>

Layer の種類と `Clone` の共有範囲は、[描画層](rendering-layer.md) にあります。合成の Layer を同じ絵に使って見比べる実験は、[実験: 描画層](experiment-rendering.md) です。

</details>

## 3. Layer ツリーをエンジン層に差し込んで SteamVR に出す

> 【未決】この節は判断が要るため、ト書きのまま残しています。オーナーが決めたあとで本文にします(追跡: #219)。

- 読み手が持って来るもの: 段2で組んだ Layer ツリーと、段1で作ったオーバーレイ
- このページが渡すもの: 描画スレッドで `GLView` と `Renderer` を作り、`OverlayWindow` に包み、`RenderPostTaskRunner` で `PostRender` するコード。png ではなく、GL テクスチャ経由で絵が出ること。`FramePacer` が描画ループの間隔を決めること
- 読み手がすること: 段1の `FromFile` を、エンジン層を使うコードに置き換えて動かす
- できた?: 段1と同じ絵が、`FromFile` を呼ばずにオーバーレイに出た
- できなかったら: 絵が出ない、例外が出る → エンジン層の解説の「スレッド」の節。`GLView` を描画スレッド以外で作っていないかを見る
- 止まる箇所: `OverlayWindow` の名前が、フレームワークの型と重なること。段4に進む前に、別名の付け方を示す
- クイズ: GL テクスチャのハンドルを `Texture_t` に包んでオーバーレイに渡すのは、どの型か。(a) `GLView` (b) `Renderer` (c) `OverlayWindow`。正解は (c)(`src/FloatSoda.Engine/OverlayWindow.cs` の 54〜61 行目)
- 次へ進む先: 段4。エンジン層の詳細は `engine-layer.md` へリンクする
- 【未決】段3を、フレームワークなしで書けるかの確認が終わるまで、本文に進めない。確認の結果、書けなければ段3を解説に回すか決める

<details>
<summary>仮に書いた本文(判断の材料。採用は未定)</summary>

段3は、段2で組んだ Layer ツリーを、png ではなく GL テクスチャとしてオーバーレイに出します。段1の `FromFile` の代わりに、エンジン層の `RenderPostTaskRunner`、`GLView`、`Renderer`、`OverlayWindow` を使います。

コンソールアプリを作り、`FloatSoda.Engine` を参照します。`FloatSoda.Engine` は、フレームワーク(`FloatSoda`)を参照しないので、フレームワークなしで書けます。

```powershell
dotnet new console -o RenderPath3
dotnet add RenderPath3 reference ./FloatSoda/src/FloatSoda.Engine/FloatSoda.Engine.csproj
```

`RenderPath3/Program.cs` を、次のコードに置き換えます。

```csharp
using System.Numerics;
using FloatSoda.Abstractions.Geometries;
using FloatSoda.Engine;
using FloatSoda.OVR;
using FloatSoda.OVR.Overlay;
using FloatSoda.Rendering.Layers;
using SkiaSharp;

var size = new SKSizeI(400, 300);

using var app = new OVRApplication(new OVRAppInfo(new AppKey("com.example.render_path")));

var identity = new OverlayIdentity($"com.example.render_path.step3.{Guid.NewGuid()}", "RenderPath step 3");
using var overlay = new DeviceTrackedOverlay(identity);
overlay.Transform.Position = new Vector3(0, 0, -1);

using var cts = new CancellationTokenSource();
var renderThread = new RenderPostTaskRunner("RenderThread", new FramePacer(60));
renderThread.Start(cts.Token);

// GLView と Renderer は描画スレッドで作る
renderThread.PostTask(() =>
{
    var renderer = new Renderer { GLView = new GLView(size) };
    var window = new OverlayWindow(overlay, renderer, new Dpm(1000));

    window.Resize(size); // 画像の幅(400 px / 1000 dpm = 0.4 m)をオーバーレイに反映する
    renderThread.PostRender(window, CreateLayerTree(size)); // Present は描画スレッドで走る
});

overlay.Visibility.Show();

Console.WriteLine("HMD の中でオーバーレイを探してください。Enter キーで終了します。");
Console.ReadLine();

renderThread.Stop();

static ILayer CreateLayerTree(SKSizeI size)
{
    using var recorder = new SKPictureRecorder();
    var canvas = recorder.BeginRecording(new SKRect(0, 0, size.Width, size.Height));
    canvas.Clear(SKColors.WhiteSmoke);
    using var paint = new SKPaint { Color = SKColors.DarkSeaGreen };
    canvas.DrawRect(new SKRect(100, 75, 300, 225), paint);

    return new ContainerLayer
    {
        Children = { new PictureLayer { Picture = recorder.EndRecording() } }
    };
}
```

コードの要点は3つです。

- `RenderPostTaskRunner` が、描画スレッドを作る。`Start` のあと、`PostTask` に渡した処理が、描画スレッドで実行される
- `GLView`、`Renderer`、`OverlayWindow` は、描画スレッドの中で作る。`OverlayWindow` は、`Present` のたびに GL テクスチャを OpenVR に渡す
- `FramePacer(60)` が、描画ループの周期を 60 回/秒にそろえる

この最小のコードは、GL のリソースの後始末を省いています。

```powershell
dotnet run --project RenderPath3
```

`OverlayWindow` は、エンジン層の型です。フレームワーク(`FloatSoda`)にも、同じ名前の `FloatSoda.Widgets.OverlayWindow` があります。段4は別のプロジェクトで作るので、ここでは重なりません。同じプロジェクトに両方を入れるときは、フレームワークと同じように、`using EngineOverlayWindow = FloatSoda.Engine.OverlayWindow;` と別名を付けます。

**できた?** 段2と同じ絵が、`FromFile` を呼ばずに、オーバーレイに出た。

<details>
<summary>できなかったら</summary>

絵が出ない、例外が出る場合は、`GLView` を描画スレッド以外で作っていないかを確かめます。GL を使う処理は、`PostTask` に入れます。スレッドの制約は、[エンジン層](engine-layer.md) の「描画スレッドは、キューのタスクを実行する」にあります。

</details>

<!-- ※要確認: 段3のコードが実機で動くか / コンパイルは確かめたが、HMD のある環境で実行していない。GL テクスチャの提出を1回だけ行うコードで、絵が出続けるか、確かめる -->
<!-- ※要確認: オーバーレイを作るスレッド / このコードはオーバーレイをメインスレッドで作り、テクスチャを描画スレッドで設定する。フレームワークは、オーバーレイの作成を描画スレッドで行う。どちらでも動くかを、実機で確かめる -->

**クイズ** GL テクスチャのハンドルを `Texture_t` に包んで、オーバーレイに渡すのは、どの型でしょうか。

- (a) `GLView`
- (b) `Renderer`
- (c) `OverlayWindow`

<details>
<summary>答え</summary>

(c) です。`OverlayWindow.Present` が、`Renderer.Render` のあとに、テクスチャのハンドルを `Texture_t` に包み、`Overlay.Texture.FromTexture_t` で OpenVR に渡します。

</details>

エンジン層の詳細は、[エンジン層](engine-layer.md) にあります。

</details>

## 4. ウィジェットで描く

段4は、段3で手書きしたものを、フレームワークに任せます。ウィジェットで色の四角を書き、ダッシュボードに出します。

コンソールアプリを作り、`FloatSoda` を参照します。

```powershell
dotnet new console -o RenderPath4
dotnet add RenderPath4 reference ./FloatSoda/src/FloatSoda/FloatSoda.csproj
```

`RenderPath4/Program.cs` を、次のコードに置き換えます。

```csharp
using FloatSoda;
using FloatSoda.Abstractions.Geometries;
using FloatSoda.Geometrics;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SkiaSharp;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddFloatSoda();

using var host = builder.Build();
var app = host.Services.GetRequiredService<FloatSodaApp>();

app.CreateWindow(new DashboardWindow
{
    Dpm = new Dpm(100),
    Title = "RenderPath",
    Child = new Align
    {
        Alignment = Alignment.Center,
        Child = new SizedBox
        {
            Width = 100,
            Height = 100,
            Child = new ColoredBox { Color = SKColors.CornflowerBlue }
        }
    }
});

await host.RunAsync();
```

SteamVR を起動してから、動かします。HMD の中でダッシュボードを開き、`RenderPath` のタブを選びます。

```powershell
dotnet run --project RenderPath4
```

段3で書いた処理は、フレームワークの次の箇所が受け持っています。

| 段3で自分が書いたもの | フレームワークの担当 |
| :-- | :-- |
| `OVRApplication` を作る | `FloatSodaApp` の起動処理 |
| `OverlayIdentity` と `DeviceTrackedOverlay` を作る | `CreateWindow` が作成を予約し、初期化処理が描画スレッドでダッシュボード用のオーバーレイを作る |
| `RenderPostTaskRunner` と `FramePacer` を作って `Start` する | `AddFloatSoda` が登録し、`FloatSodaApp` が持って開始する |
| `PostTask` で `GLView`、`Renderer`、`OverlayWindow` を作る | `WidgetBinding.EnsureInitialized` |
| Layer ツリーを手で組む | `BuildOwner` がビルドを進め、`RenderPipeline` がレイアウトと描画を処理して Layer ツリーを更新する |
| `window.Resize(size)` | `WidgetBinding.DrawFrame` が、レイアウト結果のサイズが変わったときに投入する |
| `PostRender(window, layer)` | `WidgetBinding.DrawFrame` が、`Layer` を複製して `PostRender` に渡す |
| Enter キーで待ち、`Stop` を呼ぶ | ホスト(`host.RunAsync`)と、`FloatSodaApp` の終了処理 |

**できた?** ダッシュボードに、色の四角が出た。

<details>
<summary>できなかったら</summary>

起動で例外が出る場合は、利用者向けのガイドの [エラーを読んで直す](../../guide/5-troubleshoot/read-errors.md) を確かめます。四角が出ない場合は、[反応しないときに切り分ける](../../guide/5-troubleshoot/not-responding.md) を確かめます。

</details>

<!-- ※要確認: 段4のコードが実機で動くか / 色の四角を出す GettingStarted サンプルと同じ構成で、コンパイルは確かめた。HMD のある環境で、ダッシュボードにタブと四角が出るかを確かめる -->

**クイズ** 段3で自分が書いた `PostRender` の呼び出しを、フレームワークでは、何が呼んでいるでしょうか。

- (a) `FloatSodaApp.CreateWindow`
- (b) `WidgetBinding.DrawFrame`
- (c) `FramePacer`

<details>
<summary>答え</summary>

(b) です。`WidgetBinding.DrawFrame` が、`RenderView.Layer` を複製して、`PostRender` で描画スレッドに渡します。

</details>

フレームワークの詳細は、[フレームワーク層](framework-layer.md) にあります。

## 次に読むページ

目的に合わせて、続きを選びます。

- 層をまたぐ流れを読む: [1フレームが SteamVR に届くまで](frame-to-steamvr.md)
- 層ごとの解説を読む: [OVR 層](ovr-layer.md)、[描画層](rendering-layer.md)、[エンジン層](engine-layer.md)、[フレームワーク層](framework-layer.md)、[UI 層](ui-layer.md)
- 実験で確かめる: [実験: OVR 層](experiment-ovr.md)、[実験: 描画層](experiment-rendering.md)、[実験: フレームワーク層](experiment-framework.md)
- 開発の準備に進む: [開発環境を用意する(コントリビューター)](../3-prepare/dev-environment.md)
