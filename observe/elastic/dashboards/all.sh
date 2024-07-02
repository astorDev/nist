curl -s https://raw.githubusercontent.com/astorDev/nice-shell/main/.sh -o /tmp/nice-shell.sh
source /tmp/nice-shell.sh

log "Importing 'nisters' Dashboards to Kibana. (NIST_KIBANA_URL: '$NIST_KIBANA_URL', NIST_LOGS_PREFIX: '$NIST_LOGS_PREFIX')"
if [ -z "$NIST_KIBANA_URL" ]; then
    log "NIST_KIBANA_URL is not set. Using http://localhost:5601"
    NIST_KIBANA_URL=http://localhost:5601
fi

if [ -z "$NIST_LOGS_PREFIX" ]; then
    log "NIST_LOGS_PREFIX is not set. Using docker-logs"
    NIST_LOGS_PREFIX=docker-logs
fi

export COMMAND="httpyac send import.http --name all --var host=$NIST_KIBANA_URL prefix=$NIST_LOGS_PREFIX --output none"
log "Executing command: $COMMAND"
$COMMAND