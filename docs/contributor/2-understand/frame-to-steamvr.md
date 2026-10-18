← [コントリビューター](../Home.md)

# 1フレームが SteamVR に届くまで

> このページは執筆前の骨組みです。設計シートとト書きだけを置き、本文はまだ書いていません(追跡: #219)。

## 設計シート

- 分類: 説明型(Diátaxis: explanation)
- 読み手: だれが / FloatSoda のコードを読む、または直したい人 / 何を知っている / C# と、Flutter または宣言的 UI の基本。FloatSoda の層の名前は知らなくてよい / 読んで何をしたい / 1フレームの描画が、どのスレッドのどの層を通って SteamVR のオーバーレイに届くかをつかみ、変更したい箇所がどの層にあるかを判断したい
- 出力言語と文体の指定: 日本語。敬体(CONTRIBUTING.md の「ドキュメントのトンマナ」の「文体」)
- テーマ: メインスレッドで始まった1フレームが、描画スレッドを経て SteamVR のオーバーレイのテクスチャになるまでの流れを、層をまたいで説明する。入力とリサイズの流れも含める
- 依頼の解釈: なし
- テーマファイル: explanation
- Why(目的): 層をまたぐ流れを知らないと、変更が効かない理由(スレッドの違い、複製のタイミング)を取り違える。層ごとの解説へ進む前の地図にする
- What(目標・結果): 1フレームの流れが、メインスレッドと描画スレッドの2つのスレッドの仕事として説明され、各段が属する層の解説へリンクされている
- How(手段): 時系列で、メインスレッド側(ビルド、レイアウト、描画、複製、投入)、描画スレッド側(Layer の描画、GL テクスチャ、オーバーレイへの提出)の順に説明する。入力とリサイズは、同じ流れの別の向きの話として別の節に置く
- 要素:
  - 層は6つの実装プロジェクトに分かれる。OVR、Rendering、Abstractions、Engine、フレームワーク(`src/FloatSoda`)、UI 系である(AGENTS.md の Project Structure)
  - スレッドは主に2つ。メインスレッド(名前 `FloatSodaMainThread`、STA)と、描画スレッド(名前 `RenderThread`)である(`src/FloatSoda/FloatSodaHostedService.cs` の 30〜36 行目、`src/FloatSoda/FloatSodaApp.cs` の 63 行目)
  - ほかに I/O スレッド(`IOTaskRunner`)がある。投入順に1件ずつ処理する、利用者のコード向けのスレッドである(`src/FloatSoda.Engine/TaskRunner.cs` の 46 行目の `IOTaskRunner`)。フレームの流れには入らない
  - メインループは、1周ごとに次の順で動く。保留中のタスクを実行する → OpenVR のシステムイベントをポーリングし、アクション入力を更新する → 各ウィンドウの `BeginFrame` を呼ぶ → `FramePacer.WaitForNextFrame` で待つ(`src/FloatSoda/FloatSodaApp.cs` の 158〜197 行目)
  - `WidgetBinding.BeginFrame` は、ポインタイベントの処理 → 登録済みのフレームコールバックの実行 → `DrawFrame` の順に動く(`src/FloatSoda/Core/WidgetBinding.cs` の 221〜235 行目)
  - `WidgetBinding.DrawFrame` は、`BuildOwner.BuildScope()` を実行し、`NeedsVisualUpdate` が立っていて `Window` があれば、続けて `FlushLayout`、ホバー更新、リサイズの検知、`FlushPaint` を行う(`src/FloatSoda/Core/WidgetBinding.cs` の 177〜195 行目)
  - `FlushLayout` と `FlushPaint` は、dirty なノードだけを `Depth` の昇順で処理する(`src/FloatSoda/Core/RenderPipeline.cs` の 35〜66 行目)
  - `RenderView.Layer` を `Clone()` し、`RenderPostTaskRunner.PostRender(Window, layer)` で描画スレッドに渡す(`src/FloatSoda/Core/WidgetBinding.cs` の 199〜201 行目)
  - Layer を複製するのは、描画スレッドが描いている間にメインスレッド側が次のフレームで同じツリーを変更しても、描画中のツリーに影響しないようにするため(AGENTS.md の「2. Layer Tree」の節の記述)。複製が共有するのは `SKPicture` などの描画リソースで、ツリーの構造は共有しない(`src/FloatSoda.Rendering/Layers/ILayer.cs` の 41〜44 行目、`src/FloatSoda.Rendering/Layers/PictureLayer.cs`)
  - 描画スレッドは `PostTaskRunner` のループで動く。各周で `PreUpdate`(投入されたタスクの実行)→ `Update`(`GLFW.PollEvents`)→ `PostUpdate` → `FramePacer` の待機の順である(`src/FloatSoda.Engine/TaskRunner.cs` の 490〜494 行目(ループ本体)、575 行目(`PreUpdate` の上書き))
  - `PostRender` が投入するタスクは `window.Present(layer)` を呼ぶ(`src/FloatSoda.Engine/TaskRunner.cs` の 562〜569 行目)
  - `OverlayWindow.Present` は `Renderer.Render(layer)` を呼び、描画後の GL テクスチャのハンドルを `Overlay.Texture.FromTexture_t` で OpenVR に渡す(`src/FloatSoda.Engine/OverlayWindow.cs` の 50〜62 行目)
  - `Renderer.Render` は `GLView.Clear()` → `LayerRenderer.Render`(`Layout` → `Paint`)→ `GLView.Flush()` の順に動く(`src/FloatSoda.Engine/Renderer.cs` の 27〜36 行目、`src/FloatSoda.Rendering/LayerRenderer.cs` の 33〜34 行目)
  - ウィンドウの作成は `FloatSodaApp.CreateWindow` がメインスレッドのタスクとして行い、`GLView` と `Renderer` の作成は描画スレッドのタスクとして投入される。`Window` は描画スレッドで非同期に生成されるので、メインスレッド側からは生成が観測できたフレームから使える(`src/FloatSoda/FloatSodaApp.cs` の 98〜113 行目、`src/FloatSoda/Core/WidgetBinding.cs` の 110〜119 行目、264〜266 行目のコメント)
  - リサイズの流れ: レイアウト結果のサイズが前回と変わったら、`PostTask` で `window.Resize(newSize)` を描画スレッドに投げる。`OverlayWindow.Resize` は `GLView` を作り直し、`WidthInMeters` を `Dpm` から更新する(`src/FloatSoda/Core/WidgetBinding.cs` の 204〜216 行目、`src/FloatSoda.Engine/OverlayWindow.cs` の 69〜78 行目)
  - 入力の流れ(ダッシュボードのオーバーレイ): OpenVR のマウスイベント → `OverlayMouseRawPointerSource` → `PointerController`(`Flush` でキューを取り出す)→ `WidgetBinding.HandlePointerEvent` → ヒットテスト → ジェスチャの配送(`src/FloatSoda.Engine/OverlayWindow.cs` の 41 行目、`src/FloatSoda.Engine/OverlayMouseRawPointerSource.cs`、`src/FloatSoda.Abstractions/Input/PointerController.cs`、`src/FloatSoda/Core/WidgetBinding.cs` の 267〜290 行目)
  - ダッシュボード以外のオーバーレイでは `PointerSource` が `null` で、ポインタは届かない。`ControllerPointerSystem` は `src`・`tests`・`samples` のどこからも呼ばれていない(`src/FloatSoda.Engine/OverlayWindow.cs` の 35〜42 行目、`src/FloatSoda.OVR/Overlay/ControllerPointerSystem.cs`、2026-10-17 の grep)。追跡: #182
  - デスクトップウィンドウ(`DesktopWindow`)は、同じ `Present` の流れを、GL の既定フレームバッファへの転送で終える(`src/FloatSoda.Engine/DesktopWindow.cs` の 34〜50 行目)
  - OpenVR のアクション入力(コントローラーのボタン)は、メインループの `_inputUpdater.Update()` が毎周更新する。UI のポインタ入力とは別の経路である(`src/FloatSoda/FloatSodaApp.cs` の 178 行目、`src/FloatSoda.OVR/Input/VRInputUpdater.cs`)
  - 書くときの注意(前の docs との差。コードを正とする): `FloatSodaApp` は `WidgetBinding.BeginFrame` を呼び、`DrawFrame` はその中で呼ばれる。描画スレッドの型名は `RenderPostTaskRunner` である(`RenderThreadRunner` という型はない)。依存の向きは csproj の参照で決まり、OVR はほかの FloatSoda のプロジェクトを参照せず、Abstractions は Rendering を参照する(`src/FloatSoda.Abstractions/FloatSoda.Abstractions.csproj`)
  - 別案: 複製せずに描画スレッドへ所有権を渡す案、またはツリーを二重化する案は、コードの上には検討の記録がない。理由は複製の記述(AGENTS.md)だけが一次情報である。※要確認
- 見出し:
  - 1フレームを担う2つのスレッド
  - メインスレッド: ビルドから Layer の複製まで
  - 描画スレッド: Layer を描いて SteamVR に渡す
  - なぜ Layer を複製して渡すのか
  - リサイズは同じ流れの別の枝
  - 入力は逆向きに流れる
  - 各層の解説へ
- ※要確認:
  - 複製の理由の根拠が AGENTS.md の1文だけで、コードのコメントや記録に出典がない(オーナーに確かめるか、ADR を探す)
  - 描画スレッドと メインスレッドのフレームレートの関係(`TargetFrameRate` が両方の `FramePacer` に効く。`src/FloatSoda/FloatSodaServiceCollectionExtensions.cs` の 41 行目と `src/FloatSoda/FloatSodaOptions.cs` の 19 行目を読んで確かめる)
  - OpenVR のコンポジターが、提出されたテクスチャをいつ取り込むか(OpenVR の仕様で、FloatSoda のコードからは分からない)
  - 複数ウィンドウがあるとき、描画スレッドのタスクが投入順に実行される以外の順序保証があるか

## ト書き

### 1フレームを担う2つのスレッド

- 読み手が持って来るもの: 「描画はどこで動くのか」という疑問
- このページが渡すもの: メインスレッド(STA)と描画スレッドの2つ、I/O スレッドは別枠という整理と、全体の流れの図(メインスレッドの箱から描画スレッドの箱へ、Layer の複製が渡る)
- 読み手がすること: 図で流れをつかむ
- 次へ進む先: 「メインスレッド: ビルドから Layer の複製まで」

### メインスレッド: ビルドから Layer の複製まで

- 読み手が持って来るもの: 2つのスレッドの整理
- このページが渡すもの: メインループの1周(タスク → イベント → `BeginFrame` → 待機)と、`DrawFrame` の中の順序(ビルド → レイアウト → 描画 → 複製 → 投入)。各段が属する層(フレームワーク層)の解説へのリンク
- 読み手がすること: 順序を追い、変更したい処理がどの段にあるかを見つける
- 止まる箇所: `NeedsVisualUpdate` が立っていないフレームは、レイアウトと描画を飛ばす。ここで「毎フレーム描き直すわけではない」と書く
- 次へ進む先: 「描画スレッド: Layer を描いて SteamVR に渡す」。詳細は `framework-layer.md`

### 描画スレッド: Layer を描いて SteamVR に渡す

- 読み手が持って来るもの: メインスレッドが `PostRender` するところまでの流れ
- このページが渡すもの: 描画スレッドのループ(タスクの実行 → GLFW のイベント → 待機)と、`Present` の中身(`Renderer.Render` → テクスチャのハンドル → `FromTexture_t`)
- 読み手がすること: GL テクスチャが OpenVR に渡る箇所を見つける
- 次へ進む先: 「なぜ Layer を複製して渡すのか」。詳細は `engine-layer.md`、`rendering-layer.md`、`ovr-layer.md`

### なぜ Layer を複製して渡すのか

- 読み手が持って来るもの: 「複製のコードがあるが、必要なのか」という疑問
- このページが渡すもの: 複製の理由(描画中のツリーを次のフレームの変更から守る)と、複製で共有されるもの(`SKPicture` の参照)。複製の細部は描画層の解説へリンクする
- 読み手がすること: 複製を省く変更が、何を壊すかを判断する
- 次へ進む先: 「リサイズは同じ流れの別の枝」
- 【未決】複製の理由の出典が AGENTS.md の1文だけ。ADR として記録を足すか、このページで「理由」を断定せずに書くか

### リサイズは同じ流れの別の枝

- 読み手が持って来るもの: 「ウィンドウの大きさはだれが決めるのか」という疑問
- このページが渡すもの: レイアウト結果のサイズ → `PostTask` → `OverlayWindow.Resize`(`GLView` の作り直し、`WidthInMeters`、マウススケール)の流れ
- 読み手がすること: サイズを変える処理が描画スレッドで走ることを確かめる
- 次へ進む先: 「入力は逆向きに流れる」

### 入力は逆向きに流れる

- 読み手が持って来るもの: 「コントローラーのレーザーがボタンに届くまで」の疑問
- このページが渡すもの: ダッシュボードのオーバーレイの入力経路(OpenVR → ポインタ入力源 → `PointerController` → ヒットテスト → ジェスチャ)。ダッシュボード以外には届かないという制約と、追跡: #182
- 読み手がすること: 入力の各段がどのスレッドで動くかを、`Flush` を呼ぶスレッド(メインスレッド)で確かめる
- 次へ進む先: 「各層の解説へ」。ウィジェット側は `framework-layer.md`、OpenVR 側は `ovr-layer.md`

### 各層の解説へ

- 読み手が持って来るもの: 流れの全体像
- このページが渡すもの: 層ごとの解説へのリンク(`framework-layer.md`、`rendering-layer.md`、`engine-layer.md`、`ovr-layer.md`、`ui-layer.md`)と、手を動かす入口(`tutorial-render-path.md`、各実験)
- 読み手がすること: 変更したい層の解説へ進む
- 止まる箇所: なし。ここでこのページは終わる
