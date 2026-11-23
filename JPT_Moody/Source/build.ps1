param(
 [string]$GameRoot='D:\SteamLibrary\steamapps\common\RimWorld',
 [string]$Game16Root='',
 [string]$Reference16Root='',
 [string]$WorkshopRoot='D:\SteamLibrary\steamapps\workshop\content\294100',
 [ValidateSet('1.6','1.7')][string[]]$Versions=@('1.6','1.7'),
 [string]$OutputDirectory=(Join-Path ([IO.Path]::GetTempPath()) ('MoodyJapanese_Build_'+(Get-Date -Format 'yyyyMMdd_HHmmss')))
)
$ErrorActionPreference='Stop'
$csc='C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$root=$PSScriptRoot
$build=[IO.Path]::GetFullPath($OutputDirectory)
if(Test-Path -LiteralPath $build){throw '出力先が既にあります. 未使用のOutputDirectoryを指定してください.'}
foreach($file in @('DisplayTranslation.cs','GeneratedRules.cs')){
 if(-not(Test-Path -LiteralPath (Join-Path $root $file))){throw "ソースがありません: $file"}
}
if($Versions -contains '1.6'){
 if($Reference16Root){$ref16=[IO.Path]::GetFullPath($Reference16Root)}
 else{if(-not $Game16Root){$Game16Root=$GameRoot};$ref16=Join-Path $Game16Root 'RimWorldWin64_Data\Managed'}
 $game16Assembly=Join-Path $ref16 'Assembly-CSharp.dll'
 if(-not(Test-Path -LiteralPath $game16Assembly)){throw '実際の1.6本体をGame16Rootへ指定するか, 1.6用参照アセンブリのフォルダーをReference16Rootへ指定してください.'}
 $identity=[Reflection.AssemblyName]::GetAssemblyName($game16Assembly)
 if($identity.Version.Major -ne 1 -or $identity.Version.Minor -ne 6){throw "1.6用の参照ではありません: $($identity.FullName)"}
 if(-not(Test-Path -LiteralPath (Join-Path $ref16 'mscorlib.dll'))){throw '1.6用の.NET Framework参照一式が必要です.'}
}
if($Versions -contains '1.7'){
 if(-not(Test-Path -LiteralPath (Join-Path $GameRoot 'Assembly-CSharp.dll'))){throw '1.7本体のフォルダーをGameRootへ指定してください. 1.6だけ作る場合はVersions 1.6を指定してください.'}
 if(-not(Test-Path -LiteralPath (Join-Path $PSHOME 'ref'))){throw 'PowerShell 7の参照アセンブリが必要です.'}
}
New-Item -ItemType Directory -Path $build|Out-Null
foreach($version in $Versions){
 $out=Join-Path $build "$version\Assemblies\Jpt.MoodyJapanese.dll"
 New-Item -ItemType Directory -Path (Split-Path -Parent $out) -Force|Out-Null
 $sources=@((Join-Path $root 'DisplayTranslation.cs'),(Join-Path $root 'GeneratedRules.cs'))
 $harmony=Join-Path $WorkshopRoot "2009463077\$version\Assemblies\0Harmony.dll"
 $harmonyVersion=[Reflection.AssemblyName]::GetAssemblyName($harmony).Version
 if($version -eq '1.6'){
  if($harmonyVersion.Major -ne 2){throw '1.6のビルドにはHarmony 2系が必要です.'}
  $lines=@('/nologo','/nostdlib+','/target:library','/optimize+','/define:RIMWORLD16',('/out:"'+$out+'"'))
  $lines+=Get-ChildItem -LiteralPath $ref16 -Filter '*.dll' -File|ForEach-Object {'/reference:"'+$_.FullName+'"'}
  $lines+='/reference:"'+$harmony+'"'
  $lines+=$sources|ForEach-Object {'"'+$_+'"'}
  $rsp=Join-Path $build 'compile16.rsp'
  $lines|Set-Content -LiteralPath $rsp -Encoding utf8
  & $csc /noconfig ('@'+$rsp)
  if($LASTEXITCODE){throw '1.6用のコンパイルに失敗しました.'}
 }else{
  if($harmonyVersion.Major -lt 3){throw '1.7のビルドにはHarmony 3系が必要です.'}
  $refs=@((Join-Path $GameRoot 'Assembly-CSharp.dll'),(Join-Path $GameRoot 'UnityStubs.dll'),(Join-Path $GameRoot 'Waylith.UnityCompat.dll'),$harmony)+@(Get-ChildItem -LiteralPath (Join-Path $PSHOME 'ref') -Filter '*.dll' -File|ForEach-Object FullName)
  Add-Type -Path $sources -ReferencedAssemblies $refs -OutputAssembly $out -OutputType Library -CompilerOptions '/nowarn:1701'
 }
 Get-FileHash -LiteralPath $out|Select-Object Path,Hash
}
Write-Output "ビルド出力: $build"
