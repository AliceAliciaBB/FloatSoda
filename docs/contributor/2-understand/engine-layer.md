← [コントリビューター](../Home.md)

# エンジン層

> このページは執筆前の骨組みです。設計シートとト書きだけを置き、本文はまだ書いていません(追跡: #219)。

## 設計シート

- 分類: 説明型(Diátaxis: explanation)
- 読み手: だれが / `src/FloatSoda.Engine` のコードを読む、描画スレッドや GL の周りを直したい人 / 何を知っている / C# のスレッドと、SkiaSharp の `SKSurface` の基本。OpenGL は詳しくなくてよい / 読んで何をしたい / Layer ツリーが GL テクスチャになり、描画スレッドで動く仕組みをつかみ、スレッドの制約を破らずに変更したい
- 出力言語と文体の指定: 日本語。敬体(CONTRIBUTING.md の「ドキュメントのトンマナ」の「文体」)
- テーマ: エンジン層(`FloatSoda.Engine`)が、描画スレッド、GL の描画先(`GLView`)、Layer ツリーの描画(`Renderer`)、ウィンドウ(`OverlayWindow`・`DesktopWindow`)、フレーム間隔(`FramePacer`)をどう受け持つかを説明する
- 依頼の解釈: なし
- テーマファイル: explanation
- Why(目的): GL のコンテキストは作ったスレッドでしか使えないので、スレッドの制約を知らずに変更すると、描画が止まる、または描画が壊れる。Layer を複製して渡す理由もここに置く(描画層の解説からここへリンクする)
- What(目標・結果): 描画スレッドが何を所有し、メインスレッドとどう通信し、`Present` が何をするかが説明されている
- How(手段): 構成要素で分解する。スレッドと通信 → GL の描画先 → Layer を描く → ウィンドウ → フレーム間隔 → 入力源の順に説明する
- 要素:
  - エンジン層は Abstractions・OVR・Rendering を参照する。フレームワーク(`src/FloatSoda`)は参照しない(`src/FloatSoda.Engine/FloatSoda.Engine.csproj`)
  - 描画スレッドは `RenderPostTaskRunner` が持つ。`PostTaskRunner` の専用スレッドで、`IsBackground` が真、名前は呼び出し側が決める(`src/FloatSoda.Engine/TaskRunner.cs` の 389、489 行目付近、551 行目。名前は `src/FloatSoda/FloatSodaApp.cs` の 63 行目で `RenderThread`)
  - `PostTask(Action)` は `ConcurrentQueue<Action>` に積む。任意のスレッドから呼べる(`src/FloatSoda.Engine/TaskRunner.cs` の 459 行目)
  - 描画スレッドのループは、`PreUpdate`(キューのタスクを実行)→ `Update`(`GLFW.PollEvents`)→ `PostUpdate` → `FramePacer` の待機の順である(`src/FloatSoda.Engine/TaskRunner.cs` の 490〜494 行目、575 行目、`RenderPostTaskRunner` の `Update` の上書き)
  - 1つのタスクが例外を投げても、他のタスクとスレッドは続く。拾わないと `OnStop` が走り、以降の描画がすべて止まる(`src/FloatSoda.Engine/TaskRunner.cs` の 462〜478 行目)
  - `OnStart` で `GLFW.Init()` を呼び、`OnStop` で `GLFW.Terminate()` を呼ぶ(`src/FloatSoda.Engine/TaskRunner.cs` の 197〜201、214 行目付近)
  - `GLView` は、GLFW のウィンドウ(既定では非表示)、GL のコンテキスト、Skia の `GRContext`、GL テクスチャを描画先とする `SKSurface` を持つ。OpenGL リソースを持つ描画スレッド上で作成、使用、破棄する(`src/FloatSoda.Engine/GLView.cs` の 9、60〜75、77〜98 行目)
  - `SKSurface` は `GRSurfaceOrigin.BottomLeft` で作る(`src/FloatSoda.Engine/GLView.cs` の 96 行目)
  - `GLView.Resize` は、既存のテクスチャと Surface を破棄して再作成する(`src/FloatSoda.Engine/GLView.cs` の 110〜146 行目)
  - `Renderer` は `GLView` を受け取り、`Render(ILayer)` で、クリア → `LayerRenderer.Render` → `Flush` を行う(`src/FloatSoda.Engine/Renderer.cs` の 27〜36 行目)
  - `GLView.Flush` は `Surface.Flush()`、`GrContext.Flush()`、`GL.Flush()` を呼ぶ(`src/FloatSoda.Engine/GLView.cs` の 177〜183 行目)
  - `IEngineWindow`(Abstractions)は、`Present(ILayer)`・`Resize(SKSizeI)`・`PointerSource` を持ち、`Present` と `Resize` は描画スレッドで呼ぶ(`src/FloatSoda.Abstractions/Engine/IEngineWindow.cs`)
  - `OverlayWindow` は、`IOverlay` を描画先にする。ダッシュボードのオーバーレイなら入力方式をマウスにして `OverlayMouseRawPointerSource` を作り、ほかのオーバーレイでは `PointerSource` が `null` である(`src/FloatSoda.Engine/OverlayWindow.cs` の 29〜43 行目)
  - `DesktopWindow` は、GLFW の可視ウィンドウに、描画結果を既定のフレームバッファへ転送して表示する(`src/FloatSoda.Engine/DesktopWindow.cs` の 34〜60 行目付近)。※要確認(転送の後半の行を読む)
  - `FramePacer` は、`Sleep` と `Thread.Yield` のスピン待機を組み合わせ、目標のフレームレート(既定 60)に間隔をそろえる(`src/FloatSoda.Engine/FramePacer.cs` の 9、17〜50 行目)
  - `OverlayMouseRawPointerSource` は、OpenVR のマウスイベントを `RawPointerEvent` に変換する。GL テクスチャが原点左上で届くので、y 座標を反転しない(実測で確認済み、ソースのコメント)。反転すると上下が鏡写しになる(`src/FloatSoda.Engine/OverlayMouseRawPointerSource.cs` の 13〜14 行目)。※要確認(コメントのみが根拠)
  - `GLFWRawPointerSource` は、GLFW のマウスコールバックを `RawPointerEvent` に変換する。イベントは描画スレッドで発火する(`src/FloatSoda.Engine/GLFWRawPointerSource.cs` の 6〜9 行目)
  - `IOTaskRunner` は、利用者向けの単一の専用スレッドで、投入順に処理する。描画のフレームには使わない(`src/FloatSoda.Engine/TaskRunner.cs` の 46 行目)
  - Layer を複製して描画スレッドに渡す理由は、メインスレッドが `RenderPipeline` を持ち、次のフレームで同じツリーを変更するため(AGENTS.md の「2. Layer Tree」)。投入と描画のあいだにキューがあるので、描画が始まる時点のツリーはメインスレッドが変更している可能性がある。※要確認(「可能性がある」は、キューの構造からの推定)
  - `EngineShell` は、`Start` の本体が空で、`Dispose` に TODO のコメントがあり、`src`・`tests`・`samples` のほかのどこからも使われていない(`src/FloatSoda.Engine/EngineShell.cs`、2026-10-17 の grep)
  - 前の docs に「`RenderThreadRunner`」という型名があった。コードにこの名前の型はなく、実際は `RenderPostTaskRunner` である(`src/FloatSoda.Engine/TaskRunner.cs` の 551 行目)
- 見出し:
  - エンジン層は、描画スレッドと GL の描画先を受け持つ
  - 描画スレッドは、キューのタスクを実行する
  - GLView は、GL テクスチャを描画先の Surface にする
  - Renderer と Present が、Layer を描いて SteamVR に渡す
  - なぜ Layer を複製して描画スレッドに渡すのか
  - フレーム間隔と入力源
- ※要確認:
  - 複製の理由の「描画が始まる時点のツリーを、メインスレッドが変更している可能性がある」の裏づけ(`RenderView.Layer` が再利用される経路を読み、`PostRender` から `Present` までのあいだの変更を確かめる)
  - `DesktopWindow.Present` の後半(転送のあと、`SwapBuffers` を呼ぶか)
  - `OverlayMouseRawPointerSource` のコメントの根拠(実測の再現は、HMD で確かめる)
  - `EngineShell` を解説に載せるか(使われていない型の扱い)
  - GL テクスチャが原点左下(`BottomLeft`)で作られ、SteamVR に届くときに上下がどう見えるか(OpenVR の仕様。コメントの記述だけが根拠)

## ト書き

### エンジン層は、描画スレッドと GL の描画先を受け持つ

- 読み手が持って来るもの: 「描画はどこで動くのか」という疑問
- このページが渡すもの: エンジン層の1文の定義(Layer ツリーを GL テクスチャに描き、SteamVR のオーバーレイまたはデスクトップのウィンドウに出す層)と、型の見取り図(`RenderPostTaskRunner`、`GLView`、`Renderer`、`OverlayWindow`・`DesktopWindow`、`FramePacer`)。フレームワークを参照しないので、単独でも組めること
- 読み手がすること: 見取り図で、各型の担当をつかむ
- 次へ進む先: 「描画スレッドは、キューのタスクを実行する」

### 描画スレッドは、キューのタスクを実行する

- 読み手が持って来るもの: 型の見取り図
- このページが渡すもの: `PostTask` のキュー、ループの順序、例外を拾う理由、`GLFW` の初期化と終了
- 読み手がすること: GL の呼び出しを描画スレッドに投げる書き方(`PostTask`)を知る
- 止まる箇所: GL の呼び出しを描画スレッド以外で行うと壊れる。ここは「してはいけないこと」として短く書く(禁止の文なので、代わりに取る行動として「`PostTask` に入れる」を続けて書く)
- 次へ進む先: 「GLView は、GL テクスチャを描画先の Surface にする」

### GLView は、GL テクスチャを描画先の Surface にする

- 読み手が持って来るもの: `PostTask` の書き方
- このページが渡すもの: `GLView` の持ち物(GLFW のウィンドウ、`GRContext`、テクスチャ、`SKSurface`)、`Resize` が再作成すること、テクスチャの原点が左下であること
- 読み手がすること: サイズ変更のとき、テクスチャのハンドルが変わることを確かめる
- 次へ進む先: 「Renderer と Present が、Layer を描いて SteamVR に渡す」

### Renderer と Present が、Layer を描いて SteamVR に渡す

- 読み手が持って来るもの: `GLView`
- このページが渡すもの: `Renderer.Render` の3段(クリア、`LayerRenderer.Render`、`Flush`)と、`OverlayWindow.Present` がテクスチャのハンドルを OpenVR に渡すこと。`DesktopWindow` との違い
- 読み手がすること: `Present` が、どのウィンドウでも同じ入口(`IEngineWindow`)であることを確かめる
- 次へ進む先: 「なぜ Layer を複製して描画スレッドに渡すのか」。OpenVR 側は `ovr-layer.md`

### なぜ Layer を複製して描画スレッドに渡すのか

- 読み手が持って来るもの: 描画層の解説で見た `Clone`
- このページが渡すもの: メインスレッドがツリーを更新し続け、描画スレッドが後から描くので、複製を渡すという理由。共有されるもの(`SKPicture` の参照)は `rendering-layer.md` へリンクする
- 読み手がすること: 複製を省く変更が、何を壊すかを判断する
- 次へ進む先: 「フレーム間隔と入力源」
- 【未決】理由の出典が AGENTS.md の1文と、コードからの推定だけ。ADR として記録を足すか、推定であると書くか

### フレーム間隔と入力源

- 読み手が持って来るもの: 描画スレッドのループ
- このページが渡すもの: `FramePacer` の待ち方(`Sleep` とスピン)と既定の 60。ポインタの入力源2種(`OverlayMouseRawPointerSource`、`GLFWRawPointerSource`)と、座標の原点の注意
- 読み手がすること: 入力の座標が上下鏡写しになる不具合の原因を、テクスチャの原点から推測できるようになる
- 止まる箇所: なし。ここでこのページは終わる。入力の続きは `framework-layer.md`
