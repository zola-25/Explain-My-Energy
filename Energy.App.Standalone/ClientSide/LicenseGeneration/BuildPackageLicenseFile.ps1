
$scriptDirectory = $PSScriptRoot
$finalHtmlDocumentPath = Join-Path $scriptDirectory "ThirdPartyLicenses_dn_p_l.html"
$licensePlainTextFolder = Join-Path $scriptDirectory "./LicenseTextFiles"
$licenseOverrideFile = Join-Path $scriptDirectory "./LicenseInfoOverride.json"
$licenseFolderPath = Join-Path $scriptDirectory "./NugetLicenseOutput"

$tempLicenseOverridePackageNamesFile  = Join-Path $licenseFolderPath "LicenseOverridePackageNames.json"

try {
    $parentProjectPath = Join-Path $scriptDirectory "../../Energy.App.Standalone.csproj"

    Write-Host "Clearing NuGet caches"
    dotnet nuget locals all --clear

    Write-Host "Restoring project packages"
    dotnet restore $parentProjectPath

    
    if (Test-Path $licenseFolderPath) {
        Remove-Item -Recurse $licenseFolderPath
    }
    mkdir $licenseFolderPath
    
    Get-Content $licenseOverrideFile -Raw | ConvertFrom-Json -AsHashtable | Select-Object -ExpandProperty "PackageName" | ConvertTo-Json -AsArray | Set-Content -Path "$tempLicenseOverridePackageNamesFile"

    Start-Process -FilePath "dotnet-project-licenses" `
        -ArgumentList "-i $parentProjectPath -u -t -o -j  --use-project-assets-json  --outfile .\Licenses.json --packages-filter $tempLicenseOverridePackageNamesFile --manual-package-information $licenseOverrideFile"  `
        -WorkingDirectory $licenseFolderPath -NoNewWindow -Wait

    $packageLicenceJsonFile = Join-Path $licenseFolderPath "Licenses.json"
    $rawJson = Get-Content $packageLicenceJsonFile -Raw
    $packageInfos = ConvertFrom-Json $rawJson

    Add-Type -AssemblyName System.Web


    $sb = New-Object -TypeName System.Text.StringBuilder
    $sb.AppendLine("<h2>Third Party Licenses</h2>")

    foreach ($packageInfo in $packageInfos) {
        $licenceTextFile = Join-Path $licensePlainTextFolder "$($packageInfo.LicenseType).txt"
        $licensePlainText = if (Test-Path $licenceTextFile) { Get-Content $licenceTextFile -Raw } else { throw "License text file not found: $licenceTextFile" }
        $licensePlainTextEncoded = [System.Web.HttpUtility]::HtmlEncode($licensePlainText)

        $sb.AppendLine("<h3>$($packageInfo.PackageName) $($packageInfo.PackageVersion)</h3>")
        $sb.AppendLine("<p><a href=""$($packageInfo.PackageUrl)"">$($packageInfo.PackageUrl)</a></p>")
        $sb.AppendLine("<p>Authors: $($packageInfo.Authors -join ", ")</p>")
        
        $sb.AppendLine("<span>Copyright: <pre style=""display: inline;"">$($packageInfo.Copyright)</pre></span>")
        $sb.AppendLine("<p>License: $($packageInfo.LicenseType) <a href=""$($packageInfo.LicenseUrl)"">$($packageInfo.LicenseUrl)</a></p>")

        $sb.AppendLine("<pre>$licensePlainTextEncoded</pre>")
    }

    $sb.ToString() | Set-Content -Path $finalHtmlDocumentPath

    $errorFound = $False

}
catch {
    Write-Host $_.Exception.Message
    Write-Host $_.Exception.StackTrace
    $errorFound = $True
}
finally {
    if (Test-Path $licenseFolderPath) {
        Remove-Item -Recurse $licenseFolderPath
    }
}

if ($errorFound) {
    exit 1
}
exit 0

