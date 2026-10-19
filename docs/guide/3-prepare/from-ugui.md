← [ガイド](../Home.md)

# uGUI から来た人へ

uGUI は「オブジェクトを置いて、スクリプトから書き換える」作り方で、FloatSoda は「状態を変えて、画面を作り直す」作り方です。このページは、uGUI の語彙で FloatSoda の考え方を説明し、最初のコードを書くときの食い違いを減らします。シーン、プレハブ、Inspector、`Update` を知っていて、C# の基本が書ける人を読み手にしています。

## uGUI と FloatSoda の考え方の違い

違いの要点は1つです。uGUI では画面のオブジェクトを探して値を書き換えますが、FloatSoda では `SetState` の中で状態の値を変えると、画面が作り直されます。

FloatSoda にシーンとプレハブはありません。UI は C# のコードで宣言します。この違いから、このページの対応表と、書き換えの癖の直し方が出てきます。

## 置いて書き換えるから、状態を変えて作り直すへ

「数字が増えるボタン」を2つの作り方で書いて、違いを見ます。

uGUI では、シーンにテキストを置き、スクリプトからそのテキストを書き換えます。

```csharp
// uGUI: 置いたテキストを、スクリプトから書き換える
public Text label;
int count;

public void OnClick()
{
    count++;
    label.text = count.ToString();
}
```

FloatSoda では、数字を状態として持ち、状態を変えます。画面を書き換える対象を探す必要はありません。次のコードは、ウィジェット定義の抜粋です。`using` とアプリの起動処理は省略しています。押して動かすには、`DashboardWindow` の `Child` に `new CounterWidget()` を置きます。

```csharp
public record CounterWidget : StatefulWidget<CounterWidget>
{
    public override State<CounterWidget> CreateState() => new CounterState();
}

public class CounterState : State<CounterWidget>
{
    private int _count;

    public override Widget Build(IBuildContext context)
    {
        return new GestureDetector
        {
            OnTap = () => SetState(() => _count++),
            Child = new Text(_count.ToString())
        };
    }
}
```

`SetState` の中で `_count` を変えると、`Build` がもう一度呼ばれ、新しい数字の画面ができます。`label.text = ...` のように、画面の部品へ手を伸ばす行は、FloatSoda のコードには出てきません。

**できた?** 「`SetState` の中で、状態を持つクラスの値を変える」と言える。

<details>
<summary>できなかったら</summary>

`Build` が呼ばれるタイミングが分からないときは、[状態と作り直し](../4-build/mechanics/state-and-rebuild.md)を読んでください。`record` や `=>` の書き方が分からないときは、[C# がほぼ初めての人へ](csharp-primer.md)を読んでください。

</details>

## 概念の対応表

uGUI で使ってきた概念が、FloatSoda でどうなるかを表にしました。

| uGUI | FloatSoda | 違う点 |
| :-- | :-- | :-- |
| シーン、プレハブ | なし | UI の構造を C# のコードで宣言する。シーンやプレハブに当たる外部ファイルはない |
| Hierarchy | コードの上のウィジェットの入れ子 | 親子関係は `Child` と `Children` で書く |
| Inspector で設定する値 | ウィジェットのプロパティ | `new SizedBox { Width = 100, Height = 100 }` のように、コードで設定する |
| Canvas | ウィンドウ(`DashboardWindow`、`WorldSpaceWindow`、`DeviceTrackedWindow`) | 置き場所を、ウィンドウの種類で選ぶ |
| LayoutGroup | `Row`、`Column`、`Wrap` など | 親が制約を渡し、子が大きさを返す |
| Button | `GestureDetector` で組み立てる | `Button` は提供していない |

`Button` は今は使えません(追跡: #102)。見た目をつけたボタンは、[ボタンやトグルを組み立てる](../4-build/screen/build-buttons-toggles.md)で作ります。対応する部品は、リファレンスの[部品を目的から探す](../../reference/find-widgets.md)で探せます。

<!-- ※要確認: Canvas、LayoutGroup との対応(RectTransform は表に載せていない)。筆者の案で、Unity 経験者の確認が済んでいない。確かめ方: Unity の経験者に表を読んでもらい、違和感のある行を拾う -->

## 入力と時間の対応

uGUI の EventSystem に当たるのは、ウィジェットの入力の受け取り口です。押す、離す、ドラッグするといった操作は、`GestureDetector` と `Listener` が受け取ります。ポインタが届くのは、ダッシュボードのオーバーレイだけです(追跡: #182)。

コントローラーのボタンやスティックの入力は、`InputActionMap` と `InputAction<T>` で受け取ります。Unity の Input System の ActionMap、Action、DefaultBinding に近い語彙です。受け取り方は、[コントローラーの入力を受け取る](../4-build/input/controller-input.md)で説明しています。

`Update` に当たる毎フレームの処理は、`AnimationController` と `WidgetTicker` が時間を進めます。書き方は、[アニメーションを付ける](../4-build/screen/add-animation.md)で説明しています。

<!-- ※要確認: アニメーション中に、画面を作り直すのか、描画だけを更新するのか。以前の本文は「描画だけ」と書いていたが、src/ で確かめられず削った。確かめ方: src/FloatSoda/Animation/AnimationController.cs の Changed を購読する側(SetState を呼ぶか、描画の更新だけか)を読む -->
<!-- ※要確認: Update の代わりに AnimationController と Ticker を勧めてよいか。毎フレームの処理を書く推奨の形を確かめていない。確かめ方: docs/Animation.md と src/FloatSoda/Animation/ を読み、Update 相当の処理を書く例をオーナーに確認する -->

## 落とし穴

uGUI の癖で、最初のコードに出やすい食い違いは3つあります。次の3つを守ってください。

- 画面の部品を直接書き換えず、`SetState` の中で状態を変えること
- `Button` を探さず、`GestureDetector` でボタンを組み立てること
- 押せる画面は `DashboardWindow` に置くこと(追跡: #182)

コンパイルは通り、例外も出ないのに押しても動かない、というのが、3つ目を守らなかったときの症状です。ここまで読んだら、チュートリアル[最初のオーバーレイを作る](../../tutorials/first-overlay.md)を始めてください。
