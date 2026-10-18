← [コントリビューター](../Home.md)

# コントリビューターの規約

## この一覧の使い方

規約の正本は、リポジトリ直下の2つのファイルにあります。このページは規約を写さず、場面ごとに読む節の場所だけを案内します。

| ファイル | 決めていること |
| :-- | :-- |
| [CONTRIBUTING.md](../../../CONTRIBUTING.md) | 人間とエージェントに共通する開発・コントリビューション規約 |
| [REVIEW.md](../../../REVIEW.md) | コードレビューの判断基準と、見る項目の優先度 |

どちらのファイルも、本文中の「必須」は MUST、「推奨」は SHOULD の強さで読みます。API 設計原則と Flutter parity の判断基準、エージェントの入口、リリース手順の正本は、それぞれ別のファイルです。置き場所は CONTRIBUTING.md の冒頭の表にあります。

自分の場面に合う見出しへ進んでください。

## 書き始める前に読むもの

書き始める前に、変更が Issue を要するか、ブランチ名と PR の範囲をどうするか、変更が受け入れ範囲に入るかを確かめます。

| 確かめること | 正本の節 |
| :-- | :-- |
| 着手前に Issue が要る変更か | CONTRIBUTING.md の「Issue が必須な変更」 |
| Issue のタイトルとラベル | CONTRIBUTING.md の「タイトルとラベル」 |
| ブランチ名 | CONTRIBUTING.md の「ブランチ名」 |
| PR に含めてよい変更の範囲 | CONTRIBUTING.md の「PR のスコープ」 |
| コアに受け入れる変更か | CONTRIBUTING.md の「受け入れ範囲」 |

Flutter 由来の部品を作る場合は、[Flutter から移植する](../4-write/port-from-flutter.md)も読みます。

## 書いている間に引くもの

書いている最中の疑問は、次の表の節で引きます。

| 疑問 | 正本の節 |
| :-- | :-- |
| テスト名の付け方 | CONTRIBUTING.md の「テストの命名」 |
| 何を検証するか | CONTRIBUTING.md の「テスト観点」(置き場所は [テストの分け方](test-strategy.md)) |
| namespace とディレクトリの対応 | CONTRIBUTING.md の「namespace とディレクトリ」 |
| コードの書き方 | CONTRIBUTING.md の「コーディング規約」 |
| public API の追加 | CONTRIBUTING.md の「新しい public API を追加する場合の受け入れ条件」 |
| サンプルの追加 | CONTRIBUTING.md の「サンプルを追加する場合の規約」 |
| skill の追加・変更 | CONTRIBUTING.md の「エージェント向け skill を追加・変更する場合」 |
| 文書の更新と文体 | CONTRIBUTING.md の「ドキュメントを更新する場合の注意」と「ドキュメントのトンマナ」 |

## 出す前に読むもの

変更を出す前に、CONTRIBUTING.md の「PRを出す前のチェックリスト」を上から確かめます。

- PR を出す前に、CI と同じ手順を手元で通すこと(手順は [開発環境を用意する](dev-environment.md)にあります)

PR の本文は、[PULL_REQUEST_TEMPLATE.md](../../../.github/PULL_REQUEST_TEMPLATE.md) の欄に沿って書きます。

描画に関わる変更には、範囲を決める予定の規約があります。範囲の一覧は CONTRIBUTING.md に置く方針ですが、まだ載っていません。載るまでは、変更が描画に関わるかどうかを Issue で確かめます。

<!-- ※要確認: 描画に関わる変更の範囲を CONTRIBUTING.md に載せる作業の追跡 Issue の番号が分からない。分かれば「(追跡: #NN)」を添える -->

PR を出したあとは、[レビューの指摘を受けて直す](../5-submit/address-review.md)と [CI が落ちたとき](../5-submit/ci-failures.md)を読みます。

## レビューされる側とする側が読むもの

PR を出した人も、レビューを頼まれた人も、REVIEW.md で何が見られるかを確かめます。見る順番に並べた章が、次のとおりです。

| 知りたいこと | 正本の章 |
| :-- | :-- |
| 指摘の重さの順 | REVIEW.md の「1. 重要度の順序」 |
| どんな指摘を出すか | REVIEW.md の「2. finding の基準」 |
| 判断の根拠にする資料の順 | REVIEW.md の「3. 仕様の一次情報と優先順位」 |
| FloatSoda 固有の不変条件 | REVIEW.md の「4. FloatSoda 固有の不変条件」 |
| Flutter との差異の扱い | REVIEW.md の「6. Flutter parity」 |
| 自動レビューと人のレビューの分担 | REVIEW.md の「9. レビューの層と分担」 |

指摘の受け方は [レビューの指摘を受けて直す](../5-submit/address-review.md)にあります。

## 提案するときに読むもの

新しい機能や設計の変更を提案する前に、案が受け入れ範囲に入るかを確かめます。範囲は CONTRIBUTING.md の「受け入れ範囲」、public API の受け入れ条件は同じファイルの「新しい public API を追加する場合の受け入れ条件」にあります。

提案の書き方は [提案を書く](../6-propose/write-proposal.md)にあります。

<!-- ※要確認: CONTRIBUTING.md と REVIEW.md と PR テンプレートが参照する docs/APIDesign.md と docs/WritingDocumentation.md は、いまの docs にない。API 設計原則と Flutter parity の判断基準をどこに置くかが決まっておらず、このページから案内できない -->
