<#
.SYNOPSIS
    Finds the first .csproj file in the current or parent directories.

.DESCRIPTION
    This function searches for the first .csproj file in the current directory or any parent directories. It returns the full path of the .csproj file if found, or $null if no .csproj file is found.

.PARAMETER startPath
    The starting directory to search for the .csproj file. If not specified, the current directory is used.

.EXAMPLE
    PS C:\> Search-ProjectFile
    C:\Users\JohnDoe\Documents\MyProject\MyProject.csproj

    This example searches for the first .csproj file in the current directory or any parent directories.

.EXAMPLE
    PS C:\> Search-ProjectFile -startPath "C:\Users\JohnDoe\Documents"
    C:\Users\JohnDoe\Documents\MyProject\MyProject.csproj

    This example searches for the first .csproj file in the "C:\Users\JohnDoe\Documents" directory or any parent directories.
#>

function Search-ProjectFile {
    param (
        [string]$startPath = $(Get-Location)
    )

    $item = Get-Item -Path $startPath

    if (-Not $item.PSIsContainer) {
        $item = $item.Directory
    }

    while ($null -ne $item) {
        $csproj = Get-ChildItem -Path $item.FullName -Filter *.csproj | Select-Object -First 1

        if ($null -ne $csproj) {
            return $csproj.FullName
        }

        $item = $item.Parent
    }

    return $null
}

<#
.SYNOPSIS
    Finds the first LICENSE file in the current or parent directories.

.DESCRIPTION
    This function searches for the first LICENSE file in the current directory or any parent directories. It returns the full path of the LICENSE file if found, or $null if no LICENSE file is found.

.PARAMETER startPath
    The starting directory to search for the LICENSE file. If not specified, the current directory is used.

.EXAMPLE
    PS C:\> Search-LicenseFile
    C:\Users\JohnDoe\Documents\MyProject\LICENSE

    This example searches for the first LICENSE file in the current directory or any parent directories.

.EXAMPLE
    PS C:\> Search-LicenseFile -startPath "C:\Users\JohnDoe\Documents"
    C:\Users\JohnDoe\Documents\MyProject\LICENSE

    This example searches for the first LICENSE file in the "C:\Users\JohnDoe\Documents" directory or any parent directories.
#>
function Search-LicenseFile {
    param (
        [string]$startPath = $(Get-Location)
    )

    $item = Get-Item -Path $startPath

    if (-Not $item.PSIsContainer) {
        $item = $item.Directory
    }

    while ($null -ne $item) {
        $licenseFile = Get-ChildItem -Path $item.FullName -Filter LICENSE | Select-Object -First 1

        if ($null -ne $licenseFile) {
            return $licenseFile.FullName
        }

        $item = $item.Parent
    }

    return $null
}

function Search-PackageJsonFile {
    param (
        [string]$startPath = $(Get-Location)
    )

    $item = Get-Item -Path $startPath

    if (-Not $item.PSIsContainer) {
        $item = $item.Directory
    }

    while ($null -ne $item) {
        $packagesJsonFile = Get-ChildItem -Path $item.FullName -Filter package.json | Select-Object -First 1

        if ($null -ne $packagesJsonFile) {
            return $packagesJsonFile.FullName
        }

        $item = $item.Parent
    }

    return $null
}