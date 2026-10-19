← [ガイド](../Home.md)

# FloatSoda とは

FloatSoda は、SteamVR のオーバーレイを C# のコードで作る UI フレームワークです。Flutter のように、部品を入れ子にして画面を書きます。対応する .NET の版は、リファレンスの[動作環境](../../reference/requirements.md)で確かめられます。VRChat 向けの自分用ツールや、Booth で売るオーバーレイを、コードだけで形にしたい人に向いています。このページは、FloatSoda が自分の目的に合うかを判断するための説明です。

オーバーレイという言葉が初めてなら、先に[オーバーレイとは](what-is-an-overlay.md)を読んでください。

## FloatSoda は、VR のオーバーレイを C# のコードで作る道具

FloatSoda で書いた画面は、SkiaSharp で描かれ、OpenGL を経て、OpenVR のオーバーレイのテクスチャとして SteamVR に渡されます。画面の中身は、ダッシュボード、ワールド座標に固定、デバイスに追従の3種類のどれでも、同じ書き方で組めます。ポインタで押せるのはダッシュボードだけです(追跡: #182)。

「こんな画面を VR の中に出したい」と思ったときに、描画や置き場所の管理を自分で書かずに、画面の中身だけを書けるようにするのが目的です。

## UI がすべてコードであること

FloatSoda の UI は、すべて C# のコードで決まります。シーンもプレハブも持ちません。そのため、変更の差分を読む、レビューする、検索する、AI に生成させる、といった作業がコードの扱いのままできます。

書き方は宣言的です。画面を「どんな部品がどう入れ子になっているか」で書きます。たとえば、100 × 100 の色の四角を出す画面は、次のとおりです。

```csharp
Widget root = new SizedBox
{
    Width = 100,
    Height = 100,
    Child = new ColoredBox { Color = SKColors.Tomato }
};
```

`SizedBox` が大きさを決め、`ColoredBox` が色を塗ります。`SizedBox` の `Child` に `ColoredBox` を入れる、という入れ子です。Unity の経験がある人は、シーンの Hierarchy に当たるものがコードの入れ子になった、と読んでください。Flutter や React を触ってきた人には、見慣れた形です。

入れ子を重ねた書き方は、C# の読み方を[C# がほぼ初めての人へ](../3-prepare/csharp-primer.md)で、動くコードをチュートリアル[最初のオーバーレイを作る](../../tutorials/first-overlay.md)で説明しています。

ほかの作り方との違いは、[ほかの作り方と比べる](../2-adopt/compare-approaches.md)で説明しています。

## 向いている人

FloatSoda が想定する作り手は、3タイプです。

| タイプ | FloatSoda で得るもの | 次に開くページ |
| :-- | :-- | :-- |
| AI に書かせて、自分用のツールを作る VRChatter | AI が誤用しにくいように設計した API と docs | [AI に FloatSoda を書かせる](../3-prepare/write-with-llm.md) |
| Booth でオーバーレイを売るクリエイター(Unity の経験者) | Unity の語彙で説明される考え方と、exe で配る流れ | [uGUI から来た人へ](../3-prepare/from-ugui.md) |
| uGUI を避けたいエンジニア | シーンもプレハブもない、テキストで読める UI | [ほかの作り方と比べる](../2-adopt/compare-approaches.md) |

## 作れるものの例

想定している例は、次の4つです。

- 時刻や FPS を出す、表示専用の HUD
- FaceEmo の表情を OSC で切り替えるパネル
- 写真アルバム
- フレンドのログインを知らせる通知

どの例が今の FloatSoda で作れるかは、リファレンスの[できないこと・まだ使えないもの](../../reference/limitations.md)で確かめてください。たとえば、写真アルバムに欲しいスクロールの部品(追跡: #69)や、通知に欲しいトーストの機能(追跡: #123)は、まだ使えません。動く作例は、[作例のカタログ](../../reference/sample-catalog.md)で探せます。表情切り替えは、[表情切り替えパネルを作る](../4-build/screen/expression-switcher.md)で扱います。

<!-- ※要確認: 作れるものの例4つが今できるか。確かめ方: 各例に必要な部品を limitations.md の記述とコードで突き合わせる。通知(トースト)は #123 で未実装、写真アルバムのスクロールは internal の ListView のみ -->
<!-- ※要確認: Booth で売れることを、MIT ライセンスの範囲から言ってよいか。このページでは「Booth で売るオーバーレイを作る人に向く」とだけ書き、ライセンスの条件には触れていない。確かめ方: LICENSE と NuGet の依存パッケージのライセンスを読む -->

## 開発段階

FloatSoda は Alpha の段階にあり、API は予告なく変わります。変更の内容は、リリースごとに CHANGELOG に書かれます。使うバージョンを決めたら、更新の前に [CHANGELOG.md](../../../CHANGELOG.md) で、今の版から更新先の版までの変更を読んでください。

ライセンスは MIT です。使うかどうかを決めるときは、[ほかの作り方と比べる](../2-adopt/compare-approaches.md)で引き受けることを確かめてください。

