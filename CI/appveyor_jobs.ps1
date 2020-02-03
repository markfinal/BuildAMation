function launch_project([String] $projectName) {
    "Launching AppVeyor project: $projectName"
    Start-AppveyorBuild -ApiKey $env:ACCOUNT_API_KEY -ProjectSlug $projectName -Branch $env:APPVEYOR_REPO_BRANCH -EnvironmentVariables @{BAM_BRANCH=$env:APPVEYOR_REPO_BRANCH}
}

launch_project 'bam-compress'
launch_project 'bam-imageformats'
launch_project 'bam-boost'
launch_project 'bam-parallelism'

exit

launch_project 'bam-graphicssdk'
launch_project 'bam-parser'
launch_project 'bam-python'
launch_project 'bam-qt'
launch_project 'bam-xml'
launch_project 'bam-zeromq'
