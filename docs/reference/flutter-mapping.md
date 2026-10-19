← [リファレンス](Home.md)

# Flutter の読み替え表

> このページは執筆前の骨組みです。設計シートとト書きだけを置き、本文はまだ書いていません(追跡: #219)。

## 設計シート

- 分類: 説明型(Diátaxis: reference)
- 読み手: だれが / Flutter や React のような宣言的 UI を知っている人、または Flutter の docs や記事を手本に FloatSoda のコードを書く(AI に書かせる)人 / 何を知っている / Flutter のウィジェットと `setState` を知っている。FloatSoda の API は知らない / 読んで何をしたい / Flutter で知っている書き方が、FloatSoda でそのまま通るか、変わるか、まだないかを引きたい
  - 共通の性質と専門用語の扱い: CONTRIBUTING.md の「ドキュメントのトンマナ」の「読み手」
- 出力言語と文体の指定: 日本語。敬体。トンマナは CONTRIBUTING.md の「ドキュメントのトンマナ」に従う
- テーマ: Flutter の名前と書き方が FloatSoda でどうなるかを、「同じ」「違う」「まだない」の3つに分けて引けるようにする
- 依頼の解釈: 個々の差異の詳細は、各作例の README の「Flutterとの違い」に1か所だけ書く(正本)。このページは、差異の種類で引く索引とし、詳細はコピーしない
- テーマファイル: reference
- Why(目的): 「Flutter と同じつもりで書いたら動かない」を、書く前に防ぎ、動かなかったときに原因を切り分けられるようにする
- What(目標・結果): Flutter の名前を起点に、同じ・違う・まだないの区分と、詳細へのリンクが同じ形で並ぶ。読み手が自分の書こうとしているコードの区分を1回で引ける
- How(手段): 区分ごとの表で並べる。列は「Flutter での名前または書き方」「FloatSoda での名前または書き方」「区分」「詳細(作例の README か Issue)」とする
- 要素:
  - FloatSoda は Flutter の三ツリー(Widget、Element、RenderObject)に合わせてある。ウィジェット名、レイアウト用語(`MainAxisAlignment`、`CrossAxisAlignment`)、`Child` と `Children` は Flutter の名前を使う(AGENTS.md の「Architecture: Three Trees」「API Design Conventions」、`CONTRIBUTING.md`)
  - 状態の書き方は Flutter に合わせてある。`StatefulWidget<T>` と `State<T>` を継承し、`SetState()` で再ビルドする。型の書き方が違う(`StatefulWidget<T>` はジェネリック)(AGENTS.md の「Widget/Element Tree」)
  - 子の指定は、コンストラクタの引数ではなく、オブジェクト初期化子の `Child` と `Children`(`IList<Widget>`)で行う。名前付きコンストラクタは持たない(AGENTS.md の「API Design Conventions」、`samples/FloatSoda.Samples.SizedBox/README.md`)
  - 名前が変わるもの: `ClipRRect` は `ClipRoundRect`、`ClipPath` は `ClipCustomPath`、`MouseRegion` は `PointerRegion`、`Opacity.opacity` は `Opacity.Value`、`AspectRatio.aspectRatio` は `AspectRatio.Ratio`、`Padding.padding` は `Padding.Spacing`、`behavior`(`HitTestBehavior`)は `Behaviour`(`HitTestBehaviour`)(各作例の README の「Flutterとの違い」)
  - 型の書き方が変わるもの: 色は `Color(r, g, b)`(0〜255 の整数)、フォントの太さは `int`(太字は `700`)、変換の行列は `Matrix3x2`(2次元アフィンのみ)(`samples/FloatSoda.Samples.ColoredBox/README.md`、`Text/README.md`、`Transform/README.md`)
  - リスナーの登録は `addListener` ではなく C# の `event` を使う(`CONTRIBUTING.md` と AGENTS.md の API 規約が参照する API 設計規約。再編前の `docs/APIDesign.md` の 3.6)
  - 入力の語彙は「マウス」ではなく「ポインター」で統一する(再編前の `docs/APIDesign.md` の「入力語彙は『ポインター』で統一する」)
  - 挙動が違うもの(見た目に出る): `Row`/`Column` の既定の揃え方(`Flex` は `Center`、`Row` と `Column` は `Start`。Flutter は `start`)、`Stack` は範囲外の子をクリップしない、`Image` の `Fit` の既定は `Contain`、`Container` は `Border` の寸法を余白に加算しない、子なしの `Container` の大きさ(追跡: #224、#257、#267、#268)(`samples/FloatSoda.Samples.Flex/README.md`、`Stack/README.md`、`Image/README.md`、`Container/README.md`)
  - 受け付ける項目が少ないもの: `GestureDetector` はタップと Pan のみ、`BoxDecoration` は背景色・角丸・ボーダーのみ、`Text` は折り返しと省略の指定がない、`Flex` に `spacing` がない(各作例の README)
  - 文字方向(`textDirection`)による揃え方の切り替えは、どのウィジェットにもない(`samples/FloatSoda.Samples.Align/README.md` ほか)
  - まだないもの: Material と Cupertino に当たるデザインシステム(`FloatSoda.UI` 系、未提供)、スクロール系、`Tween` と暗黙的アニメーション、`LayoutBuilder`、`CustomPaint`、`MediaQuery` に当たるもの、`Notification`(追跡: #126、#137、#163、#174、#171、#173、#175)(GitHub の Issue)
  - 実装しないもの: Semantics(アクセシビリティ)(再編前の `docs/APIDesign.md` の「実装しない API」)
  - Flutter の docs へのリンクの載せ方: 同じ区分の項目には、Flutter の docs の該当ページへのリンクを添える(docs 再編の決定、2026-10-16〜17)
- 見出し:
  - この表の使い方
  - 同じように書けるもの
  - 名前や書き方が違うもの
  - 挙動が違うもの
  - まだないもの・実装しないもの
- ※要確認:
  - `Navigator` や `Theme` に当たるものが、まったくないのか、別の名前であるのか(`src/` で確かめていない)
  - 名前が変わるものの一覧に、漏れがないか(作例のない型の改名は、README から拾えない)
  - 「同じ」と書く項目は、作例の README の「同等です」の記述を根拠にする。作例のない型(`IndexedStack` など)で同じと言えるか

## ト書き

### この表の使い方

- 読み手が持って来るもの: Flutter のコード、または Flutter の docs のページ
- このページが渡すもの: 区分の意味(同じ、違う、まだない)、表の列の意味、詳細は作例の README にあること
- 読み手がすること: 自分が書こうとしているウィジェットの名前で、表を引く
- 次へ進む先: 該当する区分の節
- 【未決】表を手で持つか、作例の README の「Flutterとの違い」から生成するか。手で持つと、差異が解消されたときに取り残される(docs のドリフト回避方針)

### 同じように書けるもの

- 読み手が持って来るもの: Flutter で知っているウィジェットの名前
- このページが渡すもの: 名前と使い方が同じと確かめたウィジェットと、Flutter の docs へのリンク。「同じ」の範囲(レイアウトのしくみ、`SetState`)
- 読み手がすること: Flutter の知識のまま書いてよいと確かめる
- 次へ進む先: 作例のカタログ、または「部品を目的から探す」

### 名前や書き方が違うもの

- 読み手が持って来るもの: Flutter で知っている書き方
- このページが渡すもの: 名前の対応表と、書き方の違い(オブジェクト初期化子、`event`、色、行列)。違いの理由は1語で(C# の流儀、オーバーロードしにくい、同名にできない)
- 読み手がすること: 自分のコードを表のとおりに書き換える
- できた?: コンパイルが通る
- できなかったら: エラーの読み方は、ガイドの「エラーを読んで直す」。C# の書き方が分からないときは、ガイドの「C# がほぼ初めての人へ」
- 次へ進む先: 「挙動が違うもの」

### 挙動が違うもの

- 読み手が持って来るもの: コンパイルは通るが、見た目や反応が Flutter と違う
- このページが渡すもの: 挙動が違うウィジェットの一覧と、症状、直し方の1行、詳細な作例の README へのリンク。意図した差異かバグかは区別して書く
- 読み手がすること: 自分の症状と一致するかを確かめ、直す
- 次へ進む先: ガイドの「反応しないときに切り分ける」、「レイアウトの考え方」
- 【未決】意図した差異とバグを、同じ表に載せるか(読み手が払うコストは同じだが、直る見込みは違う。CONTRIBUTING.md の README 規約は、同じ節に書く)

### まだないもの・実装しないもの

- 読み手が持って来るもの: Flutter にある部品が FloatSoda で見つからない
- このページが渡すもの: まだないもの、実装しないものの区別と、追跡先(Issue の番号)
- 読み手がすること: ないものを自分で組むか、待つかを決める
- 次へ進む先: リファレンスの「できないこと・まだ使えないもの」
- 止まる箇所: なし。ここでページは終わる
