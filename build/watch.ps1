# Watch for file changes in a directory, and executea command when a file is changed or renamed
param(
     [Parameter(Mandatory)]
     [string]$Path,
     [Parameter(Mandatory)]
     [string]$Command,
     [Parameter(Mandatory)]
     [string[]]$Arguments
 )
$Watcher = New-Object System.IO.FileSystemWatcher
$Watcher.Path = $Path
$Watcher.IncludeSubdirectories = $true
$Watcher.EnableRaisingEvents = $false
$Watcher.NotifyFilter = [System.IO.NotifyFilters]::LastWrite -bOr [System.IO.NotifyFilters]::FileName
Write-Output "Watches for changed files in '$($Path)', including sub directories"
Write-Output "Will run '$($Command)' with Arguments '$($Arguments)' "
while($TRUE){
	$Result = $Watcher.WaitForChanged([System.IO.WatcherChangeTypes]::Changed -bOr [System.IO.WatcherChangeTypes]::Renamed -bOr [System.IO.WatcherChangeTypes]::Created, 1000);
	if($Result.TimedOut){
		continue;
	}
    $Date = Get-Date -format "yy-MM-dd hh:mm:ss"
	Write-Output "$($Date) - '$($Result.Name)' has changed"
	Start-Process -Wait -FilePath $Command -NoNewWindow -ArgumentList $Arguments
    $Date = Get-Date -format "yy-MM-dd hh:mm:ss"
	Write-Output "$($Date) - Finished after '$($Result.Name)' had changed"
}