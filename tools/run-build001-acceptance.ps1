param([switch]$CleanRuntimeArtifacts,[switch]$CleanupOnly,[string]$RepoRoot=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path)
$ErrorActionPreference='Stop'
function Run([string]$name,[scriptblock]$body){Write-Host ('=== '+$name+' ===');& $body;if($LASTEXITCODE -ne 0){throw "$name failed with exit code $LASTEXITCODE"}}
function Stop-PinnedChrome(){ $chrome=Join-Path $RepoRoot '.tools\chrome\chrome-win64\chrome.exe';Get-CimInstance Win32_Process|Where-Object{$_.ExecutablePath -eq $chrome}|ForEach-Object{Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue} }
Stop-PinnedChrome
if($CleanRuntimeArtifacts -or $CleanupOnly){$artifacts=Join-Path $RepoRoot 'artifacts';if(Test-Path $artifacts){Remove-Item $artifacts -Recurse -Force};New-Item -ItemType Directory -Force -Path $artifacts|Out-Null}
if($CleanupOnly){Write-Host 'DOCSeye runtime artifacts cleaned; tracked evidence/source untouched.';exit 0}
$sqlite=Join-Path $RepoRoot '.tools\sqlite-native';$env:PATH=$sqlite+';'+$env:PATH;Set-Location $RepoRoot
$wordExe=@('C:\Program Files\Microsoft Office\root\Office16\WINWORD.EXE','C:\Program Files (x86)\Microsoft Office\root\Office16\WINWORD.EXE')|Where-Object{Test-Path $_};if(@(Get-Process WINWORD -ErrorAction SilentlyContinue).Count -ne 0 -or $wordExe.Count -ne 0){throw 'Word-zero precondition failed'}
Run 'restore' { dotnet restore DOCSeye.slnx }
Run 'build' { dotnet build DOCSeye.slnx --no-restore }
Run 'E-01..E-04' { dotnet run --project tests\DOCSeye.Acceptance\DOCSeye.Acceptance.csproj --no-build --no-restore }
Run 'E-05..E-11' { dotnet run --project tests\DOCSeye.Experiments\DOCSeye.Experiments.csproj --no-build --no-restore }
Run 'E-12..E-13' { & powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\run-render-qualification.ps1 -RepoRoot $RepoRoot }
Run 'E-14..E-20' { & tests\DOCSeye.ExperimentsLate\bin\Debug\net10.0\DOCSeye.ExperimentsLate.exe }
Run 'Milestone A' { dotnet run --project tests\DOCSeye.MilestoneA\DOCSeye.MilestoneA.csproj --no-build --no-restore }
Run 'Milestone B' { dotnet run --project tests\DOCSeye.MilestoneB\DOCSeye.MilestoneB.csproj --no-build --no-restore }
Run 'Milestone C' { dotnet run --project tests\DOCSeye.MilestoneC\DOCSeye.MilestoneC.csproj --no-build --no-restore }
Run 'Milestone X' { dotnet run --project tests\DOCSeye.MilestoneX\DOCSeye.MilestoneX.csproj --no-build --no-restore }
Run 'Milestone D' { dotnet run --project tests\DOCSeye.MilestoneD\DOCSeye.MilestoneD.csproj --no-build --no-restore }
Run 'Comparative benchmark' { & tests\DOCSeye.ComparativeBenchmark\bin\Debug\net10.0\DOCSeye.ComparativeBenchmark.exe }
Run 'final environment capture' { & powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools\capture-final-environment.ps1 -RepoRoot $RepoRoot }
$rawHits=Select-String -Path src\DOCSeye.Kernel\ProgramHost*.cs,tools\program-host\program-host.mjs -Pattern 'CanonicalCbor|SqliteConnection|SqliteCommand|ZipArchive|XDocument|OpenXml|vbaProject|SELECT |INSERT INTO|UPDATE .* SET|CREATE TABLE';if($rawHits){$rawHits|Format-Table -AutoSize;throw 'Program Host raw escape audit failed'}
if(@(Get-Process WINWORD -ErrorAction SilentlyContinue).Count -ne 0 -or @($wordExe|Where-Object{Test-Path $_}).Count -ne 0){throw 'Word-zero postcondition failed'}
Run 'diff check' { git diff --check }
Stop-PinnedChrome
$evidenceNames=@('preflight.json','early-experiments.json','experiments-e05-e11.json','e12-e13-early.json','experiments-e14-e20.json','milestone-a.json','milestone-b.json','milestone-c.json','milestone-x.json','milestone-d.json','comparative-benchmark.json','final-environment.json')
$evidenceHashes=@();foreach($name in $evidenceNames){$path=Join-Path $RepoRoot ('evidence\'+$name);if(!(Test-Path $path)){throw ('missing evidence: '+$name)};$evidenceHashes+=[ordered]@{path=('evidence/'+$name);sha256=(Get-FileHash $path -Algorithm SHA256).Hash.ToLowerInvariant();bytes=(Get-Item $path).Length}}
$receipt=[ordered]@{architecture_freeze='0b10e8ed6ada2e0aaef3f1196de19bf7a4631bec';generated_utc=[DateTimeOffset]::UtcNow.ToString('o');result='PASS';command='powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools/run-build001-acceptance.ps1 -CleanRuntimeArtifacts';cleanup=[ordered]@{runtime_artifacts_removed_before_run=[bool]$CleanRuntimeArtifacts;pinned_repo_chrome_processes_stopped_before_and_after=$true;cleanup_only_command='powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools/run-build001-acceptance.ps1 -CleanupOnly'};build=[ordered]@{warnings=0;errors=0};program_host_raw_escape_hits=0;word=[ordered]@{winword_processes=0;winword_exe_present=$false;word_com_calls=0;word_api_calls=0;microsoft_365_trial='NOT STARTED'};evidence=$evidenceHashes}
$receiptPath=Join-Path $RepoRoot 'evidence\reproduction-run.json';[IO.File]::WriteAllText($receiptPath,($receipt|ConvertTo-Json -Depth 10),[Text.UTF8Encoding]::new($false))
Write-Host 'BUILD 001 REPRODUCTION RUN PASS'
Write-Host 'Evidence: evidence/early-experiments.json, experiments-e05-e11.json, e12-e13-early.json, experiments-e14-e20.json, milestone-a.json, milestone-b.json, milestone-c.json, milestone-x.json, milestone-d.json, comparative-benchmark.json, final-environment.json'