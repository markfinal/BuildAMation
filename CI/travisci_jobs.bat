call :launch_job master https://api.travis-ci.org/repo/markfinal%%2FBuildAMation/requests
call :launch_job master https://api.travis-ci.org/repo/markfinal%%2Fbam-compress/requests

rem curl.exe -v -X POST -H "Content-Type: application/json" -H "Accept: application/json" -H "Travis-API-Version: 3" -H "Authorization: token %TRAVIS_API_TOKEN%" -d "{\"request\": {\"branch\":\"master\"}}"  https://api.travis-ci.org/repo/markfinal%%2Fbam-imageformats/requests
rem curl.exe -v -X POST -H "Content-Type: application/json" -H "Accept: application/json" -H "Travis-API-Version: 3" -H "Authorization: token %TRAVIS_API_TOKEN%" -d "{\"request\": {\"branch\":\"master\"}}"  https://api.travis-ci.org/repo/markfinal%%2Fbam-python/requests
echo Finished launching TravisCI jobs
goto :eof

:launch_job
echo Launching job:
echo  Branch name '%1'
echo  Repo        '%2'
echo curl.exe -v -X POST -H "Content-Type: application/json" -H "Accept: application/json" -H "Travis-API-Version: 3" -H "Authorization: token %TRAVIS_API_TOKEN%" -d "{\"request\": {\"branch\":\"%1\", \"config\":{\"merge_mode\": \"deep_merge\", \"env\":{\"global\": {\"BAM_BRANCH\":\"%APPVEYOR_REPO_BRANCH%\"}}}}}" %2
curl.exe -v -X POST -H "Content-Type: application/json" -H "Accept: application/json" -H "Travis-API-Version: 3" -H "Authorization: token %TRAVIS_API_TOKEN%" -d "{\"request\": {\"branch\":\"%1\", \"config\":{\"merge_mode\": \"deep_merge\", \"env\":{\"global\": {\"BAM_BRANCH\":\"%APPVEYOR_REPO_BRANCH%\"}}}}}" %2
goto :eof
