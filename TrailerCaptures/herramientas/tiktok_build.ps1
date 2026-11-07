# Monta los 5 videos de TikTok (guiones en TikTok\fuentes, videos en TikTok\)
$here = Split-Path $MyInvocation.MyCommand.Path
$tk = "D:\Unity\AeroBlitz\TikTok"; $src = "$tk\fuentes"; $clips = "D:\Unity\AeroBlitz\TrailerCaptures"
$R = "|112|370|856|1522"
$videos = [ordered]@{
    "01_level70_not_a_joke" = @("v1", @(
        "clip|$clips\clip_182212_vertical.mp4|0.5|9.0|rotulos\r1_level70.png$R"), "f1.png|3.3")
    "02_missiles_vs_walls" = @("v2", @(
        "clip|$clips\clip_181715_vertical.mp4|8.5|13.5|rotulos\r2_misiles.png$R",
        "clip|$clips\clip_181715_vertical.mp4|32.3|35.3|rotulos\r2_misiles.png$R"), "f2.png|3.3")
    "03_pov_you_got_cocky" = @("v3", @(
        "clip|$clips\clip_182638_vertical.mp4|2.3|5.3|rotulos\r3_pov.png$R",
        "clip|$clips\clip_182625_vertical.mp4|0.0|3.4|rotulos\r3_pov.png$R"), "f3.png|3.5")
    "04_which_world_is_hardest" = @("v4", @(
        "clip|$clips\clip_182544_vertical.mp4|0.3|2.7|rotulos\r4_ice.png$R",
        "clip|$clips\clip_183939_vertical.mp4|1.0|3.4|rotulos\r4_nebula.png$R",
        "clip|$clips\clip_182638_vertical.mp4|0.3|2.7|rotulos\r4_station.png$R",
        "clip|$clips\clip_182212_vertical.mp4|4.0|6.4|rotulos\r4_core.png$R"), "f4.png|3.6")
    "05_how_far_can_you_get" = @("v5", @(
        "clip|$clips\clip_183939_vertical.mp4|1.0|3.0|rotulos\r5_howfar.png$R",
        "clip|$clips\clip_181715_vertical.mp4|2.0|4.0|rotulos\r5_howfar.png$R",
        "clip|$clips\clip_182638_vertical.mp4|2.3|4.3|rotulos\r5_howfar.png$R",
        "clip|$clips\clip_182212_vertical.mp4|7.0|9.0|rotulos\r5_howfar.png$R"), "f5.png|2.6")
}
Push-Location $src
foreach ($name in $videos.Keys) {
    $key, $lines, $fin = $videos[$name]
    $f = $fin.Split('|')
    $guion = @("size|1080|1920") + $lines + @("image|rotulos\$($f[0])|$($f[1])", "music|audio_$key.wav")
    [IO.File]::WriteAllLines("$src\guion_$key.txt", $guion, (New-Object Text.UTF8Encoding($false)))
    $res = & "$here\framegrab\Assemble.exe" "guion_$key.txt" "$tk\$name.mp4" | Select-Object -Last 1
    "$name -> $res"
}
Pop-Location
