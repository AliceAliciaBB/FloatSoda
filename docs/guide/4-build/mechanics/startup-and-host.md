← [ガイド](../../Home.md)

# 起動と Generic Host

> このページは執筆前の骨組みです。設計シートとト書きだけを置き、本文はまだ書いていません(追跡: #219)。タイトル案: 起動のコードは何をしているの?

## 設計シート

- 分類: 説明型(Diátaxis: explanation)
- 読み手: だれが / VRChatter、Booth クリエイター。チュートリアルの起動のコードを貼って動かした / 何を知っている / 貼ったコードが動くことは知っている。各行の意味は知らない / 読んで何をしたい / 起動のコードの各行が何をしているかを知り、自分で書き換えても怖くない状態になりたい
  - 共通の性質と専門用語の扱い: CONTRIBUTING.md の「ドキュメントのトンマナ」の「読み手」
- 出力言語と文体の指定: 日本語。敬体。トンマナは CONTRIBUTING.md の「ドキュメントのトンマナ」に従う
- テーマ: 起動のコードの各行が、何を準備し、いつ動き始め、どう終わるか
- 依頼の解釈: チュートリアルが起動のコードの各行を説明するか、このページに送るだけにするかは、チュートリアル側の未決。このページは、各行の意味をすべて説明する前提で骨組みを作る
- テーマファイル: explanation
- Why(目的): 起動のコードの意味が分からないと、AI が書いた変更を点検できず、起動の失敗の原因も探せない
- What(目標・結果): `Host.CreateApplicationBuilder` から `host.RunAsync()` までに起きることを、順に説明できる
- How(手段): 起動のコードを行ごとに区切り、各行の前後で起きることを説明する
- 要素:
  - `Host.CreateApplicationBuilder(args)` は、.NET の Generic Host の builder を作る(`samples/FloatSoda.Samples.GettingStarted/Program.cs`)
  - `AddFloatSoda()` は、FloatSoda の設定、アプリ情報、フレーム管理、`FloatSodaApp` をサービスとして登録し、`FloatSodaHostedService` をホストに登録する(`src/FloatSoda/FloatSodaServiceCollectionExtensions.cs`)
  - `GetRequiredService<FloatSodaApp>()` で、登録済みの `FloatSodaApp` を取り出す(`samples/FloatSoda.Samples.GettingStarted/Program.cs`)
  - `CreateWindow` は、ウィンドウの作成を予約する。実際の作成は、メインループが次に回ったときに行われる(`src/FloatSoda/FloatSodaApp.cs`)
  - `host.RunAsync()` で `FloatSodaHostedService` が始まり、専用のメインスレッド(STA)で OpenVR を初期化して、メインループが動く(`src/FloatSoda/FloatSodaHostedService.cs`、`src/FloatSoda/FloatSodaApp.cs`)
  - 起動の完了は、初期化のあとに通知される。OpenVR の初期化に失敗すると、例外が起動を失敗させる(`src/FloatSoda/FloatSodaHostedService.cs`、`src/FloatSoda/FloatSodaApp.cs`)
  - アプリの識別に失敗しても、警告のログだけで起動は続く(`src/FloatSoda/FloatSodaApp.cs`)
  - 終了は、SteamVR の終了通知か、ホストの停止で行われる(`src/FloatSoda/FloatSodaApp.cs`)
  - `AppKey` の既定は `FloatSoda` で、アプリごとに別の値を指定する(`src/FloatSoda/FloatSodaOptions.cs`)
- 見出し:
  - 起動のコードは、5つのことをしている
  - ホストの準備: Host.CreateApplicationBuilder と AddFloatSoda
  - アプリの取り出しと、ウィンドウの予約
  - RunAsync で動き始める
  - 起動に失敗するとき
  - 終わるとき
- ※要確認:
  - `builder.Build()` の時点では SteamVR へつながないこと(`FloatSodaApp` の生成が `GetRequiredService` まで遅延されることを、実行して確かめる)
  - 同じ `Title` で `CreateWindow` を2回呼んだときの挙動(`src/FloatSoda/FloatSodaApp.cs` の `_bindings.TryAdd` を読んで確かめる)
  - `AppKey` を省略した複数のアプリが同時に動いたときの問題(実機で確かめる)

## ト書き

### 起動のコードは、5つのことをしている

- 読み手が持って来るもの: チュートリアルの起動のコード
- このページが渡すもの: 冒頭の結論(ホストを作る、FloatSoda を登録する、ウィンドウを予約する、動き始める、終わる)と、コードを5つに区切った図
- 読み手がすること: 区切りをつかむ
- 次へ進む先: 次の節

### ホストの準備: Host.CreateApplicationBuilder と AddFloatSoda

- 読み手が持って来るもの: 区切り1と2
- このページが渡すもの: .NET の Generic Host とは何か(1文)と、`AddFloatSoda` が登録するもの
- 読み手がすること: 登録されるものの名前を知る
- 止まる箇所: Generic Host 自体の詳しい説明。.NET の公式資料へリンクで渡す
- 次へ進む先: 次の節

### アプリの取り出しと、ウィンドウの予約

- 読み手が持って来るもの: 区切り3
- このページが渡すもの: `FloatSodaApp` を取り出す理由と、`CreateWindow` が予約であること
- 読み手がすること: ウィンドウが作られるタイミングを知る
- 次へ進む先: 次の節

### RunAsync で動き始める

- 読み手が持って来るもの: 区切り4
- このページが渡すもの: `RunAsync` で、メインスレッドと OpenVR の初期化が始まる流れ
- 読み手がすること: 動き始める順番をつかむ
- 次へ進む先: 次の節

### 起動に失敗するとき

- 読み手が持って来るもの: 起動の例外
- このページが渡すもの: 失敗が起動を止める場合(OpenVR の初期化)と、続行する場合(アプリの識別、入力の初期化)の違い
- 読み手がすること: 自分の症状がどちらかを知る
- 止まる箇所: 症状が出ている読み手。ガイドの「5. 困ったとき」の「エラーを読んで直す」へ送る
- 次へ進む先: 次の節

### 終わるとき

- 読み手が持って来るもの: 前の節
- このページが渡すもの: SteamVR の終了とホストの停止で終わること。「Generic Host の仕組みを使う」へのリンク
- 読み手がすること: 必要なら次のページへ進む
- 止まる箇所: なし。ここでページは終わる
