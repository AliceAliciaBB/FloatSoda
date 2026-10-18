← [コントリビューター](../Home.md)

# 1フレームが SteamVR に届くまで

## 1フレームを担う2つのスレッド

1フレームの描画は、2つのスレッドの仕事です。メインスレッドが、ウィジェットのビルドから Layer ツリーの複製までを行い、描画スレッドが、複製を GL で描いて SteamVR のオーバーレイに提出します。変更したい処理がどの層のどの段にあるかは、この流れで見つけます。

| スレッド | 名前 | 仕事 |
| :-- | :-- | :-- |
| メインスレッド | `FloatSodaMainThread`(Windows では STA) | OpenVR のイベント、入力、ビルド、レイアウト、描画命令の記録(Layer ツリーの更新)、Layer の複製 |
| 描画スレッド | `RenderThread` | GL のコンテキストを持ち、Layer ツリーをテクスチャに描いてオーバーレイに渡す |

ほかに I/O スレッド(`IOTaskRunner`)があります。利用者のコード向けに、処理を投入順に1件ずつ実行するスレッドで、フレームの流れには入りません。

```text
メインスレッド                                      描画スレッド
  タスクの実行
  OpenVR のイベント、アクション入力
  BeginFrame
    入力の処理
    ビルド → レイアウト → 描画命令の記録
    Layer の複製 ── PostRender ──────────────▶        PreUpdate: 投入されたタスクの実行
  待機                                                   (Present: Renderer.Render → OpenVR へ提出)
                                                      Update: GLFW のイベント処理
                                                      待機
```

層は、プロジェクトごとに分かれ、参照の向きは csproj が決めます。

- `FloatSoda.OVR`: ほかの FloatSoda のプロジェクトを参照しない
- `FloatSoda.Rendering`: ほかの FloatSoda のプロジェクトを参照しない
- `FloatSoda.Abstractions`: `FloatSoda.Rendering` を参照する
- `FloatSoda.Engine`: Abstractions、OVR、Rendering を参照する
- `FloatSoda`(フレームワーク): Abstractions、Engine、Rendering を参照する
- `FloatSoda.UI`: `FloatSoda` を参照する
- `FloatSoda.UI.Cream`、`FloatSoda.UI.FizzyPop`: `FloatSoda.UI` を参照する

メインループと描画ループの周期は、どちらも `FloatSodaOptions.TargetFrameRate`(既定は 60)から作った `FramePacer` が決めます。

## メインスレッド: ビルドから Layer の複製まで

メインループは、1周ごとに次の順で動きます。

1. 保留中のタスクを実行する(`CreateWindow` や、ホットリロードの再ビルドの予約)
2. OpenVR のシステムイベントをポーリングし、アクション入力を更新する
3. ウィンドウごとの `WidgetBinding.BeginFrame` を呼ぶ
4. `FramePacer.WaitForNextFrame` で次の周期まで待つ

`BeginFrame` は、ポインターイベントの処理、登録済みのフレームコールバックの実行、`DrawFrame` の順に動きます。`DrawFrame` の中の順序は、次のとおりです。各段の詳細は [フレームワーク層](framework-layer.md) にあります。

| 段 | 内容 |
| :-- | :-- |
| ビルド | `BuildOwner.BuildScope` が、dirty な Element を再構築し、RenderObject のプロパティを更新する |
| レイアウト | `RenderPipeline.FlushLayout` が、dirty なノードを `Depth` の昇順で再レイアウトする。続けて、ホバー状態を更新し、サイズが変わっていればリサイズを描画スレッドに投入する |
| 描画命令の記録 | `RenderPipeline.FlushPaint` が、dirty なノードを `Depth` の昇順で再描画し、描画命令を記録して Layer ツリーを更新する。最終画像を描くのは描画スレッド |
| 複製と投入 | `RenderView.Layer` を `Clone()` し、`RenderPostTaskRunner.PostRender` で描画スレッドに渡す |

すべてのフレームで、レイアウトと描画が走るわけではありません。ビルドは毎フレーム確かめますが、レイアウトからあとは、`NeedsVisualUpdate` が立っているフレームだけです。ウィンドウがまだ作られていないフレームも、レイアウトからあとは飛ばします。

## 描画スレッド: Layer を描いて SteamVR に渡す

描画スレッドは `PostTaskRunner` のループで動き、1周ごとに、投入されたタスクの実行、`GLFW.PollEvents`、待機の順に進みます。`PostRender` が投入するタスクは、`window.Present(layer)` を呼びます。

`OverlayWindow.Present` は、`Renderer.Render(layer)` で Layer ツリーを GL テクスチャに描きます。`Render` は、`GLView.Clear()`、`LayerRenderer.Render`(`Layout` のあとに `Paint`)、`GLView.Flush()` の順に動きます。そのあと、GL テクスチャのハンドルを `Texture_t` に包み、`Overlay.Texture.FromTexture_t` で OpenVR に渡します。GL テクスチャが OpenVR に渡るのは、この呼び出しです。デスクトップウィンドウ(`DesktopWindow`)は、同じ `Present` の流れを、テクスチャを既定のフレームバッファに転送して、`SwapBuffers` で終えます。

ウィンドウの作成にも、2つのスレッドが関わります。`FloatSodaApp.CreateWindow` は、作成の処理をメインスレッドのタスクとして積みます。`WidgetBinding.EnsureInitialized` は、`GLView` と `Renderer` と、エンジン層のウィンドウの作成を、描画スレッドのタスクとして投入します。そのため、ウィンドウは描画スレッドで非同期に作られ、メインスレッドから見えるのは、作成が終わったフレームからです。

エンジン層の詳細は [エンジン層](engine-layer.md)、Layer の描画は [描画層](rendering-layer.md)、OpenVR の型は [OVR 層](ovr-layer.md) にあります。

## なぜ Layer を複製して渡すのか

> 【未決】この節は判断が要るため、ト書きのまま残しています。オーナーが決めたあとで本文にします(追跡: #219)。

- 読み手が持って来るもの: 「複製のコードがあるが、必要なのか」という疑問
- このページが渡すもの: 複製の理由(描画中のツリーを次のフレームの変更から守る)と、複製で共有されるもの(`SKPicture` の参照)。複製の細部は描画層の解説へリンクする
- 読み手がすること: 複製を省く変更が、何を壊すかを判断する
- 次へ進む先: 「リサイズは同じ流れの別の枝」
- 【未決】複製の理由の出典が AGENTS.md の1文だけ。ADR として記録を足すか、このページで「理由」を断定せずに書くか

<details>
<summary>仮に書いた本文(判断の材料。採用は未定)</summary>

複製は、描画中の Layer を、次のフレームの変更から守るためにあります。メインスレッドは、再描画のたびに、同じ Layer のインスタンスの子を消して作り直します。描画スレッドが原本を読んでいる最中に、次のフレームの描画が始まると、読んでいる Layer が書き換わります。そこで、描画スレッドには、複製したツリーを渡します。AGENTS.md も、描画スレッドへ渡す前に複製して、データ競合を避けると説明しています。

複製が共有するのは、`SKPicture` のような描画リソースの参照です。ツリーの構造は共有しません。共有の範囲は [描画層](rendering-layer.md) の「Clone は何を共有し、何を共有しないか」にあります。複製を省く変更は、メインスレッドの次のフレームと、描画スレッドの `Present` が、同じ Layer を同時に読み書きしないかを確かめます。

<!-- ※要確認: 複製の理由の根拠は、AGENTS.md の1文と、Layer を再利用するコードの構造からの読み取りで、記録(ADR など)は見つかっていない。複製せずに所有権を渡す案や、ツリーを二重に持つ案を検討した記録の有無を、オーナーに確かめる -->

</details>

## リサイズは同じ流れの別の枝

ウィンドウの大きさは、既定ではウィジェットのレイアウト結果が決めます。`DrawFrame` は、レイアウトのあとに、サイズが前回投入した値と違うかを調べます。違えば、`PostTask` で `window.Resize(newSize)` を描画スレッドに投げます。この投入は、描画の投入より先なので、同じキューの順で、リサイズのあとに描画が走ります。

`OverlayWindow.Resize` は、`GLView` の描画先 `Surface` と GL テクスチャを作り直して、オーバーレイの幅(ピクセル幅 ÷ `Dpm`)を更新します。入力を受けるオーバーレイでは、マウスのスケールもピクセルサイズに合わせます。サイズを変える処理は、描画スレッドで走ります。

## 入力は逆向きに流れる

ダッシュボードのオーバーレイでは、コントローラーのレーザーが、次の順でボタンに届きます。

1. OpenVR のマウスイベントを、`OverlayWindow.PollInputEvents` がポーリングする
2. `OverlayMouseRawPointerSource` が、`RawPointerEvent` に変換して発火する
3. `PointerController` が、キューに積む。`Flush` で取り出し、`PointerEvent` に変換する
4. `WidgetBinding.HandlePointerEvent` が、ヒットテストを行う
5. ヒットパスの末尾で、ジェスチャ認識器にイベントが届く

1 の `PollInputEvents` も、3 の `Flush` も、メインスレッドで `BeginFrame` から呼ばれます。つまり、これらの段は、すべてメインスレッドで動きます。デスクトップウィンドウでは、GLFW のコールバックが描画スレッドで `RawPointerEvent` を発火しますが、キューに積むだけで、変換とそれ以降はメインスレッドの `Flush` が行います。

フレームワークの標準のポインター入力が接続されるのは、オーバーレイのうち、ダッシュボードだけです。ほかのオーバーレイでは、`PointerSource` が `null` です。コントローラーのレイで当たりを判定する `ControllerPointerSystem` は、どこからも呼ばれていません(追跡: [#182](https://github.com/sumx21t-3310/FloatSoda/issues/182))。

コントローラーのボタンを読むアクション入力は、別の流れです。メインループが、1周ごとに `VRInputUpdater.Update` で更新します。

ウィジェット側の入力は [フレームワーク層](framework-layer.md)、OpenVR 側は [OVR 層](ovr-layer.md) にあります。

## 各層の解説へ

変更したい層の解説に進みます。

- [フレームワーク層](framework-layer.md): Widget、Element、RenderObject と、差分更新、入力
- [描画層](rendering-layer.md): Layer ツリーと、`Clone` の共有範囲
- [エンジン層](engine-layer.md): 描画スレッド、GL の描画先、ウィンドウ
- [OVR 層](ovr-layer.md): OpenVR の包み方と、つまずきやすい点
- [UI 層](ui-layer.md): UI を3層に分けた理由

手を動かして確かめるときは、[描画が届くまで(4段)](tutorial-render-path.md) と、各層の実験([フレームワーク層](experiment-framework.md)、[描画層](experiment-rendering.md)、[OVR 層](experiment-ovr.md))を使います。
