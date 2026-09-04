#!/bin/bash

# Configuration
CONTAINER_NAME="boltoncup-postgres-1"
DB_USER="bcadmin"
DB_SUPERUSER="bcadmin"
DB_NAME="boltoncup"
BACKUP_DIR="/opt/BoltonCup/db_backups"
DATE=$(date +"%Y%m%d_%H%M%S")
DUMP_FILE="boltoncup_db_backup_$DATE.dump"
GLOBALS_FILE="boltoncup_globals_$DATE.sql"

# Check if the destination directory exists
if [ ! -d "$BACKUP_DIR" ]; then
  echo "Error: Backup directory $BACKUP_DIR does not exist."
  exit 1
fi

# Dump cluster-level globals (roles, tablespaces) — required for full restore
# Note: -t flag is intentionally omitted; it injects \r into the stream
docker exec $CONTAINER_NAME pg_dumpall -U $DB_SUPERUSER --globals-only > "$BACKUP_DIR/$GLOBALS_FILE"

if [ $? -ne 0 ]; then
  echo "Globals dump failed!"
  rm -f "$BACKUP_DIR/$GLOBALS_FILE"
  exit 1
fi

# Dump the database in custom format (supports selective restore via pg_restore)
docker exec $CONTAINER_NAME pg_dump -U $DB_USER -d $DB_NAME -F c > "$BACKUP_DIR/$DUMP_FILE"

if [ $? -eq 0 ]; then
  echo "Backup successful: $DUMP_FILE + $GLOBALS_FILE"
  find $BACKUP_DIR -type f \( -name "*.dump" -o -name "boltoncup_globals_*.sql" \) -mtime +7 -exec rm {} \;
  echo "Old backups pruned."
else
  echo "Database dump failed!"
  rm -f "$BACKUP_DIR/$DUMP_FILE" "$BACKUP_DIR/$GLOBALS_FILE"
  exit 1
fi

# To restore to a fresh PostgreSQL instance:
#   1. psql -U postgres -f boltoncup_globals_TIMESTAMP.sql
#   2. createdb -U postgres boltoncup
#   3. pg_restore -U bcadmin -d boltoncup -F c boltoncup_db_backup_TIMESTAMP.dump
#
# To restore using pgAdmin 4:
#   Step 1 — Restore roles (globals file):
#     - Connect to your target server in pgAdmin 4
#     - Open Tools > Query Tool
#     - Click the folder icon to open a file, select boltoncup_globals_TIMESTAMP.sql
#     - Press F5 (or click Execute) to run it — this recreates roles like bcadmin
#
#   Step 2 — Create the target database:
#     - In the browser panel, right-click Databases > Create > Database
#     - Set the name to "boltoncup" and owner to "bcadmin", then click Save
#
#   Step 3 — Restore the database dump:
#     - Right-click the newly created "boltoncup" database > Restore...
#     - Set Format to "Custom or tar"
#     - Under Filename, browse to and select boltoncup_db_backup_TIMESTAMP.dump
#     - Under the "Restore options" tab, enable "Pre-data", "Data", and "Post-data"
#     - Click Restore
