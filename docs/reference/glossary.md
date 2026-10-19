← [リファレンス](Home.md)

# 用語集

> このページは執筆前の骨組みです。設計シートとト書きだけを置き、本文はまだ書いていません(追跡: #219)。

## 設計シート

- 分類: 説明型(Diátaxis: reference)
- 読み手: だれが / FloatSoda の docs や AI が書いたコードを読んでいて、言葉の意味が分からない人。Unity から来た人も、React や Flutter のような宣言的 UI から来た人もいる / 何を知っている / VRChat や VR の基本は知っている。ウィジェットや State などの言葉は知らない / 読んで何をしたい / 分からない言葉の意味を、1回で引きたい
  - 共通の性質と専門用語の扱い: CONTRIBUTING.md の「ドキュメントのトンマナ」の「読み手」
- 出力言語と文体の指定: 日本語。敬体。トンマナは CONTRIBUTING.md の「ドキュメントのトンマナ」に従う
- テーマ: FloatSoda の docs とコードに出てくる言葉を、意味・出てくる場面・詳しく知るページの3つで引けるようにする
- 依頼の解釈: C# の言葉(`record`、`init`、ラムダなど)は、「C# がほぼ初めての人へ」に任せ、この用語集は名前と1文の意味だけを書いてそのページへ送る。逆に「C# がほぼ初めての人へ」からは、この用語集へリンクする
- テーマファイル: reference
- Why(目的): 言葉が分からなくて docs の読み進めが止まる状態を、1回の検索で解く
- What(目標・結果): 言葉ごとに、意味・出てくる場面・詳細へのリンクが同じ形で並ぶ。読み手が言葉を引いて、続きを読む
- How(手段): 用語を、FloatSoda の言葉・Flutter 由来の言葉・VR と OpenVR の言葉・C# の言葉の4つに分けて表で並べる。各項目は1〜2文にとどめ、詳細は該当するページへのリンクにする
- 要素:
  - ウィジェット(Widget): 画面の部品を、変更できない値として宣言したもの。`Widget` を継承する(AGENTS.md の「Architecture: Three Trees」)
  - Element: ウィジェットの木を実際に管理する、変更できる木の節。利用者は意識しなくてよい(AGENTS.md の「Widget/Element Tree」)
  - RenderObject: レイアウトと描画を担う木。ウィジェットの下に作られる(AGENTS.md の「RenderObject Tree」)
  - `StatelessWidget`、`StatefulWidget`、`State`、`SetState`: 状態を持たないウィジェット、持つウィジェット、状態を入れる入れ物、状態を変えて作り直しを頼む操作(AGENTS.md の「Widget/Element Tree」)
  - `InheritedWidget`: 木の下の全員に値を伝えるウィジェット(AGENTS.md の「Widget/Element Tree」)
  - `Key`: 同じウィジェットかどうかを見分ける印(`src/FloatSoda/Widgets/Key.cs`)
  - 制約(`BoxConstraints`): 親が子に渡す、大きさの上限と下限。「制約が下り、サイズが上る」(AGENTS.md の「Architecture: Three Trees」、`docs/` 再編前の RenderObjects)
  - 主軸と交差軸: `Row`/`Column` で、並べる方向とそれに直交する方向(`samples/FloatSoda.Samples.Flex/README.md`)
  - ウィンドウ(`DashboardWindow`、`WorldSpaceWindow`、`DeviceTrackedWindow`): `CreateWindow` に渡す、オーバーレイの置き場所の定義(README.md の「最小構成のコード」)
  - オーバーレイ: SteamVR が他のアプリの上に重ねて出す画面。ゲームの画面とは別に出る(`src/FloatSoda.OVR/Overlay/`)
  - ダッシュボード: SteamVR のメニューの画面。ポインタの座標が届くオーバーレイの置き場所(README.md、再編前の `docs/GettingStarted.md`)
  - ポインター: レーザーポインターとマウスをまとめた呼び方(`samples/FloatSoda.Samples.PointerRegion/README.md`)
  - ヒットテスト: ポインターの位置で、どのウィジェットが反応するかを決める処理(`samples/FloatSoda.Samples.GestureDetector/README.md`)
  - ジェスチャー: ポインターの動きの意味付け(タップ、ドラッグ)(`samples/FloatSoda.Samples.GestureDetector/README.md`)
  - アクション入力: コントローラーのボタンやスティックの入力。SteamVR のバインディングで割り当てが変わる(再編前の `docs/Input.md`)
  - `AnimationController`、`Ticker`、`Curve`: アニメーションの時間を進める部品、フレームごとの時計、動きの緩急(再編前の `docs/Animation.md`)
  - `Dpm`: 1メートルあたりのドット数。オーバーレイの実寸の決め方に使う(`src/FloatSoda.Abstractions/Geometries/Dpm.cs`)
  - `AppKey`: SteamVR がアプリを見分けるキー(`src/FloatSoda/FloatSodaOptions.cs`)
  - Generic Host(`Microsoft.Extensions.Hosting`): .NET の起動の仕組み。設定、ログ、依存性注入、バックグラウンド処理をまとめて持つ(`src/FloatSoda/FloatSodaServiceCollectionExtensions.cs`)
  - 依存性注入(DI): 必要な部品を、自分で作らず外から受け取る仕組み。FloatSoda では `FloatSodaApp` をこうして取り出す(README.md の「最小構成のコード」)
  - Skia(SkiaSharp): 描画のライブラリ。利用者は直接触らないが、色の型などに名前が出る(README.md)
  - OpenVR / SteamVR: VR のランタイムと、その API(`src/FloatSoda.OVR/`)
  - C# の言葉(オブジェクト初期化子、`init`、`record`、`with`、ラムダ、`event`): 「C# がほぼ初めての人へ」にある(docs 再編の決定、2026-10-16〜17)
  - 言葉の並べ方: 読み手が言葉を引く順は、五十音と ABC のどちらが引きやすいかを決めていない
- 見出し:
  - この用語集の使い方
  - FloatSoda と Flutter の言葉
  - VR と SteamVR の言葉
  - 起動と .NET の言葉
  - C# の言葉
- ※要確認:
  - オーバーレイの定義(SteamVR の公式な言い方に合わせてよいか。公式資料で確かめていない)
  - `Dpm` と実寸の関係の説明(#151 で「分かりやすくする」作業が open のため、説明が変わりうる)
  - 載せる言葉の範囲(AI が書いたコードに出る語を、実際のコードから拾って確かめていない)

## ト書き

### この用語集の使い方

- 読み手が持って来るもの: 分からない言葉1つ
- このページが渡すもの: 言葉の探し方(ブラウザの検索、見出し)と、各項目の形(意味、出てくる場面、詳細)
- 読み手がすること: 言葉を探して意味を読む
- 次へ進む先: 分類の各節
- 【未決】言葉の並び順を、五十音順にするか、ABC 順にするか、分類の中の出てくる順にするか

### FloatSoda と Flutter の言葉

- 読み手が持って来るもの: ウィジェット、State、制約など、docs に出てくる言葉
- このページが渡すもの: 言葉の意味を1〜2文、docs のどこで出てくるか、Flutter の docs のどこに当たるか
- 読み手がすること: 意味を読み、元のページへ戻る
- 次へ進む先: ガイドの「値が変わると、画面が変わる仕組み」、「レイアウトの考え方」。Flutter の対応は「Flutter の読み替え表」

### VR と SteamVR の言葉

- 読み手が持って来るもの: オーバーレイ、ダッシュボード、アクション入力、実寸など、VR の言葉
- このページが渡すもの: 言葉の意味を1〜2文、FloatSoda のどのウィンドウや設定に当たるか
- 読み手がすること: 意味を読み、元のページへ戻る
- 次へ進む先: ガイドの「オーバーレイとは」、「オーバーレイの種類と選び方」

### 起動と .NET の言葉

- 読み手が持って来るもの: Generic Host、依存性注入、`AddFloatSoda` など、起動のコードに出てくる言葉
- このページが渡すもの: 言葉の意味を1〜2文、チュートリアルのコードのどの行に当たるか
- 読み手がすること: 意味を読み、元のページへ戻る
- 次へ進む先: ガイドの「起動のコードは何をしているの?」

### C# の言葉

- 読み手が持って来るもの: オブジェクト初期化子、`record`、ラムダなど、C# の言葉
- このページが渡すもの: 名前と1文の意味だけ。詳しい説明は「C# がほぼ初めての人へ」にあること
- 読み手がすること: 「C# がほぼ初めての人へ」へ進む
- 止まる箇所: なし。ここでページは終わる
