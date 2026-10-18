← [コントリビューター](../Home.md)

# 描画層

> このページは執筆前の骨組みです。設計シートとト書きだけを置き、本文はまだ書いていません(追跡: #219)。

## 設計シート

- 分類: 説明型(Diátaxis: explanation)
- 読み手: だれが / `src/FloatSoda.Rendering` のコードを読む、または Layer の種類を足したい人 / 何を知っている / C# と SkiaSharp の `SKCanvas`・`SKPicture` の基本。FloatSoda の他の層は知らなくてよい / 読んで何をしたい / Layer ツリーが何を表し、複製で何が共有されるかをつかみ、Layer を足す・直すときの注意点を知りたい
- 出力言語と文体の指定: 日本語。敬体(CONTRIBUTING.md の「ドキュメントのトンマナ」の「文体」)
- テーマ: 描画層(`FloatSoda.Rendering`)の Layer ツリーが、記録済みの描画(`SKPicture`)と合成の操作(不透明度、変換、クリップ)をどう表し、`Layout` → `Paint` → `Clone` で何をするかを説明する
- 依頼の解釈: なし
- テーマファイル: explanation
- Why(目的): 複製で何が共有されるかを知らないと、`Clone` した Layer の描画リソースを壊す、または共有の前提で書いたコードが別の Layer に影響する
- What(目標・結果): Layer の種類と役割、`Layout` と `Paint` の順序、`Clone` の共有範囲が説明されている。短い解説で、数段落で尽きる量にする
- How(手段): 構成要素で分解する。`ILayer` の3つの操作 → Layer の種類 → CPU の Bitmap への描画 → 複製の共有範囲の順に説明する。複製を描画スレッドに渡す理由はエンジン層とフレームの流れの解説へリンクし、このページでは繰り返さない
- 要素:
  - 描画層は SkiaSharp だけを参照し、FloatSoda のほかのプロジェクトを参照しない(`src/FloatSoda.Rendering/FloatSoda.Rendering.csproj`)
  - `ILayer` は、`PaintBounds`、`Layout`、`Paint`、`Clone` を持つ(`src/FloatSoda.Rendering/Layers/ILayer.cs` の 19、27、34、44 行目)
  - `Layout` は描画境界(`PaintBounds`)を計算し、`Paint` は計算済みの内容をキャンバスに合成する。レイアウトのあとに、同じコンテキストで描画する(`src/FloatSoda.Rendering/Layers/ILayer.cs` の 5〜10 行目)
  - `LayerRenderer.Render` は、`Layout` のあとに `Paint` を呼ぶ。描画先のクリア、フラッシュ、Surface の所有権は呼び出し側が持つ(`src/FloatSoda.Rendering/LayerRenderer.cs` の 33〜34 行目、型の説明)
  - `ContainerLayer` は子の `List<ILayer>` を持ち、`Layout` は子の `PaintBounds` の和を取り、`Paint` は子を順に描く(`src/FloatSoda.Rendering/Layers/ContainerLayer.cs` の 17、41〜62、71〜76 行目付近)
  - `PictureLayer` は、記録済みの `SKPicture` を `Playback` で再生する葉である(`src/FloatSoda.Rendering/Layers/PictureLayer.cs` の 9 行目、46〜49 行目)
  - `OpacityLayer` は `Alpha`(byte)を持ち、`SaveLayer` で子を描いて `Restore` する(`src/FloatSoda.Rendering/Layers/OpacityLayer.cs` の 8、16、27〜38 行目付近)
  - `TransformLayer` は `SKMatrix` を持ち、`Save` → `Concat` → 子を描画 → `Restore` する。`PaintBounds` は変換後の矩形である(`src/FloatSoda.Rendering/Layers/TransformLayer.cs` の 8、16、24〜48 行目)
  - クリップの Layer は3種類で、矩形(`ClipRectLayer`)、角丸矩形(`ClipRoundRectLayer`)、パス(`ClipPathLayer`)である。どれも `ContainerLayer` の派生である(`src/FloatSoda.Rendering/Layers/ClipRectLayer.cs` の 13、94、178 行目)
  - `Clone` は、子を再帰的に複製して独立したツリーを作る。描画リソースの所有権は移さず、複製元と共有される場合がある(`src/FloatSoda.Rendering/Layers/ILayer.cs` の 35〜44 行目、`src/FloatSoda.Rendering/Layers/ContainerLayer.cs` の 85〜94 行目)
  - `PictureLayer.Clone` は、同じ `SKPicture` を参照する新しい `PictureLayer` を作る。ピクチャーの所有権は移さず、`PaintBounds` は複製後の `Layout` で計算する(`src/FloatSoda.Rendering/Layers/PictureLayer.cs` の 50〜63 行目)
  - 共有されるもの: `SKPicture`(参照)。共有されないもの: `Children` のリストと、各 Layer のインスタンス、`Alpha` や `Transform` などの合成の設定(コピー)(`src/FloatSoda.Rendering/Layers/OpacityLayer.cs` の 48〜58 行目、`src/FloatSoda.Rendering/Layers/TransformLayer.cs` の 56〜69 行目)
  - `LayerBitmapRenderer` は、`SKSurface` に Layer ツリーを描いて、新しい `SKBitmap` として返す。幅か高さが 0 以下なら `ArgumentOutOfRangeException` を投げる(`src/FloatSoda.Rendering/LayerBitmapRenderer.cs` の 34〜52 行目)
  - `SKBitmap` の保存には、拡張メソッド `Save` を使う(`src/FloatSoda.Rendering/SKBitmapExtensions.cs` の 29 行目)
  - 描画層は、Engine の `GLView` に依存しない。Engine 側の `Renderer` が `LayerRenderer.Render` を呼んで GL の Surface に描く(`src/FloatSoda.Engine/Renderer.cs` の 27〜36 行目)
  - 複製を描画スレッドに渡す理由は、エンジン層とフレームの流れの解説に置く(`engine-layer.md`、`frame-to-steamvr.md`)
  - 描画境界のレイヤー(`RepaintBoundary`)では、フレームワークが `TransformLayer` を再利用して子を消去し、描き直す。複製が要る理由に関わる(`src/FloatSoda/RenderObjects/PaintingContext.cs` の 265〜281 行目)
  - `ClipRectLayer.Layout` の中に、対になる `Save` のない `Canvas.Restore()` がある(`src/FloatSoda.Rendering/Layers/ClipRectLayer.cs` の 43 行目)。※要確認(影響の有無は、確かめるまで本文に書かない)
- 見出し:
  - Layer ツリーが表すもの
  - Layer の種類と、それぞれの役割
  - Layout のあとに Paint を実行する
  - Clone は何を共有し、何を共有しないか
  - CPU の Bitmap に描く
- ※要確認:
  - `ClipRectLayer.Layout` の `Canvas.Restore()` が、対になる `Save` なしで呼ばれる影響(`Layout` は `Paint` の前に呼ばれ、保存スタックが空なら無効になる見込み。実行して確かめる)
  - `ClipRoundRectLayer` と `ClipPathLayer` の `Layout` に同じ呼び出しがあるか(ソースを読んで確かめる)
  - `LayerContext` が保持するもの(キャンバスだけか)。`src/FloatSoda.Rendering/Layers/LayerContext.cs` を読んで確かめる
  - `PictureLayer.Picture` を共有した状態で、メインスレッドが `SKPicture` を破棄しないこと(所有者は呼び出し側にある。フレームワーク側の寿命管理を確かめる)

## ト書き

### Layer ツリーが表すもの

- 読み手が持って来るもの: 「Layer とは何か」という疑問。Skia の `SKCanvas` と `SKPicture` の基本
- このページが渡すもの: Layer ツリーは、描画の記録(`PictureLayer`)と合成の操作(不透明度、変換、クリップ)を木にしたもの、という1文の定義。描画層の位置づけ(他の FloatSoda のプロジェクトを参照しない)
- 読み手がすること: 木の簡単な図を見て、葉と枝の違いをつかむ
- 次へ進む先: 「Layer の種類と、それぞれの役割」

### Layer の種類と、それぞれの役割

- 読み手が持って来るもの: Layer の定義
- このページが渡すもの: 種類ごとの表(名前、持つ設定、`Paint` で何をするか)。`ContainerLayer`、`PictureLayer`、`OpacityLayer`、`TransformLayer`、クリップ3種
- 読み手がすること: 表で、合成の操作が `Save` / `Restore` の組で子を包むことを確かめる
- 次へ進む先: 「Layout のあとに Paint を実行する」。実験は `experiment-rendering.md`

### Layout のあとに Paint を実行する

- 読み手が持って来るもの: Layer の種類
- このページが渡すもの: `Layout` が `PaintBounds` を計算し、`Paint` が合成する順序。`LayerRenderer.Render` が両方を呼ぶこと。描画先のクリアとフラッシュは呼び出し側の仕事であること
- 読み手がすること: `Paint` の前に `Layout` が必要な理由(境界が先に決まる)を理解する
- 次へ進む先: 「Clone は何を共有し、何を共有しないか」

### Clone は何を共有し、何を共有しないか

- 読み手が持って来るもの: 「Layer を複製する」という操作の存在
- このページが渡すもの: 共有されるもの(`SKPicture` の参照)と、共有されないもの(ツリーの構造、合成の設定)の対比表。複製後に `Layout` で境界が再計算されること。複製を描画スレッドに渡す理由は `engine-layer.md` と `frame-to-steamvr.md` へのリンクにとどめる
- 読み手がすること: 自分が足す Layer の `Clone` が、何をコピーして何を参照するかを決める
- 止まる箇所: `SKPicture` の寿命。所有権は移らないので、複製した側が破棄してはいけない
- 次へ進む先: 「CPU の Bitmap に描く」

### CPU の Bitmap に描く

- 読み手が持って来るもの: Layer ツリーを GL なしで確かめたい
- このページが渡すもの: `LayerBitmapRenderer` で `SKBitmap` を作り、`Save` で png にする流れ。入力の検証(幅と高さは正)。ヘッドレスの確認に使えること
- 読み手がすること: 実験に進む
- 止まる箇所: なし。ここでこのページは終わる。次は `experiment-rendering.md` と `tutorial-render-path.md` の段2
