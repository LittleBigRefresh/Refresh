#!/usr/bin/env sh

# Docker volume mapping messes with folder ownership
# so it must be changed again during the entrypoint
chown -R refresh:refresh /refresh/data

cd /refresh/data || exit $?

exec $PRIV_CMD "$PRIV_USER" "/refresh/app/Refresh.$1"
exit $? # Expose error code