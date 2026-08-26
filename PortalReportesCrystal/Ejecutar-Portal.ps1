# ============================================================================
# Ejecutar-Portal.ps1 - LANZADOR DE LA APLICACION
# ============================================================================
# Compila el proyecto y lo publica en IIS Express con Windows Authentication,
# sin depender de la tecla F5 de Visual Studio.
#
# USO:  clic derecho sobre este archivo -> "Ejecutar con PowerShell"
#       o desde una consola:  .\Ejecutar-Portal.ps1
#
# Para detener el servidor: cierre esta ventana o presione Ctrl+C.
# ============================================================================

$ErrorActionPreference = 'Stop'

$proyecto = $PSScriptRoot
$config   = Join-Path $proyecto 'applicationhost.iisexpress.config'
$csproj   = Join-Path $proyecto 'PortalReportesCrystal.csproj'
$url      = 'http://localhost:58172/'

$msbuild = Join-Path $env:ProgramFiles 'Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe'
$iis     = Join-Path $env:ProgramFiles 'IIS Express\iisexpress.exe'

# --- 1. Compilar -----------------------------------------------------------
Write-Host 'Compilando el proyecto...' -ForegroundColor Cyan
& $msbuild $csproj /t:Build /p:Configuration=Debug /v:minimal /nologo
if ($LASTEXITCODE -ne 0) {
    Write-Host 'La compilacion fallo. Revise los errores anteriores.' -ForegroundColor Red
    Read-Host 'Presione Enter para salir'
    exit 1
}

# --- 2. Detener instancias previas de IIS Express --------------------------
Get-Process iisexpress -ErrorAction SilentlyContinue | Stop-Process -Force -Confirm:$false

# --- 3. Abrir el navegador en segundo plano --------------------------------
Start-Job -ScriptBlock {
    Start-Sleep -Seconds 3
    Start-Process $using:url
} | Out-Null

# --- 4. Iniciar IIS Express (bloquea hasta Ctrl+C) -------------------------
Write-Host ''
Write-Host "Portal disponible en: $url" -ForegroundColor Green
Write-Host 'Presione Ctrl+C para detener el servidor.' -ForegroundColor Yellow
Write-Host ''

& $iis "/config:$config" '/site:PortalReportesCrystal'
