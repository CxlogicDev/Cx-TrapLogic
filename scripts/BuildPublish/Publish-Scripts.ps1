function Cx-Publish-ProjConfig {
    <#
    param (
        [string]
    )
    #>

    Push-Location $PSScriptRoot

    Push-Location ..\

    $Myfile = [System.IO.Path]::Combine($PWD.Path, 'cs-Proj-Configure.psm1')

    $myDir = 'cs-Proj-Configure'

    Pop-Location
    
    $selectValue = 0;
    $runCt = 0;

    $psModPaths = $env:PSModulePath.Split(';');

    while ($selectValue -eq 0 -and $runCt -lt 5) {

        #Write-Host "Which Number location would yoou like to save the Module too" -ForegroundColor Yellow -BackgroundColor Black
        for ($i = 0; $i -lt $psModPaths.Count; $i++) {
            Write-Host "[$($i+1)] - $($psModPaths[$i])" -ForegroundColor Yellow -BackgroundColor Black
        }

        $val = Read-Host -Prompt 'Type the number of location you would like to store the Script Module?'

        Write-Host "Selection: $val" -ForegroundColor Yellow -BackgroundColor Black

        if(Test-IsNumber -testValue $val) {
            $intValue = [int]$val

            if($intValue -gt 0 -and $intValue -le $psModPaths.Count) {
                $selectValue = $intValue;
            }
            else {
                Write-Host "Invalid Seletion" -ForegroundColor Green -BackgroundColor Black
            }
        }
        else {
            Write-Host "Invalid Seletion" -ForegroundColor Red -BackgroundColor Black
        }

        $runCt++
    }

    if($selectValue -gt 0) {
        $toPath =  [System.IO.Path]::Combine($psModPaths[$selectValue - 1], $myDir)
        Write-Host "Copying Module $Myfile To: $toPath" -ForegroundColor Green -BackgroundColor Black
    }
    else {
        Write-Host "No valid seletcion made to move module" -ForegroundColor Red -BackgroundColor Black
    }

    Pop-Location
}





