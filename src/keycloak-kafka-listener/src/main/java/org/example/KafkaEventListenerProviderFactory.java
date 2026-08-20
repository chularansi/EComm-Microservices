package org.example;

import org.apache.kafka.clients.producer.KafkaProducer;
import org.apache.kafka.clients.producer.ProducerConfig;
import org.apache.kafka.common.serialization.StringSerializer;
import org.keycloak.Config;
import org.keycloak.events.EventListenerProvider;
import org.keycloak.events.EventListenerProviderFactory;
import org.keycloak.models.KeycloakSession;
import org.keycloak.models.KeycloakSessionFactory;

import java.util.Properties;

public class KafkaEventListenerProviderFactory implements EventListenerProviderFactory {

    private KafkaProducer<String, String> producer;
    private String topic;

    public KafkaEventListenerProvider create(KeycloakSession session) {
        return new KafkaEventListenerProvider(session, producer, topic);
    }

    @Override
    public void init(Config.Scope config) {
        // Read configuration values from system environment variables
        String bootstrapServers = System.getenv().getOrDefault("KAFKA_BOOTSTRAP_SERVERS", "localhost:9092");
        this.topic = System.getenv().getOrDefault("KAFKA_TOPIC", "keycloak-events");

        Properties props = new Properties();
        props.put(ProducerConfig.BOOTSTRAP_SERVERS_CONFIG, bootstrapServers);
        props.put(ProducerConfig.KEY_SERIALIZER_CLASS_CONFIG, StringSerializer.class.getName());
        props.put(ProducerConfig.VALUE_SERIALIZER_CLASS_CONFIG, StringSerializer.class.getName());

        try {
            this.producer = new KafkaProducer<>(props);
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    @Override
    public void postInit(KeycloakSessionFactory factory) {}

    @Override
    public void close() {
        if (producer != null) {
            producer.close();
        }
    }

    @Override
    public String getId() {
        return "kafka-listener"; // This exact name will appear in Keycloak Admin Console
    }
}
