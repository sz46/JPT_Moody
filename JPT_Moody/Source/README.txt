Moody日本語表示補助DLLのコンパイル前ソース

DisplayTranslation.cs: 表示文だけに適用するHarmony処理.
スキル見出しと心情グループ名は, 表示箇所に限ってDefの名称・心情段階の訳を参照します.
心情グループの名称が未設定なら最初の名前付き段階を代表名とします. Def識別子やグループ分けは変更しません.
GeneratedRules.cs: 実際のDLLに組み込んだ翻訳キー対応表・対象メソッド・文字列出現数・置き換え位置.
build.ps1: 1.6/1.7用の補助DLLを作成するスクリプト.

この2つのC#ファイルは, 配布DLLの作成に使用したソースと同一です.
GeneratedRules.csは生成済みの完全なソースを含むため, 再ビルドに元の生成スクリプトは必要ありません.
日本語の訳文はCommon/Languages/Japanese/Keyed/Moody.xmlにあります.

必要なもの:
・Windowsの.NET Framework C#コンパイラー (csc.exe).
・PowerShell 7と, 1.7ビルドに使用する参照アセンブリ ($PSHOME/ref).
・RimWorld本体およびHarmonyの1.6/1.7用アセンブリ.
既定パスが異なる環境ではGameRootとWorkshopRootを指定してください.
1.7のビルドは本体の.NET環境に対応するPowerShellの参照アセンブリが必要です.

実行例:
pwsh -NoProfile -File .\build.ps1 -GameRoot "D:\SteamLibrary\steamapps\common\RimWorld" -WorkshopRoot "D:\SteamLibrary\steamapps\workshop\content\294100" -OutputDirectory "C:\Temp\MoodyJapaneseBuild"

OutputDirectoryは未使用のフォルダーを指定します. 省略時は一時フォルダー内に新しく作成します.
元のMODや配置済みDLLへ自動で上書きしません. 生成された補助DLLだけを必要な配置先へコピーしてください.
CompileReference16/Assembly-CSharp.dllはコンパイル専用の最小参照です. ゲーム本体のDLLではありません. MODやゲームへ配置しないでください.
