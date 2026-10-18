← [チュートリアル](Home.md)

# 最初のオーバーレイを作る

## このチュートリアルで作るもの

SteamVR のダッシュボードに、数字とボタンが出るオーバーレイを作ります。ボタンを押すと、数字が1つ増えます。

完成した画面は、青い四角の中に大きな数字と赤いボタンが縦に並んだものです。<!-- ※要確認: 完成画面のスクリーンショットが未用意。HMD かデスクトップで撮影し、docs/ 以下に置いて、ここへ画像を差し込む -->

作業は3段です。

1. 色の四角をダッシュボードに出す
2. 四角を数字に置き換え、数字を書き換える
3. ボタンを足して、押すと数字を増やす

各段の終わりに、HMD の中で確かめる点を1つ置いています。

## 始める前に用意するもの

用意するものは次の3つです。

- .NET 10 の SDK(`dotnet --version` が `10.` で始まる)
- SteamVR(起動できる状態で、HMD をつないである)
- 空のコンソールアプリ

準備の手順は、ガイドの[開発環境を用意する](../guide/3-prepare/set-up-environment.md)にあります。動作環境の一覧は、リファレンスの[動作環境](../reference/requirements.md)です。足りないものがあれば、リンク先で用意してから戻ってください。

空のコンソールアプリは、次の2つのコマンドで作ります。1行ずつ実行します。

```powershell
dotnet new console -n FirstOverlay
```

```powershell
dotnet add FirstOverlay package FloatSoda
```

`FirstOverlay` フォルダーの中に `Program.cs` ができます。以降は、このファイル(`FirstOverlay/Program.cs`)の中身を段ごとに置き換えます。

## 1. オーバーレイを SteamVR に出す

> 【未決】この節は判断が要るため、ト書きのまま残しています。オーナーが決めたあとで本文にします(追跡: #219)。

- 読み手が持って来るもの: 空のコンソールアプリ
- このページが渡すもの: 起動のコード(`AddFloatSoda`、`CreateWindow`、`DashboardWindow`)と、色の四角を出すウィジェット
- 読み手がすること: コードを貼って `dotnet run` し、ダッシュボードを開く
- できた?: ダッシュボードに色の四角が出た
- できなかったら: オーバーレイが出ない、起動で例外が出る → 「5. 困ったとき」の該当する節
- 次へ進む先: 段2。起動のコードの意味は、ガイドの「起動と Generic Host」へリンクする
- 【未決】起動のコードの各行をこの段で説明するか、解説へ送るだけにするか

<details>
<summary>仮に書いた本文(判断の材料。採用は未定)</summary>

最初の段では、青い四角を1つ出します。`Program.cs` を次の内容に置き換えてください。

```csharp
using FloatSoda;
using FloatSoda.Geometrics;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SkiaSharp;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddFloatSoda();

using var host = builder.Build();
var app = host.Services.GetRequiredService<FloatSodaApp>();

Widget root = new Align
{
    Alignment = Alignment.Center,
    Child = new SizedBox
    {
        Width = 300,
        Height = 300,
        Child = new ColoredBox
        {
            Color = SKColors.CornflowerBlue
        }
    }
};

app.CreateWindow(new DashboardWindow
{
    Title = "FirstOverlay",
    Child = root
});

await host.RunAsync();
```

画面は `Align`、`SizedBox`、`ColoredBox` の3つを入れ子にして書きます。`Align` が中央に寄せ、`SizedBox` が 300 × 300 の大きさを決め、`ColoredBox` が色を塗ります。`DashboardWindow` は、この画面を SteamVR のダッシュボードに出す指定です。`Title` が、ダッシュボードのタブ名になります。

SteamVR を起動してから、`FirstOverlay` フォルダーを作った場所で、次のコマンドを実行します。

```powershell
dotnet run --project FirstOverlay
```

そのまま HMD を被り、SteamVR のダッシュボードを開きます。

**できた?** ダッシュボードの「FirstOverlay」のタブに、青い四角が出た。

<details>
<summary>できなかったら</summary>

起動で例外が出たときは、メッセージを控えて、ガイドの[起動で例外が出る: SteamVR または HMD につながらない](../guide/5-troubleshoot/read-errors.md#起動で例外が出る-steamvr-または-hmd-につながらない)を開きます。例外が出ないのにタブがないときは、同じページの[起動で警告が出るが、表示はされる](../guide/5-troubleshoot/read-errors.md#起動で警告が出るが表示はされる)も確かめてください。

</details>

`AddFloatSoda()` や `Host.CreateApplicationBuilder` の意味は、ガイドの[起動と Generic Host](../guide/4-build/mechanics/startup-and-host.md)で説明しています。ここでは、この数行を毎回の書き出しとして、そのまま使ってください。

</details>

## 2. 数字を表示して、書き換える

> 【未決】この節は判断が要るため、ト書きのまま残しています。オーナーが決めたあとで本文にします(追跡: #219)。

- 読み手が持って来るもの: 段1で動いたコード
- このページが渡すもの: `StatefulWidget` と `State`、`SetState()` で値を変えると画面が描き直されること
- 読み手がすること: 色の四角を数字の表示に置き換え、一定時間ごとに数字を増やす
- できた?: HMD の中で数字が増えていく
- できなかったら: 数字が変わらない、文字が出ない → 「5. 困ったとき」の該当する節
- 次へ進む先: 段3。仕組みは、ガイドの「状態と作り直し」へリンクする
- 【未決】数字を増やすきっかけを、タイマーにするか、段3のボタンだけにするか

<details>
<summary>仮に書いた本文(判断の材料。採用は未定)</summary>

2段目では、四角を数字の表示に置き換え、1秒ごとに数字を増やします。画面が持つ値を書き換えて画面を描き直すには、`StatefulWidget<T>` と `State<T>` を使います。値を書き換える操作が `SetState()` です。

`Program.cs` を次の内容に置き換えてください。

```csharp
using FloatSoda;
using FloatSoda.Animation;
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SkiaSharp;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddFloatSoda();

using var host = builder.Build();
var app = host.Services.GetRequiredService<FloatSodaApp>();

app.CreateWindow(new DashboardWindow
{
    Title = "FirstOverlay",
    Child = new CounterPanel()
});

await host.RunAsync();

record CounterPanel : StatefulWidget<CounterPanel>
{
    public override State<CounterPanel> CreateState() => new CounterPanelState();
}

class CounterPanelState : TickerProviderState<CounterPanel>
{
    private int _count;
    private WidgetTicker? _ticker;

    public override void InitState()
    {
        _ticker = CreateTicker(elapsed =>
        {
            var seconds = (int)elapsed.TotalSeconds;
            if (seconds != _count)
            {
                SetState(() => _count = seconds);
            }
        });
        _ticker.Start();
    }

    public override void Dispose()
    {
        _ticker?.Dispose();
        base.Dispose();
    }

    public override Widget Build(IBuildContext context)
    {
        return new Align
        {
            Alignment = Alignment.Center,
            Child = new SizedBox
            {
                Width = 300,
                Height = 300,
                Child = new ColoredBox
                {
                    Color = SKColors.CornflowerBlue,
                    Child = new Center
                    {
                        Child = new Text(_count.ToString())
                        {
                            Style = new TextStyle
                            {
                                Color = SKColors.White,
                                FontSize = 120,
                                FontWeight = 700
                            }
                        }
                    }
                }
            }
        };
    }
}
```

段1のコードから、3か所が変わっています。

- 画面を `CounterPanel` というウィジェットに分けた。値を持つウィジェットは、`StatefulWidget<T>` を継承した型と、値を入れる `State<T>` の2つで書く
- `Build()` が、`_count` の今の値で画面を返す。`SetState()` を呼ぶと、`_count` が書き換わり、`Build()` がもう一度呼ばれて画面が描き直される
- 数字を増やすきっかけに、`WidgetTicker` を使った。`WidgetTicker` は、フレームごとに開始からの経過時間を渡す部品で、フレームを処理するメインのスレッドから呼ばれる

きっかけを `System.Threading.Timer` にしないのは、別のスレッドから `SetState()` を呼ぶと、画面の更新が失われることがあるためです(追跡: #150)。`WidgetTicker` はフレームを処理するスレッドで呼ばれるので、このコードのままで `SetState()` を呼べます。

もう一度、SteamVR を起動してから `dotnet run --project FirstOverlay` を実行し、ダッシュボードを開きます。<!-- ※要確認: 数字が1秒ごとに増えるか。HMD(または --desktop 相当の表示)で実機確認していない。コンパイルだけ確かめた -->

**できた?** 青い四角の中央に数字が出て、1秒ごとに1つずつ増えていく。

<details>
<summary>できなかったら</summary>

数字が 0 のまま変わらないときは、ガイドの[数字や文字が変わらない](../guide/5-troubleshoot/not-responding.md#数字や文字が変わらない)を開きます。数字そのものが出ないときは、[文字や画像が出ない](../guide/5-troubleshoot/not-responding.md#文字や画像が出ない)です。

</details>

`SetState()` が画面を描き直す仕組みは、ガイドの[状態と作り直し](../guide/4-build/mechanics/state-and-rebuild.md)で説明しています。

</details>

## 3. ボタンを押して、数字を増やす

3段目では、数字の下にボタンを足し、押すと数字が1つ増えるようにします。`GestureDetector` で、押された操作を受け取る領域を作り、見た目は他のウィジェットで組み立てます。

`Program.cs` を次の内容に置き換えてください。1秒ごとに増やす `WidgetTicker` は外し、`TickerProviderState` を `State` に戻しています。

```csharp
using FloatSoda;
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Gesture;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SkiaSharp;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddFloatSoda();

using var host = builder.Build();
var app = host.Services.GetRequiredService<FloatSodaApp>();

app.CreateWindow(new DashboardWindow
{
    Title = "FirstOverlay",
    Child = new CounterPanel()
});

await host.RunAsync();

record CounterPanel : StatefulWidget<CounterPanel>
{
    public override State<CounterPanel> CreateState() => new CounterPanelState();
}

class CounterPanelState : State<CounterPanel>
{
    private int _count;

    public override Widget Build(IBuildContext context)
    {
        return new Align
        {
            Alignment = Alignment.Center,
            Child = new SizedBox
            {
                Width = 300,
                Height = 300,
                Child = new ColoredBox
                {
                    Color = SKColors.CornflowerBlue,
                    Child = new Flex
                    {
                        Direction = Axis.Vertical,
                        MainAxisAlignment = MainAxisAlignment.Center,
                        CrossAxisAlignment = CrossAxisAlignment.Center,
                        Children =
                        {
                            new Text(_count.ToString())
                            {
                                Style = new TextStyle
                                {
                                    Color = SKColors.White,
                                    FontSize = 120,
                                    FontWeight = 700
                                }
                            },
                            new GestureDetector
                            {
                                OnTap = () => SetState(() => _count++),
                                Child = new SizedBox
                                {
                                    Width = 200,
                                    Height = 80,
                                    Child = new ColoredBox
                                    {
                                        Color = SKColors.Tomato,
                                        Child = new Center
                                        {
                                            Child = new Text("+1")
                                            {
                                                Style = new TextStyle
                                                {
                                                    Color = SKColors.White,
                                                    FontSize = 50,
                                                    FontWeight = 700
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
    }
}
```

画面の組み立てとタップの処理の変更点は、次のとおりです。

- `Flex` に `Direction = Axis.Vertical` を指定して、数字とボタンを縦に並べた
- 赤い四角を `GestureDetector` で包み、タップされたときに `OnTap` の処理で `SetState()` を呼んで数字を増やす

前の実行をターミナルの Ctrl+C で終了してから、SteamVR を起動した状態で `dotnet run --project FirstOverlay` を実行します。HMD を被り、ダッシュボードを開いて、コントローラーのレーザーで赤い四角を押します。<!-- ※要確認: GestureDetector のボタンがダッシュボードのオーバーレイで実機で反応するか。HMD で確かめていない。コンパイルだけ確かめた -->

**できた?** 赤い四角を押すたびに、数字が1つ増える。

<details>
<summary>できなかったら</summary>

押しても反応しないときは、ガイドの[ボタンを押しても反応しない](../guide/5-troubleshoot/not-responding.md#ボタンを押しても反応しない)を開きます。`DashboardWindow` ではなく `WorldSpaceWindow` や `DeviceTrackedWindow` で試した人は、リファレンスの[できないこと・まだ使えないもの](../reference/limitations.md#置く場所と入力)を確かめてください。SteamVR 上のオーバーレイでポインターの座標が届く範囲も、そのページにあります(追跡: #182)。

</details>

ボタンの組み立て方は、ガイドの[ボタンやトグルを組み立てる](../guide/4-build/screen/build-buttons-toggles.md)で詳しく説明しています。

## 次に読むページ

動くオーバーレイができました。次に進むページは、目的で選べます。

- 通しで1つのパネルを作りたい: ガイドの[表情切り替えパネルを作る](../guide/4-build/screen/expression-switcher.md)
- 作りたいものに近い作例から始めたい: リファレンスの[作例のカタログ](../reference/sample-catalog.md)
