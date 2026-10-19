← [ガイド](../../Home.md)

# オーバーレイの種類と選び方

## オーバーレイは3種類あり、出る場所と入力の届き方が違う

FloatSoda のオーバーレイは、`DashboardWindow`、`WorldSpaceWindow`、`DeviceTrackedWindow` の3種類です。出る場所と、コントローラーで押せるかどうかが違います。押して操作する物はダッシュボード、表示だけでよい物は空間に固定するか、コントローラーに追従させます。

| 種類 | 出る場所 | 押して操作できるか | 位置の決め方 |
| :-- | :-- | :-- | :-- |
| `DashboardWindow` | SteamVR のダッシュボード | できる | SteamVR が管理する |
| `WorldSpaceWindow` | ワールドの決まった座標 | できない(表示専用) | `Position` と `Rotation` |
| `DeviceTrackedWindow` | コントローラーや HMD のそば | できない(表示専用) | `Target`、`Offset`、`Rotation` |

作る前に種類を選ぶ理由は、選び間違えると、ボタンが押せない物ができてしまうためです。表示専用になる理由は、ポインターの座標が、ダッシュボードのオーバーレイにしか届かないことにあります(追跡: #182)。

1つのアプリは、複数のウィンドウを同時に作れます。種類を混ぜてもかまいません。複数のウィンドウを作るときの条件は、次のとおりです。

- 同じアプリで作るウィンドウには、それぞれ異なる `Title` を指定すること

<!-- ※要確認: 同じ Title で CreateWindow を2回呼んだときの挙動。FloatSodaApp.CreateWindow は Title を辞書のキーに使い、2つ目は登録されない。実行して確かめる -->

## ダッシュボード: 操作できる

`DashboardWindow` は、SteamVR のダッシュボードにタブとして出ます。`Title` が、そのタブの名前になります。ポインターの座標が届くので、`GestureDetector` で組み立てたボタンを押せます。

```csharp
app.CreateWindow(new DashboardWindow { Title = "Panel", Child = panel });
```

`DashboardWindow` の表示サイズは SteamVR が管理するため、`Dpm` の指定が効かない場合があります。

<!-- ※要確認: ダッシュボードのオーバーレイのサムネイルやアイコンを指定する手段があるか。WindowWidget.cs には見当たらない -->

操作する物は、この種類で作ります。押せる部品の組み立て方は、[ボタンやトグルを組み立てる](../screen/build-buttons-toggles.md)にあります。

## 空間に固定: 表示専用

`WorldSpaceWindow` は、ワールドの座標に固定されます。座標は、Standing の追跡座標系で、プレイエリアの床の中央が原点、前方が -Z 方向、単位はメートルです。`Position` を省略すると、中央から前方 1m、高さ 1m に出ます。

```csharp
app.CreateWindow(new WorldSpaceWindow
{
    Title = "Clock",
    Position = new Vector3(0f, 1.5f, -1f),
    Child = clock,
});
```

この種類は表示専用です。コントローラーのレーザー操作を受け取れないので、`GestureDetector` のコールバックは呼ばれません(追跡: #182)。時計や字幕のように、見るだけの物に向いています。

## 手首などに追従: 表示専用

`DeviceTrackedWindow` は、`Target` に指定したデバイスに追従します。`Target` には `TrackedDevice.LeftController`、`TrackedDevice.RightController`、`TrackedDevice.HMD` を指定できます。`Offset` でデバイスからの位置を、`Rotation` で向きを決めます。`TrackedDevice` は `FloatSoda.OVR.Overlay` の名前空間にあり、`Vector3` は `System.Numerics` のものです。

```csharp
app.CreateWindow(new DeviceTrackedWindow
{
    Title = "Left Hand",
    Target = TrackedDevice.LeftController,
    Offset = new Vector3(-0.1f, 0, 0.2f),
    Child = clock,
});
```

<!-- ※要確認: `Offset` の座標系の向き(コントローラーのどの軸か)。HMD で確かめる -->

この種類も表示専用です(追跡: #182)。手元に常に出しておきたい時計のような物に向いています。

## 大きさの決まり方

`WorldSpaceWindow` と `DeviceTrackedWindow` の実際の大きさは、`Dpm` と、中身のピクセル数で決まります。`DashboardWindow` の表示サイズは、前の節のとおり SteamVR が管理します。`Dpm` は 1m あたりのピクセル数で、既定は 4000 です。幅 1000px のウィジェットは、実寸で 25cm になります。

`Size` を指定しないウィンドウは、中身のレイアウト結果にサイズが追従します。`Size` を指定すると、その大きさで中身をレイアウトします。`Dpm` は `FloatSoda.Abstractions.Geometries`、`SKSize` は `SkiaSharp` の名前空間にあります。

```csharp
app.CreateWindow(new WorldSpaceWindow
{
    Title = "Fixed",
    Dpm = new Dpm(1000),
    Size = new SKSize(800, 600),
    Child = panel,
});
```

`Size` を指定しないウィンドウには、既知の問題があります。起動したあとに中身の大きさが変わっても、SteamVR 上の表示が、最初の大きさのまま更新されません。中身の大きさが変わるウィンドウには、`Size` を指定します(追跡: #285)。

<!-- ※要確認: 3種類を同時に出したときの負荷。測って確かめる -->

## 自分の物に合う種類の選び方

押して操作する物は、`DashboardWindow` を選びます。見るだけの物は、空間のどこかに置くなら `WorldSpaceWindow`、手元に付けたいなら `DeviceTrackedWindow` を選びます。

`WorldSpaceWindow` と `DeviceTrackedWindow` では、コントローラーのレーザー操作を受け取れません(追跡: #182)。できないことの一覧は、[できないこと・まだ使えないもの](../../../reference/limitations.md)にあります。

選んだ種類で最初の画面を作るときは、[ボタンやトグルを組み立てる](../screen/build-buttons-toggles.md)に進みます。開発中の見た目の確認には、HMD を被らずに窓へ出せる `DesktopWindow` を使えます([HMD を被らずに確かめる](../input/test-without-hmd.md))。
