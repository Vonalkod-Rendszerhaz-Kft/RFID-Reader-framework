param($installPath, $toolsPath, $package, $project)

$configItem = $project.ProjectItems.Item("Vrh.EventHub.RedisPubSub.Config.xml")

# set 'Copy To Output Directory' to 'Copy if newer'
$copyToOutput = $configItem.Properties.Item("CopyToOutputDirectory")
$copyToOutput.Value = 2

# set 'Build Action' to 'None'
$buildAction = $configItem.Properties.Item("BuildAction")
$buildAction.Value = 0