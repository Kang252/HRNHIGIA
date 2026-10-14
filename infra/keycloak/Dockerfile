# Project-owned Keycloak image for local development.
# The realm import contains no production secret or user account.
FROM quay.io/keycloak/keycloak:26.8.0

COPY realm/ /opt/keycloak/data/import/

# The official image entrypoint invokes kc.sh.  A production web service must
# supply a subcommand; otherwise Keycloak prints its help and exits.
CMD ["start"]
