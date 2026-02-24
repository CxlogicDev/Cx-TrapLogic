#!/usr/bin/env dotnet run
//#:project ./Cx-TrapLogic/src/Utilities/Cx-Utility-Extensions/Cx-Utility-Extensions.csproj
using System;
using System.IO;




/*

    Console.WriteLine("What is your name? ");
    var temp = Console.ReadLine();

    if(temp.hasCharacters())
        Console.WriteLine("Hello Christifer");
    else 
        throw new ArgumentException("No Text was given to the script");

*/

//const int _ShowTime = 90; 

string MainPath = Path.Combine("..", "..", "src");

if(!System.IO.Directory.Exists(MainPath))
    throw new DirectoryNotFoundException($"[Directory Not Found] {MainPath}");
System.Console.Write($"[Found: {MainPath}] ");
System.Console.WriteLine("Moving to first project");






class Tree_Branch_Referenece(string Name, string ReferenceType)
{
    string Name { get; init; } = Name.Replace('\\', Path.DirectorySeparatorChar); 
    string ReferenceType { get; init; } = ReferenceType;    
}

class Tree_Branch
{
    ///<summary>
    /// project 
    ///</summary> 
    public string Proj_Path{ get; set; }
    ///<summary>
    /// project 
    ///</summary> 
    public string Proj_Directory { get; set; }
    ///<summary>
    /// project 
    ///</summary> 
    public string Proj_Name { get; set; }
    ///<summary>
    /// project 
    ///</summary> 
    public string Proj_Namespace { get; set; }
    ///<summary>
    /// project 
    ///</summary> 
    public string Proj_Version { get; set; }
    ///<summary>
    /// project 
    ///</summary> 
    public string Proj_Framework { get; set; }

	///<summary>
    /// project 
    ///</summary> 
    public string Proj_PackageId{ get; set; }
	///<summary>
    /// project 
    ///</summary> 
    public string Proj_Authors{ get; set; }
	///<summary>
    /// project 
    ///</summary> 
    public string Proj_Company{ get; set; }

    ///<summary>
    /// project 
    ///</summary> 
    public bool Publish { get; set; }
    ///<summary>
    /// project 
    ///</summary> 
    public int Publish_Order { get; set; }

    public List<Tree_Branch_Referenece> References = [];

    

	void ProjectReference(string Name) 
	{//'ProjectReference'
		References.Add(new Tree_Branch_Referenece(Name, "ProjectReference"));
	}

}

/*
static class Tree_Process
{
    
    static ConsoleColor OC_ForegroundColor = Console.ForegroundColor;
    static ConsoleColor OC_BackgroundColor = Console.BackgroundColor;
    static void WriteLine(string[] messagess, ConsoleColor foreground, ConsoleColor background)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        foreach(var m in messages)
            System.Console.WriteLine($"Cannot Find Directory Path: {csProjDirectory}"); // -BackgroundColor Black -ForegroundColor Red
    }

    public static void Cx_Publish_API(
        string csProjDirectory
        ,string nupkg_Dest
        ,bool MinorVerIncrease
        ,bool MajorVerIncrease
        ,bool PatchVerIncrease
        ,bool IncreaseOnly

    )
    {
        /*
            This will build a single API that passed in but is redundent at this point. 
            This needs to have a action that finds and orders the parts  
        * /
        
        if(!Path.Exists(csProjDirectory)){
            Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine($"Cannot Find Directory Path: {csProjDirectory}"); // -BackgroundColor Black -ForegroundColor Red

        }

        if($null -eq $nupkg_Dest -and !($IncreaseOnly)){
            #$cmpPath = [System.IO.Path]::Combine()
            <# The Cx Paths need to be added to the system to process the output to #>
            #if(Test-Path ) 

            write-Host "Missing the destination Path -nupkg_Dest" -ForegroundColor Red -BackgroundColor Black
            return 
        }

        //	Push-Location $csProjDirectory
        //		if($IncreaseOnly){
        //			if($MajorVerIncrease) {
        //				Update-Cs-Project-Version -CsProjDir $PWD.Path -Major
        //			}
        //			elseif($MinorVerIncrease) {
        //				Update-Cs-Project-Version -CsProjDir $PWD.Path -Minor
        //			}
        //			else {
        //				Update-Cs-Project-Version -CsProjDir $PWD.Path -Patch
        //			}
        //		}
        //		else{
        //			
        //			if($MajorVerIncrease){
        //				Pack-Cs-Project $PWD.Path -nupkg_Dest $nupkg_Dest -MajorVerIncrease
        //			}
        //			if($MinorVerIncrease){
        //				Pack-Cs-Project $PWD.Path -nupkg_Dest $nupkg_Dest -MinorVerIncrease
        //			}
        //			elseif($PatchVerIncrease){
        //				Pack-Cs-Project $PWD.Path -nupkg_Dest $nupkg_Dest -PatchVerIncrease
        //			}
        //			else{
        //				Pack-Cs-Project $PWD.Path -nupkg_Dest $nupkg_Dest
        //			}
        //		}
        //	Pop-Location
    }

}
//*/

/*




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
		if($branch.References.Length -eq 1 -and $branch.References[0].ProjName() -like (Format-Cs-Paths -PathValue "*\$($CxUtyExt.Proj_Name)") ){
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
					$refNow = ($RefProjNames | Where-Object { $keyBranch.References[0].name -like (Format-Cs-Paths -PathValue "*\$_") })
					
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

					$refNow = ($RefProjNames | Where-Object { $keyRef.name -like (Format-Cs-Paths -PathValue "*\$_") })
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


*/
