← [リファレンス](Home.md)

# 部品を目的から探す

> このページは執筆前の骨組みです。設計シートとト書きだけを置き、本文はまだ書いていません(追跡: #219)。

## 設計シート

- 分類: 説明型(Diátaxis: reference)
- 読み手: だれが / 画面を組んでいる人、または AI に画面を書かせる人 / 何を知っている / 作りたい画面のイメージは持っている。部品の名前は知らない / 読んで何をしたい / 目的(並べる、押す、動かす)から使う部品の当たりをつけ、API リファレンスで正確な定義を引きたい
  - 共通の性質と専門用語の扱い: CONTRIBUTING.md の「ドキュメントのトンマナ」の「読み手」
- 出力言語と文体の指定: 日本語。敬体。トンマナは CONTRIBUTING.md の「ドキュメントのトンマナ」に従う
- テーマ: 画面を組む目的から、使える部品の名前を引き、API リファレンスの型へ進めるようにする
- 依頼の解釈: 部品の使い方の説明は書かない。目的、部品、1文の違い、API リファレンスへのリンク、作例へのリンクの5つだけを並べる
- テーマファイル: reference
- Why(目的): 部品の名前を知らない読み手が、目的の言葉で部品を見つける。名前が分かったら API リファレンスへ移る
- What(目標・結果): 目的ごとに部品が同じ形で並び、各部品から型と作例へたどれる。使えない部品は「まだない」と分かる
- How(手段): 目的を「並べる」「大きさと余白を決める」「見た目を整える」「文字や画像を出す」「押す・反応する」「動かす」「出し分ける」「値を伝える」「出す場所を決める」の9つに分け、表で並べる。表の列は「部品」「違いの1文」「API リファレンス」「作例」とする
- 要素:
  - 並べる: `Row`/`Column`/`Flex`(一列)、`Wrap`(折り返す)、`Stack`/`Positioned`(重ねる)、`IndexedStack`(1つだけ出す)、`Expanded`/`Flexible`/`Spacer`(余りを分ける)(README.md の「実装済みの Widget」、`src/FloatSoda/Widgets/Layout/`)
  - 大きさと余白を決める: `SizedBox`、`Padding`、`Align`/`Center`、`ConstrainedBox`、`LimitedBox`、`FractionallySizedBox`、`AspectRatio`、`FittedBox`、`OverflowBox`、`IntrinsicWidth`/`IntrinsicHeight`(README.md、`src/FloatSoda/Widgets/Layout/`)
  - 見た目を整える: `ColoredBox`、`DecoratedBox`、`Container`、`Opacity`、`Transform`、`ClipRect`/`ClipRoundRect`/`ClipOval`/`ClipCustomPath`(README.md、`src/FloatSoda/Widgets/Paint/`)
  - 文字や画像を出す: `Text`、`RichText`、`Image`、`Icon`(`src/FloatSoda/Widgets/Text.cs`、`Paint/Image.cs`、`Paint/Icon.cs`)
  - 押す・反応する: `GestureDetector`(タップ、ドラッグ)、`Listener`(生のイベント)、`PointerRegion`(ホバー)、`AbsorbPointer`/`IgnorePointer`(反応を止める)。ポインタが届くのはダッシュボードだけ(`src/FloatSoda/Widgets/Gesture/`、AGENTS.md)
  - 動かす: `StatefulWidget`/`State`/`SetState`(値の変化)、`AnimationController`(時間で変わる値)、`FadeTransition`(フェード)(`src/FloatSoda/Widgets/Animation/FadeTransition.cs`、AGENTS.md の「Widget/Element Tree」)
  - 出し分ける: `Visibility`、`Offstage`、`TaskBuilder<T>`(非同期の結果で切り替える)、`Builder`(`src/FloatSoda/Widgets/`、CHANGELOG.md の 0.4.0)
  - 値を伝える: `InheritedWidget`、`DefaultTextStyle`、`ListenableBuilder`(`src/FloatSoda/Widgets/`)
  - 出す場所を決める: `DashboardWindow`、`WorldSpaceWindow`、`DeviceTrackedWindow`、`DesktopWindow`(README.md、`src/FloatSoda.Engine/DesktopWindow.cs`)
  - 自作する: `StatelessWidget`、`StatefulWidget<T>`、`LeafRenderObjectWidget<T>`、`ParentDataWidget<T>`(AGENTS.md、CHANGELOG.md の 0.4.0)
  - まだ使えない部品(`ListView`、`GridView`、`SingleChildScrollView`、`Button` など)は表に載せず、「できないこと・まだ使えないもの」へのリンクで知らせる(`src/FloatSoda/Widgets/Layout/ListView.cs` が `internal`)
  - API リファレンスは、NuGet で公開しているパッケージの型を載せ、Starlight に取り込む(docs 再編の決定、2026-10-16〜17)
- 見出し:
  - この一覧の使い方
  - 並べる
  - 大きさと余白を決める
  - 見た目を整える
  - 文字や画像を出す
  - 押す・反応する
  - 動かす
  - 出し分ける・値を伝える
  - 出す場所を決める
  - 見つからないとき
- ※要確認:
  - API リファレンスの型へのリンクの形式(DocFX の markdown 出力の取り込み後の URL で決まる)
  - `Icon` が利用者向けの公開 API として使えるか(CHANGELOG.md の 0.4.0 は追加と書くが、AGENTS.md の記述は「internal のスタブ」で食い違う。`src/FloatSoda/Widgets/Paint/Icon.cs` は `public sealed record`)
  - 目的の分け方(9つ)が、読み手の言葉に合っているか(ジュニアコーダーテストで、読み手が引けるかを確かめる)

## ト書き

### この一覧の使い方

- 読み手が持って来るもの: 作りたい画面のイメージ
- このページが渡すもの: 表の列の意味と、目的の9つの分類。部品の使い方は書いていないこと
- 読み手がすること: 目的の分類を選ぶ
- 次へ進む先: 目的の各節
- 【未決】この一覧を手で書くか、`src/` の公開ウィジェットから生成するか。手で書くと、部品が増えたときに取り残される(docs のドリフト回避方針)

### 並べる

- 読み手が持って来るもの: 部品を横一列、縦一列、重ね、折り返しで並べたい
- このページが渡すもの: 並べ方の部品の表(部品、違いの1文、API リファレンス、作例)
- 読み手がすること: 目的に合う部品を選び、API リファレンスか作例を開く
- 次へ進む先: 作例の README、API リファレンスの型

### 大きさと余白を決める

- 読み手が持って来るもの: 部品の大きさ、余白、位置を決めたい
- このページが渡すもの: 同上の表。思った大きさにならないときは、ガイドの「レイアウトの考え方」にあること
- 読み手がすること: 目的に合う部品を選ぶ
- 次へ進む先: 同上

### 見た目を整える

- 読み手が持って来るもの: 色、角丸、透明、回転、切り抜きを付けたい
- このページが渡すもの: 同上の表
- 読み手がすること: 目的に合う部品を選ぶ
- 次へ進む先: 同上。まとめて整える手順は、ガイドの「見た目を整える」

### 文字や画像を出す

- 読み手が持って来るもの: 文字、画像、アイコンを出したい
- このページが渡すもの: 同上の表。折り返しや省略の指定がないという制約への、「できないこと・まだ使えないもの」へのリンク
- 読み手がすること: 目的に合う部品を選ぶ
- 次へ進む先: 同上

### 押す・反応する

- 読み手が持って来るもの: ボタン、ホバー、ドラッグを作りたい
- このページが渡すもの: 同上の表。ボタンの部品はなく、`GestureDetector` で組み立てること。ポインタが届くのはダッシュボードだけという制約
- 読み手がすること: 目的に合う部品を選ぶ
- 次へ進む先: ガイドの「ボタンやトグルを組み立てる」、「コントローラーの入力を受け取る」

### 動かす

- 読み手が持って来るもの: 値が変わったら画面を変えたい、時間で動かしたい
- このページが渡すもの: 同上の表。使えるアニメーションの範囲は、「できないこと・まだ使えないもの」にあること
- 読み手がすること: 目的に合う部品を選ぶ
- 次へ進む先: ガイドの「値が変わると、画面が変わる仕組み」、「アニメーションを付ける」

### 出し分ける・値を伝える

- 読み手が持って来るもの: 条件で出し分けたい、離れた部品へ値を渡したい
- このページが渡すもの: 同上の表
- 読み手がすること: 目的に合う部品を選ぶ
- 次へ進む先: 同上

### 出す場所を決める

- 読み手が持って来るもの: どこにオーバーレイを出したいか
- このページが渡すもの: ウィンドウの種類の表。押せるのはダッシュボードだけという制約
- 読み手がすること: 場所を選ぶ
- 次へ進む先: ガイドの「オーバーレイの種類と選び方」

### 見つからないとき

- 読み手が持って来るもの: 目的に合う部品が見つからない
- このページが渡すもの: 見つからない理由の3つ(まだない、組み合わせで作る、名前が違う)と、それぞれの行き先。まだない部品は「できないこと・まだ使えないもの」、Flutter の名前は「Flutter の読み替え表」、組み合わせは AI に書かせる手順
- 読み手がすること: 行き先を選ぶ
- 止まる箇所: なし。ここでページは終わる
