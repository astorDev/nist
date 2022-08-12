FROM docker.elastic.co/beats/filebeat:7.17.3
COPY --chown=root:filebeat filebeat.yml /usr/share/filebeat/filebeat.yml