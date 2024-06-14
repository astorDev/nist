---
type: article
status: draft
---

# Filebeat and Elasticsearch for advanced Docker logs.

Elastic stack is one of the most robust observability systems out there. Application logs are most likely to be handled by docker. The most intuitive way to connect them I've found is Filebeat. So in this article, we will try to fire up a complete stack, that allows the export of logs from docker to elasticsearch in a manner that will lay a simple yet powerful foundation of an observability solution. So, start the beat!

![AI-generated log-lover ready to play a beat](docker-advanced-thumb.png)

## Firing up the foundations

Deploying and connecting all the basic components is a twisted task even in the most basic setup. Therefore, in my [previous article]() I've talked in depth about the basic setup. This time we'll start from what we got there. If something in the setup wouldn't make sense, feel free to refer to the [old piece](). Anyway, here's the initial setup we got:

`compose.yaml`

```yaml
services:
  elasticsearch:
    image: elasticsearch:7.17.3
    environment:
      - discovery.type=single-node
  
  kibana:
    image: kibana:7.17.3
    environment:
      - ELASTICSEARCH_HOSTS=http://elasticsearch:9200
    ports:
      - 5601:5601
  
  shipper:
    image: docker.elastic.co/beats/filebeat:8.14.0
    user: root
    volumes:
      - /var/lib/docker:/var/lib/docker:ro
      - ./filebeat.yml:/usr/share/filebeat/filebeat.yml
      - /var/run/docker.sock:/var/run/docker.sock
```

`filebeat.yaml`

```yaml
filebeat.inputs:
- type: container
  paths:
    - '/var/lib/docker/containers/*/*.log'

processors:
- add_docker_metadata:
    host: "unix:///var/run/docker.sock"

output.elasticsearch:
  hosts: elasticsearch:9200
  indices:
    - index: "docker-logs"
```

Which produce logs looking something like this:

```json
{
  "_index": "docker-logs",
  "_type": "_doc",
  "_id": "kafMFZABr1cUle7yj2na",
  "_version": 1,
  "_score": 1,
  "_ignored": [
    "message.keyword"
  ],
  "_source": {
    "@timestamp": "2024-06-14T08:10:41.019Z",
    "input": {
      "type": "container"
    },
    "ecs": {
      "version": "8.0.0"
    },
    "host": {
      "name": "56438defcbd0"
    },
    "agent": {
      "id": "5c4d1557-269c-49ff-a0b8-ac8915a6af8f",
      "name": "56438defcbd0",
      "type": "filebeat",
      "version": "8.14.0",
      "ephemeral_id": "d849fdeb-6afc-4a12-8242-0788015b2d44"
    },
    "container": {
      "id": "56438defcbd0d0bc1cfc28a3ae145a73e4745473ca0a0bc2af2f0f437c8bbbb2",
      "labels": {
        "version": "8.14.0",
        "org_opencontainers_image_version": "20.04",
        "org_label-schema_license": "Elastic License",
        "org_label-schema_version": "8.14.0",
        "io_k8s_description": "Filebeat sends log files to Logstash or directly to Elasticsearch.",
        "com_docker_compose_container-number": "1",
        "desktop_docker_io/binds/1/Source": "/Users/egortarasov/Pets/nist/observe/elastic/playground/docker-advanced/filebeat.yml",
        "url": "https://www.elastic.co/beats/filebeat",
        "org_label-schema_vendor": "Elastic",
        "org_label-schema_build-date": "2024-05-31T15:22:45Z",
        "org_label-schema_vcs-url": "github.com/elastic/beats/v7",
        "com_docker_compose_project": "docker-advanced",
        "org_opencontainers_image_vendor": "Elastic",
        "desktop_docker_io/binds/2/Target": "/var/run/docker.sock",
        "desktop_docker_io/binds/2/SourceKind": "dockerSocketProxied",
        "description": "Filebeat sends log files to Logstash or directly to Elasticsearch.",
        "desktop_docker_io/binds/1/Target": "/usr/share/filebeat/filebeat.yml",
        "org_opencontainers_image_title": "Filebeat",
        "com_docker_compose_version": "2.27.1",
        "release": "1",
        "name": "filebeat",
        "com_docker_compose_config-hash": "40bb1d7071e5df23cd55e3af6ffc532e09410a4c82c6182b6a967552f3e474cf",
        "com_docker_compose_project_working_dir": "/Users/egortarasov/Pets/nist/observe/elastic/playground/docker-advanced",
        "org_opencontainers_image_ref_name": "ubuntu",
        "maintainer": "infra@elastic.co",
        "vendor": "Elastic",
        "desktop_docker_io/binds/2/Source": "/var/run/docker.sock",
        "org_opencontainers_image_created": "2024-05-31T15:22:45Z",
        "org_label-schema_vcs-ref": "de52d1434ea3dff96953a59a18d44e456a98bd2f",
        "com_docker_compose_image": "sha256:f217457d9ebe8713742acb07e4209d8bf2b81298ff03277eaa28f098c93d2f12",
        "org_label-schema_name": "filebeat",
        "org_label-schema_schema-version": "1.0",
        "org_opencontainers_image_licenses": "Elastic License",
        "org_label-schema_url": "https://www.elastic.co/beats/filebeat",
        "io_k8s_display-name": "Filebeat image",
        "summary": "filebeat",
        "license": "Elastic License",
        "com_docker_compose_depends_on": "",
        "desktop_docker_io/binds/1/SourceKind": "hostFile",
        "com_docker_compose_service": "shipper",
        "com_docker_compose_project_config_files": "/Users/egortarasov/Pets/nist/observe/elastic/playground/docker-advanced/compose.yaml",
        "com_docker_compose_oneoff": "False"
      },
      "image": {
        "name": "docker.elastic.co/beats/filebeat:8.14.0"
      },
      "name": "docker-advanced-shipper-1"
    },
    "log": {
      "offset": 29401,
      "file": {
        "path": "/var/lib/docker/containers/56438defcbd0d0bc1cfc28a3ae145a73e4745473ca0a0bc2af2f0f437c8bbbb2/56438defcbd0d0bc1cfc28a3ae145a73e4745473ca0a0bc2af2f0f437c8bbbb2-json.log"
      }
    },
    "stream": "stderr",
    "message": "{\"log.level\":\"info\",\"@timestamp\":\"2024-06-14T08:10:41.016Z\",\"log.logger\":\"monitoring\",\"log.origin\":{\"function\":\"github.com/elastic/beats/v7/libbeat/monitoring/report/log.(*reporter).logSnapshot\",\"file.name\":\"log/log.go\",\"file.line\":187},\"message\":\"Non-zero metrics in the last 30s\",\"service.name\":\"filebeat\",\"monitoring\":{\"metrics\":{\"beat\":{\"cgroup\":{\"cpu\":{\"id\":\"/\"},\"memory\":{\"id\":\"/\",\"mem\":{\"usage\":{\"bytes\":98902016}}}},\"cpu\":{\"system\":{\"ticks\":120,\"time\":{\"ms\":120}},\"total\":{\"ticks\":640,\"time\":{\"ms\":640},\"value\":640},\"user\":{\"ticks\":520,\"time\":{\"ms\":520}}},\"handles\":{\"limit\":{\"hard\":1048576,\"soft\":1048576},\"open\":17},\"info\":{\"ephemeral_id\":\"d849fdeb-6afc-4a12-8242-0788015b2d44\",\"name\":\"filebeat\",\"uptime\":{\"ms\":30111},\"version\":\"8.14.0\"},\"memstats\":{\"gc_next\":42845240,\"memory_alloc\":22004248,\"memory_sys\":76190984,\"memory_total\":154130944,\"rss\":97427456},\"runtime\":{\"goroutines\":68}},\"filebeat\":{\"events\":{\"active\":22,\"added\":1234,\"done\":1212},\"harvester\":{\"open_files\":7,\"running\":7,\"started\":7}},\"libbeat\":{\"config\":{\"module\":{\"running\":0}},\"output\":{\"events\":{\"acked\":1205,\"active\":0,\"batches\":2,\"total\":1205},\"read\":{\"bytes\":9503,\"errors\":2},\"type\":\"elasticsearch\",\"write\":{\"bytes\":291196,\"latency\":{\"histogram\":{\"count\":2,\"max\":845,\"mean\":492.5,\"median\":492.5,\"min\":140,\"p75\":845,\"p95\":845,\"p99\":845,\"p999\":845,\"stddev\":352.5}}}},\"pipeline\":{\"clients\":1,\"events\":{\"active\":22,\"filtered\":7,\"published\":1227,\"retry\":3932,\"total\":1234},\"queue\":{\"acked\":1205,\"max_events\":3200}}},\"registrar\":{\"states\":{\"current\":7,\"update\":1212},\"writes\":{\"success\":2,\"total\":2}},\"system\":{\"cpu\":{\"cores\":2},\"load\":{\"1\":2.75,\"15\":0.22,\"5\":0.67,\"norm\":{\"1\":1.375,\"15\":0.11,\"5\":0.335}}}},\"ecs.version\":\"1.6.0\"}}"
  }
}
```

## Making them cool

## Recap

We've created a complete system for exporting docker logs to Elasticsearch. The system forms advanced index names, enabling separating logs by various index patterns. And perhaps even more importantly, the system exports not just raw log messages, but also fields of JSON logs. The export structure allows versatile filtering and dashboard building from the logs.

Thank you for reading! By the way... claps are appreciated 👉👈
