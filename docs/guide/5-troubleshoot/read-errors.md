← [ガイド](../Home.md)

# エラーを読んで直す

> このページは執筆前の骨組みです。設計シートとト書きだけを置き、本文はまだ書いていません(追跡: #219)。

## 設計シート

- 分類: 説明型(Diátaxis: how-to)
- 読み手: だれが / VRChatter、Booth クリエイター、uGUI を避けたいエンジニア。起動したら例外やエラーのログが出た / 何を知っている / コンソールの出力を読めるか、AI に貼って聞ける / 読んで何をしたい / エラーのメッセージから原因を探し、直したい
  - 共通の性質と専門用語の扱い: CONTRIBUTING.md の「ドキュメントのトンマナ」の「読み手」
- 出力言語と文体の指定: 日本語。敬体。トンマナは CONTRIBUTING.md の「ドキュメントのトンマナ」に従う
- テーマ: 起動時とフレーム処理中に出るエラーを、症状ごとに原因へ結びつけて直す手順
- 依頼の解釈: 画面の見た目が違う、押せないなどの「エラーが出ない不具合」は、このページに含めない。「反応しないときに切り分ける」と「レイアウトの考え方」に送る
- テーマファイル: how-to
- Why(目的): エラーのメッセージを、AI に貼る前に自分で原因の当たりをつけられるようにする
- What(目標・結果): 出ているエラーが、どの症状の節に当たるかが分かり、直す操作を1つ行える
- How(手段): 症状ごとの節を並べる。節は「出るメッセージ、原因、直し方、直ったことの確かめ方」の形で揃える。チュートリアルの「できなかったら」から、症状ごとの節へ直接リンクする
- 要素:
  - 起動時の OpenVR の初期化に失敗すると、例外が投げられてアプリが起動に失敗する(`src/FloatSoda/FloatSodaApp.cs` の `Initialize`)
  - 初期化の失敗の種類は、`VRInitializeException` のメッセージに入る。例: `Init_HmdNotFound`、`Init_InstallationNotFound`、`Init_VRClientDLLNotFound`(`src/FloatSoda.OVR/Exceptions/Resources/ExceptionMessages.cs`)
  - `--desktop` でも HMD の接続と SteamVR の起動は要る。HMD 未接続では `Init_HmdNotFound` で失敗する(`CONTRIBUTING.md`、追跡: #140)
  - アプリの識別に失敗しても、警告のログ(`SteamVRへのアプリケーション識別に失敗しました`)が出るだけで起動は続く。失敗した場合は、自動起動などマニフェストに関わる機能だけが動かない(`src/FloatSoda/FloatSodaApp.cs`)
  - アクション入力の初期化に失敗しても、警告のログ(`アクション入力の初期化に失敗しました`)が出るだけで、オーバーレイの表示は続く(`src/FloatSoda/FloatSodaApp.cs`)
  - メインループ中の例外は、ログ(`タスクエラー`、`イベント処理エラー`、`描画エラー`)に出て、ループが止まる(`src/FloatSoda/FloatSodaApp.cs`)
  - `TargetFrameRate` が 0 以下だと `ArgumentOutOfRangeException`(`src/FloatSoda/FloatSodaServiceCollectionExtensions.cs`)
  - `InputAction<T>` の `T` が `bool`・`float`・`Vector2` 以外だと `NotSupportedException`(`src/FloatSoda.OVR/Input/InputAction.cs`)
  - `WindowWidget.Of(context)` は、祖先にウィンドウがないと `InvalidOperationException`(`src/FloatSoda/Widgets/WindowWidget.cs`)
  - `context.GetRequiredService<T>()` は、祖先に `ServiceProvider` がない、または登録がないと `InvalidOperationException`(`src/FloatSoda/Widgets/ServiceProvider.cs`)
  - 例外の種類は、OpenVR の領域ごとに分かれている(`VRInitializeException`、`VROverlayException`、`VRInputException`、`VRApplicationException` など)(`src/FloatSoda.OVR/Exceptions/`)
  - エラーメッセージの一部は日本語で、一部は英語(`src/FloatSoda.OVR/Exceptions/Resources/`、`docs` の Localization の方針。日本語が既定)
  - 直らないときの行き先は、ガイドの「5. 困ったとき」の入口ページの「どこでだれに聞くか」にある。各ページの最後にもリンクを置く(docs 再編の決定、2026-10-16)
- 見出し:
  - 最初に、エラーのメッセージを控える
  - 起動で例外が出る: SteamVR または HMD につながらない
  - 起動で警告が出るが、表示はされる
  - 動いていたアプリが途中で止まる
  - 渡した値が不正だと言われる
  - ウィンドウやサービスが祖先に見つからないと言われる
  - それでも直らないとき
- ※要確認:
  - `VRInitializeException` の各メッセージの日本語の文言(`src/FloatSoda.OVR/Exceptions/Resources/` のリソースを読んで確かめる。メッセージの原文を本文に引用する)
  - `Init_HmdNotFound` 以外の初期化エラー(`Init_InstallationNotFound` など)が、利用者の環境でどういう状況で出るか(再現して確かめる)
  - 起動時に SteamVR が動いていない場合に出る例外の種類(SteamVR を終了して起動して確かめる)
  - 「描画エラー」が出る代表的な原因(再現して確かめる。確かめられなければ、その節は原因を挙げず、ログの読み方だけにする)
  - HMD なしで動かす手段の有無(`Init_HmdNotFound` の回避策。今はないと読めるが、`src/FloatSoda.OVR` を読み直して確かめる。追跡: #140)

## ト書き

### 最初に、エラーのメッセージを控える

- 読み手が持って来るもの: 出ているエラー
- このページが渡すもの: 控えるもの(例外の型、メッセージ、ログの先頭の語)と、症状ごとの節の一覧。AI に貼るときに添えるもの(FloatSoda のバージョン、動かした窓の種類)
- 読み手がすること: メッセージを控えて、一覧から当たる節を探す
- 次へ進む先: 当たる節

### 起動で例外が出る: SteamVR または HMD につながらない

- 読み手が持って来るもの: 起動の例外(`VRInitializeException` など)。チュートリアルの段1の「オーバーレイが出ない、起動で例外が出る」からここへ来る
- このページが渡すもの: 例外のメッセージごとの原因と直し方(SteamVR を先に起動する、HMD をつなぐ)。デスクトップの窓でも HMD の接続が要ること(追跡: #140)
- 読み手がすること: 原因を1つずつ確かめて直す
- できた?: アプリが起動し、ダッシュボードにオーバーレイが出る
- 止まる箇所: SteamVR 自体が壊れている場合。FloatSoda の外の話なので、直し方は書かず、SteamVR の資料へ送る
- 次へ進む先: 「それでも直らないとき」

### 起動で警告が出るが、表示はされる

- 読み手が持って来るもの: 起動時の警告のログ(アプリの識別、アクション入力)
- このページが渡すもの: それぞれの警告で動かなくなる機能(自動起動などのマニフェストの機能、コントローラー入力)と、直すか放置するかの判断
- 読み手がすること: 自分が使う機能に関わる警告かを判断する
- できた?: 警告の意味が分かる
- 次へ進む先: 「コントローラーの入力を受け取る」または「SteamVR と一緒に起動する」

### 動いていたアプリが途中で止まる

- 読み手が持って来るもの: 実行中に止まったアプリと、ログ
- このページが渡すもの: ログの先頭の語(タスクエラー、イベント処理エラー、描画エラー)と、止まる仕組み(例外でメインループが止まる)。例外の中身から、自分のコードの中かを見分ける方法
- 読み手がすること: ログから例外の型と呼び出し元を読む
- できた?: 自分のコードのどの部分で例外が出たかが分かる
- 次へ進む先: 「それでも直らないとき」

### 渡した値が不正だと言われる

- 読み手が持って来るもの: `ArgumentOutOfRangeException` や `NotSupportedException`
- このページが渡すもの: 起動の設定(`TargetFrameRate`)と入力(`InputAction<T>`)で、許される値の範囲
- 読み手がすること: 値を範囲に収める
- できた?: 例外が出なくなる
- 次へ進む先: 「それでも直らないとき」

### ウィンドウやサービスが祖先に見つからないと言われる

- 読み手が持って来るもの: `InvalidOperationException`(`...が祖先に見つかりません`)
- このページが渡すもの: `ServiceProvider` やウィンドウで画面を包み忘れた場合の直し方。「Generic Host の仕組みを使う」へのリンク
- 読み手がすること: 包む
- できた?: 例外が出なくなる
- 次へ進む先: 「それでも直らないとき」

### それでも直らないとき

- 読み手が持って来るもの: 直らない症状
- このページが渡すもの: ガイドの「5. 困ったとき」の入口ページの「どこでだれに聞くか」へのリンク。聞くときに添える情報の一覧(メッセージ、バージョン、窓の種類、再現手順)
- 読み手がすること: 聞く
- 止まる箇所: なし。ここでページは終わる
