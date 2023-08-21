
param (
    [Parameter(Mandatory = $True, HelpMessage = "The path to the .csproj to generate license the license html for" )]
    [string]$parentProjectPath,

    [Parameter(Mandatory = $True, HelpMessage = "The folder containing the projects packages.json" )]
    [string]$clientSideRootPath,

    [Parameter(Mandatory = $False, HelpMessage = "The folder from which the script is located" )]
    [string]$scriptDirectory = $(Split-Path -Parent -Path $MyInvocation.MyCommand.Definition),

    [Parameter(Mandatory = $False, HelpMessage = "The output path for the generated html document. Defaults to the script directory/ThirdPartyLicenses_nuget.html")]
    [string]$generatedHtmlDocumentPath = (Join-Path $scriptDirectory "./ThirdPartyLicenses_npm.html"),

    [Parameter(Mandatory = $False, HelpMessage = "The path to the temporary folder where the license generation will take place. Defaults to the script directory/NugetLicenseOutput")]
    [string]$tempLicenseOutputFolder = (Join-Path $scriptDirectory "./NpmLicenseOutput"),

    [Parameter(Mandatory = $False, HelpMessage = "The path to the license customFormat JSON file. Defaults to the script directory/customFormatExample.json")]
    [string]$customFormatFile = (Join-Path $scriptDirectory "./customFormatExample.json")
)

$originalLocation = Get-Location

try {

    if (Test-Path $tempLicenseOutputFolder) {
        Remove-Item -Recurse $tempLicenseOutputFolder
    }
    mkdir $tempLicenseOutputFolder
    
    $packageLicenceJsonFile = Join-Path $tempLicenseOutputFolder "./NpmLicenses.json" 
    $licensePlainTextFolder = Join-Path $tempLicenseOutputFolder "./NpmLicensePlainTextFiles" 
    
    mkdir $licensePlainTextFolder

    Invoke-Command -ScriptBlock {
        Set-Location -Path $clientSideRootPath
        license-checker-rseidelsohn --production --json --nopeer --excludePackagesStartingWith="explain-my-energy" `
            --out $packageLicenceJsonFile  --excludePrivatePackages --relativeModulePath --relativeLicensePath `
            --files $licensePlainTextFolder --customPath $customFormatFile
        Set-Location -Path $originalLocation
    }

    <# 
    Start-Process -FilePath $nodePath `
        -ArgumentList "$globalToolsPath/license-checker-rseidelsohn.cmd  --production --json --nopeer --excludePackagesStartingWith=""explain-my-energy"" --out $packageLicenceJsonFile   --relativeModulePath --relativeLicensePath --files $licensePlainTextFolder --customPath $customFormatFile " `
        -WorkingDirectory $clientSideRootPath -NoNewWindow -Wait

    license-checker-rseidelsohn --production --json --nopeer --excludePackagesStartingWith="explain-my-energy" `
    --out $packageLicenceJsonFile  --excludePrivatePackages --relativeModulePath --relativeLicensePath `
    --start $clientSideRootPath --files $licensePlainTextFolder --customPath $customFormatFile  #>
    
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
        $licenceTextFile = Join-Path $tempLicenseOutputFolder $packageDetails.licenseFile
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

        $licensePlainTextEncoded = $licenseContent | ConvertFrom-Markdown | Select-Object Html | ForEach-Object { [System.Web.HttpUtility]::HtmlEncode($_.Html) }

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

    $sb.ToString() | Set-Content -Path $generatedHtmlDocumentPath

    $errorFound = $False

}
catch {
    Write-Host $_.Exception.Message
    Write-Host $_.Exception.StackTrace
    $errorFound = $True
}
finally {
    Set-Location -Path $originalLocation

    if (Test-Path $tempLicenseOutputFolder) {
        Remove-Item -Recurse $tempLicenseOutputFolder
    }

}

if ($errorFound) {
    exit 1
}
exit 0

