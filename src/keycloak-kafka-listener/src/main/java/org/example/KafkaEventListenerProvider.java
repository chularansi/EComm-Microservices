package org.example;

import org.apache.kafka.clients.producer.KafkaProducer;
import org.apache.kafka.clients.producer.ProducerRecord;
import org.keycloak.events.Event;
import org.keycloak.events.EventType;
import org.keycloak.events.EventListenerProvider;
import org.keycloak.events.admin.AdminEvent;
import org.keycloak.models.KeycloakSession;
import org.keycloak.models.RealmModel;
import org.keycloak.models.UserModel;

public class KafkaEventListenerProvider implements EventListenerProvider {

    private final KeycloakSession session;
    private final KafkaProducer<String, String> producer;
    private final String topic;

    public KafkaEventListenerProvider(KeycloakSession session, KafkaProducer<String, String> producer, String topic) {
        this.session = session;
        this.producer = producer;
        this.topic = topic;
    }

    @Override
    public void onEvent(Event event) {
        // Triggers when a user registers themselves on the public signup screen
        if (EventType.REGISTER.equals(event.getType())) {
            sendUserProfile(event.getUserId(), "USER_CREATED");
        }
        // 2. Capture Profile Updates made by the user themselves
        else if (EventType.UPDATE_PROFILE.equals(event.getType()) || EventType.UPDATE_EMAIL.equals(event.getType())) {
            sendUserProfile(event.getUserId(), "USER_UPDATED");
        }
    }

    @Override
    public void onEvent(AdminEvent adminEvent, boolean includeRepresentation) {
        String resourceType = adminEvent.getResourceType().toString();
        String operationType = adminEvent.getOperationType().toString();

        if ("USER".equals(resourceType)) {
            // Extract Target User ID from the resource path (e.g., "users/abcd-1234-efgh")
            String resourcePath = adminEvent.getResourcePath() != null ? adminEvent.getResourcePath() : "";
            String userId = resourcePath.contains("/") ? resourcePath.substring(resourcePath.lastIndexOf("/") + 1) : resourcePath;

            // 3. Admin creates a user
            if ("CREATE".equals(operationType)) {
                sendUserProfile(userId, "USER_CREATED");
            }
            // 4. Admin updates a user profile
            else if ("UPDATE".equals(operationType)) {
                sendUserProfile(userId, "USER_UPDATED");
            }
            // 5. Admin deletes a user completely
            else if ("DELETE".equals(operationType)) {
                sendUserDeletion(userId, adminEvent.getRealmId());
            }
        }
    }

    // Unified helper method to extract real user attributes from the active database session context
    private void sendUserProfile(String userId, String eventType) {
        RealmModel realm = session.getContext().getRealm();
        UserModel user = session.users().getUserById(realm, userId);

        if (user != null) {
            String firstName = user.getFirstName() != null ? user.getFirstName() : "";
            String lastName = user.getLastName() != null ? user.getLastName() : "";
            String email = user.getEmail() != null ? user.getEmail() : "";

            String message = String.format(
                    "{" +
                            "\"eventType\":\"%s\"," +
                            "\"userId\":\"%s\"," +
                            "\"details\":{" +
                            "\"first_name\":\"%s\"," +
                            "\"last_name\":\"%s\"," +
                            "\"email\":\"%s\"" +
                            "}" +
                            "}",
                    eventType, user.getId(), firstName, lastName, email
            );

            sendToKafka(realm.getId(), message);
        }
    }

    private void sendUserDeletion(String userId, String realmId) {
        // Send a minimal message payload since the user data no longer exists in Keycloak
        String message = String.format(
                "{\"eventType\":\"USER_DELETED\",\"userId\":\"%s\",\"details\":null}",
                userId
        );
        sendToKafka(realmId, message);
    }

    private void sendToKafka(String key, String message) {
        if (producer != null) {
            producer.send(new ProducerRecord<>(topic, key, message));
        }
    }

    @Override
    public void close() {}
}
