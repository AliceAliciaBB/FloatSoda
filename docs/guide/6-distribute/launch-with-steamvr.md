← [ガイド](../Home.md)

# SteamVR と一緒に起動する

## この手順でできること

アプリを SteamVR に登録して、SteamVR を起動すると、オーバーレイも自動で起動するようにします。SteamVR を起動するたびに、手で `.exe` を動かす手間がなくなります。手順は 3 つです。

1. アプリのマニフェストを用意する
2. SteamVR に登録する
3. 自動起動を有効にする

「一緒に起動する」は、SteamVR の自動起動の設定を指します。Windows のスタートアップへの登録は扱いません。

マニフェストの自動生成は検討中で、今は手動で用意します(追跡: #139)。マニフェストの書き方と、登録・自動起動の呼び出し方は、OpenVR の公式資料にあります。このページでは、FloatSoda から使う型と、手順の順番を案内します。

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

SteamVR にアプリを登録するには、アプリの情報(アプリのキー、`.exe` の場所など)を書いたマニフェストのファイルが要ります。マニフェストの書き方は、OpenVR の公式資料で確かめてください。

- [OpenVR API Documentation](https://github.com/ValveSoftware/openvr/wiki/API-Documentation)

マニフェストに書くアプリのキーは、`FloatSodaOptions` の `AppKey` と同じ値にします。

## 手順2: SteamVR に登録する

マニフェストを SteamVR に登録します。FloatSoda からは、`FloatSoda.OVR` の `OVRApplications.AddManifest(path)` で登録できます。これは OpenVR の `IVRApplications` の `AddApplicationManifest` を呼ぶものです。登録の手順と、登録できたかの確かめ方は、OpenVR の公式資料で確かめてください。

- [OpenVR API Documentation](https://github.com/ValveSoftware/openvr/wiki/API-Documentation)

## 手順3: 自動起動を有効にする

登録したアプリの自動起動を有効にします。自動起動は、OpenVR の `IVRApplications` の `SetApplicationAutoLaunch` で切り替えます。FloatSoda では、`OVRApplication` の `AutoLaunch` がこの呼び出しを包んでいます。ただし、`FloatSodaApp` は内部で使う `OVRApplication` を外に出していないため、アプリの中からこのプロパティを切り替える手段は今はありません(追跡: #139)。呼び出し方は、OpenVR の公式資料で確かめてください。

- [OpenVR API Documentation](https://github.com/ValveSoftware/openvr/wiki/API-Documentation)

## 結果を確かめる

アプリと SteamVR を終了し、アプリのプロセスが残っていないことを確かめます。そのあと SteamVR だけを起動します。アプリを手で動かさなくても、ダッシュボードにオーバーレイが出れば、設定は効いています。

配布先の PC で自動起動させるときも、手順 2 の登録の実行が、その PC で 1 回必要です。マニフェストを配布物に同梱し、説明書に登録の手順を書いてください([Booth で売る準備](booth-release.md))。

<!-- ※要確認: 配布先での、マニフェストの置き場所と登録の手順は、実機で確かめていない -->

配布先で動かないときは、[利用者の環境で動かないとき](../7-maintain/user-environment-issues.md) で切り分けます。

## うまくいかないとき

自動で起動しないとき、または起動時に `SteamVRへのアプリケーション識別に失敗しました` という警告が出るときは、`AppKey` が SteamVR に登録されていない可能性があります。次の 4 点を確かめます。

- マニフェストに書いたアプリのキーが、`FloatSodaOptions` の `AppKey` と同じ値であること
- 手順 2 の登録が成功していること
- 自動起動が有効であること
- マニフェストのファイルと `.exe` を、登録のあとに移動していないこと

警告が出たときは、警告の後ろに続くメッセージも確かめます。

この警告が出ても、オーバーレイの表示は続きます。止まるのは、自動起動など、マニフェストに関わる機能だけです。警告の詳細は [エラーを読んで直す](../5-troubleshoot/read-errors.md#起動で警告が出るが表示はされる) にあります。

なお、コントローラー入力のアクションマニフェストは、自動で生成されます。このページのアプリのマニフェストとは別のものです。
