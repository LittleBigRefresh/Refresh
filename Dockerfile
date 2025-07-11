# Build stage

ARG BUILD_CONFIGURATION=Release

FROM mcr.microsoft.com/dotnet/sdk:9.0-bookworm-slim AS build
WORKDIR /build

COPY *.sln ./
COPY **/*.csproj ./

RUN dotnet sln list | grep ".csproj" \
    | while read -r line; do \
    mkdir -p $(dirname $line); \
    mv $(basename $line) $(dirname $line); \
    done;

ARG BUILD_CONFIGURATION=Release

RUN dotnet restore --use-current-runtime /p:Configuration=${BUILD_CONFIGURATION}

COPY . .

RUN dotnet publish Refresh.GameServer -c ${BUILD_CONFIGURATION} --property:OutputPath=/build/publish/ --no-restore --no-self-contained

# Final running container

FROM mcr.microsoft.com/dotnet/runtime:9.0-bookworm-slim AS final

# Add non-root user
RUN set -eux && \
apt update && \
apt install -y gosu curl && \
rm -rf /var/lib/apt/lists/* && \
gosu nobody true && \
groupadd -g 1001 refresh && \
useradd -m --home /refresh -u 1001 -g refresh refresh &&\
mkdir -p /refresh/data && \
mkdir -p /refresh/app

COPY --from=build /build/publish/publish /refresh/app
COPY --from=build /build/scripts/docker-entrypoint.sh /refresh

RUN chown -R refresh:refresh /refresh && \
chmod +x /refresh/docker-entrypoint.sh 

ENV PRIV_CMD=gosu
ENV PRIV_USER=refresh

ENTRYPOINT ["/refresh/docker-entrypoint.sh", "GameServer"]
