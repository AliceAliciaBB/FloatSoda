← [ガイド](../../Home.md)

# コントローラーの入力を受け取る

## この手順でできること

コントローラーの入力は、`InputAction<T>` で宣言します。宣言したアクションを `AddFloatSoda` に登録すると、ボタン、トリガー、スティックの入力をコードで受け取れます。画面を押す操作とは別の入力なので、ショートカットや「掴む」操作のように、画面の外から使う物に向いています。この手順を終えると、トリガーを引いたときにコンソールへ表示が出て、トリガーの引き量を毎フレーム読めるようになります。

`T` には、受け取りたい入力に合わせて次の3つの型から1つを選びます。

| `T` | 入力の種類 | 受け取れる値 |
| :-- | :-- | :-- |
| `bool` | ボタン | 押している間は `true` |
| `float` | トリガーの引き量 | 引き量の値 |
| `Vector2` | スティック、トラックパッド | X と Y の値 |

画面上のボタンをコントローラーのレーザーで押す操作は、この手順の対象外です。その場合は[ボタンやトグルを組み立てる](../screen/build-buttons-toggles.md)に進みます。

## 前提条件

- [最初のオーバーレイを作る](../../../tutorials/first-overlay.md)を終え、起動のコードが動くこと
- SteamVR を起動し、HMD を接続していること
- ソースの先頭に `using System.Numerics;`、`using FloatSoda.OVR;`、`using FloatSoda.OVR.Input;` があること

## 手順1: アクションを宣言する

使いたい入力ごとに、`InputAction<T>` を1つ作ります。必須の指定は `Name` だけです。コントローラーのどの入力に割り当てたいかは、`SuggestedPath` で提案します。

```csharp
var grab = new InputAction<bool>
{
    Name = "grab",
    SuggestedPath = "/user/hand/right/input/trigger/click",
};

var pull = new InputAction<float>
{
    Name = "pull",
    SuggestedPath = "/user/hand/right/input/trigger",
};

var move = new InputAction<Vector2>
{
    Name = "move",
    SuggestedPath = "/user/hand/left/input/thumbstick",
};
```

`SuggestedPath` は、すべてのコントローラー種別に同じパスで提案されます。`/user/hand/left/input/thumbstick` のように、コントローラーによって名前が違う入力は、種別ごとに指定できる `DefaultBindings` を使います(例は次のコードにあります)。

`SuggestedPath` の書き方は、型ごとに次のとおりです。

| `T` | 書き方の例 | 末尾の扱い |
| :-- | :-- | :-- |
| `bool` | `/user/hand/right/input/trigger/click` | 末尾が `click` か `touch` のとき、その動作を使う。省略すると `click` |
| `float` | `/user/hand/right/input/trigger` | 末尾が `pull` のとき、その動作を使う。省略すると `pull` |
| `Vector2` | `/user/hand/left/input/thumbstick` | 末尾が `position` のとき、その動作を使う。省略すると `position` |

<!-- ※要確認: `thumbstick` など、SteamVR の入力ソース名がコントローラー種別ごとに正しいか。末尾の補完規則は src/FloatSoda.OVR/Input/ActionManifestWriter.cs の SplitInputPath で確かめた。入力ソース名は SteamVR の公式資料で確かめる -->

コントローラーの種類ごとに割り当てを変えたいときは、`SuggestedPath` の代わりに `DefaultBindings` を使います。`ControllerType` には `Index`、`ViveWand`、`OculusTouch` があります。

次の例は、手順1の最初のコードにある `grab` の宣言を置き換えます。`DefaultBindings` を1つでも書くと、`SuggestedPath` はどのコントローラーにも使われません。使うコントローラーの種類は、すべて書きます。

```csharp
var grab = new InputAction<bool>
{
    Name = "grab",
    DefaultBindings =
    [
        new DefaultBinding { Controller = ControllerType.Index, Path = "/user/hand/right/input/trigger/click" },
        new DefaultBinding { Controller = ControllerType.ViveWand, Path = "/user/hand/right/input/trigger/click" },
        new DefaultBinding { Controller = ControllerType.OculusTouch, Path = "/user/hand/right/input/trigger/click" },
    ],
};
```

**できた?** コンパイルが通れば、手順1は完了です。

<details>
<summary>できなかったら</summary>

`T` に `bool`、`float`、`Vector2` 以外の型を渡すと、実行時に `TypeInitializationException` が出ます。原因は、その `InnerException` の `NotSupportedException` です。メッセージは次の形です。

```text
InputAction<Int32> は未対応です。bool / float / Vector2 を使用してください。
```

`Name` を書き忘れると、コンパイルが通りません。

</details>

## 手順2: AddFloatSoda に登録する

宣言したアクションは、`InputActionMap` にまとめて `FloatSodaOptions.InputActionMaps` に渡します。`AppKey` も、アプリごとに別の値を指定します。

起動のコードにある `AddFloatSoda` の呼び出しは、次のコードに置き換えます。アクションの宣言は、この呼び出しより前に置きます。

```csharp
using FloatSoda;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddFloatSoda(new FloatSodaOptions
{
    AppKey = new AppKey("my.first.overlay"),
    InputActionMaps = [new InputActionMap { Name = "main", Actions = [grab, pull, move] }],
});
```

アクションを1つ以上登録すると、SteamVR に渡すアクションマニフェストが自動で作られて登録されます。JSON ファイルを手で書く必要はありません。マニフェストは、一時フォルダー配下の `FloatSoda/<AppKey>/input` に書き出されます。`AppKey` が違うアプリどうしは、書き出し先が分かれます。

**できた?** 起動したとき、コンソールに「アクション入力の初期化に失敗しました」という警告が出なければ、手順2は完了です。登録後に入力が届くかどうかは、手順3で確かめます。

<details>
<summary>できなかったら</summary>

入力の初期化に失敗しても、オーバーレイの表示は続きます。警告が出た場合は、[エラーを読んで直す](../../5-troubleshoot/read-errors.md)で、メッセージの続きから原因を切り分けます。

</details>

## 手順3: 入力を受け取る

入力の受け取り方は3つあります。イベントは、`host.RunAsync()` を呼ぶ前に購読します。

| 受け取り方 | 呼ばれる、または読める場面 | 向いている使い方 |
| :-- | :-- | :-- |
| `OnPerformed` | `bool` では押した瞬間。`float` と `Vector2` では値が変わったフレーム | 押した瞬間に1回だけ動かす |
| `OnReleased` | `bool` で離した瞬間。`float` と `Vector2` では呼ばれない | 離したときに終了処理をする |
| `Value` | いつでも読める。毎フレーム更新される | 引き量やスティックの傾きを、画面に反映し続ける |

```csharp
grab.OnPerformed += _ => Console.WriteLine("grab: pressed");
grab.OnReleased += () => Console.WriteLine("grab: released");
pull.OnPerformed += value => Console.WriteLine($"pull: {value}");
```

`Value` は、たとえば `pull.Value` のように、必要な場所で読みます。`Value` を読むだけでは、画面の再構築は予約されません。画面に反映するときは、メインスレッドで `SetState()` を呼びます。

`IsActive` は、アクションが現在いずれかの入力に割り当てられているかを示します。

<!-- ※要確認: IsActive が、割り当ての有無だけでなく、コントローラーの接続やアクションセットの有効状態も反映するか。src/FloatSoda.OVR/Input/InputAction.cs は OpenVR の bActive をそのまま入れている。OpenVR の公式資料で確かめる -->

以下の確認は、例のとおりアクションをトリガーに割り当てた場合のものです。割り当てを変えた場合は、その入力を操作します。

**できた?** トリガーを引くと、コンソールに `grab: pressed` が出て、離すと `grab: released` が出れば、手順3は完了です。

<!-- ※要確認: 実機で OnPerformed が呼ばれるか。HMD でコントローラーを操作して確かめる -->

## 手順4: SteamVR のバインディングを確かめる

> 【未決】この節は判断が要るため、ト書きのまま残しています。オーナーが決めたあとで本文にします(追跡: #219)。

- 読み手が持って来るもの: 動いたアクション
- このページが渡すもの: 割り当てを決めるのは SteamVR で、提案は変更されうること。割り当てが変わっても動くコードにする理由
- 読み手がすること: SteamVR のバインディング画面で割り当てを見る
- できた?: 自分のアクションが一覧に出ている
- 次へ進む先: 「結果を確かめる」
- 【未決】SteamVR のバインディング画面の開き方を、このページに画像つきで書くか、SteamVR の資料へのリンクで済ませるか

<details>
<summary>仮に書いた本文(判断の材料。採用は未定)</summary>

実際にどの入力がアクションに結び付くかは、SteamVR が決めます。`SuggestedPath` と `DefaultBindings` は初期値の提案で、利用者は SteamVR のバインディング画面から割り当てを変えられます。実行中のコードから、割り当てを読み返すことはできません。

そのため、コードは「どのボタンか」を前提にせず、アクションの名前だけを頼りに書きます。割り当てが変わっても、同じコードのまま動きます。

SteamVR のコントローラーバインディング画面を開き、アプリの一覧から自分のアプリを選びます。

<!-- ※要確認: SteamVR のバインディング画面の開き方と、アプリ一覧での表示名。SteamVR の画面で確かめる -->

**できた?** 自分のアクション(`grab`、`pull`、`move`)が、バインディング画面の一覧に出ていれば、手順4は完了です。

ゲーム本体(VRChat など)が同じ入力を使っているときの優先順位は、SteamVR のアクションセットの優先度で決まります。ゲーム本体が使っていない入力を提案すると、衝突しにくくなります。

<!-- ※要確認: ゲーム本体と同じ入力を割り当てたときの優先順位。SteamVR の公式資料で確かめる -->

</details>

## 結果を確かめる

HMD を被り、コントローラーのトリガーを操作して(割り当てを変えた場合は、その入力を操作して)、コンソールで次の3点を見ます。

- トリガーを引いたときに `grab: pressed` が出る
- トリガーを離したときに `grab: released` が出る
- トリガーをゆっくり引くと、`pull:` の値が少しずつ変わる

自分の物を作る場合は、次に[表情切り替えパネルを作る](../screen/expression-switcher.md)に進みます。作例は[作例のカタログ](../../../reference/sample-catalog.md)にもあります。

## うまくいかないとき

入力が来ない、または起動時に警告が出たときは、[エラーを読んで直す](../../5-troubleshoot/read-errors.md)で切り分けます。
