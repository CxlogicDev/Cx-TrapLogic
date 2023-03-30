<#
	Info: Publish Script for all application Library Projects
	Make sure the ConfigureScript.psm1 has been loaded into one of your PS Script module paths 
#>
$dirSep = [System.IO.Path]::DirectorySeparatorChar
Push-Location $PSScriptRoot

$rootScript = $PWD.ProviderPath

#$csprojs = @{}
#Get-ChildItem ..\..\src\*.csproj -Recurse | ForEach-Object { $csprojs.Add( $_.Name, $_.FullName) }

$cs_projs = @()
Get-ChildItem "..$($dirSep)..$($dirSep)src$($dirSep)*.csproj" -Recurse | ForEach-Object { $cs_projs += [Tree_Branch]::new($_.FullName) }

$cs_projs = $cs_projs | Where-Object { $_.Proj_PackageId.Length -gt 0}



class Tree_Branch_Referenece {	
	
	[string] $referenceType 

    [string] $name 

    #[string] $version 

    [bool] $isLocal = $false;
	
	Tree_Branch_Referenece([string] $Name, [string] $ReferenceType<#, [string] $Version = ''#>) {
		$this.name = $Name
		$this.referenceType = $ReferenceType
		#$this.version = $Version
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

	[string] $Proj_PackageId
	[string] $Proj_Authors
	[string] $Proj_Company

    [bool] $Publish 
    [int] $Publish_Order 

	#[xml]$docx #Test Field 

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

		#$this.docx = $doc

		$this.Proj_Version = $doc.Project.PropertyGroup.Version

        $this.Proj_Framework = $doc.Project.PropertyGroup.TargetFramework
        
		$this.Proj_Namespace = $doc.Project.PropertyGroup.RootNamespace

		$this.Proj_PackageId = $doc.Project.PropertyGroup.PackageId
		$this.Proj_Authors = $doc.Project.PropertyGroup.Authors
		$this.Proj_Company = $doc.Project.PropertyGroup.Company

		<#
		foreach($node in $doc.Project.ItemGroup.PackageReference){
			Write-Host "Name: $($node.Include); Version: $($node.Version)"
			#PackageReference($node.Include, $node.Version)
		}	
		#>

		foreach($node in $doc.Project.ItemGroup.ProjectReference){
			if($null -ne $node.Include -or $node.Include.Length -gt 0){
				Write-Host "[$($this.Proj_Name)] RefProj: $($node.Include);"
				$this.ProjectReference($node.Include)
			}
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
	
	param (
		[Tree_Branch[]] $branches
	)
	

	$cs_projs_order = @()

	$CxUtyExt = $branches | where { $_.Proj_PackageId -eq 'Cx-Utility-Extensions' }

	if($null -eq $CxUtyExt){
		throw new [System.InvalidOperationException] "Missing Cx-Utility-Extensions Project"
	}

	[int]$ct = 1;
	$CxUtyExt.Publish_Order = $ct
	

	$cs_projs_order += $CxUtyExt #.Add($ct, $CxUtyExt)	

	$temp_odr = @{}


	$temp_odr.Add(0, @())

	$temp_odr[0] += $CxUtyExt

	foreach($branch in $branches | Where-Object { $_.Proj_PackageId -ne $CxUtyExt.Proj_PackageId }){

		if($branch.Proj_PackageId -eq 'Cx-Utility-Extensions'){
			Write-Host 'Skipping Project: Cx-Utility-Extensions' -ForegroundColor Yellow
			continue;
		}

		if($branch.References.Length -eq 1 -and $branch.References[0].name -like "*$($dirSep)$($CxUtyExt.Proj_Name)" ){
			$ct++
			$branch.Publish_Order = $ct
			$cs_projs_order += $branch
			#$cs_projs_order.Add($ct, $branch)
			continue;
		}
		elseif(!$temp_odr.ContainsKey($branch.References.Length)) {
			write-Host 'New Branch being loaded'
			$temp_odr.Add($branch.References.Length, @())
		}

		$temp_odr[$branch.References.Length] += $branch

		#$branch.Publish_Order = $ct;
		#$cs_projs_order.Add(1, @())

		#if(!$cs_projs_order.ContainsKey(1)){
		#	$cs_projs_order.Add($ct, @())
		#}
		#else {
		#	$cs_projs_order[1] += $branch
		#}
	}


	foreach($key in $temp_odr.Keys) {

		#Write-Host "Key: $key" -ForegroundColor Red
		$RefProjNames = @()
		$cs_projs_order | foreach { $RefProjNames += $_.Proj_Name }
		
		foreach($keyBranch in $temp_odr[$key]){
			Write-Host "[key:  $key; Branch: $($keyBranch.Proj_Name); Refs: $($keyBranch.References.Length)]" -ForegroundColor Red
			
			$RefProjs = @()
			$keyBranch.reference | foreach { $RefProjs += $_.name }

			if(($keyBranch.References | select {$_.name.Split($dirSep) | select -Last 1} ).Length -gt 0){
				Write-Host 'I have Value'
			}

			Write-Host ''
		}

		Write-Host ''
		#$temp_odr[$key] | where { $_.References[0].Proj_PackageId -ne $CxUtyExt.Proj_PackageId } | select 



	}
	
	return $temp_odr
	#return $cs_projs_order


	#return $branches


}

$cs_projsOrd = Cx-OrderProjects $cs_projs
