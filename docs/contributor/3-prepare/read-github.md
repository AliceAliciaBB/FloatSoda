← [コントリビューター](../Home.md)

# Issue・PR・マイルストーンの読み方

## この手順でできること

この手順を終えると、手伝える Issue を1件見つけられます。FloatSoda の今の状況は、この docs には書きません。件数や一覧を docs に書くと古くなるためです。状況は GitHub の Issue、PR、マイルストーンに書かれているので、読み方をここで説明します。手順は5つです。

GitHub のアカウントがあり、Issue と PR を開ける人が対象です。ラベルとマイルストーンの意味は知らなくてかまいません。

## 手順1: ラベルから Issue の種類を読む

FloatSoda の Issue には、3つの軸のラベルが付きます。

| 軸 | 意味 | 付き方 |
| :-- | :-- | :-- |
| 種別 | Issue の主目的 | 原則1つ |
| 領域(`area:*`) | 関わるプロジェクトや機能 | 複数付くことがある |
| 補助属性 | `breaking-change`、`flutter-divergence`、`needs-device-test`、`good-first-issue`、`help-wanted` | 必要なものだけ |

種別は、`bug` や `feature` のように、Issue の主目的を表します。ラベルの一覧と説明の正本は、[リポジトリの Labels ページ](https://github.com/sumx21t-3310/FloatSoda/labels)です。このページには写しません。

Issue の一覧でラベルを選ぶと、Issue を絞り込めます。

**できた?** 種別と領域で絞った Issue の一覧が出る。

<details><summary>できなかったら</summary>

一覧が空のときは、ラベルを1つ外して絞り込み直します。領域ラベルの名前は、Labels ページで確かめます。

</details>

## 手順2: マイルストーンから Phase の範囲を読む

FloatSoda の開発は Phase 単位で進みます。Phase は、フレームワークとして何ができる段階かを表す到達点です。NuGet のバージョン番号とは対応しません。同じ Phase の間に、複数のバージョンが公開されることがあります。

各 Phase の範囲は、[GitHub のマイルストーン](https://github.com/sumx21t-3310/FloatSoda/milestones)の Issue にあります。Phase の一覧と大まかな内容は、[README.md](../../../README.md) の「開発ステータス」にあります。Phase の完了条件や進み具合を束ねる Issue には、`tracking` のラベルが付きます。

関心のある Phase のマイルストーンを開いて、Issue を見ます。

**できた?** 「この Issue はどの Phase の範囲か」を言える。

<details><summary>できなかったら</summary>

Issue にマイルストーンが付いていないときは、その Issue はどの Phase にも割り当てられていません。Issue のコメントで、関係する Phase を尋ねます。

</details>

## 手順3: Issue と PR の議論を追う

1つの Issue を、上のコメントから順に読みます。Issue から PR へは、PR の本文にある `Closes #NN` の形の関連づけで、たどれます。PR の本文は、[PR テンプレート](../../../.github/PULL_REQUEST_TEMPLATE.md)の欄(変更の概要、関連 Issue、テスト、残した作業など)で書かれています。欄を順に読むと、何を、なぜ変えたかが分かります。

議論は古くなります。古い Issue のコメントだけを根拠に、決まったことと見なさないでください。新しいコメントと、マージされた PR の本文で確かめます。閉じただけの PR は、採用されなかった案のこともあります。

**できた?** その Issue がいま何を待っているか(設計判断、実装、確認)を言える。

<details><summary>できなかったら</summary>

待っているものが読み取れないときは、Issue に、読み取れなかった点を質問として書きます。

</details>

## 手順4: 決まったことを探す

「これはもう決まったことか」を確かめるには、次の順に探します。

1. 関係する Issue のコメント
2. 決定の記録(ADR)。ADR の置き場所と番号の付け方は決まっていない。Flutter との差異は、今ある台帳 [known-divergences.md](../../../.agents/skills/floatsoda-device-test-gen/references/known-divergences.md) で探す。一覧の入口は [決定の記録(ADR)の一覧](../2-understand/adr-index.md)にある
3. 規約。[CONTRIBUTING.md](../../../CONTRIBUTING.md) と [REVIEW.md](../../../REVIEW.md)

リリースごとの変更は、[CHANGELOG.md](../../../CHANGELOG.md) にあります。

見つからないときは、提案として出します。書き方は [提案を書く](../6-propose/write-proposal.md)にあります。

<!-- ※要確認: ADR の置き場所と番号の付け方が決まっていない。決まったら、探す順と上のリンク先を見直す -->

## 手順5: 手伝える Issue を見つける

手が足りない Issue には `help-wanted`、始めやすい Issue には `good-first-issue` が付きます。得意なこと(ドキュメント、動作検証、コード)に合う種別や領域のラベルと組み合わせて、Issue を絞り込みます。

1件選んだら、作業を始める前に、Issue のコメントで担当したい旨を伝えます。ほかの人の作業と重ならずに済みます。

<!-- ※要確認: good-first-issue と help-wanted の使い分けの定義が、リポジトリ内にない(Labels ページで確かめる)。また「手を挙げる前に Issue で一言伝える」は CONTRIBUTING.md にない慣行なので、オーナーの方針を確かめる -->

次の行き先は、貢献の種類によって分かれます。

- 貢献の種類を選び直す: [どんな貢献が助かるか](../1-get-involved/ways-to-help.md)
- 書く前に規約を読む: [コントリビューターの規約](conventions.md)

## うまくいかないとき

絞り込んでも Issue が見つからないときは、2つの行き先があります。

| 状況 | 行き先 |
| :-- | :-- |
| 手伝えることを提案したい | 提案として Issue を立てる([提案を書く](../6-propose/write-proposal.md)) |
| 質問がある | Issue に質問として書く |
