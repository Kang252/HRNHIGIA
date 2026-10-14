# Project-owned Keycloak image. The realm import is used only when local
# Compose overrides the command with start-dev --import-realm.
FROM quay.io/keycloak/keycloak:26.8.1

COPY infra/keycloak/realm/ /opt/keycloak/data/import/
COPY --chown=1000:0 --chmod=755 infra/keycloak/render-entrypoint.sh /opt/keycloak/bin/nhigia-entrypoint.sh

# The wrapper supplies "start" if the runtime clears CMD, while Compose can
# still replace CMD with start-dev --import-realm for local development.
ENTRYPOINT ["/opt/keycloak/bin/nhigia-entrypoint.sh"]
CMD ["start"]
