function launch_job([String] $jobName) {
    Start-AppveyorBuild -ApiKey $env:ACCOUNT_API_KEY -ProjectSlug $jobName -EnvironmentVariables @{ BAM_BRANCH = $env:APPVEYOR_REPO_BRANCH }
}

launch_job 'bam-compress'

#Start-AppveyorBuild -ApiKey $env:ACCOUNT_API_KEY -ProjectSlug 'bam-imageformats' -EnvironmentVariables @{ BAM_BRANCH = $env:APPVEYOR_REPO_BRANCH }
#Start-AppveyorBuild -ApiKey $env:ACCOUNT_API_KEY -ProjectSlug 'bam-python' -EnvironmentVariables @{ BAM_BRANCH = $env:APPVEYOR_REPO_BRANCH }
