
$script:captureOutput = $false
function Select-LicenseFileText {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true, ValueFromPipeline = $true)]
        [AllowEmptyString()]
        [string]$InputLicenseText
    )

    PROCESS {
        $expressionToMatch = 'Name\s+Version\s+Text\?\s+Types\s+URL'
        $InputLicenseText.Trim('"').Split("`r?`n") | ForEach-Object {
            $line = $_

            if ($script:captureOutput -and (-not [string]::IsNullOrWhiteSpace($line))) {
                Write-Output $line
            }

            if ($_ -match $expressionToMatch) {
                $script:captureOutput = $true
            }
        }
    }

}


function Get-LicenseFilePath {
    param(
        [string]$packageName,
        [string]$packageVersion
    )

    $licenseFilePrefix = "$($packageName)-$($packageVersion)"


    $licenseFiles = Get-ChildItem -Path "$scriptDirectory\ThirdPartyLicenses" -Filter "$licenseFilePrefix*" 

    if ($licenseFiles.Count -eq 0) {
        return $null
    }

    if ($licenseFiles.Count -gt 1) {
        throw "Multiple license files found for package '$($packageName)' and packageVersion '$($packageVersion)'"
    }
    
    $licenseFile = $licenseFiles[0]

    return $licenseFile
}

$scriptDirectory = $PSScriptRoot
$parentProjectPath = Join-Path $scriptDirectory "../Energy.App.Standalone.csproj"

$mergeMicrosoftDependencies = $true

Write-Host "Clearing NuGet caches"
dotnet nuget locals all  --clear

Write-Host "Restoring runtime dependencies"
dotnet restore $parentProjectPath

Write-Host "Clearing temp files"
Remove-Item -Path "$scriptDirectory\tempLicenseInfo.txt" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "$scriptDirectory\lizToolTempUnparsedOutput.txt" -Force -ErrorAction SilentlyContinue
write-host "Removing old license files"
Remove-Item -Path "$scriptDirectory\ThirdPartyLicenses" -Force -Recurse -ErrorAction SilentlyContinue

Start-Process -FilePath "dotnet-project-licenses" `
        -ArgumentList "-i $parentProjectPath -u -t -o -j --use-project-assets-json --include-project-file --outfile .\Licenses.json --export-license-texts" `
        -WorkingDirectory $scriptDirectory\ThirdPartyLicenses

    Wait-Process -Name "dotnet-project-licenses"


<# Start-Process -FilePath "liz" `
    -ArgumentList "$parentProjectPath -si True -sb True -i -l Information -et .\ThirdPartyLicenses" `
    -NoNewWindow -WorkingDirectory $scriptDirectory -Wait -RedirectStandardOutput "$scriptDirectory\lizToolTempUnparsedOutput.txt"

Get-Content -Path "$scriptDirectory\lizToolTempUnparsedOutput.txt" | Select-LicenseFileText | Out-File -FilePath "$scriptDirectory\tempLicenseInfo.txt"
 #>
# Read the content of the file
$licenseInfoData = Get-Content -Path "$scriptDirectory\tempLicenseInfo.txt"

<# $packageInfoData = Find-Package -ProviderName NuGet -Source $env:USERPROFILE\.nuget\packages | 
Select-Object -Property Name, Version, Summary, @{Name = "AuthorNames"; Expression = { $_.Entities | Where-Object { $_.Roles -contains "author" } | ForEach-Object { $_.Name } } }
 #>

# Initialize an empty array to store the parsed data

$sb = New-Object -TypeName System.Text.StringBuilder
$sb.AppendLine("<h2>Third Party Licenses</h2>")
$results = @()

# Process each line to extract the package information
$licenseInfoData | ForEach-Object {
    if ([string]::IsNullOrWhiteSpace($_)
        || $_ -like "Name" ) {
        return
    }



    # Split the line by spaces
    $fields = $_ -split '\s+'

    # Extract the relevant fields
    $packageName = $fields[0]?.Trim()
    $packageVersion = $fields[1]?.Trim()
    $licenseType = $fields[3]?.Trim()
    $hasLicenseInfo = $fields[2]?.Trim() -eq "Yes" && $licenseType.Length -gt 0
    $licenseUrl = $fields[4]?.Trim()

    $packageInfo = $packageInfoData  | Where-Object { ($_.Name -eq $packageName) -and ($_.Version -eq $packageVersion) }
    $packageSummary = $packageInfo.Summary
    $packageAuthors = $packageInfo.AuthorNames -join ", "

    # Create an object with the extracted data and add it to the array



    $displayPackageDetails = $false


    <# 0) Always get nuget info for non microsoft dependencies #>
    <# 1) Check if a microsoft dependency && $skipMicrosoftDependencies = true, if so don't get nuget info unless not an MIT license#>
    <# 2) always get nuget info for other dependencies, but don't displace license text unless Liz tool HasText = false, or no license file, or license file is HTML standard SPEX #>


    <# Check if HasText output from liz tool is false or if nothing found in license directory#>

    $licenseFilePath = Get-LicenseFilePath -packageName $packageName -packageVersion $packageVersion

    $fileExists = $null -ne $licenseFilePath
    
    $hasLicenseText = $fileExists -and $licenseFilePath -like "*.txt"

    if ($mergeMicrosoftDependencies -and ($packageName -like "Microsoft*" -or $packageName -like "System*")) {
        Write-Host "Microsoft package found, checking license type"

        if ($licenseType -notlike "MIT*") {
            Write-Host "Microsoft license type is $licenseType"

            $displayPackageDetails = $true
        }
        else {
            Write-Host "Microsoft license type is $licenseType, skipping"
            return 
        }
    }
    else {
        Write-Host "Non Microsoft package found, getting nuget package info"
        $displayPackageDetails = $true
    }

    $licenseText = [string]::Empty

    $sb.AppendLine("<h3>$packageName - $packageVersion</h3>")
    $sb.AppendLine("<p>$packageSummary</p>")
    $sb.AppendLine("<p>Authors: $packageAuthors</p>")

    if ($hasLicenseText) {

        $licenseText = Get-Content -Path $licenseFilePath -Raw

        $sb.AppendLine("<p>License</p>")
        $sb.AppendLine("<pre>$licenseText</pre>")
    }
    else {
        $sb.AppendLine("<p>License $($licenseType): <a href=""$licenseUrl"">$licenseUrl</a></p>")
    }
    
    <# $results += [PSCustomObject]@{      
        Name        = $packageName
        Version     = $packageVersion
        Summary     = $packageSummary
        Authors     = $packageAuthors
        LicenseText = $licenseText
        LicenseType = $licenseType
        LicenseUrl  = $licenseUrl
    } #>
}

# $results | ConvertTo-Html -Property Name, Version, Summary, Authors, LicenseType, LicenseUrl, LicenseText -Title "Third Party Licenses" | Out-File -FilePath "$scriptDirectory\ThirdPartyLicenses.html"
$sb.ToString() | Set-Content -Path "$scriptDirectory\ThirdPartyLicenses.html"

Write-Host "Clearing temp files"
Remove-Item -Path "$scriptDirectory\tempLicenseInfo.txt" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "$scriptDirectory\lizToolTempUnparsedOutput.txt" -Force -ErrorAction SilentlyContinue
write-host "Removing old license files"
Remove-Item -Path "$scriptDirectory\ThirdPartyLicenses" -Force -Recurse -ErrorAction SilentlyContinue
