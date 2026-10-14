# Project-owned Keycloak image. The realm import is used only when local
# Compose overrides the command with start-dev --import-realm.
FROM quay.io/keycloak/keycloak:26.8.0

COPY infra/keycloak/realm/ /opt/keycloak/data/import/

# The official image entrypoint invokes kc.sh.  A production web service must
# supply a subcommand; otherwise Keycloak prints its help and exits.
CMD ["start"]
