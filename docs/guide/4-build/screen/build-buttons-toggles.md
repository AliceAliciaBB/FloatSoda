← [ガイド](../../Home.md)

# ボタンやトグルを組み立てる

> このページは執筆前の骨組みです。設計シートとト書きだけを置き、本文はまだ書いていません(追跡: #219)。

## 設計シート

- 分類: 説明型(Diátaxis: how-to)
- 読み手: だれが / VRChatter、Booth クリエイター、uGUI を避けたいエンジニア。値が変わると画面が変わる仕組み(`SetState()`)を、チュートリアルで一度動かしている / 何を知っている / `GestureDetector` を1回使った。`Button` ウィジェットがまだないことは知らない / 読んで何をしたい / 押せるボタンと、押すたびに切り替わるトグルを、自分の画面に組み立てたい
  - 共通の性質と専門用語の扱い: CONTRIBUTING.md の「ドキュメントのトンマナ」の「読み手」
- 出力言語と文体の指定: 日本語。敬体。トンマナは CONTRIBUTING.md の「ドキュメントのトンマナ」に従う
- テーマ: `GestureDetector` で、押せるボタンと、状態を切り替えるトグルを組み立てる手順
- 依頼の解釈: `FloatSoda.UI` の `ButtonBase` と、デザインシステムの `Button` は使わない。どちらも提供前のため、`GestureDetector` で組み立てる手順に決めた
- テーマファイル: how-to
- Why(目的): `Button` がまだない状態でも、自分の画面に操作できる部品を足せるようにする
- What(目標・結果): ダッシュボードのオーバーレイで、ボタンを押すと値が変わり、トグルを押すたびに状態が切り替わる
- How(手段): 冒頭で、ボタンが効くオーバーレイの種類を伝える。そのあと、ボタン、トグル、押せる範囲の調整の順に組み立てる
- 要素:
  - ポインタ座標はダッシュボードのオーバーレイにだけ届くため、`WorldSpaceWindow` と `DeviceTrackedWindow` では `GestureDetector` のコールバックが呼ばれない(`README.md` の「開発ステータス」、`CONTRIBUTING.md`。追跡: #182)
  - ボタンは `GestureDetector` の `OnTap` と、見た目のウィジェット(`ColoredBox`、`ClipRoundRect`、`Text`)の組み合わせで作る(`samples/FloatSoda.Samples.OverlayApp/CounterWidget.cs`)
  - `OnTap` の中で `SetState()` を呼ぶと、値が変わって画面が描き直される(`samples/FloatSoda.Samples.OverlayApp/CounterWidget.cs`)
  - `OnTap` は押して離した時点で成立する。押しただけでは呼ばれない(`samples/FloatSoda.Samples.GestureDetector/README.md`)
  - 空白を含む領域全体で反応させるには `Behaviour = HitTestBehaviour.Opaque` を指定する(`samples/FloatSoda.Samples.GestureDetector/README.md`)
  - ドラッグは `OnPanUpdate` で受け取る(`samples/FloatSoda.Samples.OverlayApp/DragBoxWidget.cs`)
  - 生のポインターイベントとホバーは `Listener` と `PointerRegion` で受け取る(`README.md` の「入力系」)
  - トグルは、`bool` の状態を `State` に持ち、`OnTap` で反転させ、状態で見た目を分けて作る(同じ `SetState()` の仕組み。`samples/FloatSoda.Samples.OverlayApp/CounterWidget.cs`)
  - `Button` などの UI コンポーネントは未提供で、解除の条件は Phase 5(`README.md` の「まだ使えないもの」、追跡: #219)
- 見出し:
  - ボタンが効くオーバーレイの種類
  - 前提条件
  - 手順1: 押せるボタンを作る
  - 手順2: 押すたびに切り替わるトグルを作る
  - 手順3: 押せる範囲を決める
  - 結果を確かめる
  - うまくいかないとき
- ※要確認:
  - `GestureDetector` のボタンが、ダッシュボードのオーバーレイで実機で反応するか(HMD で確かめる)
  - トグルのコード例が、今の公開 API でコンパイルできるか(コード例を取り出してビルドして確かめる)
  - `samples/FloatSoda.Samples.GestureDetector` の型名が `GestureDetectorWidget` と `GestureDetector` のどちらか(`README.md` と `Program.cs` で食い違って見える。`GestureDetectorDemo.cs` を読んで確かめる)

## ト書き

### ボタンが効くオーバーレイの種類

- 読み手が持って来るもの: 「ボタンを付けたい」という目的
- このページが渡すもの: 今押せるのはダッシュボードのオーバーレイだけという制約(追跡: #182)と、`WorldSpaceWindow` と `DeviceTrackedWindow` は表示専用であること
- 読み手がすること: 自分のオーバーレイの種類を確かめる
- 止まる箇所: ダッシュボード以外に出したい読み手。ここでは回避策を書かず、「オーバーレイの種類と選び方」へ送る
- 次へ進む先: 「前提条件」

### 前提条件

- 読み手が持って来るもの: チュートリアルを終えたプロジェクト
- このページが渡すもの: 要るもの(`SetState()` の使い方)と、「状態と作り直し」へのリンク
- 読み手がすること: 確かめて、足りなければリンク先へ戻る
- 次へ進む先: 手順1

### 手順1: 押せるボタンを作る

- 読み手が持って来るもの: 見た目だけの四角
- このページが渡すもの: `GestureDetector` で包み、`OnTap` で `SetState()` を呼ぶ書き方
- 読み手がすること: 四角をボタンにする
- できた?: コントローラーで押すと値が変わる
- 次へ進む先: 手順2

### 手順2: 押すたびに切り替わるトグルを作る

- 読み手が持って来るもの: 手順1のボタン
- このページが渡すもの: `bool` の状態と、状態で見た目を変える書き方
- 読み手がすること: 押すたびにオンとオフが切り替わるようにする
- できた?: 押すたびに見た目が切り替わる
- 次へ進む先: 手順3

### 手順3: 押せる範囲を決める

- 読み手が持って来るもの: 余白が多いボタン
- このページが渡すもの: `Behaviour = HitTestBehaviour.Opaque` の意味と、押せない範囲ができる理由
- 読み手がすること: 余白の部分を押しても反応するように指定する
- できた?: ボタンの余白を押しても反応する
- 次へ進む先: 「結果を確かめる」
- 【未決】ドラッグ(`OnPanUpdate`)もこのページで扱うか。見本の `DragBoxWidget.cs` はあるが、読み手の目的は「ボタンやトグル」

### 結果を確かめる

- 読み手が持って来るもの: 手順を終えたコード
- このページが渡すもの: HMD で押して確かめる観点
- 読み手がすること: ダッシュボードで押す。HMD を被らずに確かめたいときは「HMD を被らずに確かめる」へ進む。その場合でも、ダッシュボードで動くことの確認は別に要ること(追跡: #182)を添える
- 次へ進む先: 「表情切り替えパネルを作る」

### うまくいかないとき

- 読み手が持って来るもの: 押しても反応しない
- このページが渡すもの: ガイドの「5. 困ったとき」の「反応しないときに切り分ける」へのリンク
- 読み手がすること: リンク先で切り分ける
- 止まる箇所: なし
