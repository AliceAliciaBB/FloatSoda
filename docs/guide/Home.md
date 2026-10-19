# ガイド

ガイドは、FloatSoda を知ってから、作ったツールを配って保守するまでを案内するタブです。段階ごとに7つのグループがあるので、自分の段階に近いグループのページを選んでください。

## ガイドの読み方

グループは時間の順に並んでいます。「1. 探す」から「7. メンテナンスする」までのうち、自分の段階に近いグループの節へ進んでください。

各グループの中は、手順を渡すページ(How-to)を先に、考え方を説明するページ(解説)を後に並べています。部品の一覧や制約の一覧のような調べ物は、ガイドの外のリファレンスにあります。ガイドからは、必要な場面でリンクします。最初の1本を手を動かして作るチュートリアルも別のタブにあります。

## 1. 探す

FloatSoda をまだ知らない、または名前だけ知っている段階です。自分の目的に合うかを確かめます。

- [FloatSoda とは](1-discover/what-is-floatsoda.md): FloatSoda が何で、どんな人が何を作れるか
- [オーバーレイとは](1-discover/what-is-an-overlay.md): SteamVR のオーバーレイが何で、ゲームの画面とどう違うか

作例を見たいときは、リファレンスの[作例のカタログ](../reference/sample-catalog.md)を開いてください。確かめたら、次は「2. 採用する」です。

## 2. 採用する

使うかどうかを決める段階です。

- [ほかの作り方と比べる](2-adopt/compare-approaches.md): uGUI で作る場合や OpenVR を直接呼ぶ場合と比べて、何が楽になり、何を引き受けるか

作りたいものが今できるかは、リファレンスの[できないこと・まだ使えないもの](../reference/limitations.md)で確かめられます。使うと決めたら「3. 準備する」へ進みます。

## 3. 準備する

使うと決めて、書き始める前に整える段階です。自分の経験に合うページを選んでください。

| 自分の状況 | 開くページ |
| :-- | :-- |
| PC に .NET を入れて、動かせる状態にしたい | [開発環境を用意する](3-prepare/set-up-environment.md) |
| コードは AI に書かせたい | [AI に FloatSoda を書かせる](3-prepare/write-with-llm.md) |
| VRChat と組み合わせたい | [VRChat と組み合わせるときに確かめること](3-prepare/vrchat-checklist.md) |
| Unity の uGUI で UI を作ってきた | [uGUI から来た人へ](3-prepare/from-ugui.md) |
| C# がほぼ初めてで、AI が書いたコードを読みたい | [C# がほぼ初めての人へ](3-prepare/csharp-primer.md) |

Flutter から来た人は、リファレンスの[Flutter の読み替え表](../reference/flutter-mapping.md)が近道です。用語は[用語集](../reference/glossary.md)、対応する環境は[動作環境](../reference/requirements.md)にあります。準備ができたら、チュートリアル[最初のオーバーレイを作る](../tutorials/first-overlay.md)で最初の1本を作り、次に「4. 作る」へ進みます。

## 4. 作る

最初の1本を終えて、作りたいものがある段階です。作る順(見た目、動き、操作、確かめ、起動)に並べています。

**画面を組み立てる**

- [見た目を整える](4-build/screen/style-appearance.md)
- [アニメーションを付ける](4-build/screen/add-animation.md)
- [ボタンやトグルを組み立てる](4-build/screen/build-buttons-toggles.md)
- [自分の部品を作る](4-build/screen/make-your-own-widget.md)
- [表情切り替えパネルを作る](4-build/screen/expression-switcher.md)

**入力を受け取り、確かめる**

- [コントローラーの入力を受け取る](4-build/input/controller-input.md)
- [HMD を被らずに確かめる](4-build/input/test-without-hmd.md)

**仕組みを知り、足す**

- [設定やログ、裏で動く処理を足す](4-build/mechanics/use-generic-host.md)
- [状態と作り直し](4-build/mechanics/state-and-rebuild.md)
- [オーバーレイの種類と選び方](4-build/mechanics/overlay-types.md)
- [FloatSoda の仕組み(使う人向け)](4-build/mechanics/how-floatsoda-works.md)
- [起動と Generic Host](4-build/mechanics/startup-and-host.md)

使いたい部品は、リファレンスの[部品を目的から探す](../reference/find-widgets.md)で探せます。詰まったら「5. 困ったとき」、できたら「6. 配る」へ進みます。

## 5. 困ったとき

エラーが出る、反応しない、思った大きさにならない、重い、といった症状が出た段階です。症状に合うページを選んでください。

- [エラーを読んで直す](5-troubleshoot/read-errors.md)
- [反応しないときに切り分ける](5-troubleshoot/not-responding.md)
- [負荷を抑える](5-troubleshoot/reduce-load.md)
- [レイアウトの考え方](5-troubleshoot/layout-model.md): 思った大きさや位置にならないとき

Flutter の書き方で動かないときは、[Flutter の読み替え表](../reference/flutter-mapping.md)を開いてください。ここで解けなければ、「docs で見つからないとき」へ進みます。

## 6. 配る

動くツールができて、人に渡す、売る、自動で起動する段階です。

- [exe にして配る](6-distribute/publish-exe.md)
- [Booth で売る準備](6-distribute/booth-release.md)
- [SteamVR と一緒に起動する](6-distribute/launch-with-steamvr.md)

配ったあとは「7. メンテナンスする」へ進みます。

## 7. メンテナンスする

配った相手の環境で動かない、または久しぶりに自分のツールを触る段階です。

- [利用者の環境で動かないとき](7-maintain/user-environment-issues.md)
- [画面と状態の分け方](7-maintain/separate-ui-and-state.md): 直しやすい形に整えるとき

久しぶりに触るときは、チュートリアルで手順を、リファレンスで API と制約を確かめてください。FloatSoda の版を上げるときは、[CHANGELOG.md](../../CHANGELOG.md) も読んでください。直した版を届ける手順は、[exe にして配る](6-distribute/publish-exe.md)の後半にあります。

## docs で見つからないとき

docs を読んでも解けない問題は、次の2か所に持ち込めます。

- GitHub の Issue: バグの報告と機能の要望を受け付けています。使い方や設計の相談も Issue に書いてください。GitHub の Discussions は有効にしていません。
- AI への質問: 公開サイトの `llms.txt`(索引)か `llms-full.txt`(全文)を AI に渡して聞けます。渡し方は[AI に FloatSoda を書かせる](3-prepare/write-with-llm.md)にあります。

Issue には、FloatSoda のバージョン、`dotnet --version` の出力、OS、再現するコードか手順を書いてください。表示の問題では、GPU、HMD、SteamVR のバージョンも添えると原因をたどれます。Issue の書式は、バグ報告と機能要望の2つのテンプレートで選べます。Issue を出す前に確かめたい決まりは、[CONTRIBUTING.md](../../CONTRIBUTING.md)にあります。

<!-- ※要確認: Issue 以外の窓口(VRChat 関連のコミュニティなど)を載せてよいか。リポジトリに記述がなく、載せていない。確かめ方: オーナーに確認する -->
