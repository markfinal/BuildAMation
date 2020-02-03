echo "Launching Travis project: bam-compress"
curl.exe -Ss -H "Content-Type: application/json" -H "Accept: application/json" -H "Travis-API-Version: 3" -H "Authorization: token %TRAVIS_API_TOKEN%" -d "{\"request\": {\"branch\":\"%APPVEYOR_REPO_BRANCH%\", \"config\":{\"merge_mode\": \"deep_merge\", \"env\":{\"global\": {\"BAM_BRANCH\":\"%APPVEYOR_REPO_BRANCH%\"}}}}}" https://api.travis-ci.org/repo/markfinal%%2Fbam-compress/requests

echo "Launching Travis project: bam-imageformats"
curl.exe -Ss -H "Content-Type: application/json" -H "Accept: application/json" -H "Travis-API-Version: 3" -H "Authorization: token %TRAVIS_API_TOKEN%" -d "{\"request\": {\"branch\":\"%APPVEYOR_REPO_BRANCH%\", \"config\":{\"merge_mode\": \"deep_merge\", \"env\":{\"global\": {\"BAM_BRANCH\":\"%APPVEYOR_REPO_BRANCH%\"}}}}}" https://api.travis-ci.org/repo/markfinal%%2Fbam-imageformats/requests

echo "Launching Travis project: bam-boost"
curl.exe -Ss -H "Content-Type: application/json" -H "Accept: application/json" -H "Travis-API-Version: 3" -H "Authorization: token %TRAVIS_API_TOKEN%" -d "{\"request\": {\"branch\":\"%APPVEYOR_REPO_BRANCH%\", \"config\":{\"merge_mode\": \"deep_merge\", \"env\":{\"global\": {\"BAM_BRANCH\":\"%APPVEYOR_REPO_BRANCH%\"}}}}}" https://api.travis-ci.org/repo/markfinal%%2Fbam-boost/requests

echo "Launching Travis project: bam-parallelism"
curl.exe -Ss -H "Content-Type: application/json" -H "Accept: application/json" -H "Travis-API-Version: 3" -H "Authorization: token %TRAVIS_API_TOKEN%" -d "{\"request\": {\"branch\":\"master\", \"config\":{\"merge_mode\": \"deep_merge\", \"env\":{\"global\": {\"BAM_BRANCH\":\"%APPVEYOR_REPO_BRANCH%\"}}}}}" https://api.travis-ci.org/repo/markfinal%%2Fbam-parallelism/requests

EXIT /B

echo "Launching Travis project: BuildAMation"
curl.exe -Ss -H "Content-Type: application/json" -H "Accept: application/json" -H "Travis-API-Version: 3" -H "Authorization: token %TRAVIS_API_TOKEN%" -d "{\"request\": {\"branch\":\"master\", \"config\":{\"merge_mode\": \"deep_merge\", \"env\":{\"global\": {\"BAM_BRANCH\":\"%APPVEYOR_REPO_BRANCH%\"}}}}}" https://api.travis-ci.org/repo/markfinal%%2FBuildAMation/requests

echo "Launching Travis project: bam-graphicssdk"
curl.exe -Ss -H "Content-Type: application/json" -H "Accept: application/json" -H "Travis-API-Version: 3" -H "Authorization: token %TRAVIS_API_TOKEN%" -d "{\"request\": {\"branch\":\"master\", \"config\":{\"merge_mode\": \"deep_merge\", \"env\":{\"global\": {\"BAM_BRANCH\":\"%APPVEYOR_REPO_BRANCH%\"}}}}}" https://api.travis-ci.org/repo/markfinal%%2Fbam-graphicssdk/requests

echo "Launching Travis project: bam-parser"
curl.exe -Ss -H "Content-Type: application/json" -H "Accept: application/json" -H "Travis-API-Version: 3" -H "Authorization: token %TRAVIS_API_TOKEN%" -d "{\"request\": {\"branch\":\"master\", \"config\":{\"merge_mode\": \"deep_merge\", \"env\":{\"global\": {\"BAM_BRANCH\":\"%APPVEYOR_REPO_BRANCH%\"}}}}}" https://api.travis-ci.org/repo/markfinal%%2Fbam-parser/requests

echo "Launching Travis project: bam-python"
curl.exe -Ss -H "Content-Type: application/json" -H "Accept: application/json" -H "Travis-API-Version: 3" -H "Authorization: token %TRAVIS_API_TOKEN%" -d "{\"request\": {\"branch\":\"master\", \"config\":{\"merge_mode\": \"deep_merge\", \"env\":{\"global\": {\"BAM_BRANCH\":\"%APPVEYOR_REPO_BRANCH%\"}}}}}" https://api.travis-ci.org/repo/markfinal%%2Fbam-python/requests

echo "Launching Travis project: bam-qt"
curl.exe -Ss -H "Content-Type: application/json" -H "Accept: application/json" -H "Travis-API-Version: 3" -H "Authorization: token %TRAVIS_API_TOKEN%" -d "{\"request\": {\"branch\":\"master\", \"config\":{\"merge_mode\": \"deep_merge\", \"env\":{\"global\": {\"BAM_BRANCH\":\"%APPVEYOR_REPO_BRANCH%\"}}}}}" https://api.travis-ci.org/repo/markfinal%%2Fbam-qt/requests

echo "Launching Travis project: bam-xml"
curl.exe -Ss -H "Content-Type: application/json" -H "Accept: application/json" -H "Travis-API-Version: 3" -H "Authorization: token %TRAVIS_API_TOKEN%" -d "{\"request\": {\"branch\":\"master\", \"config\":{\"merge_mode\": \"deep_merge\", \"env\":{\"global\": {\"BAM_BRANCH\":\"%APPVEYOR_REPO_BRANCH%\"}}}}}" https://api.travis-ci.org/repo/markfinal%%2Fbam-xml/requests

echo "Launching Travis project: bam-zeromq"
curl.exe -Ss -H "Content-Type: application/json" -H "Accept: application/json" -H "Travis-API-Version: 3" -H "Authorization: token %TRAVIS_API_TOKEN%" -d "{\"request\": {\"branch\":\"master\", \"config\":{\"merge_mode\": \"deep_merge\", \"env\":{\"global\": {\"BAM_BRANCH\":\"%APPVEYOR_REPO_BRANCH%\"}}}}}" https://api.travis-ci.org/repo/markfinal%%2Fbam-zeromq/requests

echo Finished launching TravisCI jobs
goto :eof
