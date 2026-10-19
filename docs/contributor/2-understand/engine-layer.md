← [コントリビューター](../Home.md)

# エンジン層

## エンジン層は、描画スレッドと GL の描画先を受け持つ

エンジン層(`FloatSoda.Engine`)は、Layer ツリーを GL テクスチャに描き、SteamVR のオーバーレイまたはデスクトップのウィンドウに出す層です。参照するプロジェクトは Abstractions、OVR、Rendering の3つで、フレームワーク(`src/FloatSoda`)は参照しません。そのため、フレームワークなしでも組めます。実際に組む手順は [描画が届くまで(4段)](tutorial-render-path.md) の3段目にあります。

型の担当は次のとおりです。

| 型 | 担当 |
| :-- | :-- |
| `RenderPostTaskRunner` | 描画専用のスレッドを持ち、投入されたタスクを順に実行する |
| `GLView` | GLFW のウィンドウ、GL のコンテキスト、Skia の描画先(Surface)と GL テクスチャを持つ |
| `Renderer` | Layer ツリーを `GLView` の Surface に描く |
| `OverlayWindow`、`DesktopWindow` | 描いた結果の出し先。どちらも `IEngineWindow` の実装 |
| `FramePacer` | ループの周期をそろえる |

次の節から、この順に1つずつ説明します。

## 描画スレッドは、キューのタスクを実行する

`RenderPostTaskRunner` は、専用スレッドを1本だけ持ちます。スレッドはバックグラウンドスレッドで、名前は作る側が渡します。フレームワークは `RenderThread` という名前で作ります。`GLView` の説明が、OpenGL リソースの作成・使用・破棄を、リソースを所有するスレッドで行うよう求めています。

- GL を使う処理は、描画スレッドで実行すること

ほかのスレッドから GL を使いたいときは、処理を `PostTask(Action)` に渡して、描画スレッドで実行させます。`PostTask` は `ConcurrentQueue<Action>` に積むだけなので、どのスレッドからでも呼べます。次の形は、フレームワークが `GLView` と `Renderer` を作る箇所と同じ生成処理だけの抜粋です。使い終わった `Renderer` と `GLView` は `Dispose` で破棄し、その呼び出しも描画スレッドで行います。

```csharp
renderThread.PostTask(() =>
{
    var renderer = new Renderer { GLView = new GLView(size) };
    // renderer を使う処理は、すべてこの描画スレッドの上で行う
});
```

描画スレッドのループは、1周ごとに次の順で動きます。

1. `PreUpdate`: キューに積まれたタスクをすべて実行する
2. `Update`: `GLFW.PollEvents` でウィンドウのイベントを処理する
3. `PostUpdate`: 既定では何もしない
4. `FramePacer` で次の周期まで待つ

ループに入る前に `GLFW.Init()` を、抜けたあとに `GLFW.Terminate()` を、どちらも描画スレッド上で呼びます。1つのタスクが例外を投げても、ほかのタスクとスレッドは動き続けます。例外はログに記録されます。ここで拾わないと、ループの外まで例外が抜けて `OnStop` が走り、以降の描画がすべて止まるためです。

ほかに、`IOTaskRunner` という専用スレッドもあります。利用者のコード向けで、投入された処理を投入順に1件ずつ実行します。フレームの描画には使いません。

## GLView は、GL テクスチャを描画先の Surface にする

`GLView` は、GL のテクスチャを描画先とする Skia の `SKSurface` を作り、持ち続けます。持ち物は次のとおりです。

- GLFW のウィンドウ
- GL のコンテキストと、Skia の `GRContext`
- 描画結果を保持する GL の 2D テクスチャ(`TextureHandle`)
- テクスチャを描画先とする `SKSurface`(`Surface`)

ウィンドウは、既定では画面に表示されません。表示の有無は、コンストラクターの `visible` 引数で指定します。

- 表示の有無は、作成時に決めること(作成後は変更できない)

`Surface` は、原点の指定を `GRSurfaceOrigin.BottomLeft` にして作ります。この向きは、最後の節で説明する入力の座標に関わります。

`Resize` は、既存の `Surface` とテクスチャを破棄して作り直します。サイズが同じなら何もしません。幅か高さが 0 以下なら `ArgumentOutOfRangeException` です。作り直すとテクスチャのハンドルが変わるので、ハンドルを使う処理は、`Resize` のあとに `Renderer.GetTextureHandle()` で取り直す必要があります。`OverlayWindow.Present` は、描くたびに取り直しています。

## Renderer と Present が、Layer を描いて SteamVR に渡す

`Renderer.Render(ILayer)` は、次の3段で Layer ツリーを描きます。

1. `GLView.Clear()` で、描画先を透明色でクリアする
2. `LayerRenderer.Render` で、`Layout` のあとに `Paint` を実行する(詳細は [描画層](rendering-layer.md))
3. `GLView.Flush()` で、Skia と GL に保留されたコマンドを送る

描いた結果を出す入口は、`IEngineWindow`(Abstractions)の `Present(ILayer)` です。`RenderPostTaskRunner.PostRender(window, layer)` は、`window.Present(layer)` を描画スレッドで呼ぶタスクを投入します。呼ぶ側が知っている型は `IEngineWindow` だけなので、ウィンドウの種類が変わっても入口は変わりません。

- `Present` と `Resize` は、描画スレッドで呼ぶこと

`OverlayWindow` と `DesktopWindow` は、`Present` の後半が違います。

| 型 | `Present` の後半 | `Resize` |
| :-- | :-- | :-- |
| `OverlayWindow` | GL テクスチャのハンドルを `Texture_t` に包み、`Overlay.Texture.FromTexture_t` で OpenVR に渡す | テクスチャを作り直し、オーバーレイの幅(ピクセル幅 ÷ `Dpm`)を更新する |
| `DesktopWindow` | テクスチャを既定のフレームバッファに転送し、`SwapBuffers` を呼ぶ | テクスチャを作り直し、GLFW のウィンドウサイズを変える |

`OverlayWindow` を作るとき、オーバーレイがダッシュボードなら、入力方式をマウスにして `OverlayMouseRawPointerSource` を作ります。ほかのオーバーレイでは、`PointerSource` は `null` です。OpenVR 側の型は [OVR 層](ovr-layer.md) で説明しています。

## なぜ Layer を複製して描画スレッドに渡すのか

メインスレッドは、Layer ツリーの原本を作り直し続けます。`PaintingContext.RepaintCompositedChild` は、再描画のたびに既存の `TransformLayer` を再利用し、`Children` を消してから子を描き直します。`WidgetBinding.DrawFrame` は、この原本を `Clone()` して `PostRender` に渡します。原本を直接渡すと、描画スレッドが読んでいる最中に、メインスレッドが次のフレームで `Children` を消す可能性があるためです。これはコードの構造から読んだ推定で、複製を省いて実際に描画が崩れるかは再現していません。AGENTS.md も、データ競合を避けるために描画スレッドへ渡す前に複製する、と説明しています。

複製しても共有されるものがあります。それが `SKPicture` の参照です。何を共有して何を共有しないかは、[描画層](rendering-layer.md) の「Clone は何を共有し、何を共有しないか」にあります。複製を省く変更を入れるときは、メインスレッドの次のフレームと描画スレッドの `Present` が、同じ Layer を同時に読み書きしないかを確かめます。

<!-- ※要確認: 複製を省いたときの実害は、キューと Layer の再利用の構造からの推論で、実行して再現していない / 複製を省く変更を試して、描画が崩れるかを確かめる -->

## フレーム間隔と入力源

`FramePacer` は、目標のフレームレート(既定は 60)から1フレームの時間を求め、前回からの経過時間との差だけ待ちます。待ち方は2段です。残りが長いあいだは `Sleep` で CPU を空け、最後の1ミリ秒ほどは `Thread.Yield` で回って待ちます。

ポインターの入力源は2つあります。どちらも `RawPointerEvent` を発火し、フレームワーク側の `PointerController` が受け取ります。

| 型 | 変換元 | イベントを発火するスレッド |
| :-- | :-- | :-- |
| `OverlayMouseRawPointerSource` | OpenVR のマウスイベント(ダッシュボードのレーザーポインター) | `PollEvents` を呼んだスレッド |
| `GLFWRawPointerSource` | GLFW のカーソルとボタンのコールバック | GLFW を動かしている描画スレッド |

`OverlayMouseRawPointerSource` は、OpenVR のマウス座標をそのまま使い、y 座標を反転しません。クラスのコメントは、y 座標を反転せずに左上原点の入力として扱う理由を、実測で確認済みと説明しています。この文書の筆者は再現していません。y を反転すると、入力の位置が上下に鏡写しになります。画像は正しいのに押した位置だけが上下逆になる不具合では、y の反転や、`GRSurfaceOrigin` を変える変更を疑ってください。

<!-- ※要確認: 座標の向きは、ソースのコメント(実測で確認済み)だけが根拠で、こちらでは再現していない / HMD で、反転した場合のずれを確かめる -->

ポインターの入力が、ヒットテストとジェスチャを経てウィジェットに届くまでは [入力が届くまでを追う](framework-input.md) にあります。1フレーム全体の流れは [1フレームが SteamVR に届くまで](frame-to-steamvr.md) にまとめています。
