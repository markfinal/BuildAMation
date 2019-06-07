curl.exe -v -X POST -H "Content-Type: application/json" -H "Accept: application/json" -H "Travis-API-Version: 3" -H "Authorization: token %TRAVIS_API_TOKEN%" -d "{\"request\": {\"branch\":\"master\"}}"  https://api.travis-ci.org/repo/markfinal%%2Fbam-imageformats/requests
curl.exe -v -X POST -H "Content-Type: application/json" -H "Accept: application/json" -H "Travis-API-Version: 3" -H "Authorization: token %TRAVIS_API_TOKEN%" -d "{\"request\": {\"branch\":\"master\"}}"  https://api.travis-ci.org/repo/markfinal%%2Fbam-python/requests
echo Finished launching TravisCI jobs
