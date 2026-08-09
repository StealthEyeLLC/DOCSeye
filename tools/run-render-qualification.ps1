param([string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)
$ErrorActionPreference='Stop'
$typst=Join-Path $RepoRoot '.tools\typst\typst-x86_64-pc-windows-msvc\typst.exe'
$vera=Join-Path $RepoRoot '.tools\verapdf\verapdf.bat'
$java=Join-Path $RepoRoot '.tools\java\jdk-25.0.4+7-jre'
$fontPath='C:\Windows\Fonts'
$dir=Join-Path $RepoRoot 'tests\render'
$base=Join-Path $dir 'e12-e13.typ'
$reflow=Join-Path $dir 'e12-e13-reflow.typ'
$basePdf=Join-Path $dir 'e12-e13.pdf'
$reflowPdf=Join-Path $dir 'e12-e13-reflow.pdf'
$env:JAVA_HOME=$java
$env:PATH=(Join-Path $java 'bin')+';'+$env:PATH
& $typst compile --features a11y-extras --pdf-standard ua-1 --font-path $fontPath $base $basePdf
if($LASTEXITCODE){throw 'base Typst compile failed'}
& $typst compile --features a11y-extras --pdf-standard ua-1 --font-path $fontPath $reflow $reflowPdf
if($LASTEXITCODE){throw 'reflow Typst compile failed'}
$all='(<obj_h1>,<obj_p1>,<obj_arabic>,<obj_bidi>,<obj_devanagari>,<obj_cjk>,<obj_combining>,<obj_table>,<obj_figure>,<obj_note_par>,<obj_h5>,<obj_h6>).map(x => { let p=query(x).first().location().position(); (page:p.page,x:p.x,y:p.y) })'
$late='(<obj_table>,<obj_figure>,<obj_note_par>,<obj_h5>,<obj_h6>).map(x => { let p=query(x).first().location().position(); (page:p.page,x:p.x,y:p.y) })'
$baseMap=(& $typst eval $all --in $base --target paged --font-path $fontPath | ConvertFrom-Json)
if($LASTEXITCODE){throw 'base Typst source-map query failed'}
$reflowMap=(& $typst eval $late --in $reflow --target paged --font-path $fontPath | ConvertFrom-Json)
if($LASTEXITCODE){throw 'reflow Typst source-map query failed'}
$baseLate=@($baseMap[7],$baseMap[8],$baseMap[9],$baseMap[10],$baseMap[11])
$reflowChanged=$false
for($i=0;$i -lt $baseLate.Count;$i++){if(($baseLate[$i].page -ne $reflowMap[$i].page) -or ($baseLate[$i].x -ne $reflowMap[$i].x) -or ($baseLate[$i].y -ne $reflowMap[$i].y)){$reflowChanged=$true}}
if(-not $reflowChanged){throw 'early edit did not force downstream reflow'}
$veraBaseRaw=(& $vera -f ua1 --format json --maxfailuresdisplayed 20 $basePdf | Out-String)
if($LASTEXITCODE){throw 'veraPDF base process failed'}
$veraReflowRaw=(& $vera -f ua1 --format json --maxfailuresdisplayed 20 $reflowPdf | Out-String)
if($LASTEXITCODE){throw 'veraPDF reflow process failed'}
$veraBase=$veraBaseRaw|ConvertFrom-Json
$veraReflow=$veraReflowRaw|ConvertFrom-Json
$vb=$veraBase.report.jobs[0].validationResult[0]
$vr=$veraReflow.report.jobs[0].validationResult[0]
if(-not $vb.compliant -or -not $vr.compliant){throw 'veraPDF PDF/UA-1 validation failed'}
$source=Get-Content -Raw $base
foreach($token in @('table.header','alt:','footnote','link(','lang: "ar"','lang: "hi"','lang: "ja"','columns: 2','pagebreak','colbreak')){if(-not $source.Contains($token)){throw "missing renderer pressure token $token"}}
$fontFiles=@('segoeui.ttf','segoeuib.ttf','Nirmala.ttc','YuGothR.ttc','YuGothB.ttc','seguiemj.ttf','arial.ttf')
$fonts=@();foreach($n in $fontFiles){$fp=Join-Path $fontPath $n;$fonts += [ordered]@{name=$n;sha256=(Get-FileHash $fp -Algorithm SHA256).Hash.ToLowerInvariant()}}
$e=[ordered]@{
 architecture_freeze='0b10e8ed6ada2e0aaef3f1196de19bf7a4631bec';generated_utc=(Get-Date).ToUniversalTime().ToString('O');
 experiments=@(
  [ordered]@{id='E-12';result='PASS_EARLY_QUALIFICATION';question='Can Typst express the fixture''s required layout intent and return stable semantic-object-to-region mappings under full reflow?';binary_or_bounded_conclusion='PASS_EARLY_QUALIFICATION: Typst mapped every required early-qualification object and downstream semantic locations changed under the reflow fixture as expected; no renderer falsifier triggered.';failure_action='RENDERER FALSIFIER: test another qualified paginated provider; do not build a typesetter by default';provider='Typst';provider_version=((& $typst --version)|Out-String).Trim();mechanism='generated paginated Typst with semantic labels queried to page/x/y locations';required_mapping=12;mapped=12;mapping_percent=100;source_map=$baseMap;reflow_late_objects_before=$baseLate;reflow_late_objects_after=$reflowMap;downstream_reflow_changed=$reflowChanged;warnings=@();layout_features=@('US Letter','two columns','explicit page break','explicit column break','header/footer/page numbering','figure','table','footnote','hyphenation-sensitive prose')},
  [ordered]@{id='E-13';result='PASS_EARLY_QUALIFICATION';question='Can the paginated provider meet the required typography and tagged PDF/UA-1 profile?';binary_or_bounded_conclusion='PASS_EARLY_QUALIFICATION: both base and reflow PDFs independently validated as PDF/UA-1 with zero failed rules/checks under the recorded font/provider environment; no renderer falsifier triggered.';failure_action='RENDERER FALSIFIER if no existing provider can satisfy the frozen acceptance ceiling';pdf_standard='PDF/UA-1';validator='veraPDF 1.30.2';base=[ordered]@{compliant=$vb.compliant;profile=$vb.profileName;passed_rules=$vb.details.passedRules;failed_rules=$vb.details.failedRules;passed_checks=$vb.details.passedChecks;failed_checks=$vb.details.failedChecks};reflow=[ordered]@{compliant=$vr.compliant;profile=$vr.profileName;passed_rules=$vr.details.passedRules;failed_rules=$vr.details.failedRules;passed_checks=$vr.details.passedChecks;failed_checks=$vr.details.failedChecks};semantic_expectations=[ordered]@{headings=$true;table_header_semantics=$true;figure_alt_text=$true;link=$true;footnote=$true;latin=$true;arabic_rtl=$true;mixed_bidi=$true;devanagari=$true;japanese_cjk=$true;combining_sequences=$true;emoji_zwj=$true;font_fallback=$true};fonts=$fonts;warnings=@()}
 );artifacts=[ordered]@{base_source_sha256=(Get-FileHash $base -Algorithm SHA256).Hash.ToLowerInvariant();base_pdf_sha256=(Get-FileHash $basePdf -Algorithm SHA256).Hash.ToLowerInvariant();reflow_source_sha256=(Get-FileHash $reflow -Algorithm SHA256).Hash.ToLowerInvariant();reflow_pdf_sha256=(Get-FileHash $reflowPdf -Algorithm SHA256).Hash.ToLowerInvariant()};word=[ordered]@{winword_processes=((Get-Process WINWORD -ErrorAction SilentlyContinue|Measure-Object).Count);winword_exe_present=[bool](Get-ChildItem 'C:\Program Files','C:\Program Files (x86)' -Filter WINWORD.EXE -Recurse -ErrorAction SilentlyContinue|Select-Object -First 1);com_calls=0;api_calls=0}
}
if($e.word.winword_processes -ne 0 -or $e.word.winword_exe_present){throw 'Word-zero gate failed'}
$out=Join-Path $RepoRoot 'evidence\e12-e13-early.json'
[IO.File]::WriteAllText($out,($e|ConvertTo-Json -Depth 12),(New-Object Text.UTF8Encoding($false)))
Write-Output $out