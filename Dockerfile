# Build stage
FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
WORKDIR /app
COPY *.sln ./
COPY **/*.csproj ./

RUN dotnet sln list | grep ".csproj" \
    | while read -r line; do \
    mkdir -p $(dirname $line); \
    mv $(basename $line) $(dirname $line); \
    done;

RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# Final running container

FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS final

# Add non-root user
RUN addgroup -S refresh --gid 1001 && \
adduser -S refresh -G refresh -h /refresh --uid 1001 && \
mkdir -p /refresh/data && \
mkdir -p /refresh/app && \
mkdir -p /refresh/temp && \
apk add --no-cache icu-libs su-exec

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Copy build files
COPY --from=build /app/publish /refresh/app
COPY --from=build /app/scripts/docker-entrypoint.sh /refresh

RUN chown -R refresh:refresh /refresh && \
chmod +x /refresh/docker-entrypoint.sh

ENTRYPOINT ["/refresh/docker-entrypoint.sh"]
