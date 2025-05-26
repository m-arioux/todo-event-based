FROM apache/kafka:latest

ADD prom-jmx-agent-config.yml /usr/app/prom-jmx-agent-config.yml
ADD jmx_prometheus_javaagent-0.15.0.jar /usr/app/jmx_prometheus_javaagent.jar

# CMD ["/bin/bash", "-c", "opt/kafka/bin/kafka-storage.sh format --config", "/opt/kafka/config/server.properties", "--cluster-id", "${/opt/kafka/bin/kafka-storage.sh random-uuid}"]

# CMD ["/bin/bash", "-c", "/opt/kafka/bin/kafka-server-start.sh /opt/kafka/config/server.properties \
#  --override listener.name.plaintext.jmx.metrics.reporter.class=io.prometheus.jmx.JmxReporter \
#  --override KAFKA_OPTS='-javaagent:/usr/app/jmx_prometheus_javaagent.jar=7071:/usr/app/prom-jmx-agent-config.yml'"]
