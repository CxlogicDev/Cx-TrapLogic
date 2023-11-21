<#
	Info: Publish Script for all application Library Projects
	Make sure the ConfigureScript.psm1 has been loaded into one of your PS Script module paths 
#>
$dirSep_api = [System.IO.Path]::DirectorySeparatorChar
Push-Location $PSScriptRoot

$cs_projs = @()
Get-ChildItem "..$($dirSep_api)..$($dirSep_api)src$($dirSep_api)*.csproj" -Recurse | ForEach-Object { $cs_projs += [Tree_Branch]::new($_.FullName) }

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

	[string] ProjName() 
	{#'ProjectReference'
		
		return $this.name.Replace('\',  [System.IO.Path]::DirectorySeparatorChar);
		#$this.References += [Tree_Branch_Referenece]::new($Name, 'ProjectReference')
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
				#Write-Host "[$($this.Proj_Name)] RefProj: $($node.Include);"
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

function Cx-Publish-API {
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

    if($null -eq $nupkg_Dest -and !($IncreaseOnly)){
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

function Cx-Publish-AllAPIs {
	param (
		[Tree_Branch[]] $OrderedBranches
		,[string] $nupkg_Dest
		,[switch] $MinorVerIncrease
		,[switch] $MajorVerIncrease
		,[switch] $PatchVerIncrease
	)

	
	if(!$MinorVerIncrease -and !$MajorVerIncrease -and !$PatchVerIncrease){
		Write-Host "No work was done please select the Release type" -ForegroundColor Red
		Write-Host "`t-PatchVerIncrease for a Patch Release" -ForegroundColor Red
		Write-Host "`t-MinorVerIncrease for a Minor Version Release" -ForegroundColor Red
		Write-Host "`t-MajorVerIncrease for a Major Version Release" -ForegroundColor Red
		return;
	}
	
	Write-Host "Current Branches" -ForegroundColor Yellow
	$OrderedBranches | Sort-Object -Property Publish_Order | Select-Object -Property Publish_Order,Proj_Name,Proj_Version | Format-Table -AutoSize

	#Process The Output
	$OrderedBranches | Sort-Object -Property Publish_Order | ForEach-Object { Cx-Publish-API -csProjDirectory $_.Proj_Directory -nupkg_Dest $nupkg_Dest }

	#update the versions
	if($MajorVerIncrease){
		$OrderedBranches | Sort-Object -Property Publish_Order | ForEach-Object { Cx-Publish-API -csProjDirectory $_.Proj_Directory -nupkg_Dest $nupkg_Dest -IncreaseOnly -MajorVerIncrease }	
	}
	if($MinorVerIncrease){
		$OrderedBranches | Sort-Object -Property Publish_Order | ForEach-Object { Cx-Publish-API -csProjDirectory $_.Proj_Directory -nupkg_Dest $nupkg_Dest -IncreaseOnly -MinorVerIncrease }
	}
	elseif($PatchVerIncrease){
		$OrderedBranches | Sort-Object -Property Publish_Order | ForEach-Object { Cx-Publish-API -csProjDirectory $_.Proj_Directory -nupkg_Dest $nupkg_Dest -IncreaseOnly -PatchVerIncrease }
	}
	
	Write-Host "`nProcessed Branches" -ForegroundColor Green
	$OrderedBranches | Sort-Object -Property Publish_Order | Select-Object -Property Publish_Order,Proj_Name,Proj_Version | Format-Table -AutoSize
}

function Cx-OrderProjects {
	
	param (
		[Tree_Branch[]] $branches
	)

	$dirSep_api = [System.IO.Path]::DirectorySeparatorChar

	$CxUtyExt = $branches | where { $_.Proj_PackageId -eq 'Cx-Utility-Extensions' }

	if($null -eq $CxUtyExt){
		throw new [System.InvalidOperationException] "Missing Cx-Utility-Extensions Project"
	}

	
	$CxUtyExt.Publish_Order = 1
	
	#The C# project branches to hold
	$cs_projs_order = @()

	$cs_projs_order += $CxUtyExt #.Add($ct, $CxUtyExt)	

	$cs_OrderedNames = @()

	$cs_OrderedNames += $CxUtyExt.Proj_Name#"*$($dirSep_api)$($CxUtyExt.Proj_Name)"

	#Temp Hash Table to hold an array of projects with number of References used
	$temp_odr = @{}

	#The fist for the base project [0] = "Cx-Utility-Extensions".
	$temp_odr.Add(0, @())

	$temp_odr[0] += $CxUtyExt

	$temp_odr.Add(1, @())

	[int]$ct = 1;

	$nBranches = $branches | Where-Object { $_.Proj_PackageId -ne $CxUtyExt.Proj_PackageId -and $_.References.Length -eq 1 -and $_.References[0].ProjName() -like "*$($dirSep_api)$($CxUtyExt.Proj_Name)"}
	
	foreach ($sbranch in $nBranches) {
		$ct++
		$sbranch.Publish_Order = $ct
		$cs_projs_order += $sbranch
		$temp_odr[1] += $sbranch
		$cs_OrderedNames += $sbranch.Proj_Name#"*$($dirSep_api)$($sbranch.Proj_Name)"
		Write-Host "[Ordered <> $($sbranch.Proj_Name)] Has Order at $ct" -ForegroundColor Green
	}

	return $cs_OrderedNames
	#$_.References[0].ProjName() -like "*$($dirSep_api)$($CxUtyExt.Proj_Name)"

	$nBranches = $branches | Where-Object { $_.Proj_PackageId -ne $CxUtyExt.Proj_PackageId -and $_.References.Length -eq 1 -and $_.References[0].ProjName() -like "*$($dirSep_api)$($CxUtyExt.Proj_Name)"}
	


	return $temp_odr;



	foreach($branch in $branches | Where-Object { $_.Proj_PackageId -ne $CxUtyExt.Proj_PackageId }){

		if($branch.Proj_PackageId -eq 'Cx-Utility-Extensions'){
			Write-Host 'Skipping Project: Cx-Utility-Extensions' -ForegroundColor Yellow
			continue;
		}

		#Need to build a conversion $branch.References[0].name
		# ex: \dir\dir\refProjName.ext <> \ need to be / in linux and same in windows
		if($branch.References.Length -eq 1 -and $branch.References[0].ProjName() -like "*$($dirSep_api)$($CxUtyExt.Proj_Name)" ){
			$ct++
			$branch.Publish_Order = $ct
			$cs_projs_order += $branch
			$temp_odr[1] += $branch
			Write-Host "[Ordered <> $($branch.Proj_Name)] Has Order at $ct" -ForegroundColor Green
			continue;
		}
		elseif(!$temp_odr.ContainsKey(($branch.References.Length + 1))) {
			write-Host 'New Branch being loaded'
			$temp_odr.Add(($branch.References.Length + 1), @())
		}

		$temp_odr[($branch.References.Length + 1)] += $branch	
	}


	$maxKey = 2;
	$curMaxKey = 0;

	while ($maxKey -gt $curMaxKey) {
		$curMaxKey = $maxKey
		Write-Host "Looping Max Key: $maxKey" -ForegroundColor Yellow

		foreach($key in $temp_odr.Keys | Sort-Object) {

			if($key -lt $curMaxKey){
				Write-Host "[Skipped key:  $key]" -ForegroundColor Yellow
				continue;
			}

			$nextKey = ($key + 1);

			$RefProjNames = @()
			$cs_projs_order | ForEach-Object { $RefProjNames += $_.Proj_Name }
			
			foreach($keyBranch in $temp_odr[$key]){
				Write-Host "[key:  $key; Branch: $($keyBranch.Proj_Name); Refs: $($keyBranch.References.Length)]" -ForegroundColor Yellow
				
				if($keyBranch.References.Length -eq 1 ){				
					$refNow = ($RefProjNames | Where-Object { $keyBranch.References[0].name -like "*$($dirSep_api)$($_)" })
					
					if($null -ne $refNow){
						$ct++
						$keyBranch.Publish_Order = $ct
						$cs_projs_order += $keyBranch
						Write-Host "[Ordered <> $($branch.Proj_Name)] Has Order at $ct" -ForegroundColor Green
						continue;
					}
					elseif(!$temp_odr.ContainsKey($nextKey)) {
						write-Host 'New Branch being loaded'
						$temp_odr.Add($nextKey, @())
					}

					$maxKey = $nextKey
					$temp_odr[$nextKey] += $keyBranch
					Write-Host "[Reordered <> $($branch.Proj_Name)] was reorder to Key: $nextKey" -ForegroundColor Yellow
					continue;
				}

				$RefProjs = @()

				foreach($keyRef in $keyBranch.reference){

					$refNow = ($RefProjNames | Where-Object { $keyRef.name -like "*$($dirSep_api)$($_)" })
					$RefProjs += $keyRef
				}

				if($RefProjs.Length -eq $keyBranch.reference.Length){
					$ct++
					$keyBranch.Publish_Order = $ct
					$cs_projs_order += $keyBranch
					Write-Host "[Ordered <> $($branch.Proj_Name)] Has Order at $ct" -ForegroundColor Green
				}
				elseif($RefProjs.Length -lt $keyBranch.reference){
					if(!$temp_odr.ContainsKey($nextKey)) {
						write-Host 'New Branch being loaded'
						$temp_odr.Add($nextKey, @())
					}

					$maxKey = $nextKey
					$temp_odr[$nextKey] += $keyBranch
					Write-Host "[Reordered <> $($branch.Proj_Name)] was reorder to Key: $nextKey" -ForegroundColor Yellow
				}
				else {
					Write-Host "[Error <> $($keyBranch.Proj_Name)] Problem with Reference Values " -ForegroundColor Red
				}
			}

			Write-Host ''			
		}

		if($maxKey -gt $curMaxKey){
			Write-Host "New Max Key: $maxKey" -ForegroundColor Yellow
		}
		else{
			Write-Host "Finished Looping Max Key: $maxKey" -ForegroundColor Yellow
		}

	}

	return $cs_projs_order
}

$cs_projsOrd = Cx-OrderProjects $cs_projs


<#

Get-ChildItem *.csproj -Recurse | Select-Object Name,FullName | ForEach-Object { $csProj.Add($_.Name, $_.FullName) }

#>