[cmdletbinding()]
param(
	[Parameter(Mandatory)]
	$Version,
	[Parameter(Mandatory)]
	$ManifestPath
)

Write-Host "Updating module manifest with version $Version"
$date = Get-Date -Format MM/dd/yyyy
$ModuleManifest = Get-Content $ManifestPath -Raw
$ModuleManifest = $ModuleManifest -replace "(ModuleVersion\W*=)\W*'(.*)'", "`$1 '$Version'"
$ModuleManifest = $ModuleManifest -replace "(Generated on:)\W*(.*)", "`$1 $date"
$ModuleManifest | Out-File -LiteralPath $ManifestPath