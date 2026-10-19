← [コントリビューター](../Home.md)

# 描画が届くまで(4段)

> このページは執筆前の骨組みです。設計シートとト書きだけを置き、本文はまだ書いていません(追跡: #219)。

## 設計シート

- 分類: 説明型(Diátaxis: tutorial)
- 読み手: だれが / FloatSoda の内部に関わりたい人。C# と SkiaSharp の基本は分かる / 何を知っている / `dotnet` でプロジェクトを作って動かせる。SteamVR を起動できる。Widget・Element・RenderObject は知らなくてよい / 読んで何をしたい / 層を1つずつ足しながら、描画が SteamVR のオーバーレイに届く道筋を、手を動かして確かめたい
- 出力言語と文体の指定: 日本語。敬体(CONTRIBUTING.md の「ドキュメントのトンマナ」の「文体」)
- テーマ: OVR 層だけでオーバーレイを出すところから始め、描画層、エンジン層、フレームワーク層の順に層を足して、描画が HMD の中に届くまでを4段で作る
- 依頼の解釈: なし
- テーマファイル: tutorial
- Why(目的): 層をまたぐ流れは読むだけでは掴みにくいので、1層ずつ足して「この層がなければ何が足りないか」を体験してもらう。コントリビューターの入口を兼ねる(docs 再編の決定、2026-10-16)
- What(目標・結果): 段ごとに、HMD の中か png の画像で目に見える結果が1つある。最後の段で、フレームワークの `FloatSodaApp` が、段3で自分が書いたものと同じ仕事をしていると分かる
- How(手段): プロジェクトの依存の向き(OVR → Rendering → Engine → フレームワーク)と同じ順に、4段で層を足す。各段に「できた?」のチェックと、選択式のクイズを1問置く。仕組みの説明は層の解説へリンクする
- 要素:
  - 依存の向きは csproj の `ProjectReference` で決まる。OVR は参照なし、Rendering は参照なし(SkiaSharp のみ)、Engine は Abstractions・OVR・Rendering を参照し、フレームワーク(`src/FloatSoda`)は Abstractions・Engine・Rendering を参照する(`src/FloatSoda.OVR/FloatSoda.OVR.csproj`、`src/FloatSoda.Rendering/FloatSoda.Rendering.csproj`、`src/FloatSoda.Engine/FloatSoda.Engine.csproj`、`src/FloatSoda/FloatSoda.csproj`)
  - 段1は OVR 層だけを使う。`OVRApplication` で SteamVR に接続し、`OverlayIdentity` と `DeviceTrackedOverlay` を作り、`overlay.Texture.FromFile` で png を読んで `overlay.Visibility.Show()` で出す(`samples/FloatSoda.Samples.PrimitiveOverlay/Program.cs` の 6、16〜17、25〜26 行目)
  - 段1の png は、デスクトップの `layer_tree_output.png` を読む(`samples/FloatSoda.Samples.PrimitiveOverlay/Program.cs` の 22〜25 行目)。このファイルは `samples/FloatSoda.Samples.PaintingSample` の出力である(`samples/FloatSoda.Samples.PaintingSample/Program.cs` の 23 行目)
  - HMD が接続されていない PC で `OVRApplication` を初期化すると、`Init_HmdNotFound` で失敗した(2026-10-17、1台で観測。試したコードは `Exp3Probe`)。※要確認
  - 段2は描画層だけを使う。`ContainerLayer` に `PictureLayer` を足して Layer ツリーを組み、`LayerBitmapRenderer.Render(root, SKSizeI)` で `SKBitmap` にして png で保存する(`src/FloatSoda.Rendering/LayerBitmapRenderer.cs` の 34 行目、`src/FloatSoda.Rendering/Layers/ContainerLayer.cs` の 17 行目、`src/FloatSoda.Rendering/Layers/PictureLayer.cs`)
  - `LayerBitmapRenderer` は `FloatSoda.Rendering` にある。`FloatSoda.Testing` にあるのは `RenderObjectBitmapRenderer` と `WidgetBitmapRenderer` である(`src/FloatSoda.Rendering/LayerBitmapRenderer.cs`、`src/FloatSoda.Testing/`)
  - `LayerBitmapRenderer.Render` は `LayerRenderer.Render` を呼び、その中で `Layout` のあとに `Paint` を実行する(`src/FloatSoda.Rendering/LayerBitmapRenderer.cs` の 47 行目、`src/FloatSoda.Rendering/LayerRenderer.cs` の 33〜34 行目)
  - 段2のコードは 22 行で動き、png を出力できた(2026-10-17 に実行を確かめた。試したコードは `Exp2LayerPng`)。出力 png は段1で表示できる
  - 段3はエンジン層を使う。`GLView` を作り、`Renderer` に渡し、`OverlayWindow(overlay, renderer, dpm)` を作り、`RenderPostTaskRunner.PostRender(window, layer)` で Layer ツリーを描画スレッドに渡す(`src/FloatSoda.Engine/GLView.cs` の 9 行目、`src/FloatSoda.Engine/Renderer.cs` の 27 行目、`src/FloatSoda.Engine/OverlayWindow.cs` の 29 行目、`src/FloatSoda.Engine/TaskRunner.cs` の 562 行目)
  - `GLView`・`Renderer`・`OverlayWindow` は描画スレッドで作る。フレームワークは `PostTask` の中で `GLView` と `Renderer` を作っている(`src/FloatSoda.Engine/GLView.cs` の 9 行目、`src/FloatSoda/Core/WidgetBinding.cs` の 110〜119 行目)
  - `OverlayWindow.Present` は `Renderer.Render` のあと、GL テクスチャのハンドルを `Texture_t` に包んで `Overlay.Texture.FromTexture_t` に渡す(`src/FloatSoda.Engine/OverlayWindow.cs` の 50〜62 行目)
  - 段3の使う型は、`FloatSoda.Engine` の参照がフレームワークを含まないので、フレームワークなしで書ける見込みである。コンパイルと実機の実行は未確認(docs 再編の決定、2026-10-17)。※要確認
  - 段3の `OverlayWindow`(`FloatSoda.Engine`)と、フレームワークの `FloatSoda.Widgets.OverlayWindow` は同じ名前である。段4で同じプロジェクトに足すと曖昧になる。フレームワークは `using EngineOverlayWindow = FloatSoda.Engine.OverlayWindow;` と別名を付けている(`src/FloatSoda/FloatSodaApp.cs` の 13 行目、`src/FloatSoda/Widgets/WindowWidget.cs` の 64 行目)。`DesktopWindow` も同様に重なる(`src/FloatSoda.Engine/DesktopWindow.cs` の 14 行目、`src/FloatSoda/Widgets/WindowWidget.cs` の 158 行目)
  - 段4はフレームワーク層を使う。`Host.CreateApplicationBuilder`、`AddFloatSoda()`、`FloatSodaApp.CreateWindow(new DashboardWindow { ... })` で起動し、`Align` → `SizedBox` → `ColoredBox` で色の四角を出す(`samples/FloatSoda.Samples.GettingStarted/Program.cs` の 11〜12、17〜35 行目)
  - `FloatSodaApp.CreateWindow` が、段3で書いた `Renderer`・`OverlayWindow` の作成を `WidgetBinding.EnsureInitialized` 経由で行う(`src/FloatSoda/FloatSodaApp.cs` の 98〜113 行目、`src/FloatSoda/Core/WidgetBinding.cs` の 100〜125 行目)
  - `WidgetBinding.DrawFrame` が、Layer ツリーを複製して `PostRender` に渡す。段3で自分が書いた呼び出しに当たる(`src/FloatSoda/Core/WidgetBinding.cs` の 199〜201 行目)
  - 段の形: 各段を目に見える結果で終わらせ、確かめる点を1つだけ置き、「できた?」でチェックする(docs 再編の決定、2026-10-16〜17)
  - 各段の終わりに、選択式のクイズを置く(docs 再編の決定、2026-10-16。コントリビューター向けのチュートリアルへの追加)
  - 実験のページ(`experiment-ovr.md`、`experiment-rendering.md`)は、段1・段2と別に残す(docs 再編の決定、2026-10-17)
- 見出し:
  - このチュートリアルで作るもの
  - 始める前に用意するもの
  - 1. OVR 層だけでオーバーレイを出す
  - 2. Layer ツリーを組んで png に描く
  - 3. Layer ツリーをエンジン層に差し込んで SteamVR に出す
  - 4. ウィジェットで描く
  - 次に読むページ
- ※要確認:
  - 段3のコードが、フレームワークなしでコンパイルでき、実機で絵が出るか(最小のコード約 50 行を書いてビルドし、HMD で確かめる)
  - 段1の png の入手方法。段1の時点では段2の出力がまだない(下の【未決】)
  - HMD がない PC で、段1・段3・段4のどこまで進めるか(段2は HMD なしで動く)
  - `Init_HmdNotFound` が、`OpenVR.IsHmdPresent()` が True でも起きるか。1台で観測しただけである
  - 段4で `FloatSoda.Widgets.OverlayWindow` を使うとき、段3のコードを同じプロジェクトに残してよいか(別名を付ければ共存できる見込み。ビルドして確かめる)
  - 選択式のクイズの設問と選択肢を、実際に通して確かめる

## ト書き

### このチュートリアルで作るもの

- 読み手が持って来るもの: 「描画が SteamVR に届くまでを、手を動かして確かめたい」という目的だけ
- このページが渡すもの: 4段の予告(OVR → 描画 → エンジン → フレームワークの順に層を足す)と、各段の終わりに何が見えるか。層の依存の向きの図
- 読み手がすること: 4段の流れをつかむ
- 次へ進む先: 「始める前に用意するもの」

### 始める前に用意するもの

- 読み手が持って来るもの: 自分の PC
- このページが渡すもの: 要るものの短い一覧(.NET の SDK、リポジトリの clone、SteamVR と HMD)と、準備のページへのリンク(`../3-prepare/dev-environment.md`)。段2だけは HMD なしで進められると伝える
- 読み手がすること: 一覧を確かめ、足りなければリンク先で準備してから戻る
- 止まる箇所: SteamVR が起動できない、HMD がつながらない。ここでは直し方を書かず、リンク先へ送る
- 次へ進む先: 段1

### 1. OVR 層だけでオーバーレイを出す

- 読み手が持って来るもの: 空のコンソールアプリと、`FloatSoda.OVR` へのプロジェクト参照
- このページが渡すもの: `OVRApplication`、`OverlayIdentity`、`DeviceTrackedOverlay`、`Texture.FromFile`、`Visibility.Show` の最小のコード。png を1枚用意する方法
- 読み手がすること: コードを貼って `dotnet run` し、HMD の中でオーバーレイを探す
- できた?: HMD の 1 m 前に付いたオーバーレイに、png の絵が出た(`Target` の既定が HMD のため。※要確認)
- できなかったら: `Init_HmdNotFound` が出る、オーバーレイが見つからない → OVR 層の解説の「落とし穴」の節、SteamVR の状態の確認
- クイズ: 段1のオーバーレイの絵は、だれが描いたか。(a) Skia (b) OpenVR が png を読んで表示した (c) OpenGL。正解は (b)。`overlay.Texture.FromFile` が `SetOverlayFromFile` を呼ぶため(`src/FloatSoda.OVR/Overlay/Capability.cs` の 190 行目)
- 次へ進む先: 段2。OVR 層の詳細は `ovr-layer.md` へリンクする
- 【未決】段1の png を、段2を先に済ませて作る順にするか、固定の画像を `docs/assets/` に置いて使うか。段の順(依存の向き)は変えない

### 2. Layer ツリーを組んで png に描く

- 読み手が持って来るもの: 段1で動いたコード(png を読んで出す)
- このページが渡すもの: `ContainerLayer`・`PictureLayer`・`SKPictureRecorder` で Layer ツリーを組み、`LayerBitmapRenderer.Render` で `SKBitmap` にして png で保存するコード。`Layout` のあとに `Paint` が走ること
- 読み手がすること: 別のコンソールアプリでコードを動かし、できた png を段1のオーバーレイで表示する
- できた?: 自分で組んだ Layer ツリーの絵が、HMD の中のオーバーレイに出た
- できなかったら: png が空、真っ黒 → 描画層の解説の「Layer の種類」の節。オーバーレイが出ない → 段1に戻る
- クイズ: `LayerBitmapRenderer.Render` が `root` に対して最初に呼ぶのはどれか。(a) `Paint` (b) `Layout` (c) `Clone`。正解は (b)(`src/FloatSoda.Rendering/LayerRenderer.cs` の 33〜34 行目)
- 次へ進む先: 段3。Layer の詳細は `rendering-layer.md` へリンクする
- 【未決】`samples/FloatSoda.Samples.PaintingSample` と段2が同じことをしている。段2の本文を書くときに、PaintingSample を案内するか、段2で別のコードを書かせるかを決める(docs 再編の決定、2026-10-17)

### 3. Layer ツリーをエンジン層に差し込んで SteamVR に出す

- 読み手が持って来るもの: 段2で組んだ Layer ツリーと、段1で作ったオーバーレイ
- このページが渡すもの: 描画スレッドで `GLView` と `Renderer` を作り、`OverlayWindow` に包み、`RenderPostTaskRunner` で `PostRender` するコード。png ではなく、GL テクスチャ経由で絵が出ること。`FramePacer` が描画ループの間隔を決めること
- 読み手がすること: 段1の `FromFile` を、エンジン層を使うコードに置き換えて動かす
- できた?: 段1と同じ絵が、`FromFile` を呼ばずにオーバーレイに出た
- できなかったら: 絵が出ない、例外が出る → エンジン層の解説の「スレッド」の節。`GLView` を描画スレッド以外で作っていないかを見る
- 止まる箇所: `OverlayWindow` の名前が、フレームワークの型と重なること。段4に進む前に、別名の付け方を示す
- クイズ: GL テクスチャのハンドルを `Texture_t` に包んでオーバーレイに渡すのは、どの型か。(a) `GLView` (b) `Renderer` (c) `OverlayWindow`。正解は (c)(`src/FloatSoda.Engine/OverlayWindow.cs` の 54〜61 行目)
- 次へ進む先: 段4。エンジン層の詳細は `engine-layer.md` へリンクする
- 【未決】段3を、フレームワークなしで書けるかの確認が終わるまで、本文に進めない。確認の結果、書けなければ段3を解説に回すか決める

### 4. ウィジェットで描く

- 読み手が持って来るもの: 段3で動いた、手書きのエンジン層のコード
- このページが渡すもの: `Host.CreateApplicationBuilder`、`AddFloatSoda()`、`CreateWindow(new DashboardWindow { ... })` の最小のコード。段3で自分が書いた仕事を、`FloatSodaApp` と `WidgetBinding` が受け持つことの対応表
- 読み手がすること: 段3のコードを置き換えて動かし、対応表で段3の各行がどこへ移ったかを見つける
- できた?: ダッシュボードに色の四角が出た
- できなかったら: 起動で例外が出る、四角が出ない → 利用者向けのガイド「困ったとき」
- クイズ: 段3で自分が書いた `PostRender` の呼び出しは、フレームワークでは何が呼んでいるか。(a) `FloatSodaApp.CreateWindow` (b) `WidgetBinding.DrawFrame` (c) `FramePacer`。正解は (b)(`src/FloatSoda/Core/WidgetBinding.cs` の 199〜201 行目)
- 次へ進む先: 「次に読むページ」。フレームワークの詳細は `framework-layer.md` へリンクする

### 次に読むページ

- 読み手が持って来るもの: 4段を通した経験と、「次はどこを読むか」という疑問
- このページが渡すもの: 目的別の行き先。層をまたぐ流れを読む(`frame-to-steamvr.md`)、層ごとの解説を読む、実験で確かめる(`experiment-framework.md`、`experiment-rendering.md`、`experiment-ovr.md`)、準備に進む(`../3-prepare/dev-environment.md`)
- 読み手がすること: どれかを選んで進む
- 止まる箇所: なし。ここでチュートリアルは終わる
