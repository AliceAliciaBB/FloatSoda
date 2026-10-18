← [コントリビューター](../Home.md)

# UI 層(3層に分けた理由)

> このページは執筆前の骨組みです。設計シートとト書きだけを置き、本文はまだ書いていません(追跡: #219)。

## 設計シート

- 分類: 説明型(Diátaxis: explanation)
- 読み手: だれが / UI の部品(ボタンなど)を足す、または UI 層の配置を判断したい人。Flutter の `material` と `widgets` の分かれ方を知っていると読みやすい / 何を知っている / C# と、FloatSoda のウィジェットの基本 / 読んで何をしたい / 新しい部品のコードを、コア、振る舞いの層(`FloatSoda.UI`)、デザインシステムの層のどこに置くかを、自分で判断したい
- 出力言語と文体の指定: 日本語。敬体(CONTRIBUTING.md の「ドキュメントのトンマナ」の「文体」)
- テーマ: UI を、コア(`FloatSoda`)、ヘッドレスの振る舞い(`FloatSoda.UI`)、2つのデザインシステム(`FloatSoda.UI.Cream`、`FloatSoda.UI.FizzyPop`)の3層に分けた理由と、置き場所を決める基準を説明する
- 依頼の解釈: なし
- テーマファイル: explanation
- Why(目的): 振る舞いをデザインシステムの層に書くと、2つ目のデザインシステムがコードを複製することになる。置き場所の基準を知らずに部品を足すと、層の分離が崩れる
- What(目標・結果): 3層の役割、2つの規約、判定基準、見た目を注入する方式が、理由つきで説明されている。実装の進み具合は書かない(追跡: Issue の番号で示す)
- How(手段): なぜ分けるか(Flutter で起きた問題) → 3層の役割 → 2つの規約と判定基準 → 見た目の注入方式の順に説明する
- 要素:
  - UI は3層に分かれる。コア(`FloatSoda`。Skia やレンダーツリーの型に触れるものと、見た目の方針を持たない合成ウィジェット)、ヘッドレス(`FloatSoda.UI`。インタラクションの状態機械と、型付きのビルダー)、デザインシステム(`FloatSoda.UI.Cream`・`FloatSoda.UI.FizzyPop`。状態から見た目への対応だけ)である(AGENTS.md の「UI Layering Rules」)
  - プロジェクトの参照は、`FloatSoda.UI` が `FloatSoda` を参照し、`Cream` と `FizzyPop` がそれぞれ `FloatSoda.UI` を参照する。2つのデザインシステムはお互いを参照しない(`src/FloatSoda.UI/FloatSoda.UI.csproj`、`src/FloatSoda.UI.Cream/FloatSoda.UI.Cream.csproj`、`src/FloatSoda.UI.FizzyPop/FloatSoda.UI.FizzyPop.csproj`)
  - 分ける理由は、Flutter で起きた「Material ロックイン」を避けるためである。振る舞いの層が独立せず、見た目と振る舞いが `material` に一体化した(前の docs の UILayering の記述)。※要確認(Flutter 側の経緯は、Flutter の資料で確かめる)
  - 規約1: 振る舞い・状態機械は必ず `FloatSoda.UI` に置く。デザインシステムの `State` には、ヘッドレスの状態から見た目への対応以外のロジックを書かない(AGENTS.md の「UI Layering Rules」)
  - 規約2: `FloatSoda.UI` のウィジェットは、デザインシステムの `InheritedWidget` がなくても動く。テーマの `Of()` は、見つからなければ `null` を返し、部品は既定のスタイルに戻る(AGENTS.md の「UI Layering Rules」、`src/FloatSoda.UI.Cream/CreamTheme.cs` の 17〜19 行目、`src/FloatSoda.UI.Cream/Button.cs` の 35 行目)
  - 判定基準: 2つ目のデザインシステムが、1つ目のコードをコピーせずに、同じ部品を作れるか(AGENTS.md の「UI Layering Rules」)
  - 2つのデザインシステムを最初から並べて作るのは、この判定基準を常に確かめるためである(前の docs の UILayering の記述)。※要確認(ADR 化されているか)
  - 見た目の注入は、型付きのビルダーで行う。`ButtonBase` は `required Func<IBuildContext, InteractionState, Widget> Builder` を持つ(`src/FloatSoda.UI/ButtonBase.cs` の 14 行目)
  - インタラクションの状態は `InteractionState`(`IsPressed`・`IsHovered`・`IsFocused`・`IsDisabled`)の `readonly record struct` で公開する(`src/FloatSoda.UI/InteractionState.cs`)
  - Cream の `Button` は `ButtonBase` を使い、`Style` が未指定なら、祖先の `CreamTheme`、それもなければ `ButtonStyle.Default` を使う(`src/FloatSoda.UI.Cream/Button.cs` の 29、35 行目)
  - 3つのプロジェクトは `IsPackable=false` で、NuGet に出していない(`src/FloatSoda.UI/FloatSoda.UI.csproj` の 8 行目ほか)
  - `ButtonBaseState` には、ジェスチャのヒットテストを配線するという TODO のコメントがある(`src/FloatSoda.UI/ButtonBase.cs` の 35〜36 行目)。追跡: #102
  - 残りの作業の追跡: Cream の `Button` が #78、FizzyPop の `Button` が #100、背景ブラーが #38。ポインタが非ダッシュボードのオーバーレイに届かない点が #182(各 Issue の番号と題名は GitHub で確かめた。状況はページに書かない)
  - 範囲外にするもの: 各部品の API の一覧、ロードマップ(Tier 1 / Tier 2 の部品の順序)は、このページに書かない。必要なら Issue か提案のページに置く
  - Avalonia との比較: Avalonia のルックレスコントロールの契約を、型付きにした形である(前の docs の UILayering の記述)。※要確認(Avalonia の資料で確かめる)
- 見出し:
  - UI を3層に分ける理由
  - 3層の役割と、置き場所の基準
  - 2つの規約と、判定基準
  - 見た目を型付きのビルダーで渡す
  - 実装の進み具合の追い方
- ※要確認:
  - Flutter の「Material ロックイン」の経緯(Flutter 側の資料で確かめる)
  - 2つのデザインシステムを並走させる決定が、記録(ADR)になっているか(`adr-index.md`)
  - Avalonia のルックレスコントロールとの対応の記述の根拠
  - 前の docs の UILayering の内容のうち、ロードマップは書かない方針でよいか(オーナーの確認)

## ト書き

### UI を3層に分ける理由

- 読み手が持って来るもの: 「なぜボタンが3つのプロジェクトにまたがるのか」という疑問
- このページが渡すもの: 分けないと起きること(見た目と振る舞いが一体化し、別の見た目を作るために振る舞いを複製する)の説明と、Flutter での実例への言及
- 読み手がすること: 分ける動機をつかむ
- 次へ進む先: 「3層の役割と、置き場所の基準」

### 3層の役割と、置き場所の基準

- 読み手が持って来るもの: 分ける動機
- このページが渡すもの: 3層の役割の表(中身、置くもの、置かないもの)と、プロジェクトの参照の向きの図。2つのデザインシステムが互いを参照しないこと
- 読み手がすること: 自分が足す部品を、どの層に置くか仮に決める
- 次へ進む先: 「2つの規約と、判定基準」

### 2つの規約と、判定基準

- 読み手が持って来るもの: 仮の置き場所
- このページが渡すもの: 規約1(振る舞いは `FloatSoda.UI`)、規約2(テーマがなくても動く)、判定基準(2つ目のデザインシステムがコピーなしで作れるか)。基準を満たさないときに壊れる例
- 読み手がすること: 判定基準を、自分の部品に当てて置き場所を確定する
- 止まる箇所: 判定基準を満たさない場合は、ヘッドレス層の API に片方のデザインシステムの都合が漏れている。置き場所を見直す
- 次へ進む先: 「見た目を型付きのビルダーで渡す」

### 見た目を型付きのビルダーで渡す

- 読み手が持って来るもの: 置き場所の判断
- このページが渡すもの: `InteractionState` と `Builder` の型、デザインシステム側が `Builder` の中で状態から見た目を作ること、テーマが見つからないときの既定のスタイル
- 読み手がすること: 自分の部品の `Builder` の形を設計する
- 次へ進む先: 「実装の進み具合の追い方」

### 実装の進み具合の追い方

- 読み手が持って来るもの: 「いまどこまでできているか」という疑問
- このページが渡すもの: 進み具合はこのページに書かず、Issue(#102、#78、#100、#38、#182)で追うという案内
- 読み手がすること: Issue を開く
- 止まる箇所: なし。ここでこのページは終わる
- 【未決】前の docs にあった部品のロードマップ(Tier 1 / Tier 2)を、どこに置くか。このページには置かない前提だが、置き場所(提案のページ、Issue、ADR)が決まっていない
