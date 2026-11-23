# [JPT] Moody 日本語翻訳

RimWorld用MOD「Moody」の日本語翻訳です. 画面・設定・ツールチップ・チュートリアル・ショートカットを翻訳します.

**2026-11-24更新: RimWorld 1.6での起動時クラッシュを修正しました.**

## ダウンロード

**[日本語翻訳MODをダウンロード](dist/%5BJPT%5D%20Moody%20%E6%97%A5%E6%9C%AC%E8%AA%9E%E7%BF%BB%E8%A8%B3.zip?raw=true)**

ZIP内のフォルダー名は `JPT_Moody` です. コンパイル前のC#ソースとビルド手順も含みます.

## 必要なMOD

- [Moody](https://steamcommunity.com/sharedfiles/filedetails/?id=826998327)
- [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077)

## 導入

1. ダウンロードしたZIPを展開します.
2. `JPT_Moody` フォルダーをRimWorldの `Mods` フォルダーに入れます.
3. MOD管理画面で `[JP Local] Moody 日本語翻訳` を有効化し, MoodyとHarmonyより後に配置します.
4. ゲームを完全に終了して再起動します.

GitHubの「Code → Download ZIP」を使った場合は, その中の `JPT_Moody` だけをModsへ入れてください.

## 翻訳範囲

- 概要・気温・心情要因・作業・スキル・健康の各画面.
- 列見出し, 並べ替え, フィルター, ツールチップ, MOD設定.
- チュートリアル7種類の名称と本文, ショートカットの表示.

表示文78種類 (出現83箇所) とXML表示定義21項目を収録しています. スキル見出し1箇所と心情グループ名2箇所では, ゲーム側の翻訳済み名称を参照します.

原作には多くの表示文に翻訳キーがないため, Harmonyを使う表示補助DLLを同梱しています. 原作のMoody.dllは変更しません.
保存設定名・Def識別子・画像パス・診断ログは翻訳対象から除外しています. 日本語以外の言語では元の表示を維持します.

## ソースとビルド

`JPT_Moody/Source` にC#ソースと `build.ps1` を収録しています. 詳細は [Source/README.txt](JPT_Moody/Source/README.txt) を参照してください.
ゲーム本体とHarmonyは別途必要です. 原作DLL・ゲーム本体DLL・Harmony本体は配布物に含めていません.

## クレジット

- 原作Moody: Giant Space Hamster.
- 日本語翻訳MODのauthor: jpt.
- **本日本語翻訳MODと表示補助コードは, ChatGPT 6.1 Solに作成させたものです.**

原作MoodyとHarmonyはそれぞれの配布ページから入手してください.

## ライセンス

独自作成部分は **CC0 1.0 Universal** です. 原作の英語原文など第三者の権利は対象外です. [NOTICE](NOTICE) に適用範囲を, [LICENSE](LICENSE) に全文を記載しています.
