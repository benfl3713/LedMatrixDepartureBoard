.SILENT:
.ONESHELL:

HOST=ben@radarlights
DEPLOY_PATH=/home/ben/departureboard

.PHONY: build
build:
	dotnet publish -r linux-arm64 -c Release --self-contained

.PHONY: deploy
deploy:
	scp -r ./bin/Release/net7.0/linux-arm64/publish/* $(HOST):$(DEPLOY_PATH)


.PHONY: deploy_dll
deploy_dll:
	scp -r ./bin/Release/net7.0/linux-arm64/publish/LedMatrixDepartureBoard* $(HOST):$(DEPLOY_PATH)
	scp -r ./bin/Release/net7.0/linux-arm64/publish/appsettings* $(HOST):$(DEPLOY_PATH)
