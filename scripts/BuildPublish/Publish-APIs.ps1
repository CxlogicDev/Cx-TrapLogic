<#
	Info: Publish Script for all application Library Projects
	Make sure the ConfigureScript.psm1 has been loaded into one of your PS Script module paths 
#>

Push-Location $PSScriptRoot

function Cx-Publish-APIs {
	param (
        [string] $csProjDirectory
        ,[string] $nupkg_Dest
		,[switch] $MinorVerIncrease
		,[switch] $MajorVerIncrease
		,[switch] $PatchVerIncrease
		,[switch] $IncreaseOnly
	)

    if(!(Test-Path $csProjDirectory)){
        Write-Host "Cannot Find Directory Path: $csProjDirectory" -BackgroundColor Black -ForegroundColor Red
    }

    if($null -eq $nupkg_Dest){
        $cmpPath = [System.IO.Path]::Combine()
        <# The Cx Paths need to be added to the system to process the output to #>
        #if(Test-Path ) 


    }

	Push-Location $csProjDirectory
		if($IncreaseOnly){
			if($MajorVerIncrease) {
				Update-Cs-Project-Version -CsProjDir $PWD.Path -Major
			}
			elseif($MinorVerIncrease) {
				Update-Cs-Project-Version -CsProjDir $PWD.Path -Minor
			}
			else {
				Update-Cs-Project-Version -CsProjDir $PWD.Path -Patch
			}
		}
		else{
			
			if($MajorVerIncrease){
				Pack-Cs-Project $PWD.Path -nupkg_Dest $nupkg_Dest -MajorVerIncrease
			}
			if($MinorVerIncrease){
				Pack-Cs-Project $PWD.Path -nupkg_Dest $nupkg_Dest -MinorVerIncrease
			}
			elseif($PatchVerIncrease){
				Pack-Cs-Project $PWD.Path -nupkg_Dest $nupkg_Dest -PatchVerIncrease
			}
			else{
				Pack-Cs-Project $PWD.Path -nupkg_Dest $nupkg_Dest
			}
		}
	Pop-Location
}
