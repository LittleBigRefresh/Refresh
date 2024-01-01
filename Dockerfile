# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0.100-1-bookworm-slim AS build
WORKDIR /build

COPY *.sln ./
COPY **/*.csproj ./

RUN dotnet sln list | grep ".csproj" \
    | while read -r line; do \
    mkdir -p $(dirname $line); \
    mv $(basename $line) $(dirname $line); \
    done;

# FIXME: .NET 8 started being dumb and requiring a runtime identifier for restores. This breaks non-amd64 caching. 
RUN dotnet restore --use-current-runtime -r linux-x64

COPY . .

RUN dotnet publish Refresh.GameServer -c Release --property:OutputPath=/build/publish/ --no-restore --use-current-runtime --self-contained

# Final running container

FROM mcr.microsoft.com/dotnet/runtime:8.0.0-bookworm-slim AS final

# Add non-root user
RUN set -eux && \
apt update && \
apt install -y gosu && \
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

ENTRYPOINT ["/refresh/docker-entrypoint.sh"]