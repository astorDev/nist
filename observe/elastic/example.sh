cd server
docker compose up -d
cd ..

while true; do
    status_code=$(curl -o /dev/null -s -w "%{http_code}\n" -I http://localhost:5601)

    if [ "$status_code" -eq 302 ]; then
        echo "Received 302 status code. Breaking."
        break
    else
        echo "Current status code: $status_code. Waiting for 302..."
    fi

    sleep 2
done

cd shipper
docker compose up -d
cd ..

cd ../beavers
sh pump.sh
cd ../elastic

cd dashboards
export NIST_LOGS_PREFIX=cool-logs && sh all.sh
export NIST_LOGS_PREFIX=cool-logs && export NIST_IMPORTED_SERVICE=beavers-webapi && sh one.sh
cd ..