← [コントリビューター](../Home.md)

# フレームワーク層

> このページは執筆前の骨組みです。設計シートとト書きだけを置き、本文はまだ書いていません(追跡: #219)。

## 設計シート

- 分類: 説明型(Diátaxis: explanation)
- 読み手: だれが / `src/FloatSoda` のコードを読む、直す、足したい人。Flutter を知っている人を含む / 何を知っている / C# と宣言的 UI の基本。Flutter の Widget・Element・RenderObject は知っていても知らなくてもよい / 読んで何をしたい / 三つのツリーと Layer のつながり、差分更新の仕組み、入力がウィジェットに届くまでをつかみ、変更がどの部品に入るかを判断したい
- 出力言語と文体の指定: 日本語。敬体(CONTRIBUTING.md の「ドキュメントのトンマナ」の「文体」)
- テーマ: フレームワーク層(`FloatSoda` プロジェクト)が、Widget・Element・RenderObject の三つのツリーでどう UI を組み、変更をどこから描き直すかを説明する
- 依頼の解釈: なし
- テーマファイル: explanation
- Why(目的): ツリーごとの役割を知らないと、プロパティを変えても描き直されない、または毎フレーム描き直される変更を入れてしまう
- What(目標・結果): 4つのツリー(Widget、Element、RenderObject、Layer)の役割と、dirty がどこで止まり、どの単位で処理されるかが説明されている
- How(手段): 構成要素で分解する。ツリーごとの役割 → ビルド(Element の更新) → レイアウトと描画の差分更新 → 入力の順に説明する。Flutter と同じところは短く、違うところは理由つきで書く
- 要素:
  - FloatSoda は Flutter の三つのツリーを写し、描画の結果として Layer ツリーを作る(AGENTS.md の「Architecture: Three Trees」)
  - Widget は不変の `abstract record` で、`CreateElement()` と任意の `Key` を持つ(`src/FloatSoda/Widgets/Widget.cs` の 9 行目、`CreateElement` の宣言)
  - `Widget.CanUpdate` は、実行時の型が同じで `Key` が等しいときに `true` を返す(`src/FloatSoda/Widgets/Widget.cs` の 26〜29 行目)
  - `Element.UpdateChild` は、`CanUpdate` の前に、古いウィジェットと新しいウィジェットが `record` として等しければ、更新を飛ばす(`src/FloatSoda/Elements/Element.cs` の 200〜204 行目)
  - `State.SetState` は、渡された処理を実行し、`Element.MarkNeedsBuild()` を呼ぶ(`src/FloatSoda/Widgets/ComponentWidget.cs` の 86〜89 行目)
  - `MarkNeedsBuild` は、Element を dirty にして `BuildOwner` の待ちリストに登録する(`src/FloatSoda/Elements/Element.cs` の 326〜332 行目)
  - `BuildOwner.BuildScope` は、dirty な Element を `Depth` の順(親が先)に並べて再構築する。再構築中に増えた dirty も同じ呼び出しで処理する(`src/FloatSoda/Elements/BuildOwner.cs` の 96〜135 行目)
  - `WidgetBinding` がウィンドウごとに1つあり、`BuildOwner` と `RenderPipeline` を持つ(`src/FloatSoda/Core/WidgetBinding.cs` の 46〜47 行目、120〜125 行目)。※要確認(持ち物の一覧を確かめる)
  - `RenderObject.MarkNeedsLayout` は、最も近い再レイアウト境界まで伝播し、その境界を `RenderPipeline.NodesNeedingLayout` に登録する(`src/FloatSoda/RenderObjects/RenderObject.cs` の 144〜155 行目)
  - `RenderObject.MarkNeedsPaint` は、再描画境界なら自分を `NodesNeedingPaint` に登録して `RequestVisualUpdate` を呼び、境界でなければ親へ伝播する(`src/FloatSoda/RenderObjects/RenderObject.cs` の 258〜273 行目)
  - `RepaintBoundary` ウィジェットが `RenderRepaintBoundary` を作り、`IsRepaintBoundary` が真になる(`src/FloatSoda/Widgets/Paint/RepaintBoundary.cs` の 9 行目、`src/FloatSoda/RenderObjects/Painting/RenderRepaintBoundary.cs` の 6 行目)
  - `RenderPipeline.FlushLayout` と `FlushPaint` は、dirty なノードを `Depth` の昇順で処理する。`FlushPaint` は開始時点の待ちリストを空にしてから処理する(`src/FloatSoda/Core/RenderPipeline.cs` の 35〜66 行目)
  - `PaintingContext.RepaintCompositedChild` は、境界ノードの `Layer`(`TransformLayer`)を再利用して子を消去し、その境界以下を描き直す(`src/FloatSoda/RenderObjects/PaintingContext.cs` の 265〜281 行目)
  - 実験で、`SetState` のあと `BuildScope` を実行すると、変えた側の `RepaintBoundary` だけが `NodesNeedingPaint` に入り、`FlushPaint` で空になった(2026-10-17 に実行を確かめた。`Exp1Repaint`)。実験は `experiment-framework.md` に置く
  - `PaintingContext` は `PushOpacity`・`PushTransform`・`PushClipRect` などで、合成用の Layer を作る(`src/FloatSoda/RenderObjects/PaintingContext.cs` の 68〜121 行目)。Layer の種類は描画層の解説(`rendering-layer.md`)に置く
  - ウィンドウの種類は `WindowWidget` から派生する。`DashboardWindow`・`WorldSpaceWindow`・`DeviceTrackedWindow`(`OverlayWindow` の派生)と、`DesktopWindow` である(`src/FloatSoda/Widgets/WindowWidget.cs` の 16、64、97、107、131、158 行目)
  - 入力: `WidgetBinding.FlushPointerEvents` が `PointerController` を作り、`HandlePointerEvent` で `RenderView.HitTest` を実行して、ヒットしたものへイベントを配送する(`src/FloatSoda/Core/WidgetBinding.cs` の 267〜290 行目、363〜380 行目)
  - ジェスチャは、`GestureArenaManager` と `PointerRouter` がヒットパスの末尾で調停する(`src/FloatSoda/Core/WidgetBinding.cs` の 41、44、394〜402 行目、`src/FloatSoda/Gesture/`)
  - ポインタが届くのはダッシュボードのオーバーレイだけである(`src/FloatSoda.Engine/OverlayWindow.cs` の 35〜42 行目)。追跡: #182
  - 違うところ: Flutter と違い、非活性の Element を保持するプールがなく、`Deactivate` は終端(unmount 相当)である。keyed の子を別の親へ移すと `State` は破棄される(REVIEW.md の「ツリーの所有権とライフサイクル」の節、`.agents/skills/floatsoda-device-test-gen/references/known-divergences.md` の 2 番)
  - 違うところ: Semantics ツリーは移植しない(ADR は `adr-index.md` に置く)。※要確認(ADR の番号と置き場所は、`adr-index.md` の【未決】が決まってから書く)
  - 違うところ: Widget が `record` なので、同じ値のウィジェットの更新を飛ばす(AGENTS.md の「Widget/Element Tree」)
  - フック(`src/FloatSoda.Hooks`)は、ビルドの流れに統合されていない(AGENTS.md の Project Structure)。この解説の範囲外とし、必要なら別のページに置く
  - Rider の MCP などの道具は、このページの範囲外とする(`3-prepare/dev-environment.md` へ)
- 見出し:
  - フレームワーク層が持つ4つのツリー
  - Widget から Element を作り、更新する
  - 変更が dirty として登録され、ビルドされる
  - レイアウトと描画は、境界までを描き直す
  - 入力は、ヒットテストからジェスチャへ流れる
  - Flutter と違うところ
  - 実験で確かめる
- ※要確認:
  - `WidgetBinding` が持つものの一覧(`BuildOwner`・`RenderPipeline`・ジェスチャの部品)を、ソースの全体を読んで確かめる
  - 持つ型の一覧を、ページに載せるか、リンクだけにするか(件数・一覧は載せない方針)
  - ホットリロード(`Reassemble`)をこのページに含めるか。含めるなら `src/FloatSoda/Core/HotReloadHandler.cs` を読んで確かめる
  - ADR の番号と、Semantics を移植しない決定が ADR になっているか(`adr-index.md` の決定待ち)

## ト書き

### フレームワーク層が持つ4つのツリー

- 読み手が持って来るもの: 「Widget と RenderObject と Layer は、何が違うのか」という疑問
- このページが渡すもの: 4つのツリーの役割を1行ずつ。Widget は設計図(不変)、Element は Widget と RenderObject の橋渡し(可変)、RenderObject はレイアウトと描画の記録、Layer は合成の結果。ツリーの流れの図
- 読み手がすること: 変更したい処理がどのツリーの仕事かを見分ける
- 次へ進む先: 「Widget から Element を作り、更新する」

### Widget から Element を作り、更新する

- 読み手が持って来るもの: ツリーごとの役割
- このページが渡すもの: `CreateElement`、`CanUpdate`(型と `Key`)、`UpdateChild` の3つの分岐(同じ値なら飛ばす、更新する、作り直す)
- 読み手がすること: 自分の書いたウィジェットが、再利用されるか作り直されるかを判断する
- 次へ進む先: 「変更が dirty として登録され、ビルドされる」

### 変更が dirty として登録され、ビルドされる

- 読み手が持って来るもの: `SetState` を呼んだ経験
- このページが渡すもの: `SetState` → `MarkNeedsBuild` → `BuildOwner` の待ちリスト → `BuildScope`(`Depth` の順)の流れ
- 読み手がすること: `SetState` を呼んだフレームの、次のフレームで再ビルドが走ることを確かめる
- 次へ進む先: 「レイアウトと描画は、境界までを描き直す」

### レイアウトと描画は、境界までを描き直す

- 読み手が持って来るもの: ビルドが `UpdateRenderObject` でプロパティを書き換える話
- このページが渡すもの: `MarkNeedsLayout`・`MarkNeedsPaint` の伝播(境界で止まる)、`NodesNeedingLayout`・`NodesNeedingPaint`、`FlushLayout`・`FlushPaint` の順序、`RepaintBoundary` を足すと描き直しの範囲が狭まること
- 読み手がすること: 変更が最小の範囲だけを描き直すかを判断する。実験で確かめる
- 止まる箇所: 値が変わっていないのに `MarkNeedsPaint` を呼ぶと、無駄な描き直しになる(REVIEW.md の「差分更新」の節)
- 次へ進む先: 「入力は、ヒットテストからジェスチャへ流れる」。実験は `experiment-framework.md`

### 入力は、ヒットテストからジェスチャへ流れる

- 読み手が持って来るもの: `GestureDetector` のコールバックが呼ばれる体験
- このページが渡すもの: `PointerController` → `HandlePointerEvent` → ヒットテスト → ジェスチャの調停の流れ。ダッシュボードのオーバーレイだけに届くという制約(追跡: #182)
- 読み手がすること: ウィジェット側の入力が、どこまで FloatSoda の仕事で、どこから OpenVR の仕事かを見分ける(OpenVR 側は `ovr-layer.md`)
- 次へ進む先: 「Flutter と違うところ」
- 【未決】この節は量が多い。入力(ヒットテスト、ジェスチャ、ポインタ)を別ページ(例: `framework-input.md`)に分けるか

### Flutter と違うところ

- 読み手が持って来るもの: Flutter の知識
- このページが渡すもの: 違う点を3つ(非活性の Element のプールがない、Semantics がない、Widget が `record` で値が同じなら更新を飛ばす)と、それぞれの理由または記録へのリンク。一覧は `adr-index.md` に置く
- 読み手がすること: Flutter のコードを移植するとき、持ち込めない前提を知る
- 次へ進む先: 「実験で確かめる」

### 実験で確かめる

- 読み手が持って来るもの: 読んだ内容を確かめたい気持ち
- このページが渡すもの: `experiment-framework.md` へのリンクと、ほかの層の解説へのリンク(描画層、エンジン層)
- 読み手がすること: 実験に進む
- 止まる箇所: なし。ここでこのページは終わる
- 【未決】このページを1本にするか、「三つのツリーと差分更新」と「入力」の2本に分けるか。調査では、フレームワーク層は量が多いので分けたほうがよいと分かっている(docs 再編の決定、2026-10-17)。分け方の決定は、オーナーに確かめる
