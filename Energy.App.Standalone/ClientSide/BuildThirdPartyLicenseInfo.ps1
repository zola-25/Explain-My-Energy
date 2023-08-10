$scriptDirectory = $PSScriptRoot

$parentProjectPath = "../Energy.App.Standalone.csproj"

$mergeMicrosoftDependencies = $true

Write-Host "Clearing NuGet caches"
dotnet nuget locals all --clear

Write-Host "Restoring runtime dependencies"
dotnet restore $parentProjectPath

liz $parentProjectPath -i  -et "$scriptDirectory\ThirdPartyLicenses" > "tempLicenseInfo.txt"

# Read the content of the file
$data = Get-Content -Path "tempLicenseInfo.txt"

# Initialize an empty array to store the parsed data
$generatedLicenseInfo = [string]::Empty

# Process each line to extract the package information
$data | ForEach-Object {

    if ([string]::IsNullOrWhiteSpace($_)
            || $_ -like "Name" ) 
    {
        continue
    }

    # Split the line by spaces
    $fields = $_ -split '\s+'

    # Extract the relevant fields
    $packageName = $fields[0]
    $version = $fields[1]
    $hasText = $fields[2]
    $licenseType = $fields[3]
    $url = $fields[4]

    # Create an object with the extracted data and add it to the array
    $packageEntry = [PSCustomObject]@{
        Name = $packageName
        Version = $version
        HasText = $hasText -eq "Yes"
        LicenseType = $licenseType
        URL = $url
    }

    $packageInfo = $packageEntry

    $getNugetPackageInfo = false
    $displayFileLicenseText = false
    $isStandardSpxtText = false


    <# 0) Always get nuget info for non microsoft dependencies #>
    <# 1) Check if a microsoft dependency && $skipMicrosoftDependencies = true, if so don't get nuget info unless not an MIT license#>
    <# 2) always get nuget info for other dependencies, but don't displace license text unless Liz tool HasText = false, or no license file, or license file is HTML standard SPEX #>


    <# Check if HasText output from liz tool is false or if nothing found in license directory#>

    $pathToLicenseFile = Get-LicenseFilePath -packageName $packageInfo.Name -packageVersion $packageInfo.Version

    $fileExists = $pathToLicenseFile -ne $null
    
    $isStandardSpxtText = $fileExists -and $pathToLicenseFile -like "*.html"

    if ($mergeMicrosoftDependencies && ($packageInfo.Name -like "Microsoft*" -or $packageInfo.Name -like "System*")) {
        Write-Host "Microsoft package found, checking license type"

        if ($packageInfo.LicenseType -notlike "MIT*") {
            Write-Host "Microsoft license type is not MIT, getting nuget package info"

            $displayFileLicenseText = $packageInfo.HasText -eq $false -or $isStandardSpxtText -eq $true

            $getNugetPackageInfo = $true
        }
    } elseif  ($packageInfo.HasText -eq $false -or $isStandardSpxtText -eq $true) {
        Write-Host "No license text found for package '$($packageInfo.Name)', getting nuget package info"
        
        $displayFileLicenseText = $false

        $getNugetPackageInfo = $true
    }

    if ($getNugetPackageInfo) {
        $nugetPackageInfo = nuget list $packageInfo.Name -Source C:\Users\miket\.nuget\packages\ -Verbosity detailed
        $generatedLicenseInfo += $nugetPackageInfo
    }

    if($displayFileLicenseText) {
        $licenseText = Get-Content -Path $pathToLicenseFile -Raw
        $generatedLicenseInfo += $licenseText
    }

    $generatedLicenseInfo += "`n"

     
}

function Get-LicenseFilePath {
    param(
        [string]$packageName,
        [string]$packageVersion
    )

    $licenseFilePrefix = "$($packageName)-$($packageVersion)"


    $licenseFiles = Get-ChildItem -Path "ThirdPartyLicenses" -Filter "$licenseFilePrefix*" 

    if ($licenseFiles.Count -eq 0) {
        return $null
    }

    if ($licenseFiles.Count -gt 1) {
        Write-Warning "Multiple license files found for package '$($packageName)' and version '$($packageVersion)'"
    }
    
    $licenseFile = $licenseFiles[0]

    return $licenseFile
}