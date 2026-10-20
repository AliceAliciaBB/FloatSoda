#!/usr/bin/env node
/**
 * dist/llms-full.txt から API リファレンスのページを外す。`npm run build` の後に自動実行される(package.json の postbuild)。
 *
 * API リファレンスを入れると llms-full.txt が 180 万文字を超え、全文を一度に読ませる使い方ができなくなる(#364)。
 * starlight-llms-txt には llms-full.txt だけからページを外す設定がない(exclude は llms-small.txt 用)ので、
 * astro.config.mjs の demote で API のページを末尾に寄せ、ここで API の入口のページから後ろを切り落とす。
 * API は llms.txt の索引、_llms-txt/api-*.txt、ページごとの .md からは引き続き取得できる
 */
import fs from "node:fs";
import path from "node:path";
import { API_TAB, HOME, sourceFileOf, titleOf, websiteDir } from "./docs-source.mjs";

const file = path.join(websiteDir, "dist", "llms-full.txt");
const apiHome = `${API_TAB}/${HOME}`;

if (!fs.existsSync(sourceFileOf(apiHome))) {
  console.log("trim-llms-full: API リファレンスが生成されていないので、何もしません");
  process.exit(0);
}

const text = fs.readFileSync(file, "utf8");
// starlight-llms-txt は各ページを「# <タイトル>」の行から始める。API の入口は demote で API のページの先頭に来る
const marker = `\n# ${titleOf(apiHome)}\n`;
const start = text.indexOf(marker);
if (start < 0) throw new Error(`llms-full.txt に API リファレンスの入口(${marker.trim()})が見つかりません`);
if (text.indexOf(marker, start + 1) >= 0) throw new Error(`llms-full.txt に ${marker.trim()} が 2 か所あり、切る位置を決められません`);

fs.writeFileSync(file, `${text.slice(0, start).trimEnd()}\n`);
console.log(`trim-llms-full: ${text.length} → ${start} chars (API リファレンスを外しました)`);
