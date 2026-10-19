← [ガイド](../../Home.md)

# 起動のコードは何をしているの?

## 起動のコードは、5つのことをしている

チュートリアルで貼った起動のコードは、ホストを作る、FloatSoda を登録する、ウィンドウを予約する、動き始める、終わる、の5つをしています。各行の意味が分かると、AI が書き換えたコードを点検でき、起動に失敗したときも原因を探せます。

次のコードの番号が、5つのうち最初の4つに対応します。5つ目の「終わる」は、コードに書かれた行ではなく、`RunAsync` の中で起きます。このコードは、`AddFloatSoda()` を設定なしで呼ぶ最小の形です。アプリごとに別の `AppKey` を指定する形は、「ホストの準備」の節で説明します。

```csharp
var builder = Host.CreateApplicationBuilder(args);          // 1. ホストを作る
builder.Services.AddFloatSoda();                            // 2. FloatSoda を登録する

using var host = builder.Build();
var app = host.Services.GetRequiredService<FloatSodaApp>(); // 3. ウィンドウを作る FloatSodaApp を取り出す

app.CreateWindow(new DashboardWindow                        // 3. ウィンドウを予約する
{
    Title = "MyDashboard",
    Child = root,
});

await host.RunAsync();                                      // 4. 動き始める
```

## ホストの準備: Host.CreateApplicationBuilder と AddFloatSoda

Generic Host は、設定、ログ、サービスの登録(依存性注入)、起動と終了の管理をまとめて持つ、.NET の標準の土台です。FloatSoda は、独自の起動の仕組みを作らず、この土台の上で動きます。詳しい説明は、.NET の公式資料の[.NET Generic Host](https://learn.microsoft.com/dotnet/core/extensions/generic-host)にあります。

`Host.CreateApplicationBuilder(args)` は、その土台の builder を作ります。次の `AddFloatSoda()` が登録するもののうち、起動のコードを読むために知っておきたいものを挙げます。内部の登録のすべてを示すものではありません。

- FloatSoda の設定(`FloatSodaOptions`)
- SteamVR がアプリを見分ける `AppKey` を持つ、アプリの情報(`OVRAppInfo`)
- フレームの間隔を管理する仕組み(`IFramePacer`)
- `FloatSodaApp`
- ホストが動き始めたときに FloatSoda を動かす役の `FloatSodaHostedService`

設定を変えたいときは、`AddFloatSoda(new FloatSodaOptions { ... })` の形で渡します。`AppKey` の既定は `FloatSoda` で、アプリごとに別の値を指定します。

<!-- ※要確認: `AppKey` を省略した複数のアプリが同時に動いたときの問題。実機で確かめる -->

## アプリの取り出しと、ウィンドウの予約

`host.Services.GetRequiredService<FloatSodaApp>()` は、登録済みの `FloatSodaApp` を取り出します。ウィンドウを作る操作 `CreateWindow` が、この `FloatSodaApp` にあるためです。

`CreateWindow` は、ウィンドウの作成を予約します。その場では作りません。メインループが動き始めて、最初の1周を回るときに、予約が実行されます。そのため、`RunAsync` の前に `CreateWindow` を呼んでも、ウィンドウは動き始めたあとに出ます。

`builder.Build()` の時点では、SteamVR にはつながりません。つなぐのは、`RunAsync` で動き始めたあとです。

<!-- ※要確認: Build() の時点で SteamVR へつながないこと。FloatSodaApp の生成と OpenVR の初期化の位置をコードで読んだ。実行しては確かめていない -->

## RunAsync で動き始める

`host.RunAsync()` を呼ぶと、ホストが `FloatSodaHostedService` を始めます。FloatSoda は、次の順に動き始めます。

1. FloatSoda 専用のメインスレッドを作る。Windows では STA として作る
2. そのスレッドで OpenVR を初期化する
3. 初期化に成功したら、ホストへ「起動が済んだ」と知らせる
4. メインループを回し続ける

メインループの1周で行うことは、[FloatSoda の仕組み](how-floatsoda-works.md)にあります。

## 起動に失敗するとき

起動の失敗は、止まる場合と続く場合に分かれます。

| 何が失敗したか | 起動はどうなるか | 見える表示 |
| :-- | :-- | :-- |
| OpenVR の初期化 | 止まる。`RunAsync` が例外で終わる | 「OpenVRの初期化に失敗しました」のログ |
| アプリの識別(SteamVR への登録) | 続く。警告だけが出る | 「SteamVRへのアプリケーション識別に失敗しました」のログ |
| コントローラー入力の初期化 | 続く。オーバーレイは表示される | 「アクション入力の初期化に失敗しました」のログ |

HMD を接続していない、または SteamVR を起動していないと、OpenVR の初期化で止まります。止まったときは、[エラーを読んで直す](../../5-troubleshoot/read-errors.md)で、メッセージから原因を切り分けます。

## 終わるとき

通常の終了は、2つの場面で始まります。SteamVR が終了を知らせたときと、ホストが止まるときです。メインループが例外で止まったときも終わります([FloatSoda の仕組み](how-floatsoda-works.md)を参照)。SteamVR を閉じると、FloatSoda のアプリも終わります。ホストが止まるのは、コンソールで Ctrl+C を押したときなどです。

メインループが止まれば、ホストも止まって、`RunAsync` が終わります。

ホストの設定、ログ、裏で動く処理を足したいときは、[設定やログ、裏で動く処理を足す](use-generic-host.md)に進みます。
