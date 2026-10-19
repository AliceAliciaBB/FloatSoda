← [コントリビューター](../Home.md)

# 開発環境を用意する(コントリビューター)

> このページは執筆前の骨組みです。設計シートとト書きだけを置き、本文はまだ書いていません(追跡: #219)。

## 設計シート

- 分類: 説明型(Diátaxis: how-to)
- 読み手: だれが / FloatSoda 本体を変えたい人、移植をする人、動作検証をする人 / 何を知っている / `git` と `dotnet` の基本操作。FloatSoda の中身は知らなくてよい / 読んで何をしたい / 手元でビルドとテストを通し、Flutter の元の実装を引ける状態にしたい
- 出力言語と文体の指定: 日本語。敬体(CONTRIBUTING.md の「ドキュメントのトンマナ」の「文体」)
- テーマ: FloatSoda をクローンして、ビルドとテストを通し、Flutter の元の実装を引ける状態にする
- 依頼の解釈: 「開発環境」にエージェントの準備を含めるかは、別ページ(エージェントと一緒に作業する)に分ける
- テーマファイル: how-to
- Why(目的): 環境がそろわないと、CI と同じ結果が手元で出ず、PR を出してから落ちる。移植では Flutter の実装を見ながら書くので、引ける状態が先に要る
- What(目標・結果): `dotnet build` と CI と同じ手順のテストが手元で通る。Flutter の元の実装を、ローカルのクローンから引ける。使いたければ Rider の MCP も使える
- How(手段): 必要なものを入れる → クローンしてビルドする → CI と同じ手順でテストする → Flutter の参照クローンを置く → 道具(Rider など)を足す
- 要素:
  - 必要なものは .NET 10 / C# 14 の SDK。SteamVR は、サンプルを実行するときだけ要る(CONTRIBUTING.md の「開発環境のセットアップ」)
  - ビルドは `dotnet build`、全テストは `dotnet test`(CONTRIBUTING.md の「開発環境のセットアップ」)
  - CI は `dotnet build --configuration Release` のあと、`FloatSoda.Rendering.Test` と `FloatSoda.Test` を `--no-build` で順に実行する(`.github/workflows/ci.yml`)
  - CI は `windows-latest` で動く(`.github/workflows/ci.yml`)
  - 特定のテストだけを動かすには `dotnet test tests/FloatSoda.Test --filter "FullyQualifiedName~..."` を使う(AGENTS.md の「Build & Run Commands」)
  - サンプルの実行は `dotnet run --project samples/FloatSoda.Samples.OverlayApp`。SteamVR を先に起動する(CONTRIBUTING.md の「開発環境のセットアップ」)
  - Flutter の元の実装は、ローカル参照クローン `~/code_reading/flutter_reference` で引く。対応する Widget / Element / RenderObject は `flutter-widget-source` skill で特定できる(CONTRIBUTING.md の「テスト観点」)
  - Flutter の参照クローンは読み取り専用にし、`pull` も `checkout` もしない(リポジトリ外の運用メモ。リポジトリ内の一次情報は※要確認)
  - 参照クローンの置き場所と構築手順を示す文書がリポジトリにあるか(※要確認)
  - Rider が入っていて MCP が使えるなら、C# のシンボル解決、リネーム、namespace の移動に使う。入っていなければ `dotnet` CLI と検索で続ける(AGENTS.md の「Rider MCP」)
  - namespace の移動やリネームは、文字列置換より IDE の意味解析を使う(CONTRIBUTING.md の「namespace とディレクトリ」)
  - ソリューションは `FloatSoda.slnx`(リポジトリ直下)
- 見出し:
  - この手順でできること
  - 前提条件
  - 手順1: SDK を入れる
  - 手順2: クローンしてビルドする
  - 手順3: CI と同じ手順でテストする
  - 手順4: Flutter の元の実装を引ける状態にする
  - 手順5: Rider などの道具を足す(任意)
  - うまくいかないとき
- ※要確認:
  - `~/code_reading/flutter_reference` の作り方(クローン先、固定するバージョン)がリポジトリの文書にあるか。なければ、読み手が自分で flutter/flutter をクローンする手順として書くか、オーナーに確かめる
  - `flutter-widget-source` skill が、リポジトリの `.agents/skills/` にない(`.claude/skills/` にもない)。skill の入手方法をどう案内するか
  - SteamVR が要るのは「サンプルを実行するとき」だけか。`dotnet test` が SteamVR なしで通るか(CI は SteamVR なしで通っているので、通る見込み。手元で確かめる)
  - `dotnet --version` の下限(`global.json` の有無と内容)

## ト書き

### この手順でできること

- 読み手が持って来るもの: 「手元でビルドとテストを動かしたい」という目的
- このページが渡すもの: 手順が終わったときの状態(ビルドとテストが通る、Flutter の実装が引ける)と、手順の数
- 読み手がすること: 自分に要る手順を確かめる(Flutter の参照は移植をする人だけ)
- 次へ進む先: 「前提条件」

### 前提条件

- 読み手が持って来るもの: Windows の PC(CI が `windows-latest` のため。他の OS は※要確認)、`git`
- このページが渡すもの: 要るもの(.NET 10 SDK、SteamVR はサンプル実行時のみ)の一覧
- 読み手がすること: 足りないものを入れる
- 次へ進む先: 手順1

### 手順1: SDK を入れる

- 読み手が持って来るもの: 前提条件を満たす PC
- このページが渡すもの: .NET 10 SDK の入れ方(公式の配布元へのリンク)と、確かめるコマンド `dotnet --version`
- 読み手がすること: SDK を入れ、版を確かめる
- できた?: `dotnet --version` が 10 系を返す
- 次へ進む先: 手順2

### 手順2: クローンしてビルドする

- 読み手が持って来るもの: SDK
- このページが渡すもの: クローンのコマンドと `dotnet build`
- 読み手がすること: クローンしてビルドする
- できた?: ビルドが警告なしで終わる(※要確認: 警告の有無は実行して確かめる)
- 次へ進む先: 手順3

### 手順3: CI と同じ手順でテストする

- 読み手が持って来るもの: ビルドが通ったクローン
- このページが渡すもの: CI と同じ3つのコマンド(Release でビルド、2つのテストプロジェクトを順に実行)と、特定のテストだけを動かす書き方。PR を出す前に同じ手順を通すことが規約であること(規約の正本は ../3-prepare/conventions.md から案内)
- 読み手がすること: 3つのコマンドを順に実行する
- できた?: 両方のテストプロジェクトが成功で終わる
- 次へ進む先: 手順4(移植をする人)、そうでなければ手順5

### 手順4: Flutter の元の実装を引ける状態にする

- 読み手が持って来るもの: 移植をしたいという目的
- このページが渡すもの: 参照クローンの置き場所と、読み取り専用で扱うこと(`pull` と `checkout` をしない理由は、行番号とリンクが固定できなくなるため)。対応する実装を探す `flutter-widget-source` skill の使い方
- 読み手がすること: 参照クローンを置き、ウィジェット名から Flutter の実装を1つ引いてみる
- できた?: 指定したウィジェットの Widget / Element / RenderObject の位置が分かる
- 次へ進む先: 移植の手順(../4-write/port-from-flutter.md)
- 【未決】参照クローンの作り方をこのページに書くか、skill のページに任せるか

### 手順5: Rider などの道具を足す(任意)

- 読み手が持って来るもの: C# のリファクタリングをしたいという目的
- このページが渡すもの: Rider と MCP を使うと何に効くか(シンボル解決、リネーム、namespace の移動)と、使わなくても進められること
- 読み手がすること: 使うかどうかを決める
- 止まる箇所: ソフトウェアのインストールは、読み手が自分で決める
- 次へ進む先: 「規約を読む」(../3-prepare/conventions.md)

### うまくいかないとき

- 読み手が持って来るもの: ビルドやテストで出たエラー
- このページが渡すもの: 症状ごとの行き先。SDK の版が合わない、テストが落ちる(自分の変更か、環境かの切り分け)
- 読み手がすること: 症状に合う行き先へ進む
- 止まる箇所: 切り分けても分からないとき。Issue で報告する(../1-get-involved/report-bugs.md)
