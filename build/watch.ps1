# Watch for file changes in a directory, and execute a command when a file is changed or renamed
param(
	[Parameter(Mandatory)]
	[string]$Path,
	[Parameter(Mandatory)]
	[string]$Command,
	[Parameter(Mandatory)]
	[string[]]$Arguments,
	[Parameter()]
	[switch]$RunAtStartup
)

function Write-Invocation {
	param (
		$Output
	)
	$TimeStamp = Get-Date -format "yy-MM-dd hh:mm:ss"
	Write-Output "$TimeStamp - $Output"
}

function Invoke-Command {
	param (
		$ChangedFile
	)
	if ($ChangedFile) {
		Write-Invocation "'$ChangedFile' has changed"
	}
	Start-Process -Wait -FilePath $Command -NoNewWindow -ArgumentList $Arguments
	if ($ChangedFile) {
		Write-Invocation "Finished after '$ChangedFile' had changed"
	}
	else {
		Write-Invocation "Started"
	}
}

$Watcher = New-Object System.IO.FileSystemWatcher
$Watcher.Path = $Path
$Watcher.IncludeSubdirectories = $true
$Watcher.EnableRaisingEvents = $false
$Watcher.NotifyFilter = [System.IO.NotifyFilters]::LastWrite -bOr [System.IO.NotifyFilters]::FileName
Write-Output "Watches for changed files under '$($Path)' and trigger the command:"
Write-Output "$($Command) $($Arguments)"

if ($RunAtStartup) {
	Invoke-Command
}

while ($TRUE) {
	$Result = $Watcher.WaitForChanged([System.IO.WatcherChangeTypes]::Changed -bOr [System.IO.WatcherChangeTypes]::Renamed -bOr [System.IO.WatcherChangeTypes]::Created, 1000);
	if($Result.TimedOut){
		continue;
	}
	Invoke-Command $Result.Name
}