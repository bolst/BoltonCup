cd ~/git/BoltonCup/BoltonCup/BoltonCup

dotnet clean

rm -rf bin/Publish
rm -rf bin/Release

dotnet publish -c Release -o ./bin/Publish

cd bin

zip -r -X ../bin/Publish.zip ./Publish/*

open .
