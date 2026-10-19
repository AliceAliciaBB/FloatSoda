← [コントリビューター](../Home.md)

# 描画層

## Layer ツリーが表すもの

Layer ツリーは、記録済みの描画(`PictureLayer`)と、描画をまとめて加工する合成の操作(不透明度、変換、クリップ)を、木にしたものです。描画層(`FloatSoda.Rendering`)は、この木と、木を描く `LayerRenderer` を持ちます。参照するのは SkiaSharp だけで、FloatSoda のほかのプロジェクトは参照しません。

木の葉は `PictureLayer` で、記録済みの `SKPicture` を再生します。枝は `ContainerLayer` とその派生型で、子を順に描きます。合成の操作は、枝として子を包みます。

```text
ContainerLayer            ルート。子を順に描く
├─ PictureLayer           葉。記録済みの絵を再生する
└─ OpacityLayer           枝。子をまとめて半透明にする
   └─ PictureLayer
```

## Layer の種類と、それぞれの役割

`ILayer` は、`PaintBounds`、`Layout`、`Paint`、`Clone` を持ちます。`PictureLayer` 以外の種類は、すべて `ContainerLayer` の派生です。

| 名前 | 持つ設定 | `Paint` がすること |
| :-- | :-- | :-- |
| `ContainerLayer` | `Children` | 子を一覧の順に描く |
| `PictureLayer` | `Picture`(`SKPicture?`) | `Picture` を `Playback` で再生する。`null` なら何も描かない |
| `OpacityLayer` | `Alpha`(`byte`。既定は 255) | `SaveLayer` で子をまとめて描き、`Restore` で `Alpha` を掛けて合成する |
| `TransformLayer` | `Transform`(`SKMatrix`) | `Save`、`Concat`、子の描画、`Restore` の順に行う |
| `ClipRectLayer` | `ClipRect`(`SKRect`)、`ClipBehavior` | `Save`、`ClipRect`、子の描画、`Restore` の順に行う |
| `ClipRoundRectLayer` | `ClipRect`(`SKRoundRect`)、`ClipBehavior` | `Save`、`ClipRoundRect`、子の描画、`Restore` の順に行う |
| `ClipPathLayer` | `ClipPath`(`SKPath`)、`ClipBehavior` | `Save`、`ClipPath`、子の描画、`Restore` の順に行う |

`ClipBehavior` の既定は `Clip.Antialias` です。3つのクリップは、`ClipRectLayer.cs` に定義されています。

表のうち、変換とクリップの Layer は `Save` と `Restore` の組で、`OpacityLayer` は `SaveLayer` と `Restore` の組で、子を包みます。`ContainerLayer` と `PictureLayer` は、`Paint` でこの組を持ちません。そのため、変換・クリップ・不透明度の Layer が変えたキャンバスの状態は、後ろに並ぶ Layer へ引き継がれません。新しい Layer を足すときも、`Paint` で変えたキャンバスの状態は `Restore` で戻します。実際に3つの Layer が同じ絵へ何をするかは、[実験: 描画層](experiment-rendering.md) で確かめられます。

## Layout のあとに Paint を実行する

`Layout` は、その Layer と子の描画境界(`PaintBounds`)を計算します。`PictureLayer` は `Picture` の `CullRect`、`ContainerLayer` は子の境界をすべて含む外接矩形、`TransformLayer` は変換後の矩形です。`Paint` は、計算済みの内容をキャンバスに合成します。`ILayer` の契約は次のとおりです。

- `Layout` で境界を確定してから、同じコンテキストで `Paint` を呼ぶこと



`LayerRenderer.Render(root, context)` が、この2つを順に呼びます。描画先のクリア、フラッシュ、`SKSurface` の所有権は、呼び出し側の仕事です。`LayerContext` が持つのは、描画先の `SKCanvas` だけです。

## Clone は何を共有し、何を共有しないか

`Clone` は、子を再帰的に複製して、独立したツリーを作ります。描画リソースの所有権は移さず、元と共有する場合があります。

| 区分 | 内容 |
| :-- | :-- |
| 共有される | `PictureLayer` の `SKPicture` の参照 |
| 共有される | `ClipRoundRectLayer` の `SKRoundRect` と、`ClipPathLayer` の `SKPath` の参照 |
| 共有されない | 木の構造(`Children` の一覧と、各 Layer のインスタンス) |
| 共有されない | 値で持つ合成の設定(`Alpha`、`Transform`、`ClipRectLayer` の `ClipRect`、`ClipBehavior`)。値のコピーになる |
| 共有されない | `PaintBounds`。複製したツリーに `Layout` を呼んだときに計算される |

自分で Layer を足すときは、`Clone` が何をコピーして何を参照するかを決めます。記録済みの絵のような描画リソースは参照にして、設定の値はコピーにします。参照にする場合は、所有者と寿命を決めます。

`SKPicture` の所有権は、複製先に移りません。複製を描いているあいだは、元の持ち主が `SKPicture` を有効な状態に保つ必要があります。複製した側で破棄すると、同じ `SKPicture` を使う元のツリーも描けなくなります。

- 共有する `SKPicture`、`SKRoundRect`、`SKPath` は、描画が完了するまで変更・破棄しないこと

複製を描画スレッドへ渡す理由は、[エンジン層](engine-layer.md) の「なぜ Layer を複製して描画スレッドに渡すのか」にあります。

<!-- ※要確認: フレームワーク側が SKPicture の寿命をどう管理しているか / src を検索すると、Picture を明示的に Dispose する箇所は見つからなかった。寿命が GC に任されているかを、PaintingContext と WidgetBinding を読んで確かめる -->

## CPU の Bitmap に描く

GL を使わずに Layer ツリーを確かめるには、`LayerBitmapRenderer` を使います。`Render(root, imageSize)` は、ツリーに `Layout` を呼んでから、透明で初期化した新しい `SKBitmap` に描いて返します。返された `SKBitmap` は、呼び出し側が破棄します。`root` が `null` なら `ArgumentNullException`、`imageSize` の幅か高さが 0 以下なら `ArgumentOutOfRangeException` です。

`SKBitmap` を png にするには、拡張メソッド `Save` を使います。既定の形式は png、品質は 100 です。保存先に同名のファイルがあれば上書きし、親のディレクトリは作りません。

```csharp
using var bitmap = new LayerBitmapRenderer().Render(root, new SKSizeI(400, 300));
bitmap.Save("output.png");
```

`LayerBitmapRenderer` と `Save` は `FloatSoda.Rendering` にあります。RenderObject ツリーや Widget ツリーを描くヘッドレスの描画器は、`FloatSoda.Testing` の `RenderObjectBitmapRenderer` と `WidgetBitmapRenderer` です。

手を動かして確かめる続きは、[実験: 描画層](experiment-rendering.md) と、[描画が届くまで(4段)](tutorial-render-path.md) の2段目にあります。
