set MASTER_BRANCH_REQUEST={""request"": {""branch"":""master""}}
where curl
curl -s -X POST -H "Content-Type: application/json" -H "Accept: application/json" -H "Travis-API-Version: 3" -H "Authorization: token %TRAVIS_API_TOKEN%" -d "%MASTER_BRANCH_REQUEST%"  https://api.travis-ci.org/repo/markfinal%%2Fbam-imageformats/requests
