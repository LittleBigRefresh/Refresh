# Build stage

FROM mcr.microsoft.com/dotnet/sdk:9.0-bookworm-slim AS build
WORKDIR /build

COPY . .

# The same as what dotnet would use if nothing was specified at all. Refresh knows to ignore the version if it's 1.0.0.
ARG VERSION=1.0.0
RUN echo We will use the following semantic version for Refresh: ${VERSION}

ARG BUILD_CONFIGURATION=Release
RUN dotnet publish Refresh.GameServer -c ${BUILD_CONFIGURATION} --property:OutputPath=/build/publish/ --no-self-contained /p:Version=${VERSION}

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
