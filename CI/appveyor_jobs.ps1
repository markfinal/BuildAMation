Start-AppveyorBuild -ApiKey $env:ACCOUNT_API_KEY -ProjectSlug 'bam-imageformats' -EnvironmentVariables @{ BAM_branch = $env:APPVEYOR_REPO_BRANCH }
