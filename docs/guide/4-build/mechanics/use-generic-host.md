← [ガイド](../../Home.md)

# Generic Host の仕組みを使う

> このページは執筆前の骨組みです。設計シートとト書きだけを置き、本文はまだ書いていません(追跡: #219)。タイトル案: 設定やログ、裏で動く処理を足す

## 設計シート

- 分類: 説明型(Diátaxis: how-to)
- 読み手: だれが / uGUI を避けたいエンジニア、AI に頼む VRChatter。起動のコード(`Host.CreateApplicationBuilder`)は動かしている / 何を知っている / .NET の設定、ログ、DI(依存性注入)の基本は知っているか、AI が知っている / 読んで何をしたい / 自分のツールに、設定ファイル、ログ、サービスの登録、裏で動く処理を足したい
  - 共通の性質と専門用語の扱い: CONTRIBUTING.md の「ドキュメントのトンマナ」の「読み手」
- 出力言語と文体の指定: 日本語。敬体。トンマナは CONTRIBUTING.md の「ドキュメントのトンマナ」に従う
- テーマ: FloatSoda の起動に使われている Generic Host に、設定、ログ、サービス、裏で動く処理を足す手順
- 依頼の解釈: 画面のウィジェットからサービスを取り出す手順は含める。含めないと、登録したサービスを画面で使えないため
- テーマファイル: how-to
- Why(目的): FloatSoda のために作り直さず、.NET の標準の仕組みで、設定とログと裏の処理を足せるようにする
- What(目標・結果): 設定の値を画面に表示し、ログが出て、裏で動く処理の結果が画面に反映される
- How(手段): 設定 → ログ → サービスの登録と画面での取り出し → 裏で動く処理、の順に足す
- 要素:
  - `Host.CreateApplicationBuilder` で作った builder に `AddFloatSoda()` を足す。`builder.Configuration`、`builder.Services`、`builder.Logging` はそのまま使える(`samples/FloatSoda.Samples.GettingStarted/Program.cs`、`src/FloatSoda/FloatSodaServiceCollectionExtensions.cs`)
  - `FloatSodaOptions` で `AppKey`、`TargetFrameRate`、`InputActionMaps` を指定する(`src/FloatSoda/FloatSodaOptions.cs`)
  - `TargetFrameRate` が 0 以下だと `ArgumentOutOfRangeException` になる(`src/FloatSoda/FloatSodaServiceCollectionExtensions.cs`)
  - FloatSoda の内部は `ILoggerFactory` があればログを出す。`builder.Logging` の設定がそのまま効く(`src/FloatSoda/FloatSodaApp.cs`)
  - FloatSoda 自身も `IHostedService`(`FloatSodaHostedService`)として登録される。裏で動く処理は、自分で `AddHostedService` で足せる(`src/FloatSoda/FloatSodaServiceCollectionExtensions.cs`、`src/FloatSoda/FloatSodaHostedService.cs`)
  - 画面のウィジェットからサービスを取り出すには、`ServiceProvider` ウィジェットで `IServiceProvider` を渡し、`context.GetRequiredService<T>()` を使う(`src/FloatSoda/Widgets/ServiceProvider.cs`)
  - `ServiceProvider` ウィジェットを挿入するコードは `src/` にない(`new ServiceProvider` を検索して確かめた)。ルートの `Child` を自分で包む(`src/FloatSoda/Widgets/ServiceProvider.cs`)
  - 裏で動く処理から画面を更新するには、状態を持つ `State` の `SetState()` を呼ぶ(スレッドの扱いは ※要確認)
  - `TaskBuilder<T>` は、`Task` の完了を待って表示を変えるウィジェット(`src/FloatSoda/Widgets/TaskBuilder.cs`)
- 見出し:
  - この手順でできること
  - 前提条件
  - 手順1: 設定の値を読む
  - 手順2: ログを出す
  - 手順3: サービスを登録して、画面で使う
  - 手順4: 裏で動く処理を足す
  - 結果を確かめる
  - うまくいかないとき
- ※要確認:
  - `ServiceProvider` で包んだルートが、`CreateWindow` に渡せて動くか(コード例を取り出してビルドして確かめる)
  - 裏で動く処理(別スレッド)から `SetState()` を呼んで安全か(`src/FloatSoda/Elements/BuildOwner.cs` を読んで確かめる)
  - `IOTaskRunner` を利用者が使える公開 API かどうか(`src/FloatSoda.Engine/TaskRunner.cs` の可視性を読んで確かめる)
  - 設定ファイル(`appsettings.json`)が、`Host.CreateApplicationBuilder` の既定どおり読まれて、公開した exe の隣でも読まれるか(実行して確かめる)

## ト書き

### この手順でできること

- 読み手が持って来るもの: 「設定やログや裏の処理を足したい」という目的
- このページが渡すもの: 足せる4つ(設定、ログ、サービス、裏で動く処理)の一覧と、完成後の動き
- 読み手がすること: 自分が足したいものを決める
- 次へ進む先: 「前提条件」

### 前提条件

- 読み手が持って来るもの: `Host.CreateApplicationBuilder` を使う起動のコード
- このページが渡すもの: 要るもの(チュートリアルを終えていること)と、起動のコードの意味を知るための「起動のコードは何をしているの?」へのリンク
- 読み手がすること: 確かめる
- 次へ進む先: 手順1

### 手順1: 設定の値を読む

- 読み手が持って来るもの: 起動のコード
- このページが渡すもの: `builder.Configuration` から値を読んで `FloatSodaOptions` に渡す書き方
- 読み手がすること: 設定ファイルの値で `TargetFrameRate` を決める
- できた?: 設定ファイルの値を変えると、フレームレートの設定が変わる
- 次へ進む先: 手順2

### 手順2: ログを出す

- 読み手が持って来るもの: 手順1のコード
- このページが渡すもの: `ILogger` の取り出し方と、FloatSoda 自身のログが同じ設定で出ること
- 読み手がすること: ログを1行出す
- できた?: コンソールにログが出る
- 次へ進む先: 手順3

### 手順3: サービスを登録して、画面で使う

- 読み手が持って来るもの: 画面のウィジェット
- このページが渡すもの: `builder.Services` への登録と、`ServiceProvider` ウィジェットで包んでから `context.GetRequiredService<T>()` で取り出す流れ
- 読み手がすること: サービスを登録し、画面で取り出す
- できた?: 画面にサービスの値が出る
- 次へ進む先: 手順4
- 【未決】`ServiceProvider` で包む手順を毎回書かせる形のままにするか、FloatSoda 側に自動挿入を求める Issue を立てるか。後者は読み手ではなく開発側の判断

### 手順4: 裏で動く処理を足す

- 読み手が持って来るもの: 手順3のサービス
- このページが渡すもの: `BackgroundService` を `AddHostedService` で登録する書き方と、結果を画面に反映する方法
- 読み手がすること: 裏で動く処理を足す
- できた?: 裏の処理の結果が画面に出る
- 次へ進む先: 「結果を確かめる」

### 結果を確かめる

- 読み手が持って来るもの: 手順を終えたコード
- このページが渡すもの: 確かめる観点(設定、ログ、サービス、裏の処理)
- 読み手がすること: 起動して確かめる
- 次へ進む先: 「exe にして配る」

### うまくいかないとき

- 読み手が持って来るもの: 起動で例外が出る、または値が画面に出ない
- このページが渡すもの: ガイドの「5. 困ったとき」の「エラーを読んで直す」へのリンク
- 読み手がすること: リンク先で切り分ける
- 止まる箇所: なし
