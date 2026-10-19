← [ガイド](../../Home.md)

# アニメーションを付ける

> このページは執筆前の骨組みです。設計シートとト書きだけを置き、本文はまだ書いていません(追跡: #219)。

## 設計シート

- 分類: 説明型(Diátaxis: how-to)
- 読み手: だれが / VRChatter、Booth クリエイター、uGUI を避けたいエンジニア。最初のオーバーレイは動かし終えている / 何を知っている / `StatefulWidget` と `SetState()` で値を変えられる / 読んで何をしたい / 画面の一部を、ふわっと出す、点滅させるなど、時間で動かしたい
  - 共通の性質と専門用語の扱い: CONTRIBUTING.md の「ドキュメントのトンマナ」の「読み手」
- 出力言語と文体の指定: 日本語。敬体。トンマナは CONTRIBUTING.md の「ドキュメントのトンマナ」に従う
- テーマ: `AnimationController` と `FadeTransition` で、子の不透明度を時間で変える手順
- 依頼の解釈: 「アニメーション」は、今ある `FadeTransition` による不透明度の変化だけを扱う。移動や拡大のアニメーションは書かない(`Tween` と暗黙的アニメーションは未提供で、解除の条件は Phase 3。追跡: #219)
- テーマファイル: how-to
- Why(目的): 画面の変化を、フレームごとの `SetState()` なしで滑らかに動かす
- What(目標・結果): HMD の中で、四角が約1.5秒周期でフェードイン・フェードアウトを繰り返す
- How(手段): `TickerProviderState` を継承した `State` で `AnimationController` を作り、`FadeTransition` に渡す
- 要素:
  - アニメーションを駆動する `State` は `TickerProviderState<T>` を継承し、`AnimationController` の `Vsync` に `this` を渡す(`src/FloatSoda/Animation/TickerProviderState.cs`)
  - `AnimationController` の `Duration` は必須で、`Forward()` / `Reverse()` / `Stop()` で動かす(`src/FloatSoda/Animation/AnimationController.cs`)
  - `StatusChanged` で `Completed` と `Dismissed` を受けて折り返すと、往復させられる(`samples/FloatSoda.Samples.OverlayApp/PulseWidget.cs`)
  - `FadeTransition` は `Opacity` に `IAnimation<double>` を受け取り、`SetState()` なしでペイントだけを更新する(`src/FloatSoda/Widgets/Animation/FadeTransition.cs`)
  - `Curve` を指定すると、変化の速さを変えられる(`src/FloatSoda/Animation/AnimationController.cs`、`src/FloatSoda/Animation/Curves.cs`)
  - 不透明度の固定値だけ変えたいときは `Opacity` ウィジェットを使う(`README.md` の「描画系」)
  - `AnimationController` は `IDisposable` で、`State` の破棄で解放する(`src/FloatSoda/Animation/AnimationController.cs`)
- 見出し:
  - この手順でできること
  - 前提条件
  - 手順1: アニメーションを動かす State を作る
  - 手順2: AnimationController を作って動かす
  - 手順3: FadeTransition で子に反映する
  - 手順4: 往復させる
  - 結果を確かめる
  - うまくいかないとき
- ※要確認:
  - `State` の破棄時に `AnimationController.Dispose()` を呼ぶ位置(`State<T>` の `Dispose` に相当するメソッド名を `src/FloatSoda/Widgets/ComponentWidget.cs` で確かめる)
  - `Curve` の指定コードが今の公開 API でコンパイルできるか(コード例を取り出してビルドして確かめる)

## ト書き

### この手順でできること

- 読み手が持って来るもの: 「画面を時間で動かしたい」という目的
- このページが渡すもの: 完成した動きの説明(往復するフェード)と、動かせるのは今は不透明度だけという範囲(追跡: #219)
- 読み手がすること: 自分の目的が不透明度の変化で足りるかを確かめる
- 止まる箇所: 移動や拡大を動かしたい読み手。ここでは書かず、リファレンスの「できないこと・まだ使えないもの」へ送る
- 次へ進む先: 「前提条件」

### 前提条件

- 読み手が持って来るもの: `StatefulWidget` を書いた経験(チュートリアルの段2)
- このページが渡すもの: 要るものの短い一覧
- 読み手がすること: 確かめて、足りなければチュートリアルへ戻る
- 次へ進む先: 手順1

### 手順1: アニメーションを動かす State を作る

- 読み手が持って来るもの: `StatefulWidget` と `State`
- このページが渡すもの: `State<T>` ではなく `TickerProviderState<T>` を継承する理由と書き方
- 読み手がすること: `State` の基底を `TickerProviderState<T>` に替える
- できた?: コンパイルが通る
- 次へ進む先: 手順2

### 手順2: AnimationController を作って動かす

- 読み手が持って来るもの: 手順1の `State`
- このページが渡すもの: `AnimationController` の作り方(`Vsync`、`Duration`)と `Forward()`
- 読み手がすること: `InitState()` で作り、`Forward()` を呼ぶ
- できた?: 実行しても例外が出ない
- 次へ進む先: 手順3

### 手順3: FadeTransition で子に反映する

- 読み手が持って来るもの: 手順2のコントローラー
- このページが渡すもの: `FadeTransition` に `Opacity` として渡す書き方と、`SetState()` が要らないこと
- 読み手がすること: 動かしたい部分を `FadeTransition` で包む
- できた?: HMD の中で四角がフェードインする
- 次へ進む先: 手順4

### 手順4: 往復させる

- 読み手が持って来るもの: 一度だけ動くフェード
- このページが渡すもの: `StatusChanged` で `Completed` なら `Reverse()`、`Dismissed` なら `Forward()` を呼ぶ書き方
- 読み手がすること: 折り返しの処理を足す
- できた?: フェードイン・フェードアウトが続く
- 次へ進む先: 「結果を確かめる」

### 結果を確かめる

- 読み手が持って来るもの: 手順を終えたコード
- このページが渡すもの: HMD で見る観点(周期、止まらないこと)
- 読み手がすること: HMD で確かめる。HMD を被らずに確かめたいときは、「HMD を被らずに確かめる」へ進む
- 次へ進む先: 「ボタンやトグルを組み立てる」

### うまくいかないとき

- 読み手が持って来るもの: 動かない、または途中で止まる
- このページが渡すもの: ガイドの「5. 困ったとき」の「反応しないときに切り分ける」と「負荷を抑える」へのリンク
- 読み手がすること: 症状に合うページへ進む
- 止まる箇所: なし
