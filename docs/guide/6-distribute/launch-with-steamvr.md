← [ガイド](../Home.md)

# SteamVR と一緒に起動する

## この手順でできること

アプリを SteamVR に登録して、SteamVR を起動すると、オーバーレイも自動で起動するようにします。SteamVR を起動するたびに、手で `.exe` を動かす手間がなくなります。手順は 3 つです。

1. アプリのマニフェストを用意する
2. SteamVR に登録する
3. 自動起動を有効にする

「一緒に起動する」は、SteamVR の自動起動の設定を指します。Windows のスタートアップへの登録は扱いません。

マニフェストの自動生成は検討中で、今は手動で用意します(追跡: #139)。このページの手順は、リポジトリ内に動作例がなく、実機でも確かめていません。確かめていない箇所には、本文のすぐ後ろに HTML コメントで印を付けています。

## 前提条件

次の 2 つを済ませてください。

- [exe にして配る](publish-exe.md) の手順 1 を終え、`publish` フォルダーに `.exe` があること
- `AppKey` が、アプリごとに固有の値であること

`AppKey` は、`FloatSodaOptions` で指定します。空にはできません。使える文字は、小文字の英字、数字、ドット、アンダースコアで、127 文字までです。

```csharp
builder.Services.AddFloatSoda(new FloatSodaOptions
{
    AppKey = new AppKey("example.my_overlay"),
});
```

## 手順1: アプリのマニフェストを用意する

> 【未決】この節は判断が要るため、ト書きのまま残しています。オーナーが決めたあとで本文にします(追跡: #219)。

- 読み手が持って来るもの: `AppKey` と exe のパス
- このページが渡すもの: マニフェストの書き方(最小の1例)と、置き場所
- 読み手がすること: マニフェストを書く
- できた?: マニフェストのファイルが指定の場所にある
- 次へ進む先: 手順2
- 【未決】マニフェストの書き方を、このページに書くか、SteamVR の公式資料へのリンクで済ませるか。リポジトリに一次情報がないため、公式資料を読んでから決める

<details>
<summary>仮に書いた本文(判断の材料。採用は未定)</summary>

マニフェストは、SteamVR にアプリの名前と起動する `.exe` の場所を伝える JSON のファイルです。`.exe` と同じフォルダーに、`my_overlay.vrmanifest` という名前で置きます。

```json
{
  "source": "builtin",
  "applications": [
    {
      "app_key": "example.my_overlay",
      "launch_type": "binary",
      "binary_path_windows": "MyOverlay.exe",
      "is_dashboard_overlay": true,
      "strings": {
        "en_us": {
          "name": "My Overlay"
        }
      }
    }
  ]
}
```

`app_key` は、`AppKey` に指定した値と同じにします。`binary_path_windows` は、マニフェストから見た `.exe` の場所です。ダッシュボードのオーバーレイを自動起動するので、`is_dashboard_overlay` を `true` にします。

**できた?** マニフェストのファイルが、`.exe` と同じフォルダーにある。

<!-- ※要確認: マニフェストの項目名と書き方は、リポジトリのコードやサンプルに例がなく、SteamVR(OpenVR)の公式資料でも確かめていない。上の例は OpenVR のアプリケーションマニフェストの形式として書いた。binary_path_windows の基準となる場所、必須の項目、置き場所は、OpenVR の公式資料を読んで確かめる -->

</details>

## 手順2: SteamVR に登録する

> 【未決】この節は判断が要るため、ト書きのまま残しています。オーナーが決めたあとで本文にします(追跡: #219)。

- 読み手が持って来るもの: マニフェスト
- このページが渡すもの: `AddManifest` の呼び方と、登録済みかを `Installed` で確かめる方法
- 読み手がすること: 登録する
- できた?: `Installed` が真になる
- 次へ進む先: 手順3

<details>
<summary>仮に書いた本文(判断の材料。採用は未定)</summary>

マニフェストの登録は、`OVRApplications.AddManifest` で行います。引数には、マニフェストの絶対パスを渡します。登録は、SteamVR に OpenVR の接続ができている間に行うので、登録専用の起動モードをアプリに足します。

```csharp
using FloatSoda.OVR;

if (args.Contains("--register-steamvr"))
{
    var appKey = new AppKey("example.my_overlay");
    using var ovr = new OVRApplication(new OVRAppInfo(appKey, ApplicationType.Utility));

    var manifestPath = Path.Combine(AppContext.BaseDirectory, "my_overlay.vrmanifest");
    OVRApplications.AddManifest(manifestPath);

    Console.WriteLine($"登録済み: {ovr.Installed}");
    return;
}
```

このコードは、`Host.CreateApplicationBuilder` を呼ぶ前、`Program.cs` の先頭に置きます。`ApplicationType.Utility` は、HMD がなくても起動し、アプリの登録に関する API が使える種類です。

`.exe` を `--register-steamvr` を付けて 1 回だけ動かします。

```powershell
.\MyOverlay.exe --register-steamvr
```

**できた?** コンソールに `登録済み: True` と出る。

<!-- ※要確認: ApplicationType.Utility で OpenVR を初期化して AddManifest を呼ぶ流れは、ソース(OVRApplication.cs の注記)から組み立てた。実機で動くこと、SteamVR が起動していなくてもよいかどうかは確かめていない。FloatSodaApp は内部の OVRApplication を公開していないため、FloatSoda の起動とは別に OVRApplication を作る形にした -->

</details>

## 手順3: 自動起動を有効にする

> 【未決】この節は判断が要るため、ト書きのまま残しています。オーナーが決めたあとで本文にします(追跡: #219)。

- 読み手が持って来るもの: 登録済みのアプリ
- このページが渡すもの: `AutoLaunch` を設定する書き方
- 読み手がすること: 有効にする
- できた?: SteamVR の設定で、自動起動の項目にアプリが出る
- 次へ進む先: 「結果を確かめる」

<details>
<summary>仮に書いた本文(判断の材料。採用は未定)</summary>

登録したアプリの自動起動は、`OVRApplication.AutoLaunch` で有効にします。手順 2 のコードに 1 行足します。

```csharp
    OVRApplications.AddManifest(manifestPath);
    ovr.AutoLaunch = true;
```

`AutoLaunch` は、アプリのキーが SteamVR に登録済みであることが前提です。登録の前に設定すると、例外になります。

手順 2 と同じコマンドを、もう一度実行します。

```powershell
.\MyOverlay.exe --register-steamvr
```

**できた?** SteamVR の設定で、自動起動の項目にアプリの名前が出る。

<!-- ※要確認: SteamVR の設定画面のどこにアプリが出るかは、実機で確かめていない。AutoLaunch が登録の前に設定されたときに投げる例外の種類は、VRApplicationException(OpenVR のエラーを包む型)だと読めるが、実行して確かめていない -->

</details>

## 結果を確かめる

アプリと SteamVR を終了し、アプリのプロセスが残っていないことを確かめます。そのあと SteamVR だけを起動します。アプリを手で動かさなくても、ダッシュボードにオーバーレイが出れば、設定は効いています。

配布先の PC で自動起動させるときも、手順 2 の登録の実行が、その PC で 1 回必要です。マニフェストを配布物に同梱し、説明書に登録の手順を書いてください([Booth で売る準備](booth-release.md))。

<!-- ※要確認: 配布先での、マニフェストの置き場所と登録の手順は、実機で確かめていない -->

配布先で動かないときは、[利用者の環境で動かないとき](../7-maintain/user-environment-issues.md) で切り分けます。

## うまくいかないとき

自動で起動しないとき、または起動時に `SteamVRへのアプリケーション識別に失敗しました` という警告が出るときは、`AppKey` が SteamVR に登録されていない可能性があります。次の 4 点を確かめます。

- マニフェストの `app_key` が、`FloatSodaOptions` の `AppKey` と同じ値であること
- 手順 2 の登録を実行して、`登録済み: True` と出たこと
- 自動起動が有効であること
- マニフェストのファイルと `.exe` を、登録のあとに移動していないこと

警告が出たときは、警告の後ろに続くメッセージも確かめます。

この警告が出ても、オーバーレイの表示は続きます。止まるのは、自動起動など、マニフェストに関わる機能だけです。警告の詳細は [エラーを読んで直す](../5-troubleshoot/read-errors.md#起動で警告が出るが表示はされる) にあります。

なお、コントローラー入力のアクションマニフェストは、自動で生成されます。このページのアプリのマニフェストとは別のものです。
