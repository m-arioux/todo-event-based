#!/bin/bash

trap 'echo "Error occured. Exiting..."; exit 1' ERR

podman compose -f podman-compose.yml build
podman compose -f podman-compose.yml up kafka --wait
podman exec kafka opt/kafka/bin/kafka-topics.sh --create --topic todo --partitions 1 --replication-factor 1 --bootstrap-server kafka:9092
podman compose -f podman-compose.yml up --wait
echo "finished setup"