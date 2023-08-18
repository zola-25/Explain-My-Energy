$scriptDirectory = $PSScriptRoot

try {

    $nugetLicenseGenerator = Join-Path $scriptDirectory "LicenseGeneration\BuildPackageLicenseFile.ps1" 
    $npmLicenseGenerator = Join-Path $scriptDirectory "NpmLicenseGeneration\BuildNodePackageLicenseFile.ps1" 

    $tempOutputDir = Join-Path $scriptDirectory "tempLicenseHtmlOutput"

    $distributeFinalFilePath = Join-Path $scriptDirectory "../wwwroot/ThirdPartyAttributions.html"

    $licenseAndAttributionHeaderFile = Join-Path $tempOutputDir "LicenseAndAttributionHeader.html"

    if (Test-Path $tempOutputDir) {
        Remove-Item $tempOutputDir -Recurse -Force
    }

    New-Item -ItemType Directory -Force -Path $tempOutputDir

    $result = node .\EmeLicenseAndAttributionHeaderBuild.js -o 'C:/Users/miket/Development/ExplainMyEnergy/Energy.App.Standalone/ClientSide/tempLicenseHtmlOutput/LicenseAndAttributionHeader.html'

    if ($result -ne "Success") {
        throw "Error generating license and attribution header"
    }
    
    &"$nugetLicenseGenerator" -outputPath $tempOutputDir

    &"$npmLicenseGenerator" -outputPath $tempOutputDir


    $outputHtmlFiles = Get-ChildItem -Path $tempOutputDir -Filter *.html 

    foreach ($file in $outputHtmlFiles) {
        $fileContent = Get-Content -Path $file.FullName
        Add-Content  -Path $distributeFinalFilePath -Value $fileContent -Encoding utf8NoBOM
    }
    $errorFound = $False
}
catch {
    $errorFound = $true

    Write-Error $_.Exception.Message
    exit 1
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