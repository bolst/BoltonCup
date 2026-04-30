#!/bin/bash

if [ -z "$1" ]; then
    echo "Updating database to the latest migration..."
    dotnet ef database update \
        --project ./BoltonCup.Infrastructure \
        --startup-project ./BoltonCup.WebAPI \
        -c BoltonCupDbContext
else
    echo "Updating database to migration: '$1'..."
    dotnet ef database update "$1" \
        --project ./BoltonCup.Infrastructure \
        --startup-project ./BoltonCup.WebAPI \
        -c BoltonCupDbContext
fi
