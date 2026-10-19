← [コントリビューター](../Home.md)

# テストの分け方(HMD が要る範囲)

## 範囲で分けると、何が決まるか

FloatSoda のテストは、どこまで結合して動かすかで、単体・結合・描画・実機の4つの範囲に分かれます。範囲によって、動かす手段、速さ、HMD が要るかが決まります。

範囲を決めずにテストを書くと、HMD が要らない確認を実機に回して遅くしたり、実機でしか壊れないものを xunit で通ったことにしたりします。このページを読むと、確かめたいことを置く範囲を自分で選べます。置き場所の決め方は、最後の節にあります。

## HMD なしで確かめられる3つの範囲

単体、結合、描画の3つは、`dotnet test` で動き、HMD も SteamVR も要りません。

| 範囲 | 対象 | 受け持つテストプロジェクト |
| :-- | :-- | :-- |
| 単体 | ジオメトリ型、RenderObject 1個、Widget 1個 | `tests/FloatSoda.Test`。Layer ツリーは `tests/FloatSoda.Rendering.Test` |
| 結合 | `WidgetBinding` から `RenderPipeline` までを通したフロー。差分更新、入力の配送 | `tests/FloatSoda.Test` |
| 描画 | ビットマップに描画した画素 | `tests/FloatSoda.Test`(`src/FloatSoda.Testing/` のビットマップ描画を使う) |

CI が動かすテストプロジェクトは、`FloatSoda.Rendering.Test` と `FloatSoda.Test` の2つです。CI は SteamVR を使いません。

テスト名は `MemberName_条件_期待結果` の形で付けます。命名の規約は CONTRIBUTING.md の「テストの命名」にあります。何を検証するかの観点は、同じファイルの「テスト観点」が正本です。このページには写しません。

<!-- ※要確認: 範囲ごとの対応は、前の docs の TestStrategy.md の定義を、tests/ の配置(Core/ に WidgetBinding の結合テスト、RenderObjects/ などが Testing のビットマップ描画を使う)と突き合わせて書いた。個々のテストクラスは範囲の割り当てを確かめていない。確かめるには tests/FloatSoda.Test を開く -->

## HMD が要る範囲

実機の範囲は、SteamVR が動いているときにしか観測できないものに使います。たとえば、オーバーレイのハンドル、コントローラーの座標、描画スレッドの GL コンテキストです。

実機の確認は、サンプルの `checks/items.jsonl` に書いた項目を、HMD を着けて1つずつ判定する形で行います。項目の生成は [`floatsoda-device-test-gen`](../../../.agents/skills/floatsoda-device-test-gen/SKILL.md)、実行と yes/no の記録は [`floatsoda-device-test-run`](../../../.agents/skills/floatsoda-device-test-run/SKILL.md) が担います。

HMD を持っていない人は、実機の確認を、持っている人に頼る形になります。自分の変更が実機を要するのに HMD がないときは、PR に「実機で未確認」と書いて、確認を頼みます。出す前に確かめることは、[コントリビューターの規約](conventions.md)から案内しています。

動作検証の結果の報告は、[不具合を切り分けて報告する](../1-get-involved/report-bugs.md)にあります。

<!-- ※要確認: 「実機の範囲」に挙げた3例(オーバーレイのハンドル、コントローラーの座標、描画スレッドの GL コンテキスト)が、SteamVR の起動だけで観測できるのか、HMD の装着まで要るのかを、項目ごとに確かめていない。確かめるには、サンプルの checks/items.jsonl と floatsoda-device-test-run を開く -->

CONTRIBUTING.md と skill には、実機のシナリオを置く `tests/FloatSoda.DeviceTest` という名前が出てきます。このディレクトリは、いまのリポジトリにはありません。

<!-- ※要確認: tests/FloatSoda.DeviceTest(ハーネス)がまだないことの追跡 Issue の番号が分からない。分かれば「(追跡: #NN)」を添える -->

## 期待値の決め方

範囲を選んだら、期待値をどう定義するかを選びます。4つあります。

| 決め方 | 期待値の定義 | 向いている対象 |
| :-- | :-- | :-- |
| 例示 | 入力に対する正確な出力を書く | サイズ計算、イベントの順序、画素の色 |
| 不変条件 | 常に成り立つ性質を書く | ツリーの親子関係と深さ、dirty list の整合 |
| ゴールデン | 保存した出力と比べる | 描画結果の意図しない変化 |
| 差分 | Flutter の挙動を正解にして比べる | Flutter 由来の Widget / RenderObject |

差分が使えるのは、Flutter から観測できる挙動に限ります。性能や並行性は、Flutter が正解になりません。

Flutter 由来の部品では、Flutter の実装の分岐と公式テストを確かめて、検証するケースを決めます。この規約は、CONTRIBUTING.md の「テスト観点」にあります。

## どこにテストを置くか決める

置き場所は、2つの基準で1つに決めます。

1. 同じ不具合を、より小さな範囲で観測できるなら、大きな範囲は使わない。実機は、実機でしか観測できないものにだけ使う
2. 選んだ範囲で期待値を定義できないなら、期待値の決め方を選び直す。それでも定義できない範囲は使わない。たとえば、性能は Flutter が正解にならないので、差分以外の決め方を選ぶ

画素の色なら、描画の範囲で例示を使います。ツリーの整合なら、単体か結合の範囲で不変条件を使います。オーバーレイのハンドルは、実機の範囲です。

docs と API の分かりやすさは、このページの範囲には入れません。LLM に docs だけを渡して実装させるジュニアコーダーテストで確かめます。CI には入れません。手順は [`floatsoda-junior-coder-test`](../../../.agents/skills/floatsoda-junior-coder-test/SKILL.md) にあります。

## 次に読むページ

目的に合うほうへ進みます。

- テストの書き方の規約を引く: [コントリビューターの規約](conventions.md)
- Flutter のテストを写す: [Flutter から移植する](../4-write/port-from-flutter.md)
