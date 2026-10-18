← [コントリビューター](../Home.md)

# 実験: フレームワーク層

> このページは執筆前の骨組みです。設計シートとト書きだけを置き、本文はまだ書いていません(追跡: #219)。

## 設計シート

- 分類: 説明型(Diátaxis: tutorial。実験のページは、動作原理の問いを1つ立てて、動かして確かめる形で扱う)
- 読み手: だれが / `framework-layer.md` を読んで、確かめてみたくなった人。HMD を持っていなくてもよい / 何を知っている / C# のコンソールアプリを作って動かせる。`SetState` と `RepaintBoundary` の名前は知っている / 読んで何をしたい / 変更が、どの RenderObject の描き直しになるかを、ログで自分の目で確かめたい
- 出力言語と文体の指定: 日本語。敬体(CONTRIBUTING.md の「ドキュメントのトンマナ」の「文体」)
- テーマ: `SetState` のあとに `RenderPipeline.NodesNeedingPaint` へ入るのが、変えた側の `RepaintBoundary` だけであることを、ログで確かめる
- 依頼の解釈: なし
- テーマファイル: tutorial
- Why(目的): 「プロパティを変えると、どこから描き直されるか」を、読むだけでなく動かして確かめる。本体の説明は層の解説(`framework-layer.md`)に置く
- What(目標・結果): 実行したログで、(1) `SetState` 直後は `NodesNeedingPaint` が空、(2) `BuildScope` のあとは変えた側の `RepaintBoundary` だけが入る、(3) `FlushPaint` のあとは空に戻る、の3点を確かめる
- How(手段): 問いを立てる → 小さなコンソールアプリを動かす → ログを読む → `RepaintBoundary` を外して結果が変わるのを見る → なぜそうなるかを1〜2段落で読む
- 要素:
  - 問い: 画面の一部の色を変えたとき、描き直されるのは画面全体か、一部か
  - 実験のコードは、公開 API だけで書ける。`RenderView`・`RenderPipeline`・`BuildOwner`・`RenderObjectToWidgetAdapter` を作り、`StatefulWidget` の `SetState` を呼び、`BuildScope`・`FlushLayout`・`FlushPaint` を順に呼ぶ(2026-10-17 に実行を確かめた。試したコードは `Exp1Repaint`。58 行)
  - ウィジェットは、`Column` の中に2つの `RepaintBoundary` を並べ、片方の `ColoredBox` の色を `SetState` で切り替える(同上)
  - 実行結果(2026-10-17 に確かめた): 初回フレームのあと `NodesNeedingPaint: []`。`SetState` 直後(`BuildScope` の前)も `[]`。`BuildScope` のあとは `[RenderRepaintBoundary@depth2]` で、変えた側の `RenderRepaintBoundary` と、その中の `RenderColoredBox` の `NeedsPaint` が `True`、もう一方は `False`。`FlushPaint` のあとは `[]`
  - `RepaintBoundary` を外して同じ実験をすると、`BuildScope` のあとは `[RenderView@depth0]` になる(2026-10-17 に確かめた。`RepaintBoundary` を外した変種)
  - 仕組み: `SetState` は `MarkNeedsBuild` を呼び、`BuildScope` が `UpdateRenderObject` でプロパティを書き換え、`MarkNeedsPaint` が最も近い再描画境界まで伝播して、その境界を `NodesNeedingPaint` に登録する(`src/FloatSoda/Widgets/ComponentWidget.cs` の 86〜89 行目、`src/FloatSoda/Elements/BuildOwner.cs` の 96〜135 行目、`src/FloatSoda/RenderObjects/RenderObject.cs` の 258〜273 行目)
  - `RenderView` は再描画境界である(実行結果の `boundary=True`。`RenderView` の `IsRepaintBoundary` の定義は `src/FloatSoda/RenderObjects/RenderView.cs` を読んで確かめる)。※要確認
  - 作り方: 新しいコンソールアプリに `FloatSoda` のプロジェクト参照を足す(試した `Exp1Repaint.csproj` は `src/FloatSoda/FloatSoda.csproj` を参照する)
  - この実験は HMD も SteamVR も要らない(`OVRApplication` を作らないため)。※要確認(ウィジェットの作成が OpenVR を呼ばないことは実行で確かめた。ほかの PC では未確認)
- 見出し:
  - 問い: 一部の色を変えると、どこが描き直されるか
  - 用意するもの
  - 動かす
  - ログを読む
  - 壊してみる: RepaintBoundary を外す
  - なぜそうなるか
  - 次に読むもの
- ※要確認:
  - `RenderView` が再描画境界になる定義(`RenderView.cs` を読む)
  - 実験のコードを docs に載せる形(本文に全文を載せるか、`samples/` に置くか)。`samples/` に置くなら、サンプルの規約(CONTRIBUTING.md)に従う
  - 実行結果が、別の PC(HMD も SteamVR もない環境)でも同じになるか

## ト書き

### 問い: 一部の色を変えると、どこが描き直されるか

- 読み手が持って来るもの: `framework-layer.md` を読んだ記憶
- このページが渡すもの: 問い1つ(色を変えたとき、描き直されるのは全体か一部か)と、予想を立てる場所。実験の完成形(ログの3つの行)の予告
- 読み手がすること: 予想を決める
- 次へ進む先: 「用意するもの」

### 用意するもの

- 読み手が持って来るもの: 自分の PC
- このページが渡すもの: コンソールアプリの作り方と、`FloatSoda` へのプロジェクト参照の足し方。HMD も SteamVR も要らないこと
- 読み手がすること: アプリを作る
- 止まる箇所: ビルドに失敗する場合は、`../3-prepare/dev-environment.md` へ送る
- 次へ進む先: 「動かす」

### 動かす

- 読み手が持って来るもの: 空のコンソールアプリ
- このページが渡すもの: 実験のコード(初回フレームの描画 → `SetState` → `BuildScope` → `FlushLayout`・`FlushPaint`。各段のあとにツリーと `NodesNeedingPaint` を出す)
- 読み手がすること: コードを貼り、`dotnet run` する
- できた?: 「初回フレーム後」「SetState 直後」「BuildScope 後」「FlushPaint 後」の4つのログが出た
- できなかったら: コンパイルが通らない → 公開 API の変更の可能性。`framework-layer.md` の要素と照らす
- 次へ進む先: 「ログを読む」

### ログを読む

- 読み手が持って来るもの: 4つのログ
- このページが渡すもの: どの行を見るか(`NodesNeedingPaint` の中身と、各 `RenderObject` の `NeedsPaint`)
- 読み手がすること: 予想と結果を比べる
- できた?: 「`BuildScope` のあとだけ、変えた側の `RenderRepaintBoundary` が入った」と言える
- 次へ進む先: 「壊してみる: RepaintBoundary を外す」

### 壊してみる: RepaintBoundary を外す

- 読み手が持って来るもの: 結果の読み方
- このページが渡すもの: `RepaintBoundary` を外す変更の指示と、予想を立てる場所
- 読み手がすること: 外して実行し、`NodesNeedingPaint` の中身を見る
- できた?: `[RenderView@depth0]` になった(描き直しの範囲が、境界から根まで広がった)
- 次へ進む先: 「なぜそうなるか」

### なぜそうなるか

- 読み手が持って来るもの: 2つの結果
- このページが渡すもの: 1〜2段落の説明。`MarkNeedsPaint` が、境界でなければ親へ伝わり、最初の境界で止まって登録されること。境界がなければ根の `RenderView` まで伝わること。詳細は `framework-layer.md` の「レイアウトと描画は、境界までを描き直す」へリンクする
- 読み手がすること: 自分の変更で、境界をどこに置くかを考える
- 次へ進む先: 「次に読むもの」

### 次に読むもの

- 読み手が持って来るもの: 実験の結果
- このページが渡すもの: `framework-layer.md`、`experiment-rendering.md`、`frame-to-steamvr.md` へのリンク
- 読み手がすること: 続きを選ぶ
- 止まる箇所: なし。ここでこのページは終わる
- 【未決】実験のコードを、本文に全文で載せるか、`samples/` に置いてリンクするか
