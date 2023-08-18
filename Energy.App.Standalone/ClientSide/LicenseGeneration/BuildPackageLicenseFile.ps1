param (
    [Parameter(Mandatory = $False, HelpMessage = "The output path for the generated html document. Defaults to the script directory.")]
    [string]$outputPath
)

$scriptDirectory = $PSScriptRoot

if ([String]::IsNullOrWhiteSpace($outputPath)) {
    $finalHtmlDocumentPath = Join-Path $scriptDirectory "ThirdPartyLicenses_nuget.html"
}
else {
    $finalHtmlDocumentPath = Join-Path $outputPath "ThirdPartyLicenses_nuget.html"
}

$licensePlainTextFolder = Join-Path $scriptDirectory "./SpdxLicensePlainTextFiles"
$licenseOverrideFile = Join-Path $scriptDirectory "./LicenseInfoOverride.json"
$licenseFolderPath = Join-Path $scriptDirectory "./NugetLicenseOutput"

$tempLicenseOverridePackageNamesFile = Join-Path $licenseFolderPath "LicenseOverridePackageNames.json"

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
    $sb.AppendLine("<div class=""eme--thirdparty--nuget--root"">")
    $sb.AppendLine("<h1>Third Party Nuget Package Attribution &amp; Licenses</h1>")
    $first = $True

    foreach ($packageInfo in $packageInfos) {
        $licenceTextFile = Join-Path $licensePlainTextFolder "$($packageInfo.LicenseType).txt"
        $licensePlainText = if (Test-Path $licenceTextFile) { Get-Content $licenceTextFile -Raw } else { throw "License text file not found: $licenceTextFile" }
        $licensePlainTextEncoded = [System.Web.HttpUtility]::HtmlEncode($licensePlainText)

        if (!$first) {
            $sb.AppendLine("<hr>")
            $first = $False
        }

        $sb.AppendLine("<div class=""eme--thirdparty--nuget--package--container"">")

        $sb.AppendLine("<h2 class=""eme--thirdparty--nuget--package-name-version"">$($packageInfo.PackageName) $($packageInfo.PackageVersion)</h2>")
        $sb.AppendLine("<p class=""eme--thirdparty--nuget--package-url""><a href=""$($packageInfo.PackageUrl)"">$($packageInfo.PackageUrl)</a></p>")
        $sb.AppendLine("<p class=""eme--thirdparty--nuget--package-authors"">Authors: $($packageInfo.Authors -join ", ")</p>")
        
        $sb.AppendLine("<span class=""eme--thirdparty--nuget--package-copyright""><pre style=""display: inline;"">$($packageInfo.Copyright)</pre></span>")
        $sb.AppendLine("<p class=""eme--thirdparty--nuget--license-type"">License: $($packageInfo.LicenseType) <a href=""$($packageInfo.LicenseUrl)"">$($packageInfo.LicenseUrl)</a></p>")

        $sb.AppendLine("<pre class=""eme--thirdparty--nuget--license-text"">$licensePlainTextEncoded</pre>")
        $sb.AppendLine("</div>")

    }

    $sb.AppendLine("</div>")
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

