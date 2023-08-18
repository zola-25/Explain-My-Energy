
try {

    $respositoryRoot = & git rev-parse --show-toplevel
    $mainApplicationProjectFolder = Join-Path $respositoryRoot "Energy.App.Standalone" -Resolve
    $mainApplicationProjectFile = Join-Path $mainApplicationProjectFolder "Energy.App.Standalone.csproj" -Resolve

    $clientSideRootPath = Join-Path $mainApplicationProjectFolder "ClientSide" -Resolve
    $attributionAndLicensingFolder = Join-Path $clientSideRootPath "AttributionAndLicensing" -Resolve

    $tempOutputDir = Join-Path $attributionAndLicensingFolder "tempLicenseHtmlOutput"

    if (Test-Path $tempOutputDir) {
        Remove-Item $tempOutputDir -Recurse -Force
    }

    New-Item -ItemType Directory -Force -Path $tempOutputDir 

    <# license and attribution header generation #>

    $licenseAndAttributionHeaderOutputFile = Join-Path $tempOutputDir "LicenseAndAttributionHeader.html"
    
    $licenseAndAttibutionJsScript = Join-Path $attributionAndLicensingFolder "MainHeaderGeneration\EmeLicenseAndAttributionHeaderBuild.js" -Resolve
    
    $licenseTemplateFile = Join-Path $attributionAndLicensingFolder "MainHeaderGeneration\EmeLicenseAndAttributionHeader.handlebars" -Resolve
    
    $result = node $($licenseAndAttibutionJsScript) `
        -o $licenseAndAttributionHeaderOutputFile `
        -l $(Join-Path $respositoryRoot "LICENSE" -Resolve) `
        -t $licenseTemplateFile `
        -v "0.1.0"

    if ($result -ne "Success") {
        throw "Error generating license and attribution header"
    }

    <# nuget html generation #>

    $nugetLicenseGenerator = Join-Path $attributionAndLicensingFolder "NugetLicenseGeneration\BuildNugetPackageLicenseFile.ps1" -Resolve
    
    $nugetLicenseHtmlOutputFile = Join-Path $tempOutputDir "ThirdPartyLicenses_nuget.html" 

    &"$nugetLicenseGenerator"  -parentProjectPath $mainApplicationProjectFile -generatedHtmlDocumentPath $nugetLicenseHtmlOutputFile
    
    <# npm html generation #>

    $npmLicenseGenerator = Join-Path $attributionAndLicensingFolder "NpmLicenseGeneration\BuildNodePackageLicenseFile.ps1" -Resolve

    $npmLicenseHtmlOutputFile = Join-Path $tempOutputDir "ThirdPartyLicenses_npm.html" 

    &"$npmLicenseGenerator" -parentProjectPath $tempOutputDir -clientSideRootPath $clientSideRootPath -generatedHtmlDocumentPath $npmLicenseHtmlOutputFile

    <# Consolidation #>    
    

    $distributeFinalFilePath = Join-Path $mainApplicationProjectFolder "/wwwroot/AttributionsAndLicense.html"
    
    $outputHtmlFiles = @($licenseAndAttributionHeaderOutputFile, $nugetLicenseHtmlOutputFile, $npmLicenseHtmlOutputFile )

    if(Test-Path $distributeFinalFilePath) {
        Remove-Item $distributeFinalFilePath -Force
    }

    foreach ($file in $outputHtmlFiles) {
        $fileContent = Get-Content -Path $file
        Add-Content  -Path $distributeFinalFilePath -Value $fileContent -Encoding utf8NoBOM
    }
    $errorFound = $False
}
catch {
    $errorFound = $true

    Write-Error $_.Exception.Message
}
finally {
    if (Test-Path $tempOutputDir) {
        Remove-Item $tempOutputDir -Recurse -Force
    }
}

if($errorFound) {
    exit 1
 }
 exit 0