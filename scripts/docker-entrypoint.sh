#!/bin/sh

# Docker volume mapping messes with folder ownership
# so it must be changed again during the entrypoint
chown -R refresh:refresh /refresh/data

cd /refresh/data

exec gosu refresh /refresh/app/Refresh.GameServer

exit $? # Expose error code