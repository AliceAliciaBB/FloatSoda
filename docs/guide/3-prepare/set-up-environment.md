← [ガイド](../Home.md)

# 開発環境を用意する

この手順で、自分の PC に .NET の SDK を入れ、FloatSoda を足した空のコンソールアプリを作って、SteamVR のダッシュボードに色の四角を出せます。手順は5つです。終わると、FloatSoda を足し、ダッシュボードに色の四角を出すコンソールアプリが手元にそろいます。チュートリアル[最初のオーバーレイを作る](../../tutorials/first-overlay.md)の最初に要る「空のコンソールアプリ」は、手順2と手順3で作るものと同じです。

エディター(IDE)は指定しません。好きなものを使ってください。必要な設定も、この手順にはありません。

## 始める前に確かめること

次の2つが必要です。

- SteamVR が PC に入っていること(Steam から入れる)
- VR の機材が PC につながっていること

動作環境の全体は、リファレンスの[動作環境](../../reference/requirements.md)にあります。SteamVR が入っていない、または OS が対応していないときは、ここでは進めず、動作環境を確かめてから戻ってください。

HMD を被らずに確かめたいときは、[HMD を被らずに確かめる](../4-build/input/test-without-hmd.md)を読んでください。

## 手順1: .NET の SDK を入れる

.NET 10 の SDK を、公式の配布元(<https://dotnet.microsoft.com/ja-jp/download>)から入れる。

環境変数と PATH の設定は、インストーラーが行います。

入れたら、ターミナルで次を実行する。

```powershell
dotnet --version
```

**できた?** `10.` で始まる版が表示された。

<details>
<summary>できなかったら</summary>

「コマンドが見つかりません」と出たときは、ターミナルを開き直してください。インストーラーが設定した PATH は、開き直したターミナルから有効になります。それでも出ないときは、SDK(ランタイムだけではない)を入れたかを確かめてください。`10.` より前の版が表示されたときは、`dotnet --list-sdks` で一覧を見て、10 系の SDK が入っているかを確かめてください。

</details>

<!-- ※要確認: SDK の入手元と、Windows 以外での手順。ここでは Windows の公式ダウンロードページだけを示している。確かめ方: 動作環境ページの OS の記述と合わせて、公式ページで対応 OS を確かめる -->

## 手順2: コンソールアプリを作る

作業用のフォルダーで、次を実行する。

```powershell
dotnet new console -n MyOverlayApp
cd MyOverlayApp
```

**できた?** `MyOverlayApp` フォルダーができ、その中に `Program.cs` と `MyOverlayApp.csproj` がある。

## 手順3: FloatSoda を足す

> 【未決】この節は判断が要るため、ト書きのまま残しています。オーナーが決めたあとで本文にします(追跡: #219)。

- 読み手が持って来るもの: コンソールアプリ
- このページが渡すもの: FloatSoda を足す方法を1つ(NuGet)。プロジェクト参照の方法は、コード例を脇に置くだけ
- 読み手がすること: コマンドを実行する
- できた?: プロジェクトファイルに FloatSoda の参照が入った
- 次へ進む先: 手順4
- 【未決】NuGet 参照とプロジェクト参照を併記するか。手順書は1つに決めるが、公開版が古いときの扱いが未確認

<details>
<summary>仮に書いた本文(判断の材料。採用は未定)</summary>

`MyOverlayApp` フォルダーで、NuGet から FloatSoda を足します。

```powershell
dotnet add package FloatSoda
```

**できた?** `MyOverlayApp.csproj` に `FloatSoda` の `PackageReference` が入った。

<details>
<summary>できなかったら</summary>

「パッケージが見つかりません」と出たときは、ネットワークと NuGet の接続を確かめてください。FloatSoda のリポジトリを取得して使うときは、`dotnet add reference <リポジトリ>/src/FloatSoda/FloatSoda.csproj` で、プロジェクト参照として足せます。

</details>

<!-- ※要確認: `dotnet add package FloatSoda` が通ること。PackageId は src/FloatSoda/FloatSoda.csproj の FloatSoda で、CHANGELOG の 0.4.0 が公開済みの版。確かめ方: nuget.org で FloatSoda のページを開き、新規プロジェクトで実行する -->
<!-- ※要確認: プロジェクト参照の手順が今も有効か。確かめ方: リポジトリを取得して dotnet add reference を実行し、dotnet build を通す -->

</details>

## 手順4: ビルドして確かめる

次を実行する。

```powershell
dotnet build
```

**できた?** 「ビルドに成功しました」と出た。

<details>
<summary>できなかったら</summary>

警告ではなく、エラーが出たときは、[エラーを読んで直す](../5-troubleshoot/read-errors.md)を読んでください。

</details>

## 手順5: SteamVR で色の四角を出す

SteamVR を起動してから、`Program.cs` を次のコードに書き換える。

```csharp
using FloatSoda;
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
    Child = new SizedBox
    {
        Width = 100,
        Height = 100,
        Child = new ColoredBox
        {
            Color = SKColors.Tomato
        }
    }
};

app.CreateWindow(new DashboardWindow { Title = "MyDashboard", Child = root });

await host.RunAsync();
```

SteamVR を起動したまま、次を実行する。

```powershell
dotnet run
```

SteamVR のダッシュボードを開く。

**できた?** ダッシュボードに、トマト色(赤みのあるだいだい色)の四角が出た。

<details>
<summary>できなかったら</summary>

四角が出ないときは、まず SteamVR が起動しているかを確かめてください。FloatSoda は、SteamVR が起動していない状態では動きません。アプリが終了したり例外が出たりしたときは、[エラーを読んで直す](../5-troubleshoot/read-errors.md)を、起動しているのに出ないときは、[反応しないときに切り分ける](../5-troubleshoot/not-responding.md)を読んでください。環境の要件は、リファレンスの[動作環境](../../reference/requirements.md)にあります。

</details>

<!-- ※要確認: 手順5の結果(ダッシュボードに四角が出る)。HMD と SteamVR でしか確かめられない。確かめ方: 空のコンソールアプリで上のコードを実行し、ダッシュボードを開く。コードは README.md の最小構成と同じ -->

最後まで進めたら、チュートリアル[最初のオーバーレイを作る](../../tutorials/first-overlay.md)へ進んでください。リポジトリを取得した人は、リポジトリのルートに移動して `dotnet run --project samples/FloatSoda.Samples.GettingStarted` を実行すると、同じ四角を出す作例も動かせます。
