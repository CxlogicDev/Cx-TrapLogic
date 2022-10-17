<#
	Info: Publish Script for all application Library Projects
	Make sure the ConfigureScript.psm1 has been loaded into one of your PS Script module paths 
#>

Push-Location $PSScriptRoot

$rootScript = $PWD.ProviderPath

class Tree_Branch_Referenece {	
	
	[string] $referenceType 

    [string] $name 

    [string] $version 

    [bool] $isLocal = $false;
	
	Tree_Branch_Referenece([string] $Name, [string] $ReferenceType, [string] $Version = '') {
		$this.name = $Name
		$this.referenceType = $ReferenceType
		$this.version = $Version
	}
}

class Tree_Branch {
	#< #Define the class. Try constructors, properties, or methods #>


	[string] $Proj_Path
    [string] $Proj_Directory 
    [string] $Proj_Name 
    [string] $Proj_Namespace 
    [string] $Proj_Version 
    [string] $Proj_Framework 

    [bool] $Publish 
    [int] $Publish_Order 

	[Tree_Branch_Referenece[]] $References = @()

	Tree_Branch([string] $Project_Path) {
		<# Initialize the class. Use $this to reference the properties of the instance you are creating #>
		if($null -eq $Project_Path -or $Project_Path.Length -eq 0){
			Write-Error -Message "The Project_Path is missing" -ErrorAction Stop
		}

		if(!(Test-Path $Project_Path)){
			Write-Error -Message "The file: [$Project_Path] was not found" -ErrorAction Stop
		}

		$this.Proj_Path = $Project_Path

		$this.Proj_Directory = (Get-ChildItem $Project_Path).Directory.FullName
		$this.Proj_Name = (Get-ChildItem $Project_Path).Name

		#Load the Properties 
<#

        $ProjectNode = doc["Project"];//["PropertyGroup"];
        if (ProjectNode == null)
            return;

        var PropertyGroup = ProjectNode["PropertyGroup"];
        if (PropertyGroup == null)
            return;

        Proj_Version = PropertyGroup["Version"]?.InnerText ?? string.Empty;
        Proj_Framework = PropertyGroup["TargetFramework"]?.InnerText ?? string.Empty;
        Proj_Name = Proj_Path?.Split(Path.DirectorySeparatorChar).Last().Split('.')[0] ?? throw new ArgumentNullException(nameof(Proj_Path));
        Proj_Namespace = PropertyGroup["RootNamespace"]?.InnerText ?? string.Empty;
#>
		[xml]$doc = Get-Content -Path $this.Proj_Path

		$this.Proj_Version = $doc.Project.PropertyGroup.Version

        $this.Proj_Framework = $doc.Project.PropertyGroup.TargetFramework
        
		$this.Proj_Namespace = $doc.Project.PropertyGroup.RootNamespace

		foreach($node in $doc.ItemGroup.PackageReference){
			Write-Host "Name: $($node.Include); Version: $($node.Version)"
			#PackageReference($node.Include, $node.Version)
		}	

		foreach($node in $doc.ItemGroup.ProjectReference){
			Write-Host "Name: $($node.Include); Version: $($node.Version)"
			#ProjectReference($node.Include)
		}

	}

	[void]PackageReference([string] $Name, [string] $Version) 
	{# 'PackageReference'
		$this.References += [Tree_Branch_Referenece]::new($Name, 'PackageReference', $Version)
	}

	[void] ProjectReference([string] $Name) 
	{#'ProjectReference'
		$this.References += [Tree_Branch_Referenece]::new($Name, 'ProjectReference')
	}



}


function Cx-Publish-APIs {
	<#
		This will build a single API that passed in but is redundent at this point. 
		This needs to have a action that finds and orders the parts  
	#>
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
        #$cmpPath = [System.IO.Path]::Combine()
        <# The Cx Paths need to be added to the system to process the output to #>
        #if(Test-Path ) 

		write-Host "Missing the destination Path -nupkg_Dest" -ForegroundColor Red -BackgroundColor Black
		return 
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


function Cx-OrderProjects {
	<#
	param (
		OptionalParameters
	)
	#>
	
	#push to the proper dirctory
	Push-Location $rootScript

		Push-Location ..\..\src

			#$src = $PWD.Path
			$csProjDirs = Get-ChildItem *.csproj -File -Recurse | Select DirectoryName

			



		Pop-Location

	Pop-Location

	

	


}