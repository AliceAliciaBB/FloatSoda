← [コントリビューター](../Home.md)

# 実験: 描画層

## 問い: 合成の Layer は、同じ絵に何をするか

同じ絵を `OpacityLayer`、`ClipRectLayer`、`TransformLayer` で1つずつ包み、png を並べて見比べる実験です。[描画層](rendering-layer.md) の Layer の種類の表を読んだあとに進みます。HMD も SteamVR も使いません。

絵は、青い矩形と橙の矩形が重なった1枚です。包まない絵と合わせて、png が4枚できます。

| ファイル | 包み方 |
| :-- | :-- |
| `plain.png` | 包まない |
| `opacity.png` | `OpacityLayer`(`Alpha` は 128) |
| `clip.png` | `ClipRectLayer`(左上の 200×150 だけを残す) |
| `transform.png` | `TransformLayer`(中心まわりに 15 度回す) |

先に予想を立てます。3つの Layer が、それぞれ絵をどう変えるかを考えてください。特に、青と橙が重なる部分が、`OpacityLayer` でどう見えるかを予想します。

## 用意するもの

FloatSoda を clone したフォルダーと同じ階層に、コンソールアプリを作ります。`FloatSoda.Rendering` のプロジェクトへの参照を足します。

```powershell
dotnet new console -o ExpRendering
dotnet add ExpRendering reference ./FloatSoda/src/FloatSoda.Rendering/FloatSoda.Rendering.csproj
```

参照先は、clone したフォルダーの名前に合わせて書き換えます。

**できた?** `dotnet build ExpRendering` が成功した。

<details>
<summary>できなかったら</summary>

SDK が見つからない、またはビルドで例外が出る場合は、[開発環境を用意する(コントリビューター)](../3-prepare/dev-environment.md) で SDK と clone の状態を確かめます。

</details>

## 4通りに包んで png に出す

`ExpRendering/Program.cs` を、次のコードに置き換えます。`CreatePicture` が絵を作り、`Save` が絵を包んで png に出します。`PictureLayer` は絵の所有権を持たないので、`Save` が絵を保持し、保存が終わってから破棄します。

```csharp
using FloatSoda.Rendering;
using FloatSoda.Rendering.Layers;
using SkiaSharp;

var size = new SKSizeI(400, 300);
var renderer = new LayerBitmapRenderer();

Save("plain", picture => picture);
Save("opacity", picture => new OpacityLayer { Alpha = 128, Children = { picture } });
Save("clip", picture => new ClipRectLayer(new SKRect(0, 0, 200, 150)) { Children = { picture } });
Save("transform", picture => new TransformLayer
{
    Transform = SKMatrix.CreateRotationDegrees(15, 200, 150),
    Children = { picture }
});

Console.WriteLine($"保存先: {Path.GetFullPath(".")}");

void Save(string name, Func<ILayer, ILayer> wrap)
{
    // PictureLayer は SKPicture の所有権を持たないので、描画と保存が終わるまで呼び出し側が保持する
    using var picture = CreatePicture();
    var root = new ContainerLayer { Children = { wrap(new PictureLayer { Picture = picture }) } };

    using var bitmap = renderer.Render(root, size);
    bitmap.Save($"{name}.png");

    Console.WriteLine($"{name}.png: 青(50,50)={bitmap.GetPixel(50, 50)} " +
                      $"橙(300,230)={bitmap.GetPixel(300, 230)} 重なり(150,120)={bitmap.GetPixel(150, 120)}");
}

SKPicture CreatePicture()
{
    using var recorder = new SKPictureRecorder();
    var canvas = recorder.BeginRecording(new SKRect(0, 0, size.Width, size.Height));

    using var blue = new SKPaint { Color = SKColors.CornflowerBlue };
    using var orange = new SKPaint { Color = SKColors.Orange };
    canvas.DrawRect(new SKRect(20, 20, 220, 160), blue);
    canvas.DrawRect(new SKRect(120, 100, 360, 260), orange);

    return recorder.EndRecording();
}
```

`Save` は、包んだ Layer をルートの `ContainerLayer` に入れ、`LayerBitmapRenderer.Render` で `SKBitmap` にして、png で保存します。あわせて、3つの点の色を出力します。

```powershell
dotnet run --project ExpRendering
```

**できた?** `plain.png`、`opacity.png`、`clip.png`、`transform.png` の4枚ができた。最後の行に出る保存先のフォルダーに、4枚がある。

<details>
<summary>できなかったら</summary>

png が空になる、サイズが 0 になる場合は、`LayerBitmapRenderer.Render` に渡した大きさと、ルートに子を入れたかを確かめます。仕組みは [描画層](rendering-layer.md) の「CPU の Bitmap に描く」にあります。

</details>

## png を見比べる

画像ビューアーで4枚を並べて開き、違いを言葉にします。背景は透明なので、ビューアーによって白や黒に見えます。

見る点は3つです。

- 青と橙が重なる部分の濃さ
- 左上の 200×150 より外に、絵が残っているか
- 絵全体が傾いているか

コンソールの出力は、4枚の点の色を数値で示します(このあとに、保存先の行が続きます)。`#80ffa500` は、先頭の 2 桁(`80`)が不透明度で、残りが色です。

```text
plain.png: 青(50,50)=#ff6495ed 橙(300,230)=#ffffa500 重なり(150,120)=#ffffa500
opacity.png: 青(50,50)=#806495ed 橙(300,230)=#80ffa500 重なり(150,120)=#80ffa500
clip.png: 青(50,50)=#ff6495ed 橙(300,230)=#00000000 重なり(150,120)=#ffffa500
transform.png: 青(50,50)=#ff6495ed 橙(300,230)=#ffffa500 重なり(150,120)=#ffffa500
```

`opacity.png` は、3つの点がどれも不透明度 `80`(10進で 128)です。重なりの点も `80` のままで、重なると濃くなる、ということはありません。`clip.png` は、橙の点(300,230)が透明(`00000000`)になり、(50,50)と(150,120)は残ります。`transform.png` は、3つの点の値が `plain.png` と同じです。回転は、点の値では分かりません。画像を開いて、絵全体が回っていることを目で確かめます。

**できた?** 「まとめた絵に不透明度を掛けると、この例の重なりは濃くならない」「指定した矩形の外は描かれない」「指定した回転が絵全体に掛かる」と言える。

<details>
<summary>できなかったら</summary>

`opacity.png` の重なりの不透明度が `80` より大きい場合は、`OpacityLayer` で包んだ対象が、2つの矩形をまとめた `PictureLayer` になっているかを確かめます。矩形ごとに別の `PictureLayer` にして、それぞれを `OpacityLayer` で包むと、重なりは濃くなります。

</details>

## なぜそうなるか

`OpacityLayer` は、`SaveLayer` で子を一時レイヤーにまとめて描きます。`Restore` のときに、`Alpha` を掛けて合成します。2つの矩形は、先に1枚の絵として描かれるので、重なりの部分は橙で上書きされたままになります。そのあとで、絵全体が半透明になります。クリップと変換は、`Save` と `Restore` の組で子を包みます。クリップは `ClipRect` で描画範囲を絞り、変換は `Concat` で座標系を変えてから、子を描きます。どちらも `Restore` で状態を戻すので、後ろに並ぶ Layer へは影響しません。

自分で Layer を足すときは、`Paint` が何を `Save` して何を `Restore` するかを先に決めます。詳細は、[描画層](rendering-layer.md) の「Layer の種類と、それぞれの役割」にあります。

<!-- ※要確認: transform.png の見た目(回転)は、画像を開いて目で確かめた。点の値では回転を確かめていない / 別の PC と別のビューアーでも同じように見えるか、確かめる -->

## 次に読むもの

> 【未決】この節は判断が要るため、ト書きのまま残しています。オーナーが決めたあとで本文にします(追跡: #219)。

- 読み手が持って来るもの: 実験の結果
- このページが渡すもの: `rendering-layer.md`、`tutorial-render-path.md` の段2、`experiment-framework.md` へのリンク
- 読み手がすること: 続きを選ぶ
- 止まる箇所: なし。ここでこのページは終わる
- 【未決】`PaintingSample` との重なり(どちらも Layer ツリーを png に出す)を、本文でどう扱うか。この実験の問いを変えることで重なりを減らす、と決めている(docs 再編の決定、2026-10-17)。`tutorial-render-path.md` の段2との分担は、段2の本文を書くときに決める

<details>
<summary>仮に書いた本文(判断の材料。採用は未定)</summary>

- [描画層](rendering-layer.md): Layer の種類と、`Clone` の共有範囲
- [描画が届くまで(4段)](tutorial-render-path.md): 2段目で、Layer ツリーを組んで png に出す
- [実験: フレームワーク層](experiment-framework.md): `SetState` のあとに描き直される範囲を確かめる

</details>
