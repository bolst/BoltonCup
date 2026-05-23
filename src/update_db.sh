#!/bin/bash

CONTEXT="BoltonCupDbContext"
MIGRATION=""

for arg in "$@"; do
    if [ "$arg" = "--auth" ]; then
        CONTEXT="AuthDbContext"
    else
        MIGRATION="$arg"
    fi
done

if [ -z "$MIGRATION" ]; then
    echo "Updating $CONTEXT to the latest migration..."
    dotnet ef database update \
        --project ./BoltonCup.Infrastructure \
        --startup-project ./BoltonCup.WebAPI \
        -c "$CONTEXT"
else
    echo "Updating $CONTEXT to migration: '$MIGRATION'..."
    dotnet ef database update "$MIGRATION" \
        --project ./BoltonCup.Infrastructure \
        --startup-project ./BoltonCup.WebAPI \
        -c "$CONTEXT"
fi
