#!/bin/bash

# Configuration
CONTAINER_NAME="ix-boltoncup-postgres-postgres-1"
DB_USER="bcadmin"
DB_NAME="boltoncup"
BACKUP_DIR="/mnt/seagate-2tb/db_backups"
DATE=$(date +"%Y%m%d_%H%M%S")
FILE_NAME="boltoncup_db_backup_$DATE.dump"

# Check if the destination directory exists
if [ ! -d "$BACKUP_DIR" ]; then
  echo "Error: Backup directory $BACKUP_DIR does not exist."
  exit 1
fi

# Run pg_dump inside the container and pipe it to the host directory
# We do not use the -f flag inside the container. We pipe standard output (>) to the host.
docker exec -t $CONTAINER_NAME pg_dump -U $DB_USER -d $DB_NAME -F c > "$BACKUP_DIR/$FILE_NAME"

# Check if the dump was successful before deleting old backups
if [ $? -eq 0 ]; then
  echo "Backup successful: $FILE_NAME"
  # Prune local backups older than 7 days
  find $BACKUP_DIR -type f -name "*.dump" -mtime +7 -exec rm {} \;
  echo "Old backups pruned."
else
  echo "Backup failed!"
  # Delete the empty/corrupted file that was just created
  rm -f "$BACKUP_DIR/$FILE_NAME"
  exit 1
fi

