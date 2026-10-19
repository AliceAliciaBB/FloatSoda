← [ガイド](../Home.md)

# uGUI から来た人へ

> このページは執筆前の骨組みです。設計シートとト書きだけを置き、本文はまだ書いていません(追跡: #219)。

## 設計シート

- 分類: 説明型(Diátaxis: explanation)
- 読み手: だれが / Unity でワールドやギミックを作れる人(Booth クリエイター)。uGUI は知っているが、宣言的な UI は初めて / 何を知っている / シーン、プレハブ、Inspector、Update の考え方を知っている。C# の基本は書ける / 読んで何をしたい / uGUI の考え方を、FloatSoda の考え方に置き換えて理解し、最初のコードを書けるようになりたい
  - 共通の性質と専門用語の扱い: CONTRIBUTING.md の「ドキュメントのトンマナ」の「読み手」
- 出力言語と文体の指定: 日本語。敬体。トンマナは CONTRIBUTING.md の「ドキュメントのトンマナ」に従う
- テーマ: uGUI の考え方(配置して書き換える)から、FloatSoda の考え方(状態を変えて作り直す)への切り替えを説明する
- 依頼の解釈: 対応表は uGUI の概念を起点に、FloatSoda の対応する概念と、違う点を並べる。Flutter の読み替えは、リファレンスの「Flutter の読み替え表」に任せる
- テーマファイル: explanation
- Why(目的): uGUI の癖(オブジェクトを探して値を書き換える)で FloatSoda を書いて詰まる手戻りを防ぐ
- What(目標・結果): シーン、Hierarchy、Inspector、Update などの概念が、FloatSoda でどうなるかを言える。「状態を変える」が基本だと言える
- How(手段): 概念ごとの対応表に、考え方の切り替えを説明する節を添える。uGUI の語彙で書く(他分野の比喩は使わない)
- 要素:
  - FloatSoda にシーンとプレハブはない。UI は C# のコードで宣言する(AGENTS.md の「Target Users」の3番目)
  - Hierarchy に当たるのは、コードの上のウィジェットの木である(再編前の `docs/TargetUsers.md`)
  - uGUI は、オブジェクトを置き、スクリプトから書き換える(`text.text = ...`)。FloatSoda は、状態を変えると UI が作り直される(`SetState`)(再編前の `docs/TargetUsers.md`)
  - Inspector で設定する値は、ウィジェットのプロパティとして、オブジェクト初期化子で書く(`new SizedBox { Width = 100, Height = 100 }`)(README.md の「最小構成のコード」)
  - Canvas に当たるのは、ウィンドウ(`DashboardWindow`、`WorldSpaceWindow`、`DeviceTrackedWindow`)である。※要確認(対応の妥当性)
  - レイアウトは、親が制約を渡し、子がサイズを返す。`Row`/`Column`/`Wrap` などが LayoutGroup に近い。※要確認(対応の妥当性)
  - ボタンは `Button` ではなく、`GestureDetector` で組み立てる(AGENTS.md の「Widget/Element Tree」)
  - 毎フレームの処理(`Update`)は、`AnimationController` と `Ticker` で時間を進める。毎フレームの再ビルドではなく、描画だけを更新する(再編前の `docs/Animation.md`)。※要確認(`Update` の代替として説明してよいか)
  - コントローラーの入力は、`InputActionMap` と `InputAction<T>` で受け取る。Unity Input System の語彙(ActionMap、Action、DefaultBinding)に近い(再編前の `docs/Input.md`)
  - 外部のアセット(シーン、プレハブ)を持たないことで、diff、レビュー、検索がそのまま使える(AGENTS.md の「Target Users」の3番目)
- 見出し:
  - uGUI と FloatSoda の考え方の違い
  - 置いて書き換えるから、状態を変えて作り直すへ
  - 概念の対応表
  - 入力と時間の対応
  - 落とし穴
- ※要確認:
  - Canvas、RectTransform、LayoutGroup、Update との対応(筆者の案。Unity 経験者によるレビューが要る)
  - FloatSoda が `Update` に当たる毎フレームの処理を、どの書き方で推奨するか
  - Booth クリエイターが uGUI の語彙で理解できるかの確認(再編前の `TestStrategy` に、確かめる手段がまだないと書かれていた)

## ト書き

### uGUI と FloatSoda の考え方の違い

- 読み手が持って来るもの: uGUI の経験(シーン、プレハブ、Inspector)
- このページが渡すもの: 違いの要点の1文(置いて書き換えるのではなく、状態を変えて作り直す)と、このページで得られる対応表の予告
- 読み手がすること: 自分の uGUI の経験と照らす
- 次へ進む先: 「置いて書き換えるから、状態を変えて作り直すへ」

### 置いて書き換えるから、状態を変えて作り直すへ

- 読み手が持って来るもの: `text.text = ...` で値を書き換える癖
- このページが渡すもの: 同じ「数字が増える」を uGUI と FloatSoda で書いた2つの短い例の対比。FloatSoda の側は `SetState` を使う
- 読み手がすること: 2つを読み比べ、FloatSoda では書き換える対象を探さないことを理解する
- できた?: 「状態を持つクラスの中で値を変える」と言える
- 次へ進む先: 「概念の対応表」。仕組みの詳細は、ガイドの「値が変わると、画面が変わる仕組み」

### 概念の対応表

- 読み手が持って来るもの: uGUI の概念の名前
- このページが渡すもの: 概念(シーン、プレハブ、Hierarchy、Inspector、Canvas、LayoutGroup、Button)ごとの、FloatSoda の対応と違う点の表
- 読み手がすること: 自分が使ってきた概念を引いて、FloatSoda での対応を確かめる
- 次へ進む先: 対応するウィジェットは「部品を目的から探す」

### 入力と時間の対応

- 読み手が持って来るもの: `Update`、EventSystem、Input System
- このページが渡すもの: ポインタ入力(ヒットテスト)、アクション入力、時間の進め方(`AnimationController`)の対応
- 読み手がすること: 対応を確かめる
- 次へ進む先: ガイドの「コントローラーの入力を受け取る」、「アニメーションを付ける」

### 落とし穴

- 読み手が持って来るもの: これから最初のコードを書く
- このページが渡すもの: uGUI の癖で起きやすい食い違い(オブジェクトを探して書き換えようとする、`Button` がない、押せるのはダッシュボードだけ)
- 読み手がすること: 食い違いを避ける
- 止まる箇所: なし
- 次へ進む先: チュートリアル「最初のオーバーレイを作る」
