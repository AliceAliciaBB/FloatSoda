← [コントリビューター](../Home.md)

# skill を足す・直す

> このページは執筆前の骨組みです。設計シートとト書きだけを置き、本文はまだ書いていません(追跡: #219)。

## 設計シート

- 分類: 説明型(Diátaxis: how-to)
- 読み手: だれが / FloatSoda のリポジトリに、エージェント向けの skill を足す人、直す人 / 何を知っている / skill がエージェントに渡す手順書だということ。FloatSoda の配置は知らなくてよい / 読んで何をしたい / 規約に沿った場所と名前で skill を足し、Claude Code 用の派生スタブも整える
- 出力言語と文体の指定: 日本語。敬体(CONTRIBUTING.md の「ドキュメントのトンマナ」の「文体」)
- テーマ: skill は `.agents/skills/` に書き、Claude Code 用のスタブを `.claude/skills/` に足す
- 依頼の解釈: なし
- テーマファイル: how-to
- Why(目的): skill の正本が分かれると、片方だけ更新されてずれる。置き場所を1か所に決めて、足すたび・直すたびに迷わないようにする
- What(目標・結果): `.agents/skills/<name>/SKILL.md` ができている。`.claude/skills/<name>/SKILL.md` に、正本へのポインタだけのスタブがある
- How(手段): 名前を決める → 正本を書く → スタブを置く → 日本語と英語の使い分けを確かめる → PR を出す
- 要素:
  - skill の唯一の正本は `.agents/skills/`。追加・変更は必ずこちらから行う(CONTRIBUTING.md の「エージェント向け skill を追加・変更する場合」)
  - `.claude/skills/` は Claude Code 用の fallback で、frontmatter と正本へのポインタだけを持つ派生物。直接編集しない(CONTRIBUTING.md の「エージェント向け skill を追加・変更する場合」)
  - 特定のエージェント向けに、同じ skill の独立したコピーを正本として持たない(CONTRIBUTING.md の「エージェント向け skill を追加・変更する場合」)
  - 追加の手順は2つ。`.agents/skills/<name>/SKILL.md` に本体を書き、参照ファイルは `references/` に置く。`.claude/skills/<name>/SKILL.md` に、同じ frontmatter と正本へのポインタだけのスタブを置く(CONTRIBUTING.md の「エージェント向け skill を追加・変更する場合」)
  - 名前は `<project>-<subject>-<verb>` の順。例は `floatsoda-device-test-gen` と `floatsoda-device-test-run`(CONTRIBUTING.md の「エージェント向け skill を追加・変更する場合」)
  - skill のディレクトリはネストできない。Claude Code は `.claude/skills/*/SKILL.md` の1階層だけを探索するため、分類は名前の接頭辞で持つ(CONTRIBUTING.md の「エージェント向け skill を追加・変更する場合」)
  - `AGENTS.md` は英語、それ以外のエージェント向けファイル(skill の本文と `references/`)は日本語。識別子、型名、パス、コマンド、コードは英語のまま(CONTRIBUTING.md の「エージェント向けファイルの言語」)
  - SKILL.md の frontmatter の `description` は、エージェントが発火を判定する材料。日本語で書き、トリガーのキーフレーズは日本語と英語の両方を含める(CONTRIBUTING.md の「エージェント向けファイルの言語」)
  - 既存の skill(例: `.agents/skills/floatsoda-release/`)と、そのスタブ(`.claude/skills/floatsoda-release/SKILL.md`)が雛形になる(CONTRIBUTING.md の「エージェント向け skill を追加・変更する場合」)
  - `.agents/skills/hello/` は `.claude/skills/` にスタブがない(`ls` で確認)。例外か抜けかは※要確認
- 見出し:
  - この手順でできること
  - 前提条件
  - 手順1: 名前を決める
  - 手順2: 正本を書く
  - 手順3: Claude Code 用のスタブを置く
  - 手順4: 言語を確かめる
  - 手順5: PR を出す
  - 既存の skill を直す場合
  - うまくいかないとき
- ※要確認:
  - `.agents/skills/hello` に対応する `.claude/skills/hello` がない。意図した例外か、抜けか
  - スタブを作る手順を自動化するスクリプトがあるか(リポジトリ内では見つからなかった)
  - skill の本文の書き方(frontmatter 以外の構成)の規約があるか。規約にないものは、このページに書かない
  - skill が正しく発火するかを確かめる手順(動作確認)の一次情報

## ト書き

### この手順でできること

- 読み手が持って来るもの: skill にしたい作業
- このページが渡すもの: 手順が終わったときの状態(正本とスタブがそろっている)と、正本が1か所である理由
- 読み手がすること: 自分の作業が skill に向くかを確かめる
- 次へ進む先: 「前提条件」

### 前提条件

- 読み手が持って来るもの: リポジトリのクローン
- このページが渡すもの: 既存の skill を雛形にできること、エージェントと一緒に作業する場合の注意(../3-prepare/work-with-agents.md)
- 読み手がすること: 雛形にする skill を1つ選ぶ
- 次へ進む先: 手順1

### 手順1: 名前を決める

- 読み手が持って来るもの: skill の用途
- このページが渡すもの: `<project>-<subject>-<verb>` の順と、広い語を先に置く理由(対になる skill が一覧で隣り合う)
- 読み手がすること: 名前を決める
- できた?: 名前が `floatsoda-` で始まり、最後が動詞である
- 次へ進む先: 手順2

### 手順2: 正本を書く

- 読み手が持って来るもの: skill の名前
- このページが渡すもの: `.agents/skills/<name>/SKILL.md` の置き場所と、`references/` の使い方。frontmatter の `description` にトリガーのキーフレーズを日英で書くこと
- 読み手がすること: 正本を書く
- できた?: `.agents/skills/<name>/SKILL.md` があり、frontmatter が入っている
- 次へ進む先: 手順3

### 手順3: Claude Code 用のスタブを置く

- 読み手が持って来るもの: 正本
- このページが渡すもの: `.claude/skills/<name>/SKILL.md` に置く内容(同じ frontmatter と、正本へのポインタだけ)。雛形は既存のスタブ
- 読み手がすること: スタブを置く
- できた?: スタブに、frontmatter とポインタ以外の手順が書かれていない
- 止まる箇所: スタブの本文に手順を足したくなったとき。手順は正本に書く
- 次へ進む先: 手順4

### 手順4: 言語を確かめる

- 読み手が持って来るもの: 書き終えた skill
- このページが渡すもの: 言語の使い分け(AGENTS.md は英語、それ以外は日本語、識別子とコマンドは英語のまま)
- 読み手がすること: 本文の言語を確かめる
- できた?: 本文が日本語で、識別子とコマンドが原文のままである
- 次へ進む先: 手順5

### 手順5: PR を出す

- 読み手が持って来るもの: 正本とスタブ
- このページが渡すもの: PR の範囲(skill の追加だけを入れる)と、ブランチ名の規約へのリンク(../3-prepare/conventions.md)
- 読み手がすること: PR を出す
- 次へ進む先: レビューの対応(../5-submit/address-review.md)

### 既存の skill を直す場合

- 読み手が持って来るもの: 直したい skill
- このページが渡すもの: 直す場所が `.agents/skills/` であること。frontmatter を変えたときは、スタブの frontmatter も同じに直すこと
- 読み手がすること: 正本を直し、必要ならスタブを合わせる
- できた?: 正本とスタブの frontmatter が一致している

### うまくいかないとき

- 読み手が持って来るもの: skill が発火しない、または片方だけ更新した状況
- このページが渡すもの: 症状ごとの行き先。発火しない → description のキーフレーズ。ずれた → 正本に合わせてスタブを直す
- 読み手がすること: 該当する手順に戻る
- 止まる箇所: なし
