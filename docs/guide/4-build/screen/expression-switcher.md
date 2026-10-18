← [ガイド](../../Home.md)

# 表情切り替えパネルを作る

## この手順でできること

VRChat の表情を、HMD の中のパネルから切り替えたいときは、ボタンを並べたパネルを FloatSoda で作り、押したボタンの番号を OSC の送信処理へ渡します。この手順は、チュートリアルのあとに、自分の物を最後まで作る通しの例です。作例は、FaceEmo で作った表情を OSC で切り替えるパネルです。

手順は3つです。

1. 表情のボタンを並べたパネルを作る
2. 押したときに、ボタンの番号を渡す接続口を作る
3. OSC の送信を AI に頼んで、接続口につなぐ

ダッシュボードのタブに、表情の名前を書いたボタンが格子状に並びます。ボタンを押すと、押したボタンの番号が OSC の送信処理に渡ります。OSC の送信そのものは、FloatSoda の外の話で、FloatSoda は OSC のライブラリに依存していません。この手順では、AI に書いてもらう前提で、渡す値と呼ぶ位置までを作ります。

## 前提条件

- [最初のオーバーレイを作る](../../../tutorials/first-overlay.md)を終え、ボタンを押すと数字が増えるオーバーレイが動くこと
- VRChat の OSC を有効にしていること。有効にする方法は、VRChat の公式資料の[OSC Overview](https://docs.vrchat.com/docs/osc-overview)を見る
- 対象のアバターで、使う表情と OSC のパラメーターの設定を確認できていること
- コードを AI に書いてもらえる環境があること。AI に FloatSoda の資料を渡す方法は、[AI に FloatSoda を書かせる](../../3-prepare/write-with-llm.md)にある

<!-- ※要確認: VRChat の公式資料の URL と、OSC を有効にする手順。VRChat の公式資料で確かめる -->
<!-- ※要確認: 対象アバターに必要な準備(表情と OSC パラメーターの対応の設定)。FaceEmo と VRChat の公式資料で確かめる -->

## 手順1: 表情のボタンを並べたパネルを作る

ボタンは、[ボタンやトグルを組み立てる](build-buttons-toggles.md)の手順1と同じ形で作ります。表情の名前は、1か所の配列にまとめます。ボタンは `Wrap` に入れると、横に並べて、幅に収まらなければ次の行に折り返します。

```csharp
public record ExpressionPanel : StatelessWidget
{
    public static readonly string[] Names = ["Neutral", "Smile", "Angry", "Sad", "Surprised", "Wink"];

    public required Action<int> OnSelected { get; init; }

    public override Widget Build(IBuildContext context)
    {
        var buttons = new List<Widget>();
        for (var i = 0; i < Names.Length; i++)
        {
            var index = i;
            buttons.Add(new GestureDetector
            {
                OnTap = () => OnSelected(index),
                Child = new ClipRoundRect
                {
                    BorderRadius = BorderRadius.All(Radius.Circular(15)),
                    Child = new SizedBox
                    {
                        Width = 240,
                        Height = 100,
                        Child = new ColoredBox
                        {
                            Color = SKColors.CornflowerBlue,
                            Child = new Center
                            {
                                Child = new Text(Names[index])
                                {
                                    Style = new TextStyle { Color = SKColors.White, FontSize = 36, FontWeight = 700 },
                                },
                            },
                        },
                    },
                },
            });
        }

        return new Align
        {
            Alignment = Alignment.Center,
            Child = new SizedBox
            {
                Width = 780,
                Child = new Wrap
                {
                    Spacing = 20,
                    RunSpacing = 20,
                    Children = buttons,
                },
            },
        };
    }
}
```

ループの中で `var index = i;` と書き写すのは、各ボタンが、自分の番号を覚えておくためです。

起動のコードでは、このパネルをダッシュボードのウィンドウに入れます。`Dpm` は `FloatSoda.Abstractions.Geometries` の名前空間にあります。

```csharp
app.CreateWindow(new DashboardWindow
{
    Dpm = new Dpm(1000),
    Title = "Expression",
    Child = new ExpressionPanel { OnSelected = index => Console.WriteLine($"expression: {index}") },
});
```

**できた?** ダッシュボードに、`Expression` のタブが出て、6つのボタンが3つずつ2行で並べば、手順1は完了です。

<!-- ※要確認: HMD のダッシュボードに表示され、押せること、6つのボタンが3列2行に折り返されること。コードのコンパイルは確かめた。表示は HMD で確かめる -->

<details>
<summary>できなかったら</summary>

ボタンが1列に並ぶ、または並ばないときは、[レイアウトの考え方](../../5-troubleshoot/layout-model.md)で、大きさの決まり方を確かめます。ボタンを押しても反応しないときは、[反応しないときに切り分ける](../../5-troubleshoot/not-responding.md)に進みます。

</details>

## 手順2: 押したときの処理を接続する

> 【未決】この節は判断が要るため、ト書きのまま残しています。オーナーが決めたあとで本文にします(追跡: #219)。

- 読み手が持って来るもの: 手順1のパネル
- このページが渡すもの: 押したボタンの番号を、関数として外へ渡す接続口の作り方
- 読み手がすること: ボタンを押すと、番号がコンソールに出るようにする
- できた?: ボタンを押すと、押した番号が出る
- 次へ進む先: 手順3
- 【未決】接続口を、関数の引数で渡す形にするか、`services` に登録した送信クラスを呼ぶ形にするか。後者は DI を使うので「Generic Host の仕組みを使う」へのリンクが増える

<details>
<summary>仮に書いた本文(判断の材料。採用は未定)</summary>

接続口は、`ExpressionPanel` の `OnSelected` です。ボタンを押すと、`OnSelected` に、押したボタンの番号(`int`)が渡されます。番号は 0 から始まり、`Names` の並びと同じです。

手順1の起動のコードは、すでに `OnSelected` に、番号をコンソールへ出す処理を渡しています。送信の処理を別のクラスにしたいときは、`builder.Services` に登録して、起動のコードから呼ぶ形にもできます。登録の方法は、[設定やログ、裏で動く処理を足す](../mechanics/use-generic-host.md)にあります。

**できた?** ボタンを押すと、コンソールに `expression: 0` のように、押した番号が出れば、手順2は完了です。

<details>
<summary>できなかったら</summary>

番号が出ないときは、ボタンが反応していません。[反応しないときに切り分ける](../../5-troubleshoot/not-responding.md)に進みます。

</details>

</details>

## 手順3: OSC の送信を AI に頼む

OSC の送信は、AI に書いてもらいます。頼むときは、次の6点を伝えます。

| 伝えること | 内容 |
| :-- | :-- |
| 渡される値 | `Action<int>` で渡る、0 から始まるパネル上の番号 |
| 送信先のホストとポート | VRChat の OSC の受信先。VRChat の公式資料で確かめた値を渡す |
| OSC メッセージのアドレス | 表情を切り替えるパラメーターのアドレス。FaceEmo の資料で確かめた値を渡す |
| 値の型 | 送る値の型(int、float、bool のどれか)。FaceEmo の資料で確かめた型を渡す |
| 番号から送る値への対応 | パネル上の番号と、送る値の対応表。パネルの番号を、そのまま送る値にできるとは限らない |
| 呼ぶ位置 | `ExpressionPanel` の `OnSelected` に渡す関数。`ExpressionPanel` 自体は書き換えない |

頼み方の例は、次のとおりです。

```text
C# で、int 型の表情の番号を受け取って、VRChat に OSC で送る関数 SendExpression(int index) を書いてください。
送信先のホストとポートは <VRChat の資料で確かめた値>、OSC メッセージのアドレスは <FaceEmo の資料で確かめたアドレス>、値の型は <確かめた型> です。
番号から送る値への対応は <対応表> です。
OSC のライブラリは NuGet から入れてください。
```

できあがったコードは、`OnSelected` に渡します。

```csharp
Child = new ExpressionPanel { OnSelected = SendExpression },
```

できあがったコードを受け取ったら、次の4点を点検します。

- 送る値が、対応表のとおりに、手順2でコンソールに出た番号から変換されている
- 送信先のホストとポートが、VRChat の公式資料と同じ
- OSC メッセージのアドレスと値の型が、FaceEmo の資料と同じ
- `ExpressionPanel` の中身を、AI が書き換えていない

<!-- ※要確認: AI に頼むときのプロンプトの形。ジュニアコーダーテストで確かめる -->
<!-- ※要確認: VRChat 側の OSC の受信ポートとアドレスの形。VRChat の公式資料で確かめる -->
<!-- ※要確認: FaceEmo の表情の番号と OSC のパラメーターの対応。FaceEmo の公式資料で確かめる -->

OSC のポートや受信の設定は、FloatSoda の外の話です。直し方はここに書かず、VRChat の公式資料に任せます。

**できた?** ボタンを押すと、VRChat の中で表情が切り替われば、手順3は完了です。

## 結果を確かめる

VRChat を起動し、HMD の中でダッシュボードを開いて、`Expression` のタブのボタンを押します。ボタンごとに、期待した表情へ切り替わるかを見ます。

<!-- ※要確認: ダッシュボードのオーバーレイを開いたまま、VRChat の表情を切り替えられるか。HMD で確かめる -->

できあがったツールを配るときは、[exe にして配る](../../6-distribute/publish-exe.md)に進みます。SteamVR と一緒に起動したいときは、[SteamVR と一緒に起動する](../../6-distribute/launch-with-steamvr.md)に進みます。

## うまくいかないとき

押しても表情が変わらないときは、次の順で切り分けます。

1. 手順2のログで、ボタンから番号が渡ることを確認する。出なければ、[反応しないときに切り分ける](../../5-troubleshoot/not-responding.md)に進む
2. 送信の関数が呼ばれたことと、送信処理の成否を、コンソールのログで確認する。ログがなければ、AI に書いてもらった送信の関数に、ログを出す1行を足してもらう。ログには、実際に確かめた段階だけを出す
3. OSC が送られているのに VRChat が反応しない場合は、FloatSoda の外の話なので、VRChat の公式資料を確かめる
