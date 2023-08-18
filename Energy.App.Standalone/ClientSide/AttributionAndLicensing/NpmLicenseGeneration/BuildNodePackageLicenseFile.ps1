
param (
    [Parameter(Mandatory = $False, HelpMessage = "The output path for the generated html document. Defaults to the script directory.")]
    [string]$outputPath
)
$scriptDirectory = $PSScriptRoot

if ([String]::IsNullOrWhiteSpace($outputPath)) {
    $finalHtmlDocumentPath = Join-Path $scriptDirectory "ThirdPartyLicenses_npm.html" 
}
else {
    $finalHtmlDocumentPath = Join-Path $outputPath "ThirdPartyLicenses_npm.html" 
}

$customFormatFile = Join-Path $scriptDirectory "./customFormatExample.json" -Resolve

$licenseCheckerWorkingDirectory = Join-Path  $scriptDirectory ".." -Resolve 

try {

    $licenseFolderPath = Join-Path $scriptDirectory "NpmLicenseOutput" 
    
    if (Test-Path $licenseFolderPath) {
        Remove-Item -Recurse $licenseFolderPath
    }
    mkdir $licenseFolderPath
    
    $packageLicenceJsonFile = Join-Path $licenseFolderPath "./NpmLicenses.json" 
    $licensePlainTextFolder = Join-Path $licenseFolderPath "./NpmLicensePlainTextFiles" 
    
    mkdir $licensePlainTextFolder

    <# Start-Process -FilePath "node.exe license-checker-rseidelsohn" `
        -ArgumentList "  --production --json --nopeer --excludePackagesStartingWith=""explain-my-energy"" --out $packageLicenceJsonFile   --relativeModulePath --relativeLicensePath --files $licensePlainTextFolder --customPath $customFormatFile --plainVertical" `
        -WorkingDirectory $licenseCheckerWorkingDirectory -NoNewWindow -Wait
 #>
    Set-Location $licenseCheckerWorkingDirectory
    license-checker-rseidelsohn.ps1 --production --json --nopeer --excludePackagesStartingWith="explain-my-energy" --out $packageLicenceJsonFile  --excludePrivatePackages --relativeModulePath --relativeLicensePath --files $licensePlainTextFolder --customPath $customFormatFile 
    Set-Location $scriptDirectory

    $rawJson = Get-Content $packageLicenceJsonFile -Raw
    $packageInfos = ConvertFrom-Json $rawJson    

    Add-Type -AssemblyName System.Web

    $sb = New-Object -TypeName System.Text.StringBuilder
    $sb.AppendLine("<div class=""eme--thirdparty--npm--root"">")
    $sb.AppendLine("<h1>Third Party Node Package Attribution &amp Licenses</h1>")
    $first = $True

    foreach ($packageInfo in $packageInfos.psobject.Properties) {

        Write-Host "Processing package $($packageInfo.Name)"

        $packageDetails = $packageInfo.Value
        $licenceTextFile = Join-Path $licenseFolderPath $packageDetails.licenseFile
        $licensePlainText = if (Test-Path $licenceTextFile) { 
            Get-Content $licenceTextFile -Raw 
        } 
        else {    
            throw "License text file not found: $licenceTextFile" 
        }

        # Use regex to find the license header and its content
        $licenseHashRegEx = '(?smi)(?<header>^#+\s+licen[sc]es?\s*)(?<license>.+?(?=#+|\z))'

        $licenseUnderlineHeaderRegEx = '(?smi)(?<header>^\s*licen[sc]es?.*\n[=-]+\n)(?<license>.+?(?==+|-{2,}|\z))'
        $hashResultRegEx = [regex]::Matches($licensePlainText, $licenseHashRegEx)
        if ($hashResultRegEx.Count -gt 0) {

            $licenseContent = $hashResultRegEx[0].Groups['license'].Value
        
        }
        else {

            $underlineResultRegEx = [regex]::Matches($licensePlainText, $licenseUnderlineHeaderRegEx)

            if ($underlineResultRegEx.Count -gt 0) {

                $licenseContent = $underlineResultRegEx[0].Groups['license'].Value

            }
            else {
                $licenseContent = $licensePlainText
            }
            
        }

        $licensePlainTextEncoded = $licenseContent | ConvertFrom-Markdown | Select-Object Html | ForEach-Object { $_.Html }
        
        if (!$first) {
            $sb.AppendLine("<hr>")
            $first = $False
        }

        $sb.AppendLine("<div class=""eme--thirdparty--npm--package--container"">")

        $sb.AppendLine("<h2 class=""eme--thirdparty--npm--package-name-version"">$($packageDetails.name) $($packageDetails.version)</h2>")
        

        if (-not [String]::IsNullOrWhiteSpace($packageDetails.repository)) { 
            $sb.AppendLine("<p class=""eme--thirdparty--npm--package-repository""><a href=""$($packageDetails.repository)"">$($packageDetails.repository)</a></p>") 
        }
        
        if (-not [String]::IsNullOrWhiteSpace($packageDetails.publisher)) { 
            $sb.AppendLine("<p class=""eme--thirdparty--npm--package-authors"">Authors: $($packageDetails.publisher)</p>") 
        }

        ## does the license text contain a copyright text or symbol?
        $lowerCaseLicenseContent = $licenseContent.ToLower()
        $licenseHasCopyrightText = $lowerCaseLicenseContent.Contains("copyright") -or $licenseContent.Contains("copy right")

        $licenseHasCopyrightSymbol = $licenseContent.Contains("©") -or $licenseContent.Contains("Ⓒ") `
            -or $licenseContent.Contains("&copy;") -or $licenseContent.Contains("&COPY;") `
            -or $licenseContent.Contains("&#169;") -or $licenseContent.Contains("&#xA9;") `
            -or $licenseContent.Contains("&#9400;") -or $licenseContent.Contains("&#x24B8;")

        $hasCopyrightInLicense = $licenseHasCopyrightSymbol -or $licenseHasCopyrightText

        if (-not $hasCopyrightInLicense -and (-not [String]::IsNullOrWhiteSpace($packageDetails.copyright))) {
            $packageDetails.copyright | ConvertFrom-Markdown | Select-Object Html | 
            ForEach-Object { $sb.AppendLine("<span class=""eme--thirdparty--npm--package-copyright"">$($_.Html)</span>") }
        }
        

        if ([String]::IsNullOrWhiteSpace($packageDetails.licenses)) { throw "License is null or whitespace for package $($packageInfo.name)" }
        
        if ($packageDetails.licenses.Split(",").Length -gt 1) { throw "Multiple licenses found for package $($packageInfo.name)" }
        
        $sb.AppendLine("<p class=""eme--thirdparty--npm--package-license-type"">License: $($packageDetails.licenses)</p>")
        $sb.AppendLine("<pre class=""eme--thirdparty--npm--package-license-text"">$licensePlainTextEncoded</pre>")
        
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

