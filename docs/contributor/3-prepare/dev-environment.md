← [コントリビューター](../Home.md)

# 開発環境を用意する(コントリビューター)

## この手順でできること

この手順を終えると、手元で `dotnet build` と、CI と同じ手順のテストが通ります。移植をする人は、Flutter の元の実装をローカルのクローンから引けるようになります。手順は5つで、手順4は移植をする人だけ、手順5は任意です。

環境がそろっていないと、CI と同じ結果が手元で出ず、PR を出してから落ちます。移植では Flutter の実装を見ながら書くので、引ける状態を先に作ります。

## 前提条件

- Windows の PC を用意すること(CI は `windows-latest` で動く)
- `git` を使えること

この手順で実行するビルドとテストに、SteamVR は要りません。SteamVR が要るのは、サンプルの実行と実機の確認です。実機の確認は [テストの分け方](test-strategy.md)で区別しています。

<!-- ※要確認: Windows 以外の OS でビルドとテストが通るかは確かめていない。確かめるには、対象の OS で手順2と手順3を実行する -->

## 手順1: SDK を入れる

1. [.NET の公式ダウンロードページ](https://dotnet.microsoft.com/download)から、.NET 10 SDK を入れる
2. 次のコマンドで版を確かめる

```bash
dotnet --version
```

**できた?** `dotnet --version` が 10 で始まる版を返す。

<details><summary>できなかったら</summary>

9 以下が返るときは、`dotnet --list-sdks` で .NET 10 SDK の有無を確かめます。入っていなければ入れ直します。入っていれば、`dotnet` が選ぶ版(PATH の順序や `global.json`)を確かめます。

</details>

## 手順2: クローンしてビルドする

リポジトリをクローンして、リポジトリ直下でビルドします。

```bash
git clone https://github.com/sumx21t-3310/FloatSoda.git
cd FloatSoda
dotnet build
```

**できた?** `dotnet build` がエラーなしで終わる。

<details><summary>できなかったら</summary>

エラーメッセージの全文を読みます。SDK の版が合わないエラーなら、手順1に戻ります。それ以外のエラーで、自分の変更を加える前に出るときは、[不具合を切り分けて報告する](../1-get-involved/report-bugs.md)に進みます。

</details>

## 手順3: CI と同じ手順でテストする

PR を出す前に、CI と同じ手順を手元で通すことが規約です。CI の定義は [ci.yml](../../../.github/workflows/ci.yml) の Build と Test のステップで、次のコマンドと同じです。食い違うときは ci.yml に合わせます。次のコマンドを順に実行します。

```bash
dotnet build --configuration Release
dotnet test tests/FloatSoda.Rendering.Test --configuration Release --no-build
dotnet test tests/FloatSoda.Test --configuration Release --no-build
```

特定のテストだけを動かすには、`--filter` を使います。

```bash
dotnet test tests/FloatSoda.Test --filter "FullyQualifiedName~AlignmentTest.ComputeOffset"
```

`--filter` に一致するテストがないと、テストを1件も実行しないまま終わります。出力の合格件数が1以上であることを確かめます。

規約の全体は [コントリビューターの規約](conventions.md)から案内しています。

**できた?** 2つのテストプロジェクトが、どちらも成功で終わる。

<details><summary>できなかったら</summary>

テストが落ちたときは、落ちたテストの名前とメッセージを確かめます。自分の変更を加える前に落ちるなら、環境の問題か FloatSoda の問題です。[不具合を切り分けて報告する](../1-get-involved/report-bugs.md)に進みます。

</details>

移植をする人は手順4へ、それ以外の人は手順5へ進みます。

## 手順4: Flutter の元の実装を引ける状態にする(移植をする人)

> 【未決】この節は判断が要るため、ト書きのまま残しています。オーナーが決めたあとで本文にします(追跡: #219)。

- 読み手が持って来るもの: 移植をしたいという目的
- このページが渡すもの: 参照クローンの置き場所と、読み取り専用で扱うこと(`pull` と `checkout` をしない理由は、行番号とリンクが固定できなくなるため)。対応する実装を探す `flutter-widget-source` skill の使い方
- 読み手がすること: 参照クローンを置き、ウィジェット名から Flutter の実装を1つ引いてみる
- できた?: 指定したウィジェットの Widget / Element / RenderObject の位置が分かる
- 次へ進む先: 移植の手順(../4-write/port-from-flutter.md)
- 【未決】参照クローンの作り方をこのページに書くか、skill のページに任せるか

<details>
<summary>仮に書いた本文(判断の材料。採用は未定)</summary>

移植では、Flutter の元の実装を見ながら書きます。flutter/flutter のクローンを、参照専用に1つ用意します。

1. 任意の場所に、flutter/flutter をクローンする
2. そのクローンでは `pull` も `checkout` もしない

参照専用にするのは、行番号とリンクを固定できるようにするためです。CONTRIBUTING.md と REVIEW.md は、参照クローンの例として `~/code_reading/flutter_reference` を挙げています。参照した版は、PR の本文の「Flutter reference」の欄に書きます。クローンの中で `git rev-parse HEAD` を実行すると、書く commit が分かります。

ウィジェット名から Flutter の Widget / Element / RenderObject を特定する手順は、`flutter-widget-source` skill にあります。

**できた?** 指定したウィジェットの Widget / Element / RenderObject の位置が、Flutter のクローンのファイルと行で分かる。

<details><summary>できなかったら</summary>

クローンのファイルが見つからないときは、クローンが完了しているかを確かめます。ウィジェット名が Flutter にないときは、FloatSoda 独自のウィジェットの可能性があります。移植の手順は [Flutter から移植する](../4-write/port-from-flutter.md)にあります。

</details>

<!-- ※要確認: 参照クローンの作り方(置き場所、固定する版)を示す文書がリポジトリにない。上の手順は一般的な取得方法。また flutter-widget-source skill はこのリポジトリの .agents/skills/ になく、読み手が入手できるかを確かめられていない -->

</details>

## 手順5: Rider などの道具を足す(任意)

JetBrains Rider が入っていて、その MCP サーバーが使えるなら、C# のシンボル解決、リネーム、namespace の移動に使うと確実です。文字列置換は、ジェネリック制約、XML ドキュメントの `cref`、`nameof` を見落とします。

Rider がなくても作業は進められます。`dotnet` CLI と検索で続けます。道具を入れるかどうかは、自分で決めます。エージェントは、ソフトウェアを入れる前に、入れるものと理由を人間に確認します。

次は [コントリビューターの規約](conventions.md)です。

## うまくいかないとき

ビルドやテストで出たエラーは、症状で行き先が分かれます。

| 症状 | 行き先 |
| :-- | :-- |
| SDK の版が合わない | 手順1 |
| 自分の変更を加える前にテストが落ちる | [不具合を切り分けて報告する](../1-get-involved/report-bugs.md) |
| 自分の変更を加えたらテストが落ちる | 落ちたテストの名前とメッセージを読み、変更を見直す |
