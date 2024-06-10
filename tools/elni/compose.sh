docker compose -f base-compose.yml up -d

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

docker compose -f filebeat-compose.yml up -d
docker compose -f elnik-compose.yml up -d