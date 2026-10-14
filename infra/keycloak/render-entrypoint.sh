#!/bin/sh
set -eu

if [ "$#" -eq 0 ]; then
    set -- start
fi

# Tolerate a stale Render Docker Command after older image revisions pinned
# "start" in the entrypoint as well.
if [ "$#" -ge 2 ] && [ "$1" = "start" ] && [ "$2" = "start" ]; then
    shift
fi

printf '%s\n' "[nhigia-keycloak] launching Keycloak: $*"
exec /opt/keycloak/bin/kc.sh "$@"
