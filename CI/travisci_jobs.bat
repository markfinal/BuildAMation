set MASTER_BRANCH_BODY='{
"request": {
"branch":"master"
}}'

curl -s -X POST -H "Content-Type: application/json" -H "Accept: application/json" -H "Travis-API-Version: 3" -H "Authorization: token $env:TRAVIS_API_TOKEN" -d %MASTER_BRANCH_BODY% https://api.travis-ci.com/repo/markfinal%2Fbam-imageformats/requests
