← [リファレンス](Home.md)

# 作例のカタログ

`samples/` には、動かして手本にできる作例が入っています。このページで、作りたいものに近い作例を探し、動かし方を確かめられます。作例は、アプリ全体の形を見るものと、ウィジェット1つ(または1グループ)を扱うカタログ型に分かれます。ウィジェットの作例は、レイアウト・描画・入力の3つで引けます。目的(時計を出したい、アルバムを作りたい)から引ける作例の整備は、追跡: #261 です。

## 作例の動かし方

作例は [FloatSoda のリポジトリ](https://github.com/sumx21t-3310/FloatSoda)に入っています。リポジトリを clone し、SteamVR を起動してから、リポジトリのルートで次のコマンドを実行します。`<名前>` を、下の表の作例の名前(`FloatSoda.Samples.` に続く部分)に置き換えます。`PaintingSample` のように、画面を PNG に書き出すだけの作例は、SteamVR を起動しなくても動きます。SteamVR が要らない場面は、[動作環境](requirements.md#steamvr-が要らない場面)で確かめられます。

```powershell
dotnet run --project samples/FloatSoda.Samples.<名前>
```

ウィジェットの作例には、`--desktop` を付けられます。ダッシュボードの代わりに、PC のデスクトップのウィンドウに表示します。HMD を被らずに見た目を確かめるときに使います。

```powershell
dotnet run --project samples/FloatSoda.Samples.Flex -- --desktop
```

`--desktop` を付けても、SteamVR の起動と HMD の接続は要ります。起動時に OpenVR を初期化するためです。

**できた?** ダッシュボード(`--desktop` なら PC のウィンドウ)に、作例の画面が出た。

<details>
<summary>できなかったら</summary>

起動で例外が出るときは、ガイドの[起動で例外が出る: SteamVR または HMD につながらない](../guide/5-troubleshoot/read-errors.md#起動で例外が出る-steamvr-または-hmd-につながらない)を開きます。PC に必要なものが足りないときは、[動作環境](requirements.md)で確かめてください。

</details>

## 総合・最小・低レベルの作例

ウィジェット単体ではなく、まとまったアプリの形を見たいときは、次の4つから選びます。この4つに README はありません。コードを直接読んでください。

| 作例 | 示すもの | SteamVR |
| :-- | :-- | :-- |
| [FloatSoda.Samples.OverlayApp](../../samples/FloatSoda.Samples.OverlayApp/Program.cs) | レイアウト、時計、アニメーション、カウンター、ドラッグの総合デモ。3種類のオーバーレイを同時に作る | 要る |
| [FloatSoda.Samples.GettingStarted](../../samples/FloatSoda.Samples.GettingStarted/Program.cs) | 色の四角を1つ出す最小のアプリ。チュートリアルの段1に近い | 要る |
| [FloatSoda.Samples.PrimitiveOverlay](../../samples/FloatSoda.Samples.PrimitiveOverlay/Program.cs) | ウィジェットを使わず、`FloatSoda.OVR` の低レベル API だけでオーバーレイを出す。デスクトップにある `layer_tree_output.png` を貼るので、先に PaintingSample を実行しておく | 要る |
| [FloatSoda.Samples.PaintingSample](../../samples/FloatSoda.Samples.PaintingSample/Program.cs) | ウィジェット、RenderObject、レイヤーの3つのツリーを、PNG の画像に書き出す。デスクトップに保存する | 要らない |

チュートリアルを先に通したいときは、[最初のオーバーレイを作る](../tutorials/first-overlay.md)を開いてください。

## ウィジェットの作例(レイアウト)

並べる、重ねる、余白を取る、大きさを決めるといった、レイアウトの目的で選びます。名前から各作例の README を開けます。

| 作例 | 扱うウィジェット |
| :-- | :-- |
| [Flex](../../samples/FloatSoda.Samples.Flex/README.md) | `Flex`、`Row`、`Column` |
| [Expanded](../../samples/FloatSoda.Samples.Expanded/README.md) | `Expanded`、`Flexible`、`Spacer` |
| [Wrap](../../samples/FloatSoda.Samples.Wrap/README.md) | `Wrap` |
| [Align](../../samples/FloatSoda.Samples.Align/README.md) | `Align`、`Center` |
| [Padding](../../samples/FloatSoda.Samples.Padding/README.md) | `Padding` |
| [Container](../../samples/FloatSoda.Samples.Container/README.md) | `Container` |
| [SizedBox](../../samples/FloatSoda.Samples.SizedBox/README.md) | `SizedBox` |
| [LimitedBox](../../samples/FloatSoda.Samples.LimitedBox/README.md) | `LimitedBox` |
| [ConstraintsTransformBox](../../samples/FloatSoda.Samples.ConstraintsTransformBox/README.md) | `ConstraintsTransformBox`、`UnconstrainedBox` |
| [Stack](../../samples/FloatSoda.Samples.Stack/README.md) | `Stack`、`Positioned` |
| [AspectRatio](../../samples/FloatSoda.Samples.AspectRatio/README.md) | `AspectRatio` |
| [FittedBox](../../samples/FloatSoda.Samples.FittedBox/README.md) | `FittedBox`、`BoxFit` |
| [FractionallySizedBox](../../samples/FloatSoda.Samples.FractionallySizedBox/README.md) | `FractionallySizedBox` |
| [OverflowBox](../../samples/FloatSoda.Samples.OverflowBox/README.md) | `OverflowBox`、`SizedOverflowBox` |

表は、`samples/` のディレクトリから手で写したものです。作例が増えたときは、`samples/` の一覧が正になります。部品を目的から探したいときは、[部品を目的から探す](find-widgets.md)を開いてください。

## ウィジェットの作例(描画)

色を塗る、文字や画像を出す、透明にする、切り抜くといった、見た目の目的で選びます。

| 作例 | 扱うウィジェット |
| :-- | :-- |
| [ColoredBox](../../samples/FloatSoda.Samples.ColoredBox/README.md) | `ColoredBox` |
| [DecoratedBox](../../samples/FloatSoda.Samples.DecoratedBox/README.md) | `DecoratedBox`、`BoxDecoration` |
| [Opacity](../../samples/FloatSoda.Samples.Opacity/README.md) | `Opacity` |
| [Transform](../../samples/FloatSoda.Samples.Transform/README.md) | `Transform` |
| [Clip](../../samples/FloatSoda.Samples.Clip/README.md) | `ClipRect`、`ClipRoundRect`、`ClipOval`、`ClipCustomPath` |
| [Text](../../samples/FloatSoda.Samples.Text/README.md) | `Text`、`RichText`、`TextSpan`、`TextStyle` |
| [Image](../../samples/FloatSoda.Samples.Image/README.md) | `Image`、`FileImageProvider`、`BoxFit` |

見た目をまとめて整える手順は、ガイドの[見た目を整える](../guide/4-build/screen/style-appearance.md)にあります。

## ウィジェットの作例(入力)

押す、触れる、ドラッグするといった、操作の目的で選びます。

| 作例 | 扱うウィジェット |
| :-- | :-- |
| [GestureDetector](../../samples/FloatSoda.Samples.GestureDetector/README.md) | `GestureDetector`(タップとドラッグ) |
| [Listener](../../samples/FloatSoda.Samples.Listener/README.md) | `Listener`(生のポインターイベント) |
| [PointerRegion](../../samples/FloatSoda.Samples.PointerRegion/README.md) | `PointerRegion`(ホバーの出入りと、`Listener`・`GestureDetector` を重ねたときのイベントの順序) |

ポインタの座標が届くのは、ダッシュボードのオーバーレイだけです。詳しくは、[できないこと・まだ使えないもの](limitations.md#置く場所と入力)を開いてください。ボタンの組み立て方は、ガイドの[ボタンやトグルを組み立てる](../guide/4-build/screen/build-buttons-toggles.md)にあります。

## 目的から作例を探す

「時計を出したい」「画像を並べたい」のような、作りたいものの言葉から引ける作例は、今のところありません。目的別のミニアプリは、#261 で追っています。

近い作例がないときは、次の2つから選べます。

- 通しで1つのパネルを作る: ガイドの[表情切り替えパネルを作る](../guide/4-build/screen/expression-switcher.md)
- コードを AI に書かせる: ガイドの[AI に FloatSoda を書かせる](../guide/3-prepare/write-with-llm.md)
