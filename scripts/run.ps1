# --- Config ---
$procName = "Card Shop Simulator" # process name (no .exe)
$appid = 3070070                  # Steam AppID for TCG Card Shop Simulator
$stopTimeout = 10                 # seconds to wait for graceful stop

# Paths (update these!)
$dllSrc = "D:\Documents\GitHub\tcg-profiler\TCGProfiler\bin\Debug\netstandard2.1\TCGProfiler.dll"
$dllDst = "D:\SteamLibrary\steamapps\common\TCG Card Shop Simulator\BepInEx\plugins\TCGProfiler\TCGProfiler.dll"
$gameDir = "D:\SteamLibrary\steamapps\common\TCG Card Shop Simulator"

# --- Helpers ---
function Stop-ProgramAndWait
{
    param(
        [Parameter(Mandatory)][string]$Name,
        [int]$TimeoutSeconds = 10
    )
    $procs = Get-Process -Name $Name -ErrorAction SilentlyContinue
    if (-not $procs)
    {
        return $true
    }

    # Try graceful stop
    $procs | Stop-Process -ErrorAction SilentlyContinue
    try
    {
        Wait-Process -Name $Name -Timeout $TimeoutSeconds
        return $true
    }
    catch
    {
        # Force kill
        Get-Process -Name $Name -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
        try
        {
            Wait-Process -Name $Name -Timeout 5; return $true
        }
        catch
        {
            return $false
        }
    }
}

function Wait-ForGameStart
{
    param([string]$ProcessName, [int]$TimeoutSeconds = 30)
    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do
    {
        Start-Sleep -Milliseconds 300
        if (Get-Process -Name $ProcessName -ErrorAction SilentlyContinue)
        {
            return $true
        }
    } while ( (Get-Date) -lt $deadline )
    return $false
}

function Get-SteamPath
{
    $steamPath = (Get-ItemProperty 'HKCU:\Software\Valve\Steam' -Name SteamPath -ErrorAction SilentlyContinue).SteamPath
    if (-not $steamPath)
    {
        return $null
    }
    return ($steamPath -replace '/', '\')
}

function Start-GameViaSteam
{
    param([int]$AppId)
    $steamPath = Get-SteamPath
    if ($steamPath -and (Test-Path "$steamPath\steam.exe"))
    {
        Start-Process -FilePath "$steamPath\steam.exe" -WorkingDirectory $steamPath -ArgumentList "-applaunch $AppId" | Out-Null
        return $true
    }
    else
    {
        # Fallback to OS URL handler
        Start-Process -FilePath explorer.exe -ArgumentList "steam://rungameid/$AppId" | Out-Null
        return $true
    }
}

function Test-FileLocked
{
    param([Parameter(Mandatory)][string]$Path)
    try
    {
        $fs = [System.IO.File]::Open(
                $Path,
                [System.IO.FileMode]::OpenOrCreate,
                [System.IO.FileAccess]::ReadWrite,
                [System.IO.FileShare]::None
        )
        $fs.Close()
        return $false
    }
    catch
    {
        return $true
    }
}

function Copy-WithRetry
{
    param(
        [Parameter(Mandatory)][string]$Source,
        [Parameter(Mandatory)][string]$Dest,
        [int]$TimeoutSeconds = 15
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)

    # wait for existing dest (if any) to unlock
    while ((Test-Path -LiteralPath $Dest) -and (Test-FileLocked -Path $Dest) -and ( (Get-Date) -lt $deadline))
    {
        Start-Sleep -Milliseconds 300
    }

    # try copy with retries in case something relocks briefly
    do
    {
        try
        {
            Copy-Item -LiteralPath $Source -Destination $Dest -Force -ErrorAction Stop
            return $true
        }
        catch
        {
            if ((Get-Date) -ge $deadline)
            {
                throw
            }
            Start-Sleep -Milliseconds 300
        }
    } while ($true)
}

# --- 1) Stop the game ---
Write-Output "Stopping $procName..."
$stopped = Stop-ProgramAndWait -Name $procName -TimeoutSeconds $stopTimeout
if (-not $stopped)
{
    Write-Output "Failed to stop $procName."
    exit 1
}
Write-Output "$procName is not running."

# --- 2) Copy the DLL ---
Write-Output "Copying new build..."
$dstDir = Split-Path -Parent $dllDst
if (-not (Test-Path $dstDir))
{
    New-Item -ItemType Directory -Path $dstDir -Force | Out-Null
}

if (-not (Test-Path $dllSrc))
{
    Write-Error "Source DLL not found: $dllSrc"
    exit 1
}

try
{
    Copy-WithRetry -Source $dllSrc -Dest $dllDst -TimeoutSeconds 15 | Out-Null
    Write-Output "Copied to $dllDst"
}
catch
{
    Write-Error "Failed to copy DLL to $dllDst after waiting. $( $_.Exception.Message )"
    exit 1
}

# --- 3) Launch via Steam and verify ---
Write-Output "Launching $procName via Steam (AppID $appid)..."
Start-GameViaSteam -AppId $appid | Out-Null

if (Wait-ForGameStart -ProcessName $procName -TimeoutSeconds 45)
{
    Write-Output "Launched $procName."
}
else
{
    Write-Output "Steam launch issued, but process '$procName' not detected. Check AppID/process name or Steam status."
}
