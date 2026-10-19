← [コントリビューター](../Home.md)

# フレームワーク層

## フレームワーク層が持つ4つのツリー

フレームワーク層(`src/FloatSoda`)は、Flutter の三つのツリー(Widget、Element、RenderObject)を写し、描画の結果として Layer ツリーを作ります。どの変更をどのツリーに入れるかは、次の役割で見分けます。

| ツリー | 役割 | 置き場所 |
| :-- | :-- | :-- |
| Widget | UI の設計図。不変の `record` | `src/FloatSoda/Widgets/` |
| Element | Widget と RenderObject の橋渡し。可変で、ビルドの状態を持つ | `src/FloatSoda/Elements/` |
| RenderObject | レイアウトと描画の記録。サイズを決め、描画命令を記録する | `src/FloatSoda/RenderObjects/` |
| Layer | 合成の結果。描画層が持つ(→ [描画層](rendering-layer.md)) | `src/FloatSoda.Rendering/Layers/` |

流れは次のとおりです。Widget から Element を作り、RenderObject を持つウィジェットの Element が RenderObject を作って更新し、RenderObject の描画が Layer ツリーになります。

```text
Widget ──CreateElement──▶ Element ──作成・更新──▶ RenderObject ──Paint──▶ Layer
```

ウィンドウごとに1つの `WidgetBinding` があり、このツリーの更新を進めます。`WidgetBinding` は、`BuildOwner`(再ビルドの予約)、`RenderPipeline`(レイアウトと描画の予約)、ジェスチャの調停の部品を持ちます。

## Widget から Element を作り、更新する

`Widget` は不変の `abstract record` で、`CreateElement()` と任意の `Key` を持ちます。ビルドで新しい Widget が来るたびに、`Element.UpdateChild` が、既存の子 Element をどう扱うかを次の順で決めます。

1. 新しい Widget が `null` なら、子を取り除く
2. 子 Element がまだなければ、Widget から Element を作る
3. 古い Widget と新しい Widget が `record` として等しければ、何もせずに終える
4. `Widget.CanUpdate` が `true` なら、子 Element を再利用して `Update` する
5. それ以外は、古い子を外して、新しい Element を作り直す

`CanUpdate` が `true` になる条件は、実行時の型が同じで、`Key` が等しいことです。つまり、自分の書いたウィジェットが、同じ位置で同じ型のまま `Key` も変わらなければ、Element は再利用されます。`StatefulWidget` の `State` も、その Element の中で保たれます。型か `Key` が変わると、作り直しになり、`State` は破棄されます。

## 変更が dirty として登録され、ビルドされる

`State.SetState` は、渡された処理を実行してから `Element.MarkNeedsBuild()` を呼びます。`MarkNeedsBuild` は、Element を dirty にして、`BuildOwner` の待ちリストに登録します。登録されたことは、`WidgetBinding.EnsureVisualUpdate` で「次のフレームで視覚の更新が要る」と記録されます。

`BuildOwner.BuildScope` は、`DrawFrame` のたびに呼ばれ、待ちリストの Element を `Depth` の昇順で再ビルドします。親が先に再ビルドされます。再ビルドの途中で増えた dirty も、同じ呼び出しの中で処理されます。つまり、`SetState` を呼んだあとの最初の `DrawFrame` で、再ビルドが走ります。

## レイアウトと描画は、境界までを描き直す

再ビルドは、`UpdateRenderObject` で RenderObject のプロパティを書き換えます。プロパティの設定が値の変化を検知すると、レイアウトか描画の再実行を要求します。たとえば `RenderColoredBox.Color` は、値が変わったときだけ `MarkNeedsPaint()` を呼びます。

```csharp
public SKColor Color
{
    get;
    set
    {
        if (field == value) return;
        field = value;
        MarkNeedsPaint();
    }
} = SKColors.Black;
```

再実行の要求は、境界で止まります。

| 呼び出し | 伝わり方 | 登録先 |
| :-- | :-- | :-- |
| `MarkNeedsLayout` | 最も近い再レイアウト境界まで、親へ伝わる | `RenderPipeline.NodesNeedingLayout` |
| `MarkNeedsPaint` | 自分が再描画境界なら、そこで止まる。そうでなければ親へ伝わる | `RenderPipeline.NodesNeedingPaint` |

登録されるのは、伝わった先の境界のノードです。どちらも、登録と同時に `RequestVisualUpdate` で視覚の更新を予約します。

視覚の更新が必要なフレーム(`NeedsVisualUpdate` が立っているフレーム)では、`RenderPipeline` が2段で登録済みのノードを処理します。

1. `FlushLayout`: リストが空になるまで、登録されたノードを `Depth` の昇順で再レイアウトする
2. `FlushPaint`: 開始時点のリストを取り出して空にし、取り出したノードを `Depth` の昇順で再描画する

`FlushLayout` は、処理中に増えたノードも同じ呼び出しで処理します。`FlushPaint` の処理中に増えたノードは、次のフレームで処理します。

再描画では、`PaintingContext.RepaintCompositedChild` が、境界ノードの `Layer`(`TransformLayer`)を再利用します。子の Layer を消してから、その境界から下だけを描き直します。`RepaintBoundary` ウィジェットは、`IsRepaintBoundary` が真の `RenderRepaintBoundary` を作ります。`RenderView` も再描画境界です。そのため、`RepaintBoundary` を足すと、描き直しの範囲が、その境界の中に狭まります。

値が変わっていないのに `MarkNeedsPaint` や `MarkNeedsLayout` を呼ぶと、無駄な描き直しになります。値が変わったときだけ dirty になるよう、プロパティの setter が値を比べます。`UpdateRenderObject` から値を設定するときは、この setter を通すこと(→ [REVIEW.md](../../../REVIEW.md) の「差分更新」)。

`SetState` のあとに、どのノードが `NodesNeedingPaint` に入るかは、[実験: フレームワーク層](experiment-framework.md) で、ログで確かめられます。

## 入力の流れは別のページにある

ポインター入力が、ヒットテストとジェスチャ認識器を経てウィジェットに届くまでは、[入力が届くまでを追う](framework-input.md) に書いています。

## Flutter と違うところ

Flutter のコードを移植するときは、次の3つの前提を持ち込めません。

| 違い | 内容 | 台帳または記録 |
| :-- | :-- | :-- |
| 非活性の Element のプールがない | `Deactivate` は終端で、unmount と同じ扱いになる。Flutter が同一フレーム内で再活性化できる Element も再利用せず、`State` を破棄する。親をまたぐ移動に使う `GlobalKey` は未実装 | 台帳の 2 番と 3 番。[REVIEW.md](../../../REVIEW.md) の「ツリーの所有権とライフサイクル」 |
| Semantics を移植しない | スクリーンリーダー向けの情報の仕組みを持たない | [REVIEW.md](../../../REVIEW.md) の「Flutter parity」、[制限事項](../../reference/limitations.md) |
| Widget が `record` で、値が同じなら更新を飛ばす | 同じ位置で型と `Key` が同じ Widget を、Flutter は同一性で比べる。そのため新しいインスタンスは `didUpdateWidget` に届く。FloatSoda は値で比べ、同じ値なら `Update` が走らず、子も再ビルドされない | 台帳の 1 番 |

台帳は、Flutter との差異を項目ごとに残すファイルです([known-divergences.md](../../../.agents/skills/floatsoda-device-test-gen/references/known-divergences.md))。各項目のラベルと、決定の記録の一覧は [決定の記録(ADR)の一覧](adr-index.md) にあります。移植するウィジェットに関わる差異があるかは、移植の前に台帳で探します。

## 実験で確かめる

ここまでの内容は、[実験: フレームワーク層](experiment-framework.md) で動かして確かめられます。ほかの層は、[描画層](rendering-layer.md) と [エンジン層](engine-layer.md) に続きます。1フレームの流れ全体は [1フレームが SteamVR に届くまで](frame-to-steamvr.md) にあります。

