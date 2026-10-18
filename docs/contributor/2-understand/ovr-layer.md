← [コントリビューター](../Home.md)

# OVR 層

> このページは執筆前の骨組みです。設計シートとト書きだけを置き、本文はまだ書いていません(追跡: #219)。

## 設計シート

- 分類: 説明型(Diátaxis: explanation)
- 読み手: だれが / `src/FloatSoda.OVR` のコードを読む、OpenVR まわりを直したい人 / 何を知っている / C# の基本。OpenVR は詳しくなくてよい / 読んで何をしたい / OVR 層が OpenVR のどこをどう包んでいるかをつかみ、OpenVR の資料の読み方と、マニフェストの違いと、つまずきやすい点を知りたい
- 出力言語と文体の指定: 日本語。敬体(CONTRIBUTING.md の「ドキュメントのトンマナ」の「文体」)
- テーマ: OVR 層(`FloatSoda.OVR`)が、OpenVR の生のバインディングの上に、アプリケーションの初期化、オーバーレイ、イベント、アクション入力をどう包んでいるかを説明する。OpenVR の資料の読み方と、マニフェスト2種類の違いも含める
- 依頼の解釈: なし
- テーマファイル: explanation
- Why(目的): OpenVR の挙動はこちらのコードから変えられないので、OpenVR の資料の読み方と、FloatSoda が決めている扱いを知らないと、同じ場所で何度もつまずく
- What(目標・結果): OVR 層の構成、OpenVR の資料への案内、2種類のマニフェストの比較、つまずきやすい点が説明されている
- How(手段): 構成要素で分解する。構成 → 資料の読み方 → アプリケーションとオーバーレイ → イベント → 入力とマニフェスト → つまずきやすい点の順に説明する。OpenVR の仕様そのものは書かず、OpenVR の資料へ案内する
- 要素:
  - OVR 層は、ほかの FloatSoda のプロジェクトを参照しない。ネイティブの `openvr_api.dll` を同梱する(`src/FloatSoda.OVR/FloatSoda.OVR.csproj`)
  - 生のバインディングは `src/FloatSoda.OVR/openvr_api.cs` で、Valve の自動生成ファイルである。名前空間は `FloatSoda.OVR` である(`src/FloatSoda.OVR/openvr_api.cs` の 1〜14 行目)
  - OpenVR の資料の入口は、OpenVR の Wiki(`https://github.com/ValveSoftware/openvr/wiki`)である。Wiki の記述と食い違うときは、正しいのは `src/FloatSoda.OVR/openvr_api.cs` である(docs 再編の決定、2026-10-17)。※要確認(Wiki の URL は未取得。開いて確かめる)
  - `OVRApplication` が `OpenVR.Init` を呼び、失敗は `ThrowIfError` で例外にする。`OVRAppInfo` の種類の既定は `Overlay` である(`src/FloatSoda.OVR/OVRApplication.cs` の 69、90〜98 行目)
  - `AppKey` は、ASCII の小文字、数字、ドット、アンダースコアだけで、127文字までである。大文字を使うと `FormatException` になる(`src/FloatSoda.OVR/OVRApplication.cs` の 41〜56 行目)
  - オーバーレイは `IOverlay` の型で表す。`DashboardOverlay`、`DeviceTrackedOverlay`、`WorldSpaceOverlay` の3種類である(`src/FloatSoda.OVR/Overlay/Overlay.cs` の 4、67、135、143 行目)
  - オーバーレイの機能は、`Opacity`・`WidthInMeters`・`Curvature`・`Transform`・`Texture`・`Visibility`・`State`・`Vibration`・`Input`・`EventDispatcher` などの部品に分かれている(`src/FloatSoda.OVR/Overlay/Capability.cs`、`src/FloatSoda.OVR/Overlay/Overlay.cs` の 67〜92 行目)
  - `OverlayIdentity` は、作成時に `OpenVR.Overlay.CreateOverlay` を呼び、`Dispose` で `DestroyOverlay` を呼ぶ(`src/FloatSoda.OVR/Overlay/Identity.cs` の 38〜55 行目)
  - `OverlayTexture` は、画像ファイル(`FromFile`)と、OpenVR のテクスチャ記述(`FromTexture_t`)の2通りで絵を設定する(`src/FloatSoda.OVR/Overlay/Capability.cs` の 186〜196 行目)
  - イベントは `VREventDispatcher` に `Register` した処理が、`PollEvents` を呼んだスレッドで呼ばれる。システム全体の `VRSystemEventDispatcher` と、オーバーレイごとの `OverlayEventDispatcher` がある(`src/FloatSoda.OVR/SystemEventDispatcher.cs` の 10、52、67 行目)
  - アクション入力は `InputAction<T>`(`bool`・`float`・`Vector2`)と `InputActionMap` で定義し、`VRInputUpdater` が毎周更新する(`src/FloatSoda.OVR/Input/InputAction.cs`、`src/FloatSoda.OVR/Input/VRInputUpdater.cs`)
  - 2種類のマニフェストを比べる節を置く(docs 再編の決定、2026-10-17)
  - アクションマニフェスト: FloatSoda が、アクション入力の定義がある場合に起動のたびに自動で作る。`ActionManifestWriter.Write` が一時ディレクトリ配下(`%TEMP%/FloatSoda/{AppKey}/input`)に JSON を書き、`IVRInput.SetActionManifestPath` で登録する(`src/FloatSoda/FloatSodaApp.cs` の 234〜239 行目、`src/FloatSoda.OVR/Input/ActionManifestWriter.cs` の 23〜28 行目、`src/FloatSoda.OVR/Input/VRInputUpdater.cs` の 29 行目)
  - アクションマニフェストが使えなくても、オーバーレイの表示は続く。`VRInputException` は警告にして続行する(`src/FloatSoda/FloatSodaApp.cs` の 241〜246 行目付近)
  - アプリケーションマニフェスト: FloatSoda は自動では作らない。`OVRApplications.AddManifest(path, temporal)` で登録する(`src/FloatSoda.OVR/OVRApplication.cs` の 152 行目)
  - アプリケーションマニフェストの `app_key` と、プロセスの対応付けは `OVRApplication.Identify()` が行う。未登録の AppKey では `UnknownApplication` で失敗する。FloatSoda は失敗を警告にして続行する。`AutoLaunch` やシーン遷移などのマニフェスト連携機能だけが動かなくなる(`src/FloatSoda.OVR/OVRApplication.cs` の 111〜125 行目、`src/FloatSoda/FloatSodaApp.cs` の 206〜216 行目)
  - アプリケーションマニフェストの拡張子 `.vrmanifest` と中身の項目は、このリポジトリに記述がない。OpenVR の資料で確かめる。※要確認
  - 比べる観点: 何のためか、だれが作るか、どこに置くか、いつ登録するか、使えないと何が起きるか(上の要素から埋める)
  - つまずきやすい点の候補は下の表にある。どれを書くかは、解説を書くときに選ぶ(docs 再編の決定、2026-10-17)。1番は書くと決まっている

    | # | つまずきやすい点 | 出どころ | 確かめた状況 |
    | --: | :-- | :-- | :-- |
    | 1 | マニフェストが2種類あり、FloatSoda での扱いが違う | `src/FloatSoda/FloatSodaApp.cs` の 238 行目、`src/FloatSoda.OVR/OVRApplication.cs` の 152 行目 | ソースで確かめた |
    | 2 | `OpenVR.IsHmdPresent()` が `True` でも、初期化が `Init_HmdNotFound` で失敗することがある | 2026-10-17 の実験(`Exp3Probe`) | 1台で観測した。※要確認 |
    | 3 | 初期化が通ったあと、`OverlayIdentity` の作成で `NullReferenceException` になったことがある(`OpenVR.Overlay` が `null` だったと推測) | `src/FloatSoda.OVR/Overlay/Identity.cs` の 44 行目、2026-10-17 の実験 | 1回だけ。再現しなかった。※要確認 |
    | 4 | `AppKey` に大文字を使うと `FormatException` になる | `src/FloatSoda.OVR/OVRApplication.cs` の 41〜56 行目 | ソースで確かめた |
    | 5 | GL テクスチャは原点左上で届くので、マウスの y 座標を反転すると上下が鏡写しになる | `src/FloatSoda.Engine/OverlayMouseRawPointerSource.cs` の 13〜14 行目 | ソースのコメントだけ。エンジン層の話でもある。※要確認 |
    | 6 | ポインタ入力が届くのはダッシュボードのオーバーレイだけで、`ControllerPointerSystem` はどこからも呼ばれていない | `src/FloatSoda.OVR/Overlay/ControllerPointerSystem.cs`、2026-10-17 の grep | grep で確かめた。追跡: #182 |
    | 7 | GL を使うものは描画スレッドで作る。オーバーレイをどのスレッドで作るべきかは未確認 | `src/FloatSoda.Engine/GLView.cs` の 9 行目 | オーバーレイのスレッドは未確認。※要確認 |

  - 落とし穴の見分け方の案: OpenVR の Wiki やメソッドのシグネチャから読み取れる挙動と、実際の挙動または FloatSoda の扱いが違うところ(エージェントの案。オーナーは決めていない)
  - `ControllerPointerSystem` は、`MakeOverlaysInteractiveIfVisible` を使わずに、コントローラーのレイとオーバーレイの交差を毎フレーム計算する。SteamVR の入力の横取りが起きないので、VRChat の AFK 判定を避けられる、とクラスの説明に書かれている(`src/FloatSoda.OVR/Overlay/ControllerPointerSystem.cs` の 7〜12 行目)。※要確認(AFK 判定への効果は、コメントのみが根拠)
  - `samples/FloatSoda.Samples.PrimitiveOverlay` は、OVR 層だけでオーバーレイを出す書きかけのサンプルである。実験は `experiment-ovr.md` に置く
- 見出し:
  - OVR 層は、OpenVR の生のバインディングを包む
  - OpenVR の資料の読み方
  - アプリケーションとオーバーレイ
  - イベントは、ポーリングしたスレッドで呼ばれる
  - アクション入力とマニフェスト
  - 2種類のマニフェストを比べる
  - つまずきやすい点
- ※要確認:
  - OpenVR の Wiki の URL と、Wiki と `openvr_api.cs` が食い違う実例(開いて比べる)
  - `.vrmanifest` の形式と、`AddManifest` に渡すパスの要件(OpenVR の資料で確かめる)
  - 落とし穴の候補 2、3、5、7 の再現
  - `ControllerPointerSystem` の AFK 判定への効果
  - 落とし穴を選ぶ基準を、オーナーが決めるか

## ト書き

### OVR 層は、OpenVR の生のバインディングを包む

- 読み手が持って来るもの: 「OpenVR とは何か」の最小限の知識
- このページが渡すもの: OVR 層の1文の定義(OpenVR の初期化、オーバーレイ、イベント、アクション入力を C# の型で包む層)と、構成の図(生のバインディング → ラッパー → 他の層が使う型)。ほかの FloatSoda のプロジェクトを参照しないこと
- 読み手がすること: 図で、自分が直したい箇所を見つける
- 次へ進む先: 「OpenVR の資料の読み方」

### OpenVR の資料の読み方

- 読み手が持って来るもの: OpenVR の挙動を調べたい
- このページが渡すもの: OpenVR の Wiki を入口として使うこと、Wiki と食い違ったときに正しいのは `src/FloatSoda.OVR/openvr_api.cs` であること、そのファイルが自動生成であること
- 読み手がすること: 調べたい API を `openvr_api.cs` で見つけ、Wiki の説明と照らす
- 次へ進む先: 「アプリケーションとオーバーレイ」

### アプリケーションとオーバーレイ

- 読み手が持って来るもの: OpenVR の資料の読み方
- このページが渡すもの: `OVRApplication` の初期化、`AppKey` の規則、オーバーレイ3種類の違い、機能の部品、`OverlayTexture` の2通りの設定
- 読み手がすること: 自分のオーバーレイがどの種類かを選ぶ
- 止まる箇所: `AppKey` の規則を破ると `FormatException`。禁止の文として短く書き、代わりに取る行動(小文字、数字、ドット、アンダースコアで 127 文字まで)を続ける
- 次へ進む先: 「イベントは、ポーリングしたスレッドで呼ばれる」

### イベントは、ポーリングしたスレッドで呼ばれる

- 読み手が持って来るもの: オーバーレイの型
- このページが渡すもの: `Register` と `PollEvents` の関係、システムのイベントとオーバーレイのイベントの違い、`PollEvents` を呼ぶスレッドでハンドラーが動くこと
- 読み手がすること: ハンドラーの中で触れる状態が、どのスレッドのものかを確かめる
- 次へ進む先: 「アクション入力とマニフェスト」

### アクション入力とマニフェスト

- 読み手が持って来るもの: コントローラーのボタンを読みたい
- このページが渡すもの: `InputAction<T>` の型の制約、`InputActionMap`、`VRInputUpdater` の更新、アクションマニフェストが自動生成されること
- 読み手がすること: 入力の定義を書く
- 次へ進む先: 「2種類のマニフェストを比べる」

### 2種類のマニフェストを比べる

- 読み手が持って来るもの: アクションマニフェストが自動で作られるという知識
- このページが渡すもの: アクションマニフェストとアプリケーションマニフェストの比較表(何のためか、だれが作るか、どこに置くか、いつ登録するか、使えないと何が起きるか)
- 読み手がすること: 自分のアプリで足りない側のマニフェストを見つける
- 次へ進む先: 「つまずきやすい点」

### つまずきやすい点

- 読み手が持って来るもの: OpenVR まわりで詰まった経験、またはこれから書く不安
- このページが渡すもの: 選んだ落とし穴ごとに、症状、原因、取る行動
- 読み手がすること: 自分の症状に合う項目を探す
- 止まる箇所: なし。ここでこのページは終わる。実験は `experiment-ovr.md`
- 【未決】落とし穴の候補7つのうち、どれを書くか。1番は書くと決まっている。確かめた状況が「1回だけ」「コメントだけ」のものを、確かめてから載せるか、載せないか
