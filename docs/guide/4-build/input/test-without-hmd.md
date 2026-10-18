← [ガイド](../../Home.md)

# HMD を被らずに確かめる

## どちらの方法を使うか

> 【未決】この節は判断が要るため、ト書きのまま残しています。オーナーが決めたあとで本文にします(追跡: #219)。

- 読み手が持って来るもの: 「HMD を被る手間を減らしたい」という目的
- このページが渡すもの: 2つの方法の違い(窓に出すと目で見られる、画像にすると自動で比べられる)と、どちらも HMD の接続と SteamVR の起動は要る方法と要らない方法があること
- 読み手がすること: 自分の目的に合う方法を選ぶ
- 次へ進む先: 「前提条件」
- 【未決】2つの方法をこのページに置くか、2ページに分けるか。分けると、タイトルが「窓に出す」と「画像で比べる」になる

<details>
<summary>仮に書いた本文(判断の材料。採用は未定)</summary>

見た目や動きを確かめるたびに HMD を被る回数は、デスクトップの窓に出す方法か、画像にして見る方法で減らせます。窓に出す方法は、画面をモニターで見ながらマウスで操作できます。画像にする方法は、ウィジェットを PNG ファイルに書き出すので、SteamVR も HMD も要りません。

| 方法 | できること | SteamVR と HMD |
| :-- | :-- | :-- |
| 方法1: デスクトップの窓に出す | 画面をモニターで見る。マウスで押す | 起動と接続が要る |
| 方法2: 画像にして比べる | ウィジェットを PNG にして、保存した画像を開いて見る | 要らない |

どちらの方法も、HMD の中で動くことの確認の代わりにはなりません。最後は HMD で確かめます。

</details>

## 前提条件

- 方法1: `DashboardWindow` で出している起動のコードがあること
- 方法1: SteamVR を起動し、HMD を接続していること
- 方法2: テストしたいウィジェットがあること
- 方法2: プロジェクトに `FloatSoda.Testing` パッケージを足していること

<!-- ※要確認: `FloatSoda.Testing` の取得元(NuGet での公開の有無)と導入コマンド。src/FloatSoda.Testing/FloatSoda.Testing.csproj に PackageId はあるが、NuGet で公開されているかは確かめていない -->

方法1でも HMD の接続が要る理由は、FloatSoda が表示先によらず、起動時に OpenVR を初期化するためです。HMD が未接続だと `Init_HmdNotFound` で起動に失敗します(追跡: #140)。方法1で省けるのは、HMD を頭に付ける手間だけです。

<!-- ※要確認: HMD を接続していない PC で `--desktop` が動かない状況の回避策が今あるか。FloatSodaApp.Initialize() を読み直して確かめる -->

## 方法1: デスクトップの窓に出す

起動のコードで `DashboardWindow` を渡している箇所を、`DesktopWindow` に替えます。`DesktopWindow` は、開発機のデスクトップ上に見える窓として画面を出します。

```csharp
var useDesktop = args.Contains("--desktop");
var hostArgs = args.Where(argument => argument != "--desktop").ToArray();

var builder = Host.CreateApplicationBuilder(hostArgs);
builder.Services.AddFloatSoda();

using var host = builder.Build();
var app = host.Services.GetRequiredService<FloatSodaApp>();

app.CreateWindow(useDesktop
    ? new DesktopWindow { Title = "MyOverlay", Child = root }
    : new DashboardWindow { Dpm = new Dpm(100), Title = "MyOverlay", Child = root });

await host.RunAsync();
```

`Dpm` は `FloatSoda.Abstractions.Geometries` の名前空間にあります。`--desktop` を付けて起動すると窓に出て、付けないと従来どおりダッシュボードに出ます。`--desktop` はこのアプリで処理する引数なので、`Host.CreateApplicationBuilder` に渡す引数から取り除きます。このコードは、[GestureDetector のサンプル](../../../../samples/FloatSoda.Samples.GestureDetector/Program.cs)と同じ形です。

```powershell
dotnet run -- --desktop
```

**できた?** デスクトップに窓が出て、マウスで操作できれば、方法1は完了です。

<!-- ※要確認: `DesktopWindow` の窓のサイズが `Size` の指定どおりになるか。実行して確かめる -->

<details>
<summary>できなかったら</summary>

窓が出ない、または起動時に例外が出るときは、[エラーを読んで直す](../../5-troubleshoot/read-errors.md)で原因を切り分けます。HMD を接続していない場合は、前提条件のとおり `Init_HmdNotFound` で失敗します。

</details>

デスクトップで動いても、ダッシュボードで動く証明にはなりません。ウィンドウの種類ごとに、ポインターの座標が届く仕組みが違うためです(追跡: #182)。ボタンを押せる画面は、最後に HMD のダッシュボードでも確かめます。

## 方法2: 画像にして比べる

> 【未決】この節は判断が要るため、ト書きのまま残しています。オーナーが決めたあとで本文にします(追跡: #219)。

- 読み手が持って来るもの: テストしたいウィジェット
- このページが渡すもの: `WidgetBitmapRenderer.Render` の呼び方と、画像を保存して見る方法。非同期で読み込む画像を使うときの注意
- 読み手がすること: ウィジェットを画像にして、保存した画像を開く
- できた?: PNG ファイルが保存され、画面と同じ見た目になっている
- 次へ進む先: 「結果を確かめる」
- 【未決】期待する画像との比較を、このページで書くか。書くなら、比較の基準(画素の一致か、許容する差か)を決める必要がある

<details>
<summary>仮に書いた本文(判断の材料。採用は未定)</summary>

`WidgetBitmapRenderer.Render` にウィジェットと画像のサイズを渡すと、ビルド、レイアウト、ペイントを1回実行して、結果を `SKBitmap` で返します。この方法は SteamVR を使いません。

```csharp
using FloatSoda.Testing;
using SkiaSharp;

var renderer = new WidgetBitmapRenderer();

using var bitmap = renderer.Render(root, new SKSizeI(400, 300));
using var image = SKImage.FromBitmap(bitmap);
using var data = image.Encode(SKEncodedImageFormat.Png, 100);

File.WriteAllBytes("widget.png", data.ToArray());
```

`root` には、HMD の中に出したい画面のウィジェットを渡します。`Render` は1回しか実行しないので、`Image` ウィジェットのように、画像を非同期で読み込む部品は、読み込み前の状態で描かれます。画像まで描きたいときは、`Render` の前に、同じプロバイダーで `await provider.ResolveAsync()` を実行します。返されたハンドルは、描画が終わるまで保持します。

`FloatSoda.Samples.PaintingSample` も、ウィジェットのツリーを PNG に書き出します。コードは[PaintingSample の Program.cs](../../../../samples/FloatSoda.Samples.PaintingSample/Program.cs)にあります。

<!-- ※要確認: `FloatSoda.Testing` が NuGet で配布されているか(csproj に PackageId はある。公開の有無は NuGet で確かめる) -->

画像を期待する画像と自動で比べる方法は、この手順では扱いません。書き出した PNG を開いて、目で見て確かめます。

**できた?** `widget.png` が保存され、開くと、HMD の中に出す画面と同じ見た目になっていれば、方法2は完了です。

<details>
<summary>できなかったら</summary>

画像の中の配置や大きさが思ったとおりでないときは、[レイアウトの考え方](../../5-troubleshoot/layout-model.md)で、大きさの決まり方を確かめます。例外が出るときは、[エラーを読んで直す](../../5-troubleshoot/read-errors.md)に進みます。

</details>

</details>

## 結果を確かめる

窓または画像で、次の3点を見ます。

- 配置: 部品が思った場所に、思った大きさで並んでいる
- 色: 背景と文字の色が指定どおりに出ている
- 文字: 文字が切れずに、読める大きさで出ている

HMD の中で最後に確かめる項目は、押せるかどうかと、実寸の大きさです。見た目の指定は[見た目を整える](../screen/style-appearance.md)、押せる部品は[ボタンやトグルを組み立てる](../screen/build-buttons-toggles.md)で扱います。

## うまくいかないとき

窓が出ない、または起動で例外が出るときは、[エラーを読んで直す](../../5-troubleshoot/read-errors.md)で原因を切り分けます。
