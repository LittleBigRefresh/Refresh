# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /build

COPY . .

# The same as what dotnet would use if nothing was specified at all. Refresh knows to ignore the version if it's 1.0.0.
ARG VERSION=1.0.0
RUN echo We will use the following semantic version for Presence: ${VERSION}

ARG BUILD_CONFIGURATION=Release
RUN dotnet publish Refresh.PresenceServer -c ${BUILD_CONFIGURATION} --property:OutputPath=/build/publish/ --no-self-contained /p:Version=${VERSION}

# Final running container

FROM mcr.microsoft.com/dotnet/runtime:9.0-alpine AS final

# Add non-root user
RUN set -eux && \
apk add --no-cache su-exec && \
su-exec nobody true && \
addgroup -g 1001 refresh && \
adduser -D -h /refresh -u 1001 -G refresh refresh && \
mkdir -p /refresh/data && \
mkdir -p /refresh/app

COPY --from=build /build/publish/publish /refresh/app
COPY --from=build /build/scripts/docker-entrypoint.sh /refresh

RUN chown -R refresh:refresh /refresh && \
chmod +x /refresh/docker-entrypoint.sh 

ENV PRIV_CMD=su-exec
ENV PRIV_USER=refresh:refresh

ENTRYPOINT ["/refresh/docker-entrypoint.sh", "PresenceServer"]
