← [コントリビューター](../Home.md)

# Issue・PR・マイルストーンの読み方

> このページは執筆前の骨組みです。設計シートとト書きだけを置き、本文はまだ書いていません(追跡: #219)。

## 設計シート

- 分類: 説明型(Diátaxis: how-to)
- 読み手: だれが / FloatSoda に関わりたい人。とくに GitHub の Issue や PR の読み方に慣れていない人 / 何を知っている / GitHub のアカウントがある。ラベルとマイルストーンの意味は知らなくてよい / 読んで何をしたい / 今の状況と、決まったことと、手が足りないところを、GitHub から自分で読み取りたい
- 出力言語と文体の指定: 日本語。敬体(CONTRIBUTING.md の「ドキュメントのトンマナ」の「文体」)
- テーマ: FloatSoda の今の状況は docs ではなく GitHub の Issue・PR・マイルストーンに書かれていて、ラベルと議論の追い方で読み取れる
- 依頼の解釈: 「今の状況そのもの」を書くのではなく「状況の読み方」を書く
- テーマファイル: how-to
- Why(目的): 状況を docs に書くと古くなる。GitHub を読める人が増えれば、docs を更新しなくても、手伝える箇所や決まったことを自分で見つけられる
- What(目標・結果): ラベルの3つの軸、Phase とバージョンの違い、議論の追い方が分かる。手が足りない Issue を1件見つけられる
- How(手段): ラベルの読み方 → マイルストーンと Phase の読み方 → Issue の議論の追い方 → 決まったことの探し方 → 手を挙げる前の確認
- 要素:
  - ラベルは種別、領域(`area:*`)、補助属性の3軸。一覧と説明は GitHub の Labels ページが一次情報(CONTRIBUTING.md の「タイトルとラベル」)
  - 種別ラベルは `feature`、`bug`、`design`、`refactor`、`performance`、`documentation`、`tracking`、`release`、`maintenance`(CONTRIBUTING.md の「タイトルとラベル」)
  - 補助属性は `breaking-change`、`flutter-divergence`、`needs-device-test`、`good-first-issue`、`help-wanted`(CONTRIBUTING.md の「タイトルとラベル」)
  - 手伝いやすい Issue は `good-first-issue` と `help-wanted` で探せる(CONTRIBUTING.md の「タイトルとラベル」。ラベルの意味は Labels ページで確かめる)
  - Phase は機能上の到達点で、NuGet のバージョン番号とは対応しない。各 Phase の詳細な範囲は GitHub のマイルストーンにある(README.md の「ロードマップ」の節)
  - マイルストーンの Issue 雛形は `.github/ISSUE_TEMPLATE/milestone.md`(ラベル `tracking`)
  - Issue が必須な変更の種類は CONTRIBUTING.md の「Issue が必須な変更」にある
  - PR の本文は `.github/PULL_REQUEST_TEMPLATE.md` の欄(Summary、Related Issue、Flutter reference、Breaking changes、Tests、Documentation、Scope / follow-up)で書かれている
  - 関連 Issue は PR 本文で `Closes #123` の形で結ぶ(`.github/PULL_REQUEST_TEMPLATE.md`)
  - リリースごとの変更は CHANGELOG.md にある
  - 今の状況の件数や一覧はページに書かない(docs 再編の決定、2026-10-16〜17)
- 見出し:
  - この手順でできること
  - 手順1: ラベルから Issue の種類を読む
  - 手順2: マイルストーンから Phase の範囲を読む
  - 手順3: Issue と PR の議論を追う
  - 手順4: 決まったことを探す
  - 手順5: 手伝える Issue を見つける
  - うまくいかないとき
- ※要確認:
  - `good-first-issue` と `help-wanted` の使い分けの定義が Labels ページにあるか(GitHub のページは未確認。リポジトリ内では見つからない)
  - 決まったことの置き場所(ADR、Issue のコメント、docs)の優先順位。ADR を置く場所は未決(write-adr.md 参照)
  - GitHub のプロジェクトボードを使っているか
  - 現在 `area:*` のラベルに何があるか(ページには書かず、Labels ページへ送る)

## ト書き

### この手順でできること

- 読み手が持って来るもの: 「今 FloatSoda がどうなっているか知りたい」という目的
- このページが渡すもの: 手順が終わったときの状態(手伝える Issue を1件見つけた)と、状況を docs に書かない理由(古くなるため)
- 読み手がすること: 自分の目的がこのページの対象かを確かめる
- 次へ進む先: 手順1

### 手順1: ラベルから Issue の種類を読む

- 読み手が持って来るもの: Issue の一覧
- このページが渡すもの: 3つの軸(種別、領域、補助属性)の見方。ラベルの一覧は Labels ページ(GitHub)へリンクし、ここに写さない
- 読み手がすること: ラベルで Issue を絞り込む
- できた?: 種別と領域で絞った Issue の一覧が出る
- 次へ進む先: 手順2

### 手順2: マイルストーンから Phase の範囲を読む

- 読み手が持って来るもの: Phase という言葉
- このページが渡すもの: Phase とバージョン番号は別物であること。各 Phase の範囲はマイルストーンの Issue にあること
- 読み手がすること: マイルストーンを開き、関心のある Phase の Issue を見る
- できた?: 「この Issue はどの Phase の範囲か」を言える
- 次へ進む先: 手順3

### 手順3: Issue と PR の議論を追う

- 読み手が持って来るもの: 1つの Issue
- このページが渡すもの: Issue から PR へのたどり方(`Closes #NN`)、PR 本文の欄の意味、議論の古さに注意すること(古い Issue だけを根拠に決めない)
- 読み手がすること: Issue の議論を上から読み、関連する PR を開く
- できた?: その Issue がいま何を待っているか(設計判断、実装、確認)を言える
- 次へ進む先: 手順4

### 手順4: 決まったことを探す

- 読み手が持って来るもの: 「これはもう決まったことか」という疑問
- このページが渡すもの: 探す場所の順。Issue のコメント、ADR(../6-propose/write-adr.md)、規約(CONTRIBUTING.md)
- 読み手がすること: 順に探す
- 止まる箇所: 見つからないとき。提案として出す(../6-propose/write-proposal.md)
- 【未決】ADR の置き場所が決まるまで、「決まったことの探し方」の順をどう書くか

### 手順5: 手伝える Issue を見つける

- 読み手が持って来るもの: 手伝いたいという目的と、自分の得意なこと(ドキュメント、動作検証、コード)
- このページが渡すもの: 補助属性のラベルでの探し方と、手を挙げる前に Issue で一言伝えること(※要確認: 規約にはない慣行。オーナーの方針を確かめる)
- 読み手がすること: Issue を1件選ぶ
- 次へ進む先: 貢献の種類(../1-get-involved/ways-to-help.md)、規約(../3-prepare/conventions.md)

### うまくいかないとき

- 読み手が持って来るもの: 探しても見つからない状況
- このページが渡すもの: 見つからないときの行き先(Issue を立てる、または質問を書く)
- 読み手がすること: 行き先を選ぶ
- 止まる箇所: なし
