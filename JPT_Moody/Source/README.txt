Moody日本語表示補助DLLのコンパイル前ソース

DisplayTranslation.cs: 表示用Harmony処理. 1.6ではRIMWORLD16シンボルを指定し, 読み込み完了後のメインスレッドへパッチ適用を遅延します.
GeneratedRules.cs: 対象メソッド, 翻訳キー, 文字列出現数と置換位置.
build.ps1: 実際のゲームDLLまたは許可を得て公開されている参照アセンブリを使用してビルドします. 独自のゲーム型スタブは生成しません.

1.6のビルド例 (インストール済みゲームを参照):
pwsh -NoProfile -File .\build.ps1 -Versions 1.6 -Game16Root "D:\SteamLibrary\steamapps\common\RimWorld" -OutputDirectory "C:\Temp\MoodyBuild16"

ゲーム本体を参照できない場合:
NuGetのKrafs.Rimworld.Ref (対象例: 1.6.4871) を展開し, ref\net472フォルダーをReference16Rootへ指定します.
https://www.nuget.org/packages/Krafs.Rimworld.Ref/1.6.4871
参照アセンブリはコンパイル用です. これだけではゲーム起動試験になりません. 配布MODへコピーしないでください.

1.7のビルド例:
pwsh -NoProfile -File .\build.ps1 -Versions 1.7 -GameRoot "D:\SteamLibrary\steamapps\common\RimWorld" -OutputDirectory "C:\Temp\MoodyBuild17"
1.7本体とPowerShell 7の参照アセンブリが必要です. 1.7ビルドへRIMWORLD16シンボルを指定しないでください.

HarmonyはWorkshopRootにある版別の0Harmony.dllを参照します. 1.6用にはHarmony 2系, 1.7用にはHarmony 3系を使用します.
OutputDirectoryには未使用のフォルダーを指定してください. ゲームや配置済みMODへ自動上書きしません.

2026-11-24の更新では配布済み1.7用DLLを維持し, 1.6用DLLを実際の1.6.4871本体で再ビルドしました.
1.6ではキャッシュ済みのスキル名とタブのツールチップも表示時に日本語化します. 設定キーやDef識別子は変更しません.
