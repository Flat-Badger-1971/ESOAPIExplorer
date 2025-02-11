if (Test-Path -Path .\ESOAPIExplorer\bin\publish) {
    Remove-Item -Recurse -Force .\ESOAPIExplorer\bin\publish
}
 dotnet publish ./ESOAPIExplorer/ESOAPIExplorer.csproj `
-c Release `
-p:Platform=x64 `
-p:PublishSingleFile=false `
--self-contained true `
-p:WindowsAppSDKSelfContained=true `
-p:WindowsPackageType=None `
-o .\ESOAPIExplorer\bin\publish\ `
-p:EnableMsixTooling=true `
-p:ErrorOnDuplicatePublishOutputFiles=false `
-P:PublishTrimmed=false
 explorer ".\ESOAPIExplorer\bin\publish"