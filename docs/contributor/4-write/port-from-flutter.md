← [コントリビューター](../Home.md)

# Flutter から移植する

## この手順でできること

Flutter の Widget や RenderObject を FloatSoda に移植し、Flutter の観測できる挙動と合った状態で PR に出せます。挙動が合わない箇所を残す場合は、理由とテストつきで記録されている状態になります。手順は5つです。

1. Flutter の元の実装を見つける
2. どこまでそろえるかを決める
3. 差異を残すなら、記録する
4. Flutter のテストを FloatSoda のテストに写す
5. C# として自然な書き方に置き換える

先に、移植したい部品が Flutter 由来かを確かめてください。Flutter に対応する概念がない部品は、このページの対象外です。

## 前提条件

次の2つがそろっていれば、手順1から始められます。

- 開発環境と Flutter の参照クローンを用意しておくこと([開発環境を用意する](../3-prepare/dev-environment.md))
- 移植の Issue を立てておくこと([コントリビューターの規約](../3-prepare/conventions.md))

Flutter との差異の判断は、Issue が必須な変更に含まれます。

## 手順1: Flutter の元の実装を見つける

移植したい部品について、対応する Widget と、関係する Element・RenderObject の定義位置を、Flutter の版とあわせて控えます。部品専用の Element や RenderObject がないときは、継承元や委譲先の定義位置を控えます。

Flutter の元の実装は、ローカルの参照クローンで引きます(CONTRIBUTING.md の「テスト観点」では `~/code_reading/flutter_reference`)。Widget は `packages/flutter/lib/src/widgets/`、RenderObject は `packages/flutter/lib/src/rendering/` にあります。Widget / Element / RenderObject の位置は、参照クローンでクラス名を検索して特定します。`flutter-widget-source` skill でも特定できますが、この skill は `.agents/skills/` にありません。

<!-- ※要確認: flutter-widget-source skill の入手方法。.agents/skills/ に無く、参照クローンの作り方もリポジトリの文書に無い。確かめ方: オーナーに入手先を確認する -->

版は、参照クローンで `git rev-parse HEAD` を実行して commit を控えます。

**できた?** Widget、Element、RenderObject の定義位置(専用の定義がなければ継承元や委譲先)と、Flutter の commit を書き出せます。

## 手順2: どこまでそろえるかを決める

Flutter 由来の Widget と RenderObject は、明示された理由がない限り、観測できる挙動に差異を作りません。そろえる対象は次のとおりです(REVIEW.md の「6. Flutter parity」)。

- プロパティの意味
- 既定値
- レイアウト
- 描画とクリップ
- ヒットテスト
- 子の扱い
- Widget の更新
- 不正・退化入力の扱い
- dirty の条件
- Element と State のライフサイクル

「実装しやすい」「こちらの方が安全」「こちらの方が自然」だけは、差異の理由になりません。Flutter を期待して書く利用者と LLM が、その差異で困るためです。

仕様が食い違うときは、次の順に従います(REVIEW.md の「3. 仕様の一次情報と優先順位」)。

1. FloatSoda で明示された差異・設計判断
2. Flutter の仕様・実装・公式テスト
3. 既存の FloatSoda 実装(根拠になりません)

Semantics のように、すでに移植しないと決めたものは、parity を理由に破りません。

**できた?** 「差異なし」か「差異あり、理由は〜」を一文で言えます。差異なしなら手順4へ、差異ありなら手順3へ進みます。

Flutter 由来かどうかの判断が割れるときは、実装を始める前に Issue で基準を明示してください。

## 手順3: 差異を残すなら、記録する

差異を残すときは、次の5項目を記録します(REVIEW.md の「6. Flutter parity」)。

| 項目 | 書くこと |
| :-- | :-- |
| Flutter の挙動 | Flutter が何をするか。対応するファイルつき |
| FloatSoda の挙動 | FloatSoda が何をするか。`src/` のファイルと行つき |
| 差異が必要な理由 | 手順2の「理由にならないもの」以外の理由 |
| 差異を固定するテスト | 差異を戻すと失敗するテストのファイルとメソッド名 |
| 利用者向けのドキュメント | 利用者から観測できる場合の、docs やサンプルの `## Flutterとの違い` 節 |

差異を固定するテストがないと、次の移植で差異が戻されます。テストは省かないでください。

記録先は、差異の台帳 [known-divergences.md](../../../.agents/skills/floatsoda-device-test-gen/references/known-divergences.md) です。エントリの形と、`deliberate` / `not ported` / `port mistake` / `unlabelled` のラベルの意味は、台帳の冒頭にあります。台帳を ADR にまとめる移行と、ADR の置き場所と番号の付け方は、決まっていません。決まるまでは、台帳へ追記してください。設計判断として ADR も書く場合は [決定を ADR に書く](../6-propose/write-adr.md) を読んでください。

<!-- ※要確認: 差異の記録先を台帳から ADR に移したあとの案内の書き方。確かめ方: 移行の決定後にオーナーへ確認する -->

台帳の `Observation` 欄は、差異が HMD なしで観測できるか(`HEADLESS`)、SteamVR が要るか(`VR`)を示します。HMD が要る範囲の分け方は [テストの分け方](../3-prepare/test-strategy.md) にあります。

**できた?** 5項目がすべて埋まり、差異を戻すと落ちるテストがあります。

## 手順4: Flutter のテストを FloatSoda のテストに写す

Flutter 由来の Widget と RenderObject のテストは、FloatSoda 独自にケースを想像するだけで済ませません(CONTRIBUTING.md の「テスト観点」)。Flutter の実装の分岐、公式テスト、API contract を確かめ、対応する重要なケースを FloatSoda 側でも検証します。Flutter が明示的にテストしている退化ケースは、FloatSoda でも壊れます。

Flutter の公式テストは、参照クローンの `packages/flutter/test/widgets/` と `packages/flutter/test/rendering/` にあります。FloatSoda 側のテストは、`tests/FloatSoda.Test` の `Widgets/` と `RenderObjects/` に置きます。

<!-- ※要確認: Flutter のテストを1対1で写すのか、観点を写すのかを示す一次情報がリポジトリに無い。確かめ方: オーナーに方針を確認する -->

写したテストの名前は `対象メンバー名_条件_期待結果` の形にし、条件と期待結果を日本語で書きます(CONTRIBUTING.md の「テストの命名」)。バグ修正の場合は、修正を戻すと落ちるテストを足します。

**できた?** Flutter のテストのケースと、FloatSoda のテストの対応を表にできます。

## 手順5: C# として自然な書き方に置き換える

次の置き換えは、差異に数えません(REVIEW.md の「6. Flutter parity」)。

- `event`
- `init` / `required`
- `record` / `record struct`
- .NET 標準機構の利用

迷ったら、利用者から見た挙動が変わるかで切り分けます。変わらなければ置き換えです。変わるなら、手順2に戻って差異として扱います。

**できた?** 置き換えの前後で、観測できる挙動が同じです。

## PR に書くこと

PR の本文は、[PR テンプレート](../../../.github/PULL_REQUEST_TEMPLATE.md)の「Flutter reference」欄に次の4つを書きます。PR から、Flutter の版、参照した実装、参照したテストを追跡できるようにするためです。

- Flutter version / commit
- Referenced source
- Referenced tests
- Behavioral differences from Flutter(差異がなければ「なし(parity)」)

出す前のチェックは [コントリビューターの規約](../3-prepare/conventions.md) にあります。出したあとの指摘への対応は [レビューの指摘を受けて直す](../5-submit/address-review.md) に進んでください。

## うまくいかないとき

<details><summary>Flutter とどちらを正にするか迷う</summary>

手順2に戻り、優先順位の表で確かめます。FloatSoda で明示された差異がなければ、Flutter が正です。既存の FloatSoda 実装は根拠になりません。

</details>

<details><summary>差異の記録の書き方が分からない</summary>

手順3の5項目と、台帳の既存エントリを見比べてください。

</details>

判断が割れるときは、Issue で相談してください。
