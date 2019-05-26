set MASTER_BRANCH_REQUEST={""request"": {""branch"":""master""}}
where curl
echo Try curl
curl.exe -V
echo Running curl
curl.exe -v -X POST -H "Content-Type: application/json" -H "Accept: application/json" -H "Travis-API-Version: 3" -H "Authorization: token %TRAVIS_API_TOKEN%" -d "%MASTER_BRANCH_REQUEST%"  https://api.travis-ci.org/repo/markfinal%%2Fbam-imageformats/requests
echo All done
