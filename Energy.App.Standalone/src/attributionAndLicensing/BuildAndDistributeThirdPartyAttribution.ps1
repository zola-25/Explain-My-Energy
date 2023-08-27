


param(

    [Parameter(Mandatory = $True, HelpMessage = "The output path of the generated html document" )]
    [string]$finalOutputPath,

    [Parameter(Mandatory = $false, HelpMessage = "The path to the .csproj for determining dependent nuget packages. If not specified, the script will search for the nearest parent .csproj file" )]
    [string]$projectCsprojPath,

    [Parameter(Mandatory = $false, HelpMessage = "The folder containing the project's packages.json ")]
    [string]$clientSideRootPath,

    [Parameter(Mandatory = $False, HelpMessage = "The path to the app's LICENSE file. If not specified, the script will search for the LICENSE file in the current or parent directories")]
    [string]$licenseFilePath,

    [Parameter(Mandatory = $False, HelpMessage = "The applications version number. If not specified, the version number will not be included in the generated html document")]
    [string]$appVersionNumber,

    [Parameter(Mandatory = $False, HelpMessage = "The value to set in the header templates css link href")]
    [string]$attribsCssPath
)

try {
    
    $attributionAndLicensingFolder = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
    

    Import-Module "$attributionAndLicensingFolder\LicenseAndAttribModule.psm1" -Force

    if ([string]::IsNullOrEmpty($projectCsprojPath) -eq $true)
    {
        $projectCsprojPath = Search-ProjectFile -startPath $attributionAndLicensingFolder
        if($null -eq $projectCsprojPath) {
            throw "Could not find a .csproj file in the current or parent directories"
        }
    }

    if ([string]::IsNullOrEmpty($licenseFilePath) -eq $true)
    {
        $licenseFilePath = Search-LicenseFile -startPath $attributionAndLicensingFolder || [string]::Empty
        if([string]::IsNullOrEmpty($licenseFilePath)) {
            Write-Host "Could not find a LICENSE file in the current or parent directories"
        }
    }

    if ([string]::IsNullOrEmpty($clientSideRootPath) -eq $true)
    {
        $packageJsonFile = Search-PackageJsonFile -startPath $attributionAndLicensingFolder
        if([string]::IsNullOrEmpty($packageJsonFile)) {
            throw "Could not find a package.json file in the current or parent directories"
        }

        $clientSideRootPath = Split-Path -Parent -Path $packageJsonFile
    }



    $tempOutputDir = Join-Path $attributionAndLicensingFolder "tempLicenseHtmlOutput"

    if (Test-Path $tempOutputDir) {
        Remove-Item $tempOutputDir -Recurse -Force
    }

    New-Item -ItemType Directory -Force -Path $tempOutputDir 

    <# license and attribution header generation #>

    $licenseAndAttributionHeaderOutputFile = Join-Path $tempOutputDir "LicenseAndAttributionHeader.html"
    
    $licenseAndAttibutionJsScript = Join-Path $attributionAndLicensingFolder "MainHeaderGeneration\EmeLicenseAndAttributionHeaderBuild.js" -Resolve
    
    $appHeaderTemplateFilepath = Join-Path $attributionAndLicensingFolder "MainHeaderGeneration\EmeLicenseAndAttributionHeader.handlebars" -Resolve
    
    $result = node $($licenseAndAttibutionJsScript) `
        -o $licenseAndAttributionHeaderOutputFile `
        -l $licenseFilePath `
        -t $appHeaderTemplateFilepath `
        -v $appVersionNumber `
        -c $attribsCssPath

    if ($result -ne "Success") {
        throw "Error generating license and attribution header"
    }

    <# nuget html generation #>

    $nugetLicenseGenerator = Join-Path $attributionAndLicensingFolder "NugetLicenseGeneration\BuildNugetPackageLicenseFile.ps1" -Resolve
    
    $nugetLicenseHtmlOutputFile = Join-Path $tempOutputDir "ThirdPartyLicenses_nuget.html" 

    &"$nugetLicenseGenerator"  -parentProjectPath $projectCsprojPath -generatedHtmlDocumentPath $nugetLicenseHtmlOutputFile
    
    <# npm html generation #>

    $npmLicenseGenerator = Join-Path $attributionAndLicensingFolder "NpmLicenseGeneration\BuildNodePackageLicenseFile.ps1" -Resolve

    $npmLicenseHtmlOutputFile = Join-Path $tempOutputDir "ThirdPartyLicenses_npm.html" 

    &"$npmLicenseGenerator" -parentProjectPath $tempOutputDir -clientSideRootPath $clientSideRootPath -generatedHtmlDocumentPath $npmLicenseHtmlOutputFile

    <# Consolidation #>    
    
    $outputHtmlFiles = @($licenseAndAttributionHeaderOutputFile, $nugetLicenseHtmlOutputFile, $npmLicenseHtmlOutputFile )

    if (Test-Path $finalOutputPath) {
        Remove-Item $finalOutputPath -Force
    }

    foreach ($file in $outputHtmlFiles) {
        $fileContent = Get-Content -Path $file
        Add-Content  -Path $finalOutputPath -Value $fileContent -Encoding utf8NoBOM
    }

    $errorFound = $False
}
catch {
    $errorFound = $true
    Write-Error $_.Exception.Message

    if (Test-Path $finalOutputPath) {
        Remove-Item $finalOutputPath -Force
    }
}
finally {
    if (Test-Path $tempOutputDir) {
        Remove-Item $tempOutputDir -Recurse -Force
    }
}

if ($errorFound) {
    exit 1
}
exit 0