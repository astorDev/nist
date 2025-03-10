docker compose up -d --build --pull always
sleep 5
curl localhost:1111/about
echo