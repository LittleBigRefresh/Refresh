#!/bin/sh

chown -R refresh:refresh /refresh/data

if [ -d "/refresh/temp" ]; then
  cp -rf /refresh/temp/* /refresh/data
  rm -rf /refresh/temp
fi

# run from cmd

cd /refresh/data

exec su-exec refresh:refresh /refresh/app/Refresh."$SERVER"

exit $? # Expose error code
