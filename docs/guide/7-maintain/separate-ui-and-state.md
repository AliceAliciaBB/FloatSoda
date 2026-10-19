← [ガイド](../Home.md)

# 画面と状態の分け方

## 画面と処理が混ざると、直すたびに壊れる

ファイルが長くなったら、画面の組み立てと、状態や処理を分けます。変更する場所を追いやすくなります。分けないままだと、画面を直したつもりが通信の処理を壊す、といったことが起きます。

たとえば、次のような 1 つの `State` があるとします。

```csharp
public class PanelState : State<PanelWidget>
{
    private int _count;
    private readonly HttpClient _http = new();
    private Timer? _timer;

    public override void InitState()
    {
        _timer = new Timer(async _ =>
        {
            var json = await _http.GetStringAsync("http://localhost:8080/count");
            File.WriteAllText("count.json", json);
            SetState(() => _count = int.Parse(json));
        }, null, 0, 5000);
    }

    // …ここに、100 行の画面の組み立て…
}
```

この例は責務の混在を示す反例で、通信やタイマーの実装例ではありません。通信、保存、タイマー、画面が、1 つのクラスに入っています。AI に「ボタンの色を変えて」と頼んだとき、AI は通信の行も同じ場所で読むことになり、関係のない箇所を巻き込んで書き換える余地が生まれます。画面を確かめるにも、通信やタイマーが動く状態で確かめることになり、表示する値を固定して画面だけを確かめにくくなります。

FloatSoda は UI のすべてを C# のコードで書くので、分けたコードは diff で読めて、AI が直しやすくなります。分け方の考え方を、次の節から順に説明します。

## 画面は「値を受け取って画面を返す」部分に絞る

画面のウィジェットは、受け取った値をどう描くかだけを持つ部品にします。この例のウィジェットは、`init` プロパティで値を受け取る `record` で、値を受け取って画面を返す設計図です。持つものは、表示する値と、押されたときに呼ぶ処理(`Action`)だけです。

```csharp
public record CounterView : StatelessWidget
{
    public required int Count { get; init; }
    public required Action OnIncrement { get; init; }

    public override Widget Build(IBuildContext context) => new GestureDetector
    {
        Behaviour = HitTestBehaviour.Opaque,
        OnTap = OnIncrement,
        Child = new Text($"カウント: {Count}"),
    };
}
```

この `CounterView` は、通信も保存も知りません。何を表示し、押されたら何を呼ぶかを、外から渡されます。画面に置くものを決めるときは、「この行は、渡された値から画面を作るために要るか」を問います。要らない行は、次の節で外に出します。

## 状態と処理は、画面の外に置く

状態と処理は、画面とは別のクラスに置きます。外に出すものの例は、通信、保存、タイマーです。

```csharp
public sealed class CounterModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public int Count { get; private set; }

    public void Increment()
    {
        Count++;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
    }
}
```

`CounterModel` は、ウィジェットを 1 つも知りません。値が変わったら `PropertyChanged` で知らせるだけです。通信や保存を足すときも、このクラスの中に足します。画面へ渡す値と `Action` が変わらなければ、画面のファイルは変わりません。

## 画面の外の状態を、画面から取り出す

外に置いた状態を画面から取り出す手段は、2 つあります。違いは、取り出すものの性質です。

| 手段 | 向いているもの | 値が変わったときの動き |
| :-- | :-- | :-- |
| `ServiceProvider` | アプリの間ずっと使うオブジェクト(通信、保存、状態を持つクラス) | 取り出しただけでは画面は変わらない。`ListenableBuilder` などで変更通知を受ける |
| `InheritedWidget` | 画面の上から下へ渡す値(設定、見た目の指定など) | 値が変わると、その値に依存する子孫が作り直される |

`ServiceProvider` は、画面のルートを包み、`IServiceProvider` を子孫へ渡します。子孫は `context.GetRequiredService<T>()` で取り出します。取り出した状態の変更は、`ListenableBuilder` で受け取ります。`ListenableBuilder` は、`INotifyPropertyChanged` の通知のたびに、自分自身を作り直し、`ChildBuilder` で新しいウィジェットを作ります。`ChildBuilder` が返したウィジェットから下は、通常の作り直しと同じ規則で更新されます。

```csharp
app.CreateWindow(new DashboardWindow
{
    Title = "Counter",
    Child = new FloatSoda.Widgets.ServiceProvider
    {
        Services = host.Services,
        Child = new CounterScreen(),
    },
});

public record CounterScreen : StatelessWidget
{
    public override Widget Build(IBuildContext context)
    {
        var model = context.GetRequiredService<CounterModel>();

        return new ListenableBuilder
        {
            Listenable = model,
            ChildBuilder = _ => new CounterView
            {
                Count = model.Count,
                OnIncrement = model.Increment,
            },
        };
    }
}
```

`CounterModel` は、起動のコードで `builder.Services.AddSingleton<CounterModel>()` として登録します。登録の手順は [Generic Host の仕組みを使う](../4-build/mechanics/use-generic-host.md) にあります。

`ListenableBuilder` は、変更通知を、ウィジェットがマウントされたスレッド(通常は FloatSoda のメインループ)から出すことを前提にしています。別のスレッドから `PropertyChanged` を起こすと、`InvalidOperationException` になります。バックグラウンドのスレッドで状態を更新するときは、メインループに処理を戻してから `PropertyChanged` を起こします。

`InheritedWidget` は、自作の `record` で作ります。子孫は `context.DependOnInheritedWidgetOfExactType<T>()` で取り出し、その値に依存します。`InheritedWidget` の中の値を書き換えても、通知は起きません。親が作り直されて、新しい `InheritedWidget` が渡され、`UpdateShouldNotify` が `true` を返したときだけ、依存した子孫が作り直されます。

<!-- ※要確認: 上のコードは、ServiceProvider / ListenableBuilder / StatelessWidget の公開 API の定義を読んで書いた。ビルドして動かして確かめていない。ServiceProvider で包んだルートを CreateWindow の Child に渡す書き方は、src/ にその例がなく、実行して確かめていない。InheritedWidget を自作する例は、ここには載せていない -->

## 分けると、HMD を被らずに確かめられる

画面が「値を受け取って画面を返す」部品に絞られていると、状態を作らなくても、決めた値で画面を作れます。たとえば `new CounterView { Count = 42, OnIncrement = () => { } }` は、通信が動いていなくても組み立てられます。

組み立てた画面は、`WidgetBitmapRenderer` で画像にして確かめられます。HMD を被る必要はありません。手順は [HMD を被らずに確かめる](../4-build/input/test-without-hmd.md) にあります。値を変えた画像を何枚か書き出しておくと、AI に画面を直してもらったあとの見た目の変化を、画像で比べられます。

<!-- ※要確認: サービスを使う画面(ServiceProvider で包んだルート)を WidgetBitmapRenderer で画像にできるかは、実行して確かめていない。値だけを受け取る CounterView の形なら、サービスなしで画像にできるはず -->

## 分けすぎない

ツールが小さいうちは、分けなくてかまいません。次の 3 つがすべて当てはまるなら、1 つの `State` に置いたままで足ります。

- 通信、保存、タイマーのどれも使わない
- 画面のファイルが 1 つで、読み返して全体を追える
- 同じ値を、複数の画面で使わない

1 つでも外れたら、状態を外に出す時期です。

分ける対象は、ツール全体の構造です。1 つの部品の中の状態、つまり `SetState()` の仕組みは [状態と作り直し](../4-build/mechanics/state-and-rebuild.md) にあります。

FloatSoda は、状態管理の特定のライブラリを強制しません。上の例の `INotifyPropertyChanged` のほかに、自作のクラスや、使い慣れたライブラリを使えます。`FloatSoda.Hooks` は、今は使えません(追跡: #72)。
