← [コントリビューター](../Home.md)

# 実験: 描画層

> このページは執筆前の骨組みです。設計シートとト書きだけを置き、本文はまだ書いていません(追跡: #219)。

## 設計シート

- 分類: 説明型(Diátaxis: tutorial。実験のページは、動作原理の問いを1つ立てて、動かして確かめる形で扱う)
- 読み手: だれが / `rendering-layer.md` を読んで、Layer の合成を確かめてみたくなった人。HMD を持っていなくてもよい / 何を知っている / C# のコンソールアプリを作って動かせる。`SKPicture` と `SKCanvas` の基本は分かる / 読んで何をしたい / 不透明度、クリップ、変換の Layer が、同じ絵に何をするかを、png を見比べて確かめたい
- 出力言語と文体の指定: 日本語。敬体(CONTRIBUTING.md の「ドキュメントのトンマナ」の「文体」)
- テーマ: 同じ絵を `OpacityLayer`・`ClipRectLayer`・`TransformLayer` で1つずつ包んだ png を並べて見比べ、合成の Layer が何をするかを確かめる
- 依頼の解釈: なし
- テーマファイル: tutorial
- Why(目的): 「合成の Layer は、子の絵に対して何をするか」を、動かして確かめる。本体の説明は層の解説(`rendering-layer.md`)に置く(docs 再編の決定、2026-10-17。実験の問いを、`PaintingSample` と重ならない形に変えた)
- What(目標・結果): 同じ絵の png が4枚(包まない、不透明度、クリップ、変換)でき、違いを目で見て説明できる
- How(手段): 問いを立てる → 絵を1つ決める → 4通りに包んで png に出す → 見比べる → なぜそうなるかを1〜2段落で読む
- 要素:
  - 問い: 同じ絵を、合成の Layer で包むと、何が変わるか
  - 絵は、青い矩形(20,20)-(220,160)と、橙の矩形(120,100)-(360,260)を、400×300 の描画範囲に記録した `SKPicture` 2つである(2026-10-17 に確かめたコード。`Exp2b`)
  - `OpacityLayer { Alpha = 128 }`、`ClipRectLayer(new SKRect(0, 0, 200, 150))`、`TransformLayer { Transform = SKMatrix.CreateRotationDegrees(15, 200, 150) }` で、同じ絵を包む(同上)
  - 実行結果(2026-10-17 に画素値で確かめた): 包まない絵では、青の中の点(50,50)が不透明な青、橙の中の点(300,230)が不透明な橙、重なりの点(150,120)が橙、絵の外の点(300,50)が透明。不透明度では、色は同じで、青と橙の点のアルファが 128、重なりの点(150,120)もアルファ 128 で、重なりが濃くならない。クリップでは、(300,230)が透明になり、(50,50)と(150,120)は残る。変換では、15 度回転した絵になる
  - 重なりが濃くならないのは、`OpacityLayer.Paint` が `SaveLayer` でまとめて描いてから合成するため(`src/FloatSoda.Rendering/Layers/OpacityLayer.cs` の 27〜38 行目付近)
  - クリップは `Save` → `ClipRect` → 子を描画 → `Restore` する(`src/FloatSoda.Rendering/Layers/ClipRectLayer.cs` の 56〜64 行目付近)
  - 変換は `Save` → `Concat` → 子を描画 → `Restore` する(`src/FloatSoda.Rendering/Layers/TransformLayer.cs` の 40〜48 行目付近)
  - ヘッドレスで動く。`LayerBitmapRenderer.Render(root, SKSizeI)` で `SKBitmap` を作る(`src/FloatSoda.Rendering/LayerBitmapRenderer.cs` の 34 行目)。HMD も SteamVR も要らない
  - `samples/FloatSoda.Samples.PaintingSample` は、Layer ツリー、RenderObject ツリー、Widget ツリーを png に出す。この実験は、そのうち Layer ツリーの1本と重なるが、問いが違う(`samples/FloatSoda.Samples.PaintingSample/Program.cs` の 19、23 行目)
  - 作り方: `FloatSoda.Rendering` へのプロジェクト参照だけのコンソールアプリ(試した `Exp2b.csproj` は `src/FloatSoda.Rendering/FloatSoda.Rendering.csproj` を参照する)
  - png の保存は、試したコードでは `SKImage.Encode` と `File.WriteAllBytes` で行った。`SKBitmapExtensions.Save` も使える(`src/FloatSoda.Rendering/SKBitmapExtensions.cs` の 29 行目)。※要確認(`Save` の引数と出力を読む)
- 見出し:
  - 問い: 合成の Layer は、同じ絵に何をするか
  - 用意するもの
  - 4通りに包んで png に出す
  - png を見比べる
  - なぜそうなるか
  - 次に読むもの
- ※要確認:
  - 変換の png の見た目(画素の点の比較では、回転の効果を示せていない。画像を開いて確かめる)
  - `SKBitmapExtensions.Save` の使い方
  - 実験のコードの載せ方(本文に載せるか、リンクにするか)
  - `ClipRectLayer.Layout` の `Canvas.Restore()`(`rendering-layer.md` の※要確認)が、この実験の結果に影響しないこと

## ト書き

### 問い: 合成の Layer は、同じ絵に何をするか

- 読み手が持って来るもの: `rendering-layer.md` の Layer の種類の表
- このページが渡すもの: 問い1つと、完成形(4枚の png を並べた図)の予告。予想を立てる場所
- 読み手がすること: 3つの Layer がそれぞれ何をするか、予想を決める
- 次へ進む先: 「用意するもの」

### 用意するもの

- 読み手が持って来るもの: 自分の PC
- このページが渡すもの: コンソールアプリの作り方と、`FloatSoda.Rendering` へのプロジェクト参照。HMD も SteamVR も要らないこと
- 読み手がすること: アプリを作る
- 止まる箇所: ビルドに失敗する場合は、`../3-prepare/dev-environment.md` へ送る
- 次へ進む先: 「4通りに包んで png に出す」

### 4通りに包んで png に出す

- 読み手が持って来るもの: 空のコンソールアプリ
- このページが渡すもの: 絵を作る関数と、包むラッパーを差し替えて4枚の png に出すコード
- 読み手がすること: コードを貼り、`dotnet run` する
- できた?: `plain.png`、`opacity.png`、`clip.png`、`transform.png` の4枚ができた
- できなかったら: png が真っ黒、またはサイズが 0 → `rendering-layer.md` の「CPU の Bitmap に描く」の節
- 次へ進む先: 「png を見比べる」

### png を見比べる

- 読み手が持って来るもの: 4枚の png
- このページが渡すもの: 見る点(重なりの濃さ、切れた範囲、回転)と、予想との比べ方
- 読み手がすること: 画像ビューアで並べて開き、違いを言葉にする
- できた?: 「不透明度は重なりを濃くしない」「クリップは範囲の外を消す」「変換は絵全体を回す」と言える
- 次へ進む先: 「なぜそうなるか」

### なぜそうなるか

- 読み手が持って来るもの: 見比べた結果
- このページが渡すもの: 1〜2段落の説明。`OpacityLayer` が `SaveLayer` で子をまとめて描くので重なりが濃くならないこと。クリップと変換は `Save`・`Restore` の組で子を包むこと。詳細は `rendering-layer.md` の「Layer の種類と、それぞれの役割」へリンクする
- 読み手がすること: 自分が足す Layer の `Paint` が、何を `Save` し何を `Restore` するかを考える
- 次へ進む先: 「次に読むもの」

### 次に読むもの

- 読み手が持って来るもの: 実験の結果
- このページが渡すもの: `rendering-layer.md`、`tutorial-render-path.md` の段2、`experiment-framework.md` へのリンク
- 読み手がすること: 続きを選ぶ
- 止まる箇所: なし。ここでこのページは終わる
- 【未決】`PaintingSample` との重なり(どちらも Layer ツリーを png に出す)を、本文でどう扱うか。この実験の問いを変えることで重なりを減らす、と決めている(docs 再編の決定、2026-10-17)。`tutorial-render-path.md` の段2との分担は、段2の本文を書くときに決める
