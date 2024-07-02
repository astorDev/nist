curl -s https://raw.githubusercontent.com/astorDev/nice-shell/main/.sh -o /tmp/nice-shell.sh
source /tmp/nice-shell.sh

log "Importing Nist Service Dashboards to Kibana. (NIST_KIBANA_URL: '$NIST_KIBANA_URL', NIST_LOGS_PREFIX: '$NIST_LOGS_PREFIX', NIST_IMPORTED_SERVICE: '$NIST_IMPORTED_SERVICE')"

if [ -z "$NIST_IMPORTED_SERVICE" ]; then
    throw "NIST_IMPORTED_SERVICE is not set"
fi

if [ -z "$NIST_KIBANA_URL" ]; then
    log "NIST_KIBANA_URL is not set. Using http://localhost:5601"
    NIST_KIBANA_URL=http://localhost:5601
fi

if [ -z "$NIST_LOGS_PREFIX" ]; then
    log "NIST_LOGS_PREFIX is not set. Using docker-logs"
    NIST_LOGS_PREFIX=docker-logs
fi

export COMMAND="httpyac send .http --name one --var host=$NIST_KIBANA_URL prefix=$NIST_LOGS_PREFIX service=$NIST_IMPORTED_SERVICE --output none"
log "Executing command: $COMMAND"
$COMMAND