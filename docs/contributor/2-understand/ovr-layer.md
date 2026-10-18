← [コントリビューター](../Home.md)

# OVR 層

## OVR 層は、OpenVR の生のバインディングを包む

OVR 層(`FloatSoda.OVR`)は、OpenVR の初期化、オーバーレイ、イベント、アクション入力を、C# の型で包む層です。ほかの FloatSoda のプロジェクトを参照せず、ネイティブの `openvr_api.dll` を同梱します。上の層(エンジン層、フレームワーク層)は、この層の型を通して OpenVR を使います。

```text
openvr_api.cs、openvr_api.dll    OpenVR の生のバインディングと、ネイティブライブラリ
        ▲
OVRApplication、オーバーレイの型、イベントディスパッチャー、InputAction    C# の型で包んだ層
        ▲
エンジン層、フレームワーク層
```

直したい箇所は、この図で探します。OpenVR の呼び出し自体を直したいときは、2段目の型を見ます。生のバインディングは、次の節のとおり、直さずに読みます。

## OpenVR の資料の読み方

OpenVR の挙動は、FloatSoda のコードからは変えられません。調べるときの入口は、OpenVR の Wiki(<https://github.com/ValveSoftware/openvr/wiki>)です。C# の呼び出し署名は、同梱の `src/FloatSoda.OVR/openvr_api.cs` で確認します。挙動が資料と食い違うときは、資料とバインディングと実行環境のバージョンを記録し、食い違いを未解決として報告します。

`openvr_api.cs` は、Valve が自動生成したバインディングです。ファイルの冒頭に、編集しないよう書かれています。名前空間は `FloatSoda.OVR` です。

資料を読むときは、次の順に進めます。

1. 調べたい API を `openvr_api.cs` で探す
2. 同じ名前の項目を Wiki で探して、説明を読む
3. 署名に食い違いがあれば、`openvr_api.cs` の署名に従う

入力方式を例にすると、該当するのは `OpenVR.Overlay.SetOverlayInputMethod` です。FloatSoda では `OverlayInput.Method` が包んでいます。

<!-- ※要確認: Wiki と openvr_api.cs が食い違う実例 / Wiki の該当ページを開いて、署名を openvr_api.cs と比べる -->

## アプリケーションとオーバーレイ

`OVRApplication` は、作ると `OpenVR.Init` を呼びます。失敗は例外になります(`VRInitializeException` など)。種類を決める `OVRAppInfo` の `Type` の既定は `Overlay` です。`Dispose` で、`OpenVR.Shutdown` を呼びます。

`AppKey` は、SteamVR がアプリケーションを識別するキーです。型の説明が求める形は、ASCII の小文字、数字、ドット、アンダースコアだけで、127 文字までです。この規則は、`Value` の `init` アクセサーが検証します。つまり、`with` 式や初期化子で `Value` を設定すると、大文字を含む値は `FormatException`、128 文字以上は `ArgumentOutOfRangeException` になります。一方で、`new AppKey("...")` のコンストラクターは、検証を通りません。コンストラクターに渡す値は、規則に合う値にすること。

<!-- ※要確認: コンストラクターが検証を通らないことは、実行して確かめた(大文字、200 文字、空文字の3通りで例外にならなかった)。型の説明の「validated」と食い違うので、意図かを確かめる / OpenVR が規則に反するキーを受け付けるかは、OpenVR の資料で確かめる -->

オーバーレイは、`IOverlay` の型で表します。種類は3つです。

| 型 | 位置の決まり方 | 追加で持つもの |
| :-- | :-- | :-- |
| `DashboardOverlay` | SteamVR のダッシュボードが管理する。位置を持たない | `Thumbnail`(ダッシュボードのサムネイル) |
| `DeviceTrackedOverlay` | 追跡デバイスからの相対位置。既定の追従先は HMD | `Visibility`、`Intersection`、`Transform`(`Target` で追従先を選ぶ) |
| `WorldSpaceOverlay` | 追跡空間の絶対位置。`Origin` の既定は立った状態の空間 | `Visibility`、`Intersection`、`Transform` |

どの種類も、機能を小さな部品に分けて持ちます。`Opacity`、`WidthInMeters`、`Curvature`、`Texture`、`State`、`Vibration`、`Input`、`EventDispatcher` です。位置を動かせる2種類は、さらに `Visibility`、`Intersection`、`Transform` を持ちます。

`OverlayIdentity` は、作るときに `OpenVR.Overlay.CreateOverlay` を呼び、`Dispose` で `DestroyOverlay` を呼びます。`OverlayTexture` は、絵を2通りで設定します。

| 呼び出し | 渡すもの | 使いどころ |
| :-- | :-- | :-- |
| `FromFile` | 画像ファイルのパス。OpenVR がファイルを読む | 画像を出すだけのとき |
| `FromTexture_t` | OpenVR のテクスチャ記述(`Texture_t`)。GL テクスチャのハンドルなどを包む | 描画した結果を出すとき(エンジン層がこちらを使う) |

どの型で作るかは、自分のオーバーレイが、ダッシュボードのタブか、頭やコントローラーに付くものか、空間に固定するものかで選びます。

## イベントは、ポーリングしたスレッドで呼ばれる

OpenVR のイベントは、`VREventDispatcher` に `Register` した処理が、`PollEvents` を呼んだスレッドで呼ばれます。`PollEvents` を呼ぶまで、ハンドラーは動きません。

ディスパッチャーは、見る先が違う2つがあります。

| 型 | ポーリング先 | 受け取るイベントの例 |
| :-- | :-- | :-- |
| `VRSystemEventDispatcher` | プロセス全体のシステムのイベントの待ち行列 | `VREvent_Quit`、デバイスの接続 |
| `OverlayEventDispatcher` | そのオーバーレイ宛のイベントの待ち行列 | `VREvent_FocusEnter`、`VREvent_MouseMove`、`VREvent_MouseButtonDown` |

イベントを配るとき、ハンドラーの一覧は複製されます。そのため、ハンドラーの中で `Register` や `Unregister` を呼べます。

ハンドラーの中で触る状態が、どのスレッドのものかを確かめます。FloatSoda は、システムのイベントも、入力を持つオーバーレイのイベントも、メインスレッドで `PollEvents` を呼びます。ハンドラーはメインスレッドで動くので、描画スレッドの状態に触れる処理は、描画スレッドへ渡します(→ [エンジン層](engine-layer.md))。

## アクション入力とマニフェスト

コントローラーのボタンは、アクション入力で読みます。`InputAction<T>` が1つの入力を表し、`T` に使えるのは `bool`、`float`、`System.Numerics.Vector2` だけです。ほかの型は `NotSupportedException` で拒否されます。`InputActionMap` が複数のアクションを束ね、`Enabled` でまとめて有効にしたり無効にしたりします。

`VRInputUpdater` が、マニフェストを SteamVR に登録し、毎周 `Update` でアクションの値とイベント(`OnPerformed`、`OnReleased`)を更新します。入力の定義は C# のコードだけで書きます。マニフェストの JSON は、FloatSoda が自動で作ります。

## 2種類のマニフェストを比べる

SteamVR にはマニフェストが2種類あり、FloatSoda での扱いが違います。アクションマニフェストは自動で作られますが、アプリケーションマニフェストは作られません。

| 観点 | アクションマニフェスト | アプリケーションマニフェスト |
| :-- | :-- | :-- |
| 何のためか | 入力アクションの名前、型、既定の割り当てを SteamVR に伝える | プロセスを `app_key` に結びつけ、`AutoLaunch` やシーン遷移などの連携を使えるようにする |
| だれが作るか | FloatSoda が起動のたびに自動で作る。入力の定義が1つ以上あるときだけ | 利用者。FloatSoda は作らない |
| どこに置くか | 一時ディレクトリ配下の `%TEMP%/FloatSoda/{AppKey}/input` | 利用者が決める絶対パス |
| いつ登録するか | 起動時に `VRInputUpdater` が `IVRInput.SetActionManifestPath` で登録する | `OVRApplications.AddManifest(path, temporal)` を呼んだとき |
| 使えないと何が起きるか | 警告にして続行する。オーバーレイの表示は続き、アクション入力だけ使えない | `OVRApplication.Identify()` が `UnknownApplication` で失敗し、警告にして続行する。マニフェスト連携の機能だけ動かない |

アプリケーションマニフェストは、`OVRApplication.Identify()` がプロセスと `app_key` を結びつけるときに要ります。フレームワークの `FloatSodaApp` は起動時に `Identify()` を呼びますが、AppKey が登録されていなくても止まりません。OVR 層を単独で使う場合は、`Identify()` の呼び出しと失敗時の処理を、呼び出し側が決めます。`AutoLaunch` やシーン遷移を使うアプリは、`AddManifest` で登録します。

足りない側は、症状で見分けます。入力が動かなければアクションマニフェストを、`AutoLaunch` などが動かなければアプリケーションマニフェストを確かめます。

<!-- ※要確認: アプリケーションマニフェストのファイル形式(拡張子と中身の項目)と、AddManifest に渡すパスの要件 / このリポジトリには記述がない。OpenVR の資料で確かめる -->

## つまずきやすい点

> 【未決】この節は判断が要るため、ト書きのまま残しています。オーナーが決めたあとで本文にします(追跡: #219)。

- 読み手が持って来るもの: OpenVR まわりで詰まった経験、またはこれから書く不安
- このページが渡すもの: 選んだ落とし穴ごとに、症状、原因、取る行動
- 読み手がすること: 自分の症状に合う項目を探す
- 止まる箇所: なし。ここでこのページは終わる。実験は `experiment-ovr.md`
- 【未決】落とし穴の候補7つのうち、どれを書くか。1番は書くと決まっている。確かめた状況が「1回だけ」「コメントだけ」のものを、確かめてから載せるか、載せないか

<details>
<summary>仮に書いた本文(判断の材料。採用は未定)</summary>

OpenVR まわりで詰まりやすい点を、症状、原因、取る行動の順に並べます。

### マニフェストが2種類あり、FloatSoda での扱いが違う

- 症状: `AutoLaunch` やシーン遷移が動かない
- 原因: アクションマニフェストは自動で登録されるが、アプリケーションマニフェストは自動では登録されない
- 取る行動: `OVRApplications.AddManifest` でアプリケーションマニフェストを登録する。詳細は前の節の比較表にある

### `OpenVR.IsHmdPresent()` が `True` でも、初期化が失敗する

- 症状: `OVRApplication` を作ると、`Init_HmdNotFound` の `VRInitializeException` になる
- 原因: `IsHmdPresent()` が `True` でも起きる。条件は確かめられていない
- 取る行動: 初期化の成否は、`IsHmdPresent()` ではなく、`OVRApplication` の例外で判断する。SteamVR の状態と HMD の接続を確かめる

<!-- ※要確認: 再現条件 / 確認は1台の環境だけで、どの状態で起きるかは分かっていない。HMD の接続と SteamVR の起動順を変えて確かめる -->

### 初期化が通ったあと、`OverlayIdentity` の作成で `NullReferenceException` になる

- 症状: `OVRApplication` の作成は通り、`new OverlayIdentity(...)` で `NullReferenceException` になる。スタックトレースは `OverlayIdentity` のコンストラクターの `OpenVR.Overlay.CreateOverlay` の行を指す
- 原因: 確かめられていない。`OpenVR.Overlay` が `null` だった可能性がある
- 取る行動: 起きたときは、SteamVR の起動状態とスタックトレースの全文を添えて、Issue に報告する(→ [不具合を切り分けて報告する](../1-get-involved/report-bugs.md))

<!-- ※要確認: 再現条件 / 同じ操作で起きるときと、起きないときがあった。起きない場合と比べて、差を確かめる -->

### `AppKey` の検証が、コンストラクターでは働かない

- 症状: 大文字を含む `AppKey` を作っても、例外にならない
- 原因: 検証は `Value` の `init` アクセサーにあり、`new AppKey("...")` のコンストラクターは通らない
- 取る行動: 小文字、数字、ドット、アンダースコアだけで、127 文字までの値を渡す

### ポインター入力が届くのは、ダッシュボードのオーバーレイだけ

- 症状: ダッシュボード以外のオーバーレイで、`GestureDetector` のコールバックが呼ばれない
- 原因: エンジン層の `OverlayWindow` が入力源を作るのは、ダッシュボードのときだけ。`ControllerPointerSystem` は、どこからも呼ばれていない
- 取る行動: ダッシュボード以外へのポインター入力は、Issue で追跡している(追跡: [#182](https://github.com/sumx21t-3310/FloatSoda/issues/182))

`ControllerPointerSystem` は、`MakeOverlaysInteractiveIfVisible` を使わずに、コントローラーのレイとオーバーレイの交差を毎フレーム計算します。SteamVR の入力の横取りが起きないので、VRChat の AFK 判定を避けられる、とクラスの説明に書かれています。

<!-- ※要確認: AFK 判定を避けられるかは、クラスの説明だけが根拠 / VRChat で、AFK 判定の挙動を確かめる -->

OVR 層だけでオーバーレイを出して入力を受ける実験は、[実験: OVR 層](experiment-ovr.md) にあります。

</details>
