← [コントリビューター](../Home.md)

# Flutter から移植する

> このページは執筆前の骨組みです。設計シートとト書きだけを置き、本文はまだ書いていません(追跡: #219)。

## 設計シート

- 分類: 説明型(Diátaxis: how-to)
- 読み手: だれが / Flutter の Widget や RenderObject を FloatSoda に移植する人。Flutter を知っているエンジニアを含む / 何を知っている / C# と、Flutter の Widget の使い方。FloatSoda の規約は知らなくてよい / 読んで何をしたい / Flutter の元の実装を読み、どこまでそろえるかを決めて、テストと一緒に写したい
- 出力言語と文体の指定: 日本語。敬体(CONTRIBUTING.md の「ドキュメントのトンマナ」の「文体」)
- テーマ: Flutter の部品を FloatSoda に移植するときの5つの問い(元の実装を見つける、どこまでそろえるか、差異をどう記録するか、テストをどう写すか、C# らしく書いてよい範囲)に順に答える
- 依頼の解釈: なし
- テーマファイル: how-to
- Why(目的): 「実装しやすい」「こちらが自然」だけで挙動を変えると、Flutter の挙動を期待して書く利用者と LLM が困る。移植の判断を手順にして、差異が理由とテストつきで残るようにする
- What(目標・結果): 移植した部品が、Flutter の観測できる挙動と合っている。合わない箇所は、理由とテストつきで記録されている。PR から Flutter の版、参照した実装、参照したテストを追跡できる
- How(手段): 5つの問いを手順に並べる。1 元の実装を見つける → 2 どこまでそろえるか決める → 3 差異を残すなら記録する → 4 Flutter のテストを写す → 5 C# として自然な書き方に置き換える
- 要素:
  - Flutter の元の実装は、ローカル参照クローン `~/code_reading/flutter_reference` で引く。Widget / Element / RenderObject の特定には `flutter-widget-source` skill を使う(CONTRIBUTING.md の「テスト観点」、REVIEW.md の「3. 仕様の一次情報と優先順位」)
  - Flutter 由来の Widget と RenderObject は、明示された理由のない限り、観測できる挙動に差異を作らない(REVIEW.md の「6. Flutter parity」)
  - parity の対象は、プロパティの意味、既定値、レイアウト、描画とクリップ、ヒットテスト、子の扱い、Widget の更新、不正・退化入力の扱い、dirty の条件、Element と State のライフサイクル(REVIEW.md の「6. Flutter parity」)
  - 理由にならないもの。「実装しやすい」「こちらの方が安全」「こちらの方が自然」だけ(REVIEW.md の「6. Flutter parity」)
  - C# や .NET として自然な表現への置き換え(`event`、`init` / `required`、`record` / `record struct`、.NET 標準機構)は、差異に数えない。迷ったら「利用者から見た挙動が変わるか」で切り分ける(REVIEW.md の「6. Flutter parity」、前の docs の `APIDesign.md`(退避済み))
  - 差異を残すときの記録必須は5項目。Flutter の挙動、FloatSoda の挙動、差異が必要な理由、差異を固定するテスト、利用者に影響する場合のドキュメント(REVIEW.md の「6. Flutter parity」)
  - 差異の記録先は `known-divergences.md`(`.agents/skills/floatsoda-device-test-gen/references/known-divergences.md`)。ADR にまとめる移行は後回しの決定(docs 再編の決定、2026-10-17)
  - 差異のラベルは deliberate、not ported、port mistake、unlabelled。ラベルで書くテストが逆になる(`.agents/skills/floatsoda-device-test-gen/references/known-divergences.md`、前の docs の `TestStrategy.md`(退避済み)の「5.」)
  - 仕様が食い違うときの優先順位は、FloatSoda で明示された差異・設計判断、Flutter の仕様・実装・公式テスト、既存の FloatSoda 実装(根拠にならない)の順(REVIEW.md の「3. 仕様の一次情報と優先順位」)
  - Flutter 由来の Widget と RenderObject のテストは、Flutter の実装分岐、公式テスト、API contract を確かめ、対応する重要ケースを FloatSoda 側でも検証する(CONTRIBUTING.md の「テスト観点」)
  - 追跡のために、PR に Flutter の版または commit、参照した source、参照した tests を書く(`.github/PULL_REQUEST_TEMPLATE.md` の「Flutter reference」、REVIEW.md の「6. Flutter parity」)
  - 既に決めた非移植方針(Semantics など)を parity を理由に破らない(REVIEW.md の「6. Flutter parity」)
  - 受け入れ範囲は、`basic.dart` 相当のプリミティブ層(CONTRIBUTING.md の「受け入れ範囲」)
- 見出し:
  - この手順でできること
  - 前提条件
  - 手順1: Flutter の元の実装を見つける
  - 手順2: どこまでそろえるかを決める
  - 手順3: 差異を残すなら、記録する
  - 手順4: Flutter のテストを FloatSoda のテストに写す
  - 手順5: C# として自然な書き方に置き換える
  - PR に書くこと
  - うまくいかないとき
- ※要確認:
  - 差異の記録先が ADR に移るまでの、記録の置き場所の案内の書き方(今は `known-divergences.md`。移行は後回し)
  - `flutter-widget-source` skill の入手方法(`.agents/skills/` にない)
  - Flutter の「公式テスト」の置き場所と、FloatSoda のテストへ写すときの具体的な型(1対1で写すのか、観点を写すのか)を示す一次情報があるか
  - `known-divergences.md` の「Observation: HEADLESS か VR」の使い分けを、このページが説明するか、案内だけにするか
  - ※要確認: CONTRIBUTING.md の「テスト観点」にある `~/code_reading/flutter_reference` が、読み手の PC で別の場所のことがある。パスの書き方

## ト書き

### この手順でできること

- 読み手が持って来るもの: 移植したい部品の名前
- このページが渡すもの: 手順が終わったときの状態(挙動が合っている、または差異が理由とテストつきで記録されている)と、5つの手順
- 読み手がすること: 自分の部品が Flutter 由来かを確かめる(Flutter に対応する概念がなければ、このページの対象外)
- 次へ進む先: 「前提条件」

### 前提条件

- 読み手が持って来るもの: Issue と、手元のビルド環境
- このページが渡すもの: 開発環境と参照クローンが用意できていること(../3-prepare/dev-environment.md)、Issue を先に立てる規約(../3-prepare/conventions.md)
- 読み手がすること: 足りなければ先に用意する
- 次へ進む先: 手順1

### 手順1: Flutter の元の実装を見つける

- 読み手が持って来るもの: 移植したい部品の名前
- このページが渡すもの: 参照クローンと skill の使い方。Widget、Element、RenderObject の3つの位置を、Flutter の版とあわせて控えること
- 読み手がすること: 3つの位置を見つけ、版を控える
- できた?: 3つのファイルと版(commit)が書き出せる
- 次へ進む先: 手順2

### 手順2: どこまでそろえるかを決める

- 読み手が持って来るもの: Flutter の実装を読んだ結果
- このページが渡すもの: parity の対象(10項目)と、理由にならないもの。判断の優先順位(FloatSoda の明示 → Flutter → 既存の実装は根拠にしない)。非移植方針は破らないこと
- 読み手がすること: 差異が要る箇所があるかを決める。なければ手順4へ
- できた?: 「差異なし」か「差異あり、理由は〜」を一文で言える
- 次へ進む先: 差異ありなら手順3、なしなら手順4
- 止まる箇所: Flutter 由来かの判断が割れるとき。実装を始める前に、Issue で基準を明示する

### 手順3: 差異を残すなら、記録する

- 読み手が持って来るもの: 差異が要るという判断
- このページが渡すもの: 記録必須の5項目と、記録先。5項目のうち、差異を固定するテストがないと、次の移植で戻されること。差異は ADR にも書くこと(../6-propose/write-adr.md)
- 読み手がすること: 5項目を書く。利用者に見える差異は、利用者向けの docs にも書く
- できた?: 5項目がすべて埋まり、差異を戻すと落ちるテストがある
- 次へ進む先: 手順4
- 【未決】ADR への移し方が決まるまで、記録先を `known-divergences.md` と ADR のどちらで案内するか

### 手順4: Flutter のテストを FloatSoda のテストに写す

- 読み手が持って来るもの: 手順1の公式テストの位置
- このページが渡すもの: Flutter の実装分岐と公式テストから重要なケースを拾い、FloatSoda 側でも検証する規約。Flutter が明示的にテストしている退化ケースは、FloatSoda でも壊れること。写したテストの名前の付け方(CONTRIBUTING.md の「テストの命名」)
- 読み手がすること: ケースを拾い、テストを書く
- できた?: Flutter のテストのケースと、FloatSoda のテストの対応が表にできる
- 次へ進む先: 手順5

### 手順5: C# として自然な書き方に置き換える

- 読み手が持って来るもの: 移植した実装
- このページが渡すもの: 差異に数えない置き換えの種類(`event`、`init` / `required`、`record`、.NET 標準機構)と、迷ったときの切り分け(利用者から見た挙動が変わるか)
- 読み手がすること: 置き換えが挙動を変えていないか確かめる
- できた?: 置き換えの前後で、観測できる挙動が同じである
- 次へ進む先: 「PR に書くこと」

### PR に書くこと

- 読み手が持って来るもの: 移植を終えた変更
- このページが渡すもの: PR テンプレートの「Flutter reference」欄に書く4つ(版、参照した source、参照した tests、差異)
- 読み手がすること: 欄を埋めて PR を出す
- 次へ進む先: 出す前のチェック(../3-prepare/conventions.md)、レビュー(../5-submit/address-review.md)

### うまくいかないとき

- 読み手が持って来るもの: 手順で迷った状況
- このページが渡すもの: 症状ごとの行き先。Flutter とどちらを正にするか迷う → 手順2、差異の記録の書き方が分からない → 手順3
- 読み手がすること: 該当する手順に戻る
- 止まる箇所: 判断が割れるとき。Issue で相談する
