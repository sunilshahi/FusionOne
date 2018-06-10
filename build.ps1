Push-Location -Path ".\scripts"

try{
    $scriptPath = ".\build.ps1"
    Invoke-Expression "&`"$scriptPath`" $args"
}
finally
{
    Pop-Location  
}


