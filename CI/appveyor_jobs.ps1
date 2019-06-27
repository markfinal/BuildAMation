function launch_project([String] $projectName) {
    "Launching AppVeyor project: $projectName"
    Start-AppveyorBuild -ApiKey $env:ACCOUNT_API_KEY -ProjectSlug $projectName -EnvironmentVariables @{BAM_BRANCH=$env:APPVEYOR_REPO_BRANCH}
}

launch_project 'bam-compress'

#Start-AppveyorBuild -ApiKey $env:ACCOUNT_API_KEY -ProjectSlug 'bam-imageformats' -EnvironmentVariables @{ BAM_BRANCH = $env:APPVEYOR_REPO_BRANCH }
#Start-AppveyorBuild -ApiKey $env:ACCOUNT_API_KEY -ProjectSlug 'bam-python' -EnvironmentVariables @{ BAM_BRANCH = $env:APPVEYOR_REPO_BRANCH }
