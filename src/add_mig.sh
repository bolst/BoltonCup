#!/bin/bash

if [ -z "$1" ]; then
    echo "Error: Please provide a migration name."
    echo "Usage: ./add-mig.sh <MigrationName>"
    exit 1
fi

echo "Adding migration '$1' to BoltonCupDbContext..."

dotnet ef migrations add "$1" \
    --project ./BoltonCup.Infrastructure \
    --startup-project ./BoltonCup.WebAPI \
    -c BoltonCupDbContext
