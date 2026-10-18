← [コントリビューター](../Home.md)

# レビューの指摘を受けて直す

## この手順でできること

PR についたレビューの指摘を、重さで見分けて、直すものと返すものに分けられます。手順が終わると、blocking の指摘が残っておらず、すべての指摘に「直した」または理由つきで「直さない」という返事がある状態になります。手順は4つです。

1. 指摘を重要度で分ける
2. blocking の指摘を直す
3. 非 blocking とスコープ外の指摘に返事をする
4. 直した差分を確かめて、返事を書く

指摘の重さが分からないまま全部を直すと、PR が膨らみます。重い指摘を見落とすこともあります。そのため、先に重さの基準を使って分けます。基準の正本は [REVIEW.md](../../../REVIEW.md) です。このページは使い方だけを書きます。

## 前提条件

次の2つがそろっていれば、手順1から始められます。

- 手元でビルドとテストが通っていること([開発環境を用意する](../3-prepare/dev-environment.md))
- レビューの基準の正本 REVIEW.md の場所を確かめておくこと([コントリビューターの規約](../3-prepare/conventions.md) に案内があります)

## 手順1: 指摘を重要度で分ける

指摘を「blocking」と「非 blocking」に分けます。非 blocking のうち、PR の範囲外のものは「別 Issue 向け」とします。

重要度は上ほど重くなります(順序の正本は REVIEW.md の「1. 重要度の順序」)。

1. behavioral correctness
2. tree lifecycle / state transitions
3. incremental update correctness
4. public API consistency
5. test coverage
6. documentation consistency
7. performance
8. style / maintainability

blocking になるのは、重要度が1〜5で、concrete failure mode を説明できる指摘です。concrete failure mode とは、どの入力や状態で何が壊れるかのことです。6〜8は、原則として非 blocking の提案です。concrete failure mode を書けない指摘は、書けるようになるまで格下げされます(REVIEW.md の「2. finding の基準」)。

スコープ外の改善要求は、「別 Issue 向け」と明示した非 blocking として伝えられます。

自動レビュー(CodeRabbit)は blocking の判断をしません。`.coderabbit.yaml` で `request_changes_workflow: false` にしてあり、blocking の判断は人間・エージェントによる PR レビューが行います(REVIEW.md の「9. レビューの層と分担」)。

**できた?** すべての指摘が、blocking か非 blocking のどちらかに入っています。範囲外の指摘には、「別 Issue 向け」と書いてあります。

## 手順2: blocking の指摘を直す

直す範囲は、指摘に絞ります。無関係なリファクタリング、リネーム、クリーンアップ、依存の追加・削除は、同じ PR に混ぜません([コントリビューターの規約](../3-prepare/conventions.md) の「PR のスコープ」)。

バグ修正には regression test が要ります。判定基準は、修正を戻すとテストが落ちるかです(REVIEW.md の「5. テスト」)。

**できた?** blocking の指摘ごとに、直した内容と、解消を確かめた根拠(再現コードか回帰テストの結果)を言えます。

**できた?** 手元で CI と同じ手順が通ります([CI が落ちたとき](ci-failures.md) の手順2にコマンドがあります)。

指摘に同意できないときは、返事の前に、concrete failure mode が本当にあるかを再現コードで確かめます。既存のコードがそうなっているという理由だけでは、正しい仕様とは判断されません。仕様が食い違うときは、FloatSoda で明示された差異・設計判断、Flutter の仕様、の順に確かめます。既存の実装は根拠になりません(REVIEW.md の「3. 仕様の一次情報と優先順位」)。

## 手順3: 非 blocking とスコープ外の指摘に返事をする

残りの指摘に、1つずつ返事を書きます。

- 非 blocking: 直す、または直さない理由を書く
- 別 Issue 向け: Issue を立ててリンクを書く、または立てない理由を書く

<!-- ※要確認: 返事の書き方(コメントの形、再レビューの依頼の仕方)の慣行、指摘に同意できないときの最終判断者、CodeRabbit の指摘への返事の要否。リポジトリの文書に一次情報が無い。確かめ方: オーナーに確認する -->

**できた?** 返事がない指摘が残っていません。

## 手順4: 直した差分を確かめて、返事を書く

差分が、指摘への対応だけになっているかを確かめます。そのうえで、PR 本文の「Tests」と「Scope / follow-up」の欄を、今の差分に合わせて更新します。最後に、PR のコメントへ、直した旨と変更点を書きます。

**できた?** PR 本文が今の差分と合っています。

CI が落ちたときは [CI が落ちたとき](ci-failures.md) に進んでください。

## うまくいかないとき

<details><summary>指摘の意味が分からない</summary>

指摘の根拠を、PR のコメントで尋ねます。どの入力や状態で何が壊れるか、を聞いてください。

</details>

<details><summary>指摘に同意できない</summary>

手順2の再現コードで確かめます。確かめたうえでも判断が割れるときは、PR のコメントで相談してください。

</details>
