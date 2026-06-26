#!/bin/bash

# Configuration
ES_URL="http://localhost:9200"
INDEX_NAME="products"
SCRIPT_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" &> /dev/null && pwd)

MAPPING_FILE="${SCRIPT_DIR}/mapping.json"

if [ ! -f "$MAPPING_FILE" ]; then
    echo "Error: mapping file missing at location ${MAPPING_FILE}! Stopping execution."
    return 1 2>/dev/null || exit 1
fi

echo "Checking if index '$INDEX_NAME' exists..."
STATUS=$(curl -o /dev/null -s -w "%{http_code}" "$ES_URL/$INDEX_NAME")

if [ "$STATUS" -eq 200 ]; then
  echo "Index exists. Deleting it..."
  curl -X DELETE "$ES_URL/$INDEX_NAME"
  echo -e "\nIndex deleted."
fi

echo "Creating new index '$INDEX_NAME' using $MAPPING_FILE..."
curl -X PUT "$ES_URL/$INDEX_NAME" \
     -H 'Content-Type: application/json' \
     -d @"$MAPPING_FILE"
if [ $? -eq 0 ]; then
    echo -e "\nIndex creation complete!"n
else
    echo "cannot create index"
fi
