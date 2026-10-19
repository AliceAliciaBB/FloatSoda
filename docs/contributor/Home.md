# コントリビューター

## FloatSoda に関わる人へ

FloatSoda を一緒に育ててもらうための案内です。コントリビューター向けの docs は、関わり始めてから提案するまでの順に、6つのグループに分かれています。コードを書かなくても、ドキュメント、動作検証、提案、エージェント向けの skill という入口があります。どのグループから読んでもかまいません。迷ったら「1. はじめに」から読んでください。

## 1. はじめに

関わってよいのか、何ができるのかを知るグループです。FloatSoda が大事にしていることを読むと、方針に共感できるかを確かめられます。どんな貢献が助かるかを読むと、自分の得意なことの入口が見つかります。自分のツールで不具合を踏んだ人は、切り分けて報告するページへ進みます。

- [FloatSoda が大事にしていること](1-get-involved/values.md)
- [どんな貢献が助かるか](1-get-involved/ways-to-help.md)
- [不具合を切り分けて報告する](1-get-involved/report-bugs.md)

## 2. 理解する

全体の仕組みと、そう作った理由を知るグループです。手を動かして学ぶ入口と、読んで学ぶ入口があります。

- [描画が届くまで(4段、手を動かして学ぶ)](2-understand/tutorial-render-path.md)
- [1フレームが SteamVR に届くまで(読んで学ぶ)](2-understand/frame-to-steamvr.md)
- [決定の記録(ADR)の一覧](2-understand/adr-index.md)

層ごとの解説は [フレームワーク層](2-understand/framework-layer.md)(入力は [入力が届くまでを追う](2-understand/framework-input.md))、[描画層](2-understand/rendering-layer.md)、[エンジン層](2-understand/engine-layer.md)、[OVR 層](2-understand/ovr-layer.md)、[UI 層](2-understand/ui-layer.md) にあります。層ごとの実験は [フレームワーク層](2-understand/experiment-framework.md)、[描画層](2-understand/experiment-rendering.md)、[OVR 層](2-understand/experiment-ovr.md) にあります。

## 3. 準備する

手元でビルドとテストを動かし、規約の場所を知るグループです。足りない準備のページだけ開けば足ります。

- [開発環境を用意する](3-prepare/dev-environment.md)
- [テストの分け方](3-prepare/test-strategy.md)
- [エージェントと一緒に作業する](3-prepare/work-with-agents.md)
- [Issue・PR・マイルストーンの読み方](3-prepare/read-github.md)
- [コントリビューターの規約](3-prepare/conventions.md)

## 4. 書く

やりたい貢献が決まっている人が、規約に沿って形にするグループです。規約の正本は [CONTRIBUTING.md](../../CONTRIBUTING.md) にあります。

- [Flutter から移植する](4-write/port-from-flutter.md)
- [skill を足す・直す](4-write/skills.md)

## 5. 出す

変更ができて、PR を出してレビューを通すグループです。

- [レビューの指摘を受けて直す](5-submit/address-review.md)
- [CI が落ちたとき](5-submit/ci-failures.md)

設計の変更を提案したい人は、次の「6. 提案する」へ進みます。

## 6. 提案する

新しい機能や設計の変更を提案し、決まったことを記録するグループです。提案が通る前なら「提案を書く」、通った後なら「ADR を書く」を開きます。

- [提案を書く](6-propose/write-proposal.md)
- [決定を ADR に書く](6-propose/write-adr.md)
