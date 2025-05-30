#!/bin/bash

trap 'echo "Error occured. Exiting..."; exit 1' ERR

podman compose -f podman-compose.yml build
podman compose -f podman-compose.yml up kafka --wait
podman compose -f podman-compose.yml up kafka-cli --wait
podman compose -f podman-compose.yml up --wait
echo "finished setup"