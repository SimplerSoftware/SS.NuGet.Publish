[cmdletbinding()]
param(
	[Parameter()]
	$ApiKey,
	[Parameter(Mandatory)]
	$Module
)

if (!$ApiKey -and (Test-Path '..\.Nuget.key')) {
	# For local testing/publishing
	$ApiKey = (Get-Content -Raw '..\..\.Nuget.key')
}

if ($ApiKey){
	Publish-Module -Name $Module -NuGetApiKey $ApiKey
} else {
	Write-Error "Nuget API key is missing, please create the file with one line that contains your API key for nuget or pass it in via the ApiKey parameter."
}