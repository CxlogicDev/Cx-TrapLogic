$pubScrDS = [System.IO.Path]::DirectorySeparatorChar

#Gets the User Module Path
$userPath = (Get-ChildItem env:PSModulePath).Value.Split(':') | Where-Object { $_ -like "$($HOME)*" } | Select-Object -First 1

#Find Number Function
function Test-Number {
    param (
        [string] $testValue
    )

    if($testValue.Count -gt 0){
        return ([System.Char[]]$testValue | Where-Object { ![System.Char]::IsDigit($_) }).Count -eq 0
    }

    return $false
}

#function Cx-Publish-ProjConfigModule {
    <#
        This Will Publish the Under lining cs-Proj-Configure.psm1 
    #>
    <#
    param (
        [string]
    )
    #>

    Push-Location $PSScriptRoot

    Push-Location "..$($pubScrDS)"

    $myDir = 'cs-Proj-Configure'

    $Myfile = [System.IO.Path]::Combine($PWD.Path, "$myDir.psm1")

    Pop-Location
    
    $selectValue = 0;
    $runCt = 0;

    #Linux Uses : while Microsoft uses ;   $env:PSModulePath.Value.Split(';',':')
    $PSModulePath = $env:PSModulePath;
    $delim = ';'
    if($PSModulePath.Contains(':')){
        $delim = ':'
    }

    $psModPaths = $env:PSModulePath.Split($delim);

    while ($selectValue -eq 0 -and $runCt -lt 5) {

        #Write-Host "Which Number location would yoou like to save the Module to" -ForegroundColor Yellow -BackgroundColor Black
        for ($i = 0; $i -lt $psModPaths.Count; $i++) {
            Write-Host "[$($i+1)] - $($psModPaths[$i])" -ForegroundColor Yellow -BackgroundColor Black
        }

        $val = Read-Host -Prompt 'Type the number of location you would like to store the Script Module?'

        Write-Host "Selection: $val" -ForegroundColor Yellow -BackgroundColor Black

        if(Test-Number -testValue $val) {
            $intValue = [int]$val

            if($intValue -gt 0 -and $intValue -le $psModPaths.Count) {
                $selectValue = $intValue;
            }
            else {
                Write-Host "Invalid Selection" -ForegroundColor Green -BackgroundColor Black
            }
        }
        else {
            Write-Host "Invalid Selection" -ForegroundColor Red -BackgroundColor Black
        }

        $runCt++
    }

    if($selectValue -gt 0) {
        $toPath =  [System.IO.Path]::Combine($psModPaths[$selectValue - 1], $myDir)

        if(Test-Path $psModPaths[$selectValue - 1]){
            Push-Location $psModPaths[$selectValue - 1]
            
            $test = [System.IO.Path]::Combine($PWD.Path, $myDir)

            if(!([System.IO.Directory]::Exists($test))){
                mkdir $myDir
            }
            else{
                Remove-Item [System.IO.Path]::Combine($toPath, "$myDir.psm1") -Force
            }

            Write-Host "Copying Module $Myfile To: $toPath" -ForegroundColor Green -BackgroundColor Black

            Copy-Item $Myfile $toPath 
        }
        else{
            Write-Host "Invalid Path: $($psModPaths[$selectValue - 1])" -ForegroundColor Red -BackgroundColor Black    
        }        
    }
    else {
        Write-Host "No valid seletcion made to move module" -ForegroundColor Red -BackgroundColor Black
    }

    Pop-Location
#}






