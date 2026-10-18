← [コントリビューター](../Home.md)

# 実験: OVR 層

> このページは執筆前の骨組みです。設計シートとト書きだけを置き、本文はまだ書いていません(追跡: #219)。

## 設計シート

- 分類: 説明型(Diátaxis: tutorial。実験のページは、動作原理の問いを1つ立てて、動かして確かめる形で扱う)
- 読み手: だれが / `ovr-layer.md` を読んで、OVR 層だけで動かしてみたくなった人 / 何を知っている / C# のコンソールアプリを作って動かせる。SteamVR と HMD が使える / 読んで何をしたい / FloatSoda のほかのプロジェクトなしで、OVR 層だけでオーバーレイを出し、レーザーポインターの入力を受けられることを確かめたい
- 出力言語と文体の指定: 日本語。敬体(CONTRIBUTING.md の「ドキュメントのトンマナ」の「文体」)
- テーマ: `FloatSoda.OVR` だけを参照したコンソールアプリで、オーバーレイを出し、ホバー、クリック、移動のイベントと振動を確かめる
- 依頼の解釈: なし
- テーマファイル: tutorial
- Why(目的): 「OVR 層は、ほかの層なしで動くか」を、動かして確かめる。本体の説明は層の解説(`ovr-layer.md`)に置く
- What(目標・結果): HMD の 1 m 前に付いたオーバーレイに画像が出て、レーザーポインターのイベントがコンソールに出る。ホバーとクリックで振動する
- How(手段): 問いを立てる → サンプルを動かす → HMD の中で操作する → ログを読む → なぜそうなるかを1〜2段落で読む。サンプルは書きかけなので、動かない点も正直に書く
- 要素:
  - 問い: OVR 層だけで、オーバーレイを出して入力を受けられるか
  - `FloatSoda.OVR` のプロジェクトは、ほかのプロジェクトを参照しない(`src/FloatSoda.OVR/FloatSoda.OVR.csproj`)。サンプルのプロジェクトは `FloatSoda.OVR` だけを参照する(`samples/FloatSoda.Samples.PrimitiveOverlay/FloatSoda.Samples.PrimitiveOverlay.csproj`)
  - サンプルは `samples/FloatSoda.Samples.PrimitiveOverlay` にある。書きかけである
  - 流れ: `OVRApplication` を作る → `VRSystemEventDispatcher` で `VREvent_Quit` を待つ → `OverlayIdentity` と `DeviceTrackedOverlay` を作る → 位置を `(0, 0, -1)` にする → デスクトップの `layer_tree_output.png` を `Texture.FromFile` で読む → `Visibility.Show()` で出す(`samples/FloatSoda.Samples.PrimitiveOverlay/Program.cs` の 6〜26 行目)
  - 入力: `Input.Method` をマウスにし、`MakeOverlaysInteractiveIfVisible` を有効にする。`FocusEnter` と `FocusLeave` で振動させ、`MouseButtonDown`・`MouseButtonUp`・`MouseMove` をコンソールに出す(`samples/FloatSoda.Samples.PrimitiveOverlay/Program.cs` の 29〜58 行目)
  - メインループは、ホバー中に `hovered` を出し続け、2つのディスパッチャーの `PollEvents` を呼び、`1000 / 72` ミリ秒眠る(`samples/FloatSoda.Samples.PrimitiveOverlay/Program.cs` の 60〜71 行目)
  - 入力の画像は、デスクトップの `layer_tree_output.png` である。`samples/FloatSoda.Samples.PaintingSample` の出力で、ない場合は先に `PaintingSample` を動かす(`samples/FloatSoda.Samples.PrimitiveOverlay/Program.cs` の 22〜25 行目、`samples/FloatSoda.Samples.PaintingSample/Program.cs` の 23 行目)
  - ビルドは通る(2026-10-17 に確かめた)
  - HMD がない PC では、`OVRApplication` の初期化で `Init_HmdNotFound` になる(2026-10-17 に1台で観測した)。この実験は HMD が要る
  - サンプルの書きかけの点(読んで気づいたこと): 画像の読み込み先がデスクトップ固定、`hovered` が毎周出力される、例外が起きると `app.Dispose()` に届かない、`Identify()` を呼ばないのでアプリケーションマニフェストと結びつかない。※要確認(どれを直すか、オーナーが決める)
  - 実機で動かした記録は、まだない。※要確認
  - 仕組み: 絵は OpenVR が画像ファイルから読み、入力は `VREvent_*` のイベントとして `PollEvents` で受ける。ポインターの扱いは、フレームワークが `OverlayMouseRawPointerSource` で包んでいる(`src/FloatSoda.OVR/Overlay/Capability.cs` の 186〜196、253〜283 行目、`src/FloatSoda.Engine/OverlayMouseRawPointerSource.cs`)
- 見出し:
  - 問い: OVR 層だけで、オーバーレイと入力が扱えるか
  - 用意するもの
  - サンプルを動かす
  - HMD の中で操作する
  - ログと振動を読む
  - なぜそうなるか
  - 次に読むもの
- ※要確認:
  - サンプルを実機で動かして、絵、入力、振動が出るか(HMD で確かめる)
  - サンプルの書きかけの範囲と、本文に載せる前に直すか
  - `layer_tree_output.png` の入手手順(`PaintingSample` を先に動かす手順を書くか)
  - `DeviceTrackedOverlay` の追従先は、既定で HMD である(`src/FloatSoda.OVR/Overlay/Capability.cs` の 155 行目)。サンプルは `Target` を設定せず、位置を `(0, 0, -1)` にするので、HMD の 1 m 前に付く見込み。実機で見え方を確かめる
  - `Init_HmdNotFound` の再現条件

## ト書き

### 問い: OVR 層だけで、オーバーレイと入力が扱えるか

- 読み手が持って来るもの: `ovr-layer.md` を読んだ記憶
- このページが渡すもの: 問い1つと、完成形(HMD の 1 m 前のオーバーレイに画像が出る)の予告。予想を立てる場所
- 読み手がすること: 予想を決める
- 次へ進む先: 「用意するもの」

### 用意するもの

- 読み手が持って来るもの: SteamVR と HMD
- このページが渡すもの: リポジトリの clone、`layer_tree_output.png` を先に作る手順(`PaintingSample` を1回動かす)、HMD が要ること
- 読み手がすること: 画像を作る
- 止まる箇所: HMD がない場合は、この実験を飛ばして `experiment-rendering.md` へ送る
- 次へ進む先: 「サンプルを動かす」

### サンプルを動かす

- 読み手が持って来るもの: デスクトップの画像
- このページが渡すもの: `dotnet run --project samples/FloatSoda.Samples.PrimitiveOverlay` の1行と、コードの流れの短い説明
- 読み手がすること: SteamVR を起動して、サンプルを動かす
- できた?: コンソールにエラーが出ずに、待機状態になった
- できなかったら: `Init_HmdNotFound` → SteamVR の状態と HMD の接続を確かめる。`ovr-layer.md` の「つまずきやすい点」へリンクする
- 次へ進む先: 「HMD の中で操作する」

### HMD の中で操作する

- 読み手が持って来るもの: 動いているサンプル
- このページが渡すもの: 見る場所(正面の 1 m 先のオーバーレイ)と、やること(レーザーを当てる、クリックする)
- 読み手がすること: 正面の 1 m 先(HMD に追従する位置)でオーバーレイを探し、レーザーを当ててクリックする
- できた?: 画像が見え、ホバーとクリックで振動した
- できなかったら: オーバーレイが見えない → 位置と追跡対象のデバイス(`Transform`)を確かめる。ホバーしない → `Input.Method` と `MakeOverlaysInteractiveIfVisible` を確かめる
- 次へ進む先: 「ログと振動を読む」

### ログと振動を読む

- 読み手が持って来るもの: 操作の結果
- このページが渡すもの: コンソールの `クリック: (x, y)` と `移動: (x, y)` の見方、座標の範囲
- 読み手がすること: 座標とイベントの順序を見る
- できた?: クリックした位置の座標が出た
- 次へ進む先: 「なぜそうなるか」

### なぜそうなるか

- 読み手が持って来るもの: ログ
- このページが渡すもの: 1〜2段落の説明。OVR 層だけで、画像の提出(`FromFile`)と入力の受信(イベントのポーリング)ができること。描画とレイアウトは、上の層(描画層、エンジン層、フレームワーク層)が足すこと。詳細は `ovr-layer.md` へリンクする
- 読み手がすること: 上の層が足すものを、`tutorial-render-path.md` で確かめる
- 次へ進む先: 「次に読むもの」

### 次に読むもの

- 読み手が持って来るもの: 実験の結果
- このページが渡すもの: `ovr-layer.md`、`tutorial-render-path.md`、`experiment-rendering.md` へのリンク
- 読み手がすること: 続きを選ぶ
- 止まる箇所: なし。ここでこのページは終わる
- 【未決】サンプルが書きかけのまま実験のページを書くか、先にサンプルを直すか。直す場合の範囲(画像の読み込み先、ログの量、後始末)
