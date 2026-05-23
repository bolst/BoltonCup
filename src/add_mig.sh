#!/bin/bash

if [ -z "$1" ]; then
    echo "Error: Please provide a migration name."
    echo "Usage: ./add_mig.sh <MigrationName> [--auth]"
    exit 1
fi

MIGRATION_NAME=$1
CONTEXT="BoltonCupDbContext"

if [ "$2" = "--auth" ]; then
    CONTEXT="AuthDbContext"
fi

echo "Adding migration '$MIGRATION_NAME' to $CONTEXT..."

dotnet ef migrations add "$MIGRATION_NAME" \
    --project ./BoltonCup.Infrastructure \
    --startup-project ./BoltonCup.WebAPI \
    -c "$CONTEXT"
