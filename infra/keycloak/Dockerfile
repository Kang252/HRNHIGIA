# Build PostgreSQL and health support into the distribution so production
# startup does not run Quarkus augmentation on every container replacement.
FROM quay.io/keycloak/keycloak:26.8.1 AS builder

ENV KC_DB=postgres \
    KC_HEALTH_ENABLED=true

WORKDIR /opt/keycloak
RUN /opt/keycloak/bin/kc.sh build

FROM quay.io/keycloak/keycloak:26.8.1

COPY --from=builder /opt/keycloak/ /opt/keycloak/

# These non-secret build options must match the optimized distribution.
# Database credentials, hostname and proxy settings remain runtime secrets.
ENV KC_DB=postgres \
    KC_HEALTH_ENABLED=true

# The realm directory is imported only when local Compose overrides CMD with
# start-dev --import-realm. Production identity data stays in PostgreSQL.
COPY --chown=1000:0 infra/keycloak/realm/ /opt/keycloak/data/import/

ENTRYPOINT ["/opt/keycloak/bin/kc.sh"]
CMD ["start", "--optimized"]
