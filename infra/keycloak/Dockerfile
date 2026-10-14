# Project-owned Keycloak image. The realm import is used only when local
# Compose overrides the command with start-dev --import-realm.
FROM quay.io/keycloak/keycloak:26.8.0

COPY infra/keycloak/realm/ /opt/keycloak/data/import/

# Pin the production action in the entrypoint so this image cannot launch
# Keycloak without a subcommand and exit after printing its help.
ENTRYPOINT ["/opt/keycloak/bin/kc.sh", "start"]
