← [コントリビューター](../Home.md)

# 入力が届くまでを追う

## 入力は、ヒットテストからジェスチャへ流れる

入力は、フレームの先頭でウィジェットに届きます。`WidgetBinding.BeginFrame` は、`DrawFrame` の前に `FlushPointerEvents` を呼びます。流れは次のとおりです。

1. `OverlayWindow.PollInputEvents` が、オーバーレイ宛の OpenVR のイベントをポーリングする。入力源が `RawPointerEvent` を発火し、`PointerController` のキューに積む。デスクトップウィンドウでは、GLFW のコールバックが描画スレッドで同じキューに積む
2. `PointerController.Flush` が、キューを取り出して `PointerEvent` に変換する。変換は `Flush` を呼んだスレッド(メインスレッド)で行う
3. `HandlePointerEvent` が、`RenderView.HitTest` でヒットしたノードのパス(ヒットパス)を求める。パスの末尾に `WidgetBinding` 自身を加える
4. ヒットパスの各ノードへイベントを配る。押下中は、押した時点のヒットパスへ、移動と離す操作を配り続ける
5. パスの末尾の `WidgetBinding.HandleEvent` が、`PointerRouter` でジェスチャ認識器へイベントを届ける。押下では `GestureArena` の参加受付を締め切り、離す操作では未確定のアリーナの既定勝者を確定する

認識器は `src/FloatSoda/Gesture/` にあります。ウィジェットとして使うのは `GestureDetector` や `Listener` です。

オーバーレイのうち、ポインターが届くのはダッシュボードだけです。エンジン層の `OverlayWindow` が入力源を作るのが、ダッシュボードのときだけだからです。ほかのオーバーレイへの接続は、Issue で追跡しています(追跡: [#182](https://github.com/sumx21t-3310/FloatSoda/issues/182))。

OpenVR のイベントがポインター入力になるまでは、[エンジン層](engine-layer.md) と [OVR 層](ovr-layer.md) の担当です。ウィジェット側の仕事は、`PointerEvent` を受け取るところから始まります。

## 関連するページ

- [フレームワーク層](framework-layer.md): Widget、Element、RenderObject と、差分更新
- [エンジン層](engine-layer.md): ポインターの入力源と、y 座標の向き
- [1フレームが SteamVR に届くまで](frame-to-steamvr.md): 入力が逆向きに流れる段の全体
