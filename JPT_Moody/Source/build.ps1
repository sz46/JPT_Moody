param(
    [string]$GameRoot='D:\SteamLibrary\steamapps\common\RimWorld',
    [string]$WorkshopRoot='D:\SteamLibrary\steamapps\workshop\content\294100',
    [string]$OutputDirectory=(Join-Path ([IO.Path]::GetTempPath()) ('MoodyJapanese_Build_'+(Get-Date -Format 'yyyyMMdd_HHmmss')))
)
$ErrorActionPreference='Stop'
$csc='C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$root=$PSScriptRoot
$build=[IO.Path]::GetFullPath($OutputDirectory)
if (Test-Path -LiteralPath $build) { throw '出力先が既にあります. 未使用のOutputDirectoryを指定してください.' }
foreach ($file in @('DisplayTranslation.cs','GeneratedRules.cs')) {
    if (-not (Test-Path -LiteralPath (Join-Path $root $file))) { throw "ソースがありません: $file" }
}
if (-not (Test-Path -LiteralPath (Join-Path $PSHOME 'ref'))) { throw 'PowerShell 7の参照アセンブリが必要です.' }
New-Item -ItemType Directory -Path $build | Out-Null
$stubDirectory=Join-Path $build 'CompileReference16'
New-Item -ItemType Directory -Path $stubDirectory | Out-Null
$stub=Join-Path $stubDirectory 'Assembly-CSharp.dll'
$stubSource=Join-Path $stubDirectory 'CompileStubs.cs'
@'
using System.Reflection;
[assembly: AssemblyVersion("1.6.9371.18214")]
namespace Verse {
 public class ModContentPack {}
 public class Mod { public Mod(ModContentPack pack) {} }
}
'@ | Set-Content -LiteralPath $stubSource -Encoding UTF8
& $csc /nologo /target:library "/out:$stub" $stubSource
if ($LASTEXITCODE -ne 0) { throw '1.6のコンパイル用参照を作成できませんでした.' }
foreach ($version in @('1.6','1.7')) {
    $out=Join-Path $build "$version\Assemblies\Jpt.MoodyJapanese.dll"
    New-Item -ItemType Directory -Path (Split-Path -Parent $out) -Force | Out-Null
    $sources=@((Join-Path $root 'DisplayTranslation.cs'),(Join-Path $root 'GeneratedRules.cs'))
    $harmony=Join-Path $WorkshopRoot "2009463077\$version\Assemblies\0Harmony.dll"
    if ($version -eq '1.6') {
        & $csc /nologo /target:library /optimize+ "/out:$out" "/reference:$stub" "/reference:$harmony" /reference:System.Core.dll $sources
        if ($LASTEXITCODE -ne 0) { throw '1.6用のコンパイルに失敗しました.' }
    } else {
        $refs=@((Join-Path $GameRoot 'Assembly-CSharp.dll'),(Join-Path $GameRoot 'UnityStubs.dll'),(Join-Path $GameRoot 'Waylith.UnityCompat.dll'),$harmony)+@(Get-ChildItem -LiteralPath (Join-Path $PSHOME 'ref') -Filter '*.dll' -File | ForEach-Object FullName)
        Add-Type -Path $sources -ReferencedAssemblies $refs -OutputAssembly $out -OutputType Library -CompilerOptions '/nowarn:1701'
    }
    Get-FileHash -LiteralPath $out | Select-Object Path,Hash
}
Write-Output "ビルド出力: $build"
