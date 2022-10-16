<#Process Cs Project Files#>

$PackDir = ".\bin\Release\publish\"

$DotPrefix = '..............................'
$donePrefix =       "[Done].......$DotPrefix"
$startingPrefix =   "[Starting]...$DotPrefix"
$ProcessingPrefix = "[Processing].$DotPrefix"
$successPrefix =    "[Success]....$DotPrefix"
$failPrefix =       "[Fail].......$DotPrefix"
$NoWork =           "[No Work]....$DotPrefix"

<# Extented Variables #>
## Below Line Not need as of yet
#$cs_ScriptRoot = $PSScriptRoot

<# CS Project Functions #>
function Get-Cs-Project-version {
    param (
        [string]
        $CsProjDir 
    )

	if($null -eq $CsProjDir){
		Write-Host 'The $CsProjDir variable path is null' -ForegroundColor Red
		return;
	}
	elseif(!(Test-Path $CsProjDir)){
		
		Write-Host "The Path '$CsProjDir' does not Exist" -ForegroundColor Red
		return;
	}

    Push-Location $CsProjDir

    $csProj = ".\*.csproj"

    $latest = Get-ChildItem $csProj | Select-Object -First 1

    $csProj = ".\$($latest.Name)"

    if(!(Test-Path $csProj)){        
        Write-Host "$donePrefix No Project File Found." -ForegroundColor red -BackgroundColor Black
        return   
    }

    $Projfile=[xml](Get-Content $csProj);
    
    $versionNode = $Projfile.Project.PropertyGroup.Version

	if ($null -eq $versionNode) {		
        $versionNode = "1.0.0.0"
	}
	
    Write-Host "$successPrefix Version $versionNode" -ForegroundColor Green -BackgroundColor Black
    
    Write-Host "$donePrefix Pulling project Version" -ForegroundColor Yellow -BackgroundColor Black

    Pop-Location

    return $versionNode
}

function Get-Cs-Project-PackageId {
    param (
        [string]
        $CsProjDir 
    )

	if($null -eq $CsProjDir){
		Write-Host 'The $CsProjDir variable path is null' -ForegroundColor Red
		return;
	}
	elseif(!(Test-Path $CsProjDir)){
		
		Write-Host "The Path '$CsProjDir' does not Exist" -ForegroundColor Red
		return;
	}

    Push-Location $CsProjDir

    $csProj = ".\*.csproj"

    $latest = Get-ChildItem $csProj | Select-Object -First 1

    $csProj = ".\$($latest.Name)"

    if(!(Test-Path $csProj)){        
        Write-Host "$donePrefix No Project File Found." -ForegroundColor red -BackgroundColor Black
        return   
    }

    $Projfile=[xml](Get-Content $csProj);
    
    $PackageId = $Projfile.Project.PropertyGroup.PackageId

	if ($null -eq $PackageId) {		
		Write-Host "$failPrefix PackageId $PackageId" -ForegroundColor Red -BackgroundColor Black
    
		Write-Host "$donePrefix Pulling project PackageId" -ForegroundColor Yellow -BackgroundColor Black

		return
	}
	
    Write-Host "$successPrefix PackageId $PackageId"  -ForegroundColor Green -BackgroundColor Black
    
    Write-Host "$donePrefix Pulling project PackageId" -ForegroundColor Yellow -BackgroundColor Black

    Pop-Location

    return $PackageId
}

function Update-Cs-Project-Version {
    param (
        # Updates the Verison Minor number when pushed       
         [string] $CsProjDir
		,[switch] $Release
		,[switch] $Major
		,[switch] $Minor
		,[switch] $Patch

    )
    
    Push-Location $CsProjDir

    Write-Host "$startingPrefix Pulling project Version" -ForegroundColor Yellow -BackgroundColor Black

    $csProj = ".\*.csproj"

    $latest = Get-ChildItem $csProj | Select-Object -First 1

    $csProj = ".\$($latest.Name)"

    if(Test-Path $csProj){
        Write-Host "$ProcessingPrefix Update to Project Version."         
    }
    else{
        Write-Host "$donePrefix No Project File Found." -ForegroundColor red -BackgroundColor Black
        return   
    }

    $Projfile=[xml](Get-Content $csProj);
    
    $versionNode = $Projfile.Project.PropertyGroup.Version

	if ($null -eq $versionNode) {
		# If you have a new project and have not changed the version number the Version tag may not exist
		$versionNode = $Projfile.CreateElement("Version")
		$Projfile.Project.PropertyGroup.AppendChild($versionNode)
        Write-Host "$ProcessingPrefix Version Tag Add"  -ForegroundColor Green -BackgroundColor Black
        $versionNode = "1.0.0.0"
	}
	   
    #$ver
    $Ver1 = [int]($versionNode.Split('.')[0])
	$Ver2 = [int]($versionNode.Split('.')[1])
	$Ver3 = [int]($versionNode.Split('.')[2])
	$Ver4 = [int]($versionNode.Split('.')[3])

	if($Release){
		$Ver1 = $Ver1 + 1		
	}

	if($Major){ 
		$Ver2 = $Ver2 + 1
		$Ver3 = 1
		$Ver4 = 0
	}
	elseif($Minor){
		$Ver3 = $Ver3 + 1
		$Ver4 = 0
	}
	elseif($Patch){
		$Ver4 = $Ver4 + 1
	}
    
    $Projfile.Project.PropertyGroup.Version = "$Ver1.$Ver2.$Ver3.$Ver4"
    
    Write-Host "$ProcessingPrefix Version Changed from $versionNode To $($Projfile.Project.PropertyGroup.Version)" -ForegroundColor Green -BackgroundColor Black
        
    $Projfile.Save((Resolve-Path "$csProj").Path)
    
    Write-Host "$donePrefix Pulling project Version" -ForegroundColor Yellow -BackgroundColor Black

    Pop-Location
}

function Pack-Cs-Project {
    param (
		[string] $CsProjDir
		,[string] $nupkg_Dest
        # Updates the Verison Minor number when other wise just a patch      
		#,[switch] $UpdateVersion
		,[switch] $MinorVerIncrease
		,[switch] $MajorVerIncrease
		,[switch] $PatchVerIncrease
    )

    Write-Host "$startingPrefix Project Publishing" -ForegroundColor Yellow -BackgroundColor Black
    
    if(Test-Path "$CsProjDir") {
        <#
            - The Base Path Exists  
        #>
    

		Push-Location $CsProjDir		
			$prjPath = $PWD.Path
			$updateVersion = $false
			$version = Get-Cs-Project-version -CsProjDir $prjPath
			$PackageId = Get-Cs-Project-PackageId -CsProjDir $prjPath

			$constTxt = "$PackageId Version $versionNode"

			#Pack
            Write-Host "$startingPrefix Packing $constTxt" -ForegroundColor Yellow -BackgroundColor Black
			
			dotnet pack --configuration release --nologo -o $PackDir

            Write-Host "$donePrefix Packing $constTxt" -ForegroundColor Yellow -BackgroundColor Black

			#Push-Location .\bin\Release\
			if((Test-Path "$($PackDir)$PackageId.$version.nupkg") -and (Test-Path $nupkg_Dest)) {
				Move-Item "$($PackDir)$PackageId.$version.nupkg" -Destination $nupkg_Dest
				$UpdateVersion = Test-Path "$nupkg_Dest\$PackageId.$version.nupkg"
			}
			#Pop-Location

			if($updateVersion){
				if($MajorVerIncrease) {
					Update-Cs-Project-Version -CsProjDir $prjPath -Major
				}
				elseif($MinorVerIncrease) {
					Update-Cs-Project-Version -CsProjDir $prjPath -Minor
				}
				elseif($PatchVerIncrease){
					Update-Cs-Project-Version -CsProjDir $prjPath -Patch
				}				
			}

		Pop-Location
    }
    else {
        Write-Host "$NoWork No Action Taken"
    }   
    
    Write-Host "$donePrefix Project Publishing" -ForegroundColor Yellow -BackgroundColor Black
}

function Test-IsNumber {
    param (
        [string] $testValue
    )

    if($testValue.Count -gt 0){
        return ([System.Char[]]$testValue | Where-Object { ![System.Char]::IsDigit($_) }).Count -eq 0
    }

    return $false
}