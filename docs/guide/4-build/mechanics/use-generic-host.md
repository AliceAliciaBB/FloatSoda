← [ガイド](../../Home.md)

# 設定やログ、裏で動く処理を足す

## この手順でできること

FloatSoda の起動には、.NET の Generic Host が使われています。そのため、設定ファイル、ログ、サービスの登録、裏で動く処理は、FloatSoda 用に作り直さず、.NET の標準の書き方でそのまま足せます。たとえば、設定ファイルの値で動きを変え、裏で動く処理の結果を画面に出す、といった物を形にできます。

この手順では、次の4つを順に足します。

- 設定: 設定ファイルの値を読む
- ログ: コンソールにログを出す
- サービス: 自分のクラスを登録して、画面から取り出す
- 裏で動く処理: `BackgroundService` を足して、結果を画面に出す

終わると、設定の値がコンソールに出て、ログが流れ、サービスの値と裏の処理の結果が画面に出ます。足したいものだけを選んで読んでもかまいませんが、手順3は手順4の前提です。

手順を足し合わせるときの条件は、次のとおりです。

- 設定の読み込み、ログの設定、サービスの登録は、すべて `builder.Build()` より前に置くこと
- `builder.Build()` は1回だけ呼ぶこと

## 前提条件

- [最初のオーバーレイを作る](../../../tutorials/first-overlay.md)を終え、`Host.CreateApplicationBuilder` を使う起動のコードが動くこと
- 起動のコードの各行の意味が分かっていること
- 起動のコードを編集し、ビルド結果と実行時のログを確認できること

起動のコードの意味が分からない場合は、[起動のコードは何をしているの?](startup-and-host.md)を先に読みます。

## 手順1: 設定の値を読む

`builder.Configuration` から値を読み、`FloatSodaOptions` に渡します。次の例は、フレームレートの目標値を設定ファイルで決めます。値が無いときは 60 にします。

まず、プロジェクトに `appsettings.json` を作り、ビルドしたときに出力先へコピーされるようにします。

```json
{
  "FloatSoda": {
    "TargetFrameRate": 90
  }
}
```

```xml
<ItemGroup>
  <None Update="appsettings.json" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

次に、起動のコードで値を読みます。

```csharp
var builder = Host.CreateApplicationBuilder(args);

var frameRate = builder.Configuration.GetValue("FloatSoda:TargetFrameRate", 60);
Console.WriteLine($"TargetFrameRate: {frameRate}");

builder.Services.AddFloatSoda(new FloatSodaOptions { TargetFrameRate = frameRate });
```

`TargetFrameRate` に 0 以下の値を渡すと、`AddFloatSoda` が `ArgumentOutOfRangeException` を出します。

**できた?** 起動したとき、コンソールに `TargetFrameRate: 90` と出れば、手順1は完了です。`appsettings.json` の値を変えて再起動すると、表示も変わります。

<!-- ※要確認: appsettings.json が Host.CreateApplicationBuilder の既定どおり読まれ、公開した exe の隣でも読まれるか。実行して確かめる -->

<details>
<summary>できなかったら</summary>

表示が `TargetFrameRate: 60` のままのときは、`appsettings.json` が出力先にコピーされているかを確かめます。出力先のフォルダーに `appsettings.json` が無ければ、上の `CopyToOutputDirectory` の指定を見直します。Host は `appsettings.json` を作業フォルダー(コンテンツルート)から読むため、実際に起動したフォルダーに `appsettings.json` があるかも確かめます。

</details>

## 手順2: ログを出す

`builder.Logging` の設定は、FloatSoda 自身のログにもそのまま効きます。自分のログは、`ILogger<T>` を取り出して書きます。

```csharp
using Microsoft.Extensions.Logging;

builder.Logging.SetMinimumLevel(LogLevel.Debug);

using var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("起動します");
```

**できた?** コンソールに `起動します` が出て、ウィンドウを作ったあとに、`<Title>を作成しました` の形のログが続けば、手順2は完了です。後者は FloatSoda 自身が出すログで、`<Title>` にはウィンドウの `Title` が入ります。

<details>
<summary>できなかったら</summary>

ログが何も出ないときは、`SetMinimumLevel` の指定と、`appsettings.json` の `Logging` の指定を見直します。`LogLevel.None` は、ログを出さない指定です。

</details>

## 手順3: サービスを登録して、画面で使う

> 【未決】この節は判断が要るため、ト書きのまま残しています。オーナーが決めたあとで本文にします(追跡: #219)。

- 読み手が持って来るもの: 画面のウィジェット
- このページが渡すもの: `builder.Services` への登録と、`ServiceProvider` ウィジェットで包んでから `context.GetRequiredService<T>()` で取り出す流れ
- 読み手がすること: サービスを登録し、画面で取り出す
- できた?: 画面にサービスの値が出る
- 次へ進む先: 手順4
- 【未決】`ServiceProvider` で包む手順を毎回書かせる形のままにするか、FloatSoda 側に自動挿入を求める Issue を立てるか。後者は読み手ではなく開発側の判断

<details>
<summary>仮に書いた本文(判断の材料。採用は未定)</summary>

自分のクラスは、`builder.Services` に登録します。画面のウィジェットから取り出すには、ウィンドウの `Child` を `ServiceProvider` ウィジェットで包んで、`IServiceProvider` を渡します。

```csharp
public class GreetingService
{
    public string Message => "こんにちは";
}
```

```csharp
builder.Services.AddSingleton<GreetingService>();
```

画面側のウィジェットでは、`context.GetRequiredService<T>()` で取り出します。

```csharp
public record GreetingView : StatelessWidget
{
    public override Widget Build(IBuildContext context)
    {
        var greeting = context.GetRequiredService<GreetingService>();
        return new Text(greeting.Message);
    }
}
```

最後に、ウィンドウを作るところで `ServiceProvider` を挟みます。

```csharp
var app = host.Services.GetRequiredService<FloatSodaApp>();

app.CreateWindow(new DashboardWindow
{
    Dpm = new Dpm(1000),
    Title = "Greeting",
    Child = new FloatSoda.Widgets.ServiceProvider
    {
        Services = host.Services,
        Child = new GreetingView(),
    },
});
```

ここで `FloatSoda.Widgets.ServiceProvider` と、名前空間つきで書く理由は、`Microsoft.Extensions.DependencyInjection` にも同じ名前の `ServiceProvider` があり、`ServiceProvider` だけでは、どちらか決まらずコンパイルが通らないためです。

**できた?** ダッシュボードのタブ `Greeting` に、`こんにちは` が出れば、手順3は完了です。

<!-- ※要確認: HMD のダッシュボードに表示されること。コードのコンパイルは確かめた。表示は HMD で確かめる -->

<details>
<summary>できなかったら</summary>

`ServiceScopeが祖先に見つかりません。` というメッセージで例外が出るときは、`GetRequiredService<T>()` を呼ぶウィジェットが、`ServiceProvider` の子孫に置かれているかを確かめます。サービスが登録されていないという例外のときは、`builder.Services.Add...` の行があるかを確かめます。

</details>

</details>

## 手順4: 裏で動く処理を足す

裏で動く処理は、`BackgroundService` を継承したクラスを作り、`AddHostedService` で足します。FloatSoda 自身も `IHostedService` として登録されているので、同じ仕組みの上で並んで動きます。

裏の処理から画面に値を渡すには、`TaskBuilder<T>` を使います。`TaskBuilder<T>` は、`Task` が終わるのを待って、表示を切り替えるウィジェットです。次の例では、サービスが結果の入れ物(`TaskCompletionSource`)を持ち、裏の処理が3秒後に結果を入れます。

```csharp
public class StatusService
{
    private readonly TaskCompletionSource<string> _ready = new();

    public Task<string> Ready => _ready.Task;

    public void SetReady(string text) => _ready.TrySetResult(text);
}

public class Worker(StatusService status, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("裏の処理を始めます");
        await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
        status.SetReady("完了しました");
    }
}
```

```csharp
builder.Services.AddSingleton<StatusService>();
builder.Services.AddHostedService<Worker>();
```

画面側は、サービスの `Ready` を `TaskBuilder<string>` に渡します。

```csharp
public record StatusView : StatelessWidget
{
    public override Widget Build(IBuildContext context)
    {
        var status = context.GetRequiredService<StatusService>();

        return new TaskBuilder<string>
        {
            Task = status.Ready,
            Builder = (_, snapshot) => new Text(snapshot.HasData ? snapshot.Data! : "読み込み中"),
        };
    }
}
```

`StatusView` は、手順3と同じように、`ServiceProvider` の子孫に置きます。

別のスレッドから `SetState()` を呼ぶ書き方は避けます。この手順の方法なら、`Task` の完了は、ウィジェットのツリーを処理するメインスレッドに戻されて、`TaskBuilder<T>` が受け取ります。この手順で扱うのは、1回きりの結果の反映です。値を何度も更新し続ける方法は、ここでは扱いません。

<!-- ※要確認: 別スレッドから SetState() を呼んで安全か。BuildOwner の dirty リストが排他制御されていないことをコードで読んだ。実行して確かめる -->

**できた?** 裏の処理が終わるまでは `読み込み中`、終わったあとは `完了しました` と表示されれば、手順4は完了です。`Worker` は起動から3秒ほどで処理を終えます。

<details>
<summary>できなかったら</summary>

`読み込み中` のまま変わらないときは、`Worker` が起動しているかを、ログで確かめます。`裏の処理を始めます` が出ていなければ、`AddHostedService<Worker>()` の行があるかを見直します。

</details>

## 結果を確かめる

起動して、次の4点を見ます。

- 設定: コンソールに、`appsettings.json` で決めた値が出る
- ログ: 自分のログと、FloatSoda のログが、同じ設定で出る
- サービス: 画面に、登録したサービスの値が出る
- 裏の処理: 画面の表示が、処理の終わりに合わせて切り替わる

作ったものを配るときは、[exe にして配る](../../6-distribute/publish-exe.md)に進みます。

## うまくいかないとき

起動で例外が出る、または値が画面に出ないときは、[エラーを読んで直す](../../5-troubleshoot/read-errors.md)で原因を切り分けます。
