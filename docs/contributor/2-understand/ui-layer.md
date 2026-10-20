← [コントリビューター](../Home.md)

# UI 層(3層に分けた理由)

## UI を3層に分ける理由

FloatSoda は、UI の部品(ボタンなど)を、コア、ヘッドレス、デザインシステムの3層に分けます。置き場所のプロジェクトは、コア(`FloatSoda`)、ヘッドレスの振る舞い(`FloatSoda.UI`)、デザインシステム(`FloatSoda.UI.Cream` と `FloatSoda.UI.FizzyPop`)の4つです。分ける理由は、見た目と振る舞いが一体になることを避けるためです。

一体になると、別の見た目の部品を作りたいときに、振る舞いのコードを複製することになります。プロジェクトの設計方針は、Flutter で起きた「Material ロックイン」を例にしています。振る舞いの層が独立せず、見た目と振る舞いが `material` パッケージに一体化したため、別の見た目の `cupertino` が振る舞いを複製することになった、という問題です。

<!-- ※要確認: Flutter 側の経緯 / プロジェクトの設計方針の記述に基づく。Flutter の資料で、material と cupertino の振る舞いの重複を確かめる -->

## 3層の役割と、置き場所の基準

新しい部品を足すときは、置く層を、次の表で仮に決めます。

| 層 | プロジェクト | 置くもの | 置かないもの |
| :-- | :-- | :-- | :-- |
| コア | `FloatSoda` | Skia や RenderObject の型に触れるウィジェット。見た目の方針を持たない合成ウィジェット(`Center`、`Container` など) | インタラクションの状態機械、具体的な見た目 |
| ヘッドレス | `FloatSoda.UI` | インタラクションの状態機械(`ButtonBase`、`InteractionState`)。見た目を受け取る型付きの入口 | 色、余白、角丸 |
| デザインシステム | `FloatSoda.UI.Cream`、`FloatSoda.UI.FizzyPop` | ヘッドレスの状態から見た目への対応。スタイルの `record`(`ButtonStyle`)、テーマ | 振る舞いの状態機械 |

プロジェクトの参照は、次の向きです。

```text
FloatSoda  ◀──  FloatSoda.UI  ◀──  FloatSoda.UI.Cream
                     ▲
                     └──────────  FloatSoda.UI.FizzyPop
```

2つのデザインシステムは、お互いを参照しません。デザインシステムは、`FloatSoda.UI` を通して、コアのウィジェットも使えます。

## 2つの規約と、判定基準

置き場所は、次の2つの規約と、1つの判定基準で確定します。

1. 振る舞いと状態機械は `FloatSoda.UI` に置き、デザインシステムの `State` には、ヘッドレスの状態から見た目への対応だけを書くこと
2. `FloatSoda.UI` のウィジェットは、デザインシステムの `InheritedWidget` がなくても動くこと

規約1は、Flutter の `material` に振る舞いが入った轍を踏まないためです。規約2のため、ヘッドレス層はデザインシステムのテーマに依存させません。デザインシステム側のテーマの `Of()` は、祖先にテーマが見つからないときに `null` を返し、部品は既定のスタイルに戻ります。`FloatSoda.UI.Cream` の `Button` は、スタイルが未指定なら祖先の `CreamTheme` を、それもなければ `ButtonStyle.Default` を使います。

判定基準は、「2つ目のデザインシステムが、1つ目のコードをコピーせずに、同じ部品を作れるか」です。Cream と FizzyPop を最初から並べて作るのは、この基準を常に確かめるためです。ヘッドレス層の API に片方のデザインシステムの都合が漏れると、もう片方が作れなくなるので、漏れに気づけます。

基準を満たさない場合は、必要な状態や拡張点がヘッドレス層に足りないか、片方のデザインシステムの都合がヘッドレス層の API に漏れていないかを確かめます。置き場所と API の形を見直します。

<!-- ※要確認: 2つのデザインシステムを並べて作る決定が、記録(ADR)になっているか / 決定の記録の一覧(adr-index.md)と、過去の議論を探して確かめる -->

## 見た目を型付きのビルダーで渡す

ヘッドレス層は、見た目を `Builder` で受け取ります。`ButtonBase` は、`required Func<IBuildContext, InteractionState, Widget> Builder` を持ちます。`InteractionState` は、`IsPressed`、`IsHovered`、`IsFocused`、`IsDisabled` を持つ `readonly record struct` で、インタラクションの状態を型で公開します。デザインシステムは、`Builder` の中で、この状態を見た目に対応づけます。

次は、`FloatSoda.UI.Cream` の `Button` の処理の概略です。`Builder` が `Widget` を返す部分を省略しているため、このままではコンパイルできません。

```csharp
new ButtonBase
{
    OnPressed = OnPressed,
    IsDisabled = IsDisabled,
    Builder = (ctx, state) =>
    {
        var style = Style ?? CreamTheme.Of(ctx)?.ButtonStyle ?? ButtonStyle.Default;
        var background = state switch
        {
            { IsDisabled: true } => style.DisabledBackgroundColor,
            { IsPressed: true } => style.PressedBackgroundColor,
            _ => style.BackgroundColor
        };
        // background で ColoredBox を作り、Child を包んで返す
    }
};
```

押下やホバーの状態を入力イベントから更新する処理は、ヘッドレス層に置きます。`Builder` は、渡された `state` に応じて色を選びます。自分の部品の `Builder` の形を設計するときは、デザインシステムが必要とする状態を、`InteractionState` に足すかどうかを先に決めます。

この方式は、Avalonia のルックレスコントロール(状態を疑似クラス、見た目の注入を `PART_` のテンプレートパーツで行う契約)を、型付きにした形です。`InteractionState` のコメントが、この対応を述べています。文字列の疑似クラスの代わりに型付きの `InteractionState` を、名前で探す `PART_` の代わりに型付きの `Builder` を使います。

<!-- ※要確認: Avalonia の契約の説明 / InteractionState のコメントと、プロジェクトの設計方針の記述に基づく。Avalonia の資料で、疑似クラスと PART_ の契約を確かめる -->

## 実装の進み具合の追い方

このページは、実装の進み具合を書きません。進み具合は、次の Issue で追います。

| Issue | 追うもの |
| :-- | :-- |
| [#102](https://github.com/sumx21t-3310/FloatSoda/issues/102) | `ButtonBase` の押下・ホバー状態(`GestureDetector` との接続) |
| [#78](https://github.com/sumx21t-3310/FloatSoda/issues/78) | Cream の `Button` と、テーマ |
| [#100](https://github.com/sumx21t-3310/FloatSoda/issues/100) | FizzyPop の `Button` と、テーマ |
| [#38](https://github.com/sumx21t-3310/FloatSoda/issues/38) | 背景ブラー |
| [#182](https://github.com/sumx21t-3310/FloatSoda/issues/182) | ダッシュボード以外のオーバーレイへのポインター入力 |

3つのプロジェクトを NuGet で配布しているかは、各プロジェクトの `.csproj` の `IsPackable` で確かめます。

