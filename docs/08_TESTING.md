# Testing-Strategie

## 1. Test-Pyramide

```
            ┌─────────┐
            │   E2E   │  10%  (Critical Workflows)
            └─────────┘
         ┌──────────────┐
         │ Integration  │  30%  (API, Database, RabbitMQ)
         └──────────────┘
    ┌────────────────────┐
    │    Unit Tests      │  60%  (Business Logic, Crypto)
    └────────────────────┘
```

## 2. Test-Ziele & Metriken

| Metrik | Zielwert | Priorität |
|--------|----------|-----------|
| **Code Coverage** | ≥ 80% | Hoch |
| **Crypto Code Coverage** | ≥ 90% | Kritisch |
| **API Integration Coverage** | ≥ 70% | Mittel |
| **E2E Critical Workflows** | 100% | Hoch |
| **Performance Tests** | Alle Crypto-Ops | Kritisch |

## 3. Unit Tests

### 3.1 Kryptographie-Tests (Highest Priority)

> **📋 Vollständige Code-Beispiele**: Siehe Repository `/tests/Crypto/`

**Test-Kategorien**:
- ✅ **Encrypt/Decrypt Round-trips**
- ✅ **Tampering Detection** (Modified Ciphertext/Tag)
- ✅ **Edge Cases** (Empty plaintext, verschiedene Sizes)
- ✅ **Security** (No plaintext leaks, secure RNG)
- ✅ **Performance** (< 100ms Layer 1, < 20ms Layer 2)

**Beispiel-Test-Struktur**:
```pseudocode
TEST_SUITE ChaChaPolyE2EServiceTests

    TEST EncryptDecrypt_ValidPlaintext_ReturnsOriginal
        // Arrange
        plaintext = "Secret message"
        recipientPublicKey = GENERATE_TEST_KEYPAIR().publicKey
        ownPrivateKey = GENERATE_TEST_KEYPAIR().privateKey
        
        // Act
        encrypted = ENCRYPT(plaintext, recipientPublicKey)
        decrypted = DECRYPT(encrypted, ownPrivateKey)
        
        // Assert
        ASSERT decrypted == plaintext
    END TEST
    
    TEST Encrypt_SamePlaintext_ProducesDifferentCiphertexts
        // Arrange
        plaintext = "Same message"
        recipientPublicKey = GENERATE_TEST_KEYPAIR().publicKey
        
        // Act
        ciphertext1 = ENCRYPT(plaintext, recipientPublicKey)
        ciphertext2 = ENCRYPT(plaintext, recipientPublicKey)
        
        // Assert
        ASSERT ciphertext1 != ciphertext2  // Different nonces
    END TEST
    
    TEST Decrypt_TamperedCiphertext_ThrowsCryptographicException
        // Arrange
        plaintext = "Secret"
        encrypted = ENCRYPT(plaintext, publicKey)
        
        // Act - Tamper with ciphertext
        encrypted.ciphertext[0] = encrypted.ciphertext[0] XOR 0xFF
        
        // Assert
        ASSERT_THROWS(DECRYPT(encrypted, privateKey), CryptographicException)
    END TEST
    
    TEST Decrypt_TamperedTag_ThrowsCryptographicException
        // Arrange
        plaintext = "Secret"
        encrypted = ENCRYPT(plaintext, publicKey)
        
        // Act - Tamper with authentication tag
        encrypted.tag[0] = encrypted.tag[0] XOR 0xFF
        
        // Assert
        ASSERT_THROWS(DECRYPT(encrypted, privateKey), CryptographicException)
    END TEST
    
    TEST EncryptDecrypt_VariousPlaintextSizes_WorksCorrectly
        PARAMETER sizes = [0, 1, 16, 256, 1024, 65536]
        
        FOR EACH size IN sizes DO
            // Arrange
            plaintext = GENERATE_RANDOM_STRING(size)
            
            // Act
            encrypted = ENCRYPT(plaintext, publicKey)
            decrypted = DECRYPT(encrypted, privateKey)
            
            // Assert
            ASSERT decrypted == plaintext
            ASSERT LENGTH(decrypted) == size
        END FOR
    END TEST

END TEST_SUITE
```

### 3.2 Service-Layer Tests

**Mocking-Pattern**:
```pseudocode
TEST_SUITE MessageServiceTests

    SETUP
        repositoryMock = CREATE_MOCK(IMessageRepository)
        eventPublisherMock = CREATE_MOCK(IEventPublisher)
        service = NEW MessageService(repositoryMock, eventPublisherMock)
    END SETUP

    TEST SendMessage_ValidMessage_ReturnsSuccess
        // Arrange
        message = {
            senderId: GUID_1,
            recipientId: GUID_2,
            content: "Encrypted content"
        }
        
        MOCK_RETURN(repositoryMock.Save, SUCCESS)
        MOCK_RETURN(eventPublisherMock.Publish, SUCCESS)
        
        // Act
        result = service.SendMessage(message)
        
        // Assert
        ASSERT result.success == TRUE
        VERIFY_CALLED(repositoryMock.Save, ONCE)
        VERIFY_CALLED(eventPublisherMock.Publish, ONCE)
    END TEST
    
    TEST GetMessages_ReturnsDecryptedMessages
        // Arrange
        encryptedMessages = [
            {id: GUID_1, encryptedContent: "ABC123"},
            {id: GUID_2, encryptedContent: "DEF456"}
        ]
        
        MOCK_RETURN(repositoryMock.GetMessages, encryptedMessages)
        
        // Act
        messages = service.GetMessages(userId: GUID_1)
        
        // Assert
        ASSERT LENGTH(messages) == 2
        VERIFY_CALLED(repositoryMock.GetMessages, ONCE)
    END TEST

END TEST_SUITE
```

> **📋 Weitere Service Tests**: Siehe Repository `/tests/Services/`

## 4. Integration Tests

### 4.1 API Integration Tests

**Test-Framework**: `WebApplicationFactory<Program>`

```pseudocode
TEST_SUITE MessageApiIntegrationTests

    SETUP
        testServer = CREATE_TEST_SERVER(Application)
        httpClient = testServer.CreateClient()
        authToken = GENERATE_TEST_AUTH_TOKEN()
    END SETUP

    TEST SendMessage_AuthenticatedUser_ReturnsCreated
        // Arrange
        request = {
            method: POST,
            url: "/api/messages",
            headers: {Authorization: "Bearer " + authToken},
            body: {recipientId: GUID_2, content: "Encrypted"}
        }
        
        // Act
        response = httpClient.Send(request)
        
        // Assert
        ASSERT response.status == 201  // Created
        ASSERT response.body.id IS NOT NULL
    END TEST
    
    TEST GetMessages_ValidConversation_ReturnsMessages
        // Arrange
        contactId = GUID_2
        
        // Act
        response = httpClient.GET("/api/messages/" + contactId, {
            headers: {Authorization: "Bearer " + authToken}
        })
        
        // Assert
        ASSERT response.status == 200  // OK
        ASSERT response.body IS ARRAY
    END TEST
    
    TEST SendMessage_Unauthenticated_ReturnsUnauthorized
        // Arrange
        request = {
            method: POST,
            url: "/api/messages",
            headers: {},  // No auth token
            body: {recipientId: GUID_2, content: "Test"}
        }
        
        // Act
        response = httpClient.Send(request)
        
        // Assert
        ASSERT response.status == 401  // Unauthorized
    END TEST

END TEST_SUITE
```

### 4.2 Database Integration Tests

**Test-DB**: In-Memory (für schnelle Tests) oder Testcontainers (für realistische Tests)

```pseudocode
TEST_SUITE MessageRepositoryIntegrationTests

    SETUP
        testDatabase = CREATE_IN_MEMORY_DATABASE()
        repository = NEW MessageRepository(testDatabase)
    END SETUP
    
    TEARDOWN
        testDatabase.DISPOSE()
    END TEARDOWN

    TEST CreateMessage_ValidMessage_StoresInDatabase
        // Arrange
        message = {
            id: GENERATE_GUID(),
            senderId: GUID_1,
            recipientId: GUID_2,
            encryptedContent: "ABC",
            timestamp: CURRENT_TIMESTAMP()
        }
        
        // Act
        repository.Create(message)
        stored = repository.GetById(message.id)
        
        // Assert
        ASSERT stored IS NOT NULL
        ASSERT stored.id == message.id
        ASSERT stored.encryptedContent == message.encryptedContent
    END TEST
    
    TEST GetMessages_FiltersDeletedMessages
        // Arrange
        message1 = CREATE_TEST_MESSAGE(deleted: FALSE)
        message2 = CREATE_TEST_MESSAGE(deleted: TRUE)
        
        repository.Create(message1)
        repository.Create(message2)
        
        // Act
        messages = repository.GetMessages(userId: GUID_1)
        
        // Assert
        ASSERT LENGTH(messages) == 1
        ASSERT messages[0].id == message1.id
    END TEST

END TEST_SUITE
```

### 4.3 RabbitMQ Integration Tests

```pseudocode
TEST_SUITE MessageQueueIntegrationTests

    SETUP
        rabbitMQ = CREATE_TEST_RABBITMQ_CONTAINER()
        publisher = NEW EventPublisher(rabbitMQ)
        consumer = NEW EventConsumer(rabbitMQ)
    END SETUP
    
    TEARDOWN
        rabbitMQ.STOP()
    END TEARDOWN

    TEST PublishMessageSentEvent_ConsumerReceivesEvent
        // Arrange
        event = {
            type: "MessageSent",
            messageId: GENERATE_GUID(),
            senderId: GUID_1,
            recipientId: GUID_2
        }
        
        receivedEvent = NULL
        consumer.Subscribe("MessageSent", LAMBDA(e) {
            receivedEvent = e
        })
        
        // Act
        publisher.Publish(event)
        WAIT_FOR(5 seconds, UNTIL receivedEvent IS NOT NULL)
        
        // Assert
        ASSERT receivedEvent IS NOT NULL
        ASSERT receivedEvent.messageId == event.messageId
    END TEST

END TEST_SUITE
```

## 5. End-to-End Tests

### 5.1 Complete Message Flow

```pseudocode
TEST_SUITE E2EMessageFlowTests

    TEST CompleteMessageFlow_AliceToBob_Success
        // 1. Alice registers and logs in
        aliceToken = REGISTER_AND_LOGIN("alice@example.com", "AlicePass123!")
        
        // 2. Bob registers and logs in
        bobToken = REGISTER_AND_LOGIN("bob@example.com", "BobPass123!")
        
        // 3. Alice encrypts and sends message
        bobPublicKey = GET_PUBLIC_KEY(bobToken)
        encryptedMessage = ENCRYPT("Hello Bob!", bobPublicKey)
        
        messageResponse = POST("/api/messages", {
            recipientId: bobUserId,
            encryptedContent: encryptedMessage
        }, aliceToken)
        
        ASSERT messageResponse.status == 201
        messageId = messageResponse.body.id
        
        // 4. Bob receives and decrypts message
        messages = GET("/api/messages/" + aliceUserId, bobToken)
        ASSERT LENGTH(messages) >= 1
        
        receivedMessage = messages[0]
        decrypted = DECRYPT(receivedMessage.encryptedContent, bobPrivateKey)
        ASSERT decrypted == "Hello Bob!"
        
        // 5. Bob marks as read
        PATCH("/api/messages/" + messageId + "/read", {}, bobToken)
        
        // 6. Alice erhält Lesebestätigung
        WAIT_FOR_SIGNALR_EVENT("MessageRead", aliceConnection)
        
        updatedMessage = GET("/api/messages/" + messageId, aliceToken)
        ASSERT updatedMessage.status == "Read"
    END TEST

END TEST_SUITE
```

### 5.2 UI Automation Tests (Selenium - Optional)

```pseudocode
TEST_SUITE LoginUITests

    SETUP
        driver = CREATE_WEBDRIVER("Chrome")
        driver.NAVIGATE_TO("http://localhost:5000")
    END SETUP
    
    TEARDOWN
        driver.QUIT()
    END TEARDOWN

    TEST Login_ValidCredentials_RedirectsToChat
        // Arrange
        usernameField = driver.FIND_ELEMENT(By.Id("username"))
        passwordField = driver.FIND_ELEMENT(By.Id("password"))
        loginButton = driver.FIND_ELEMENT(By.Id("loginButton"))
        
        // Act
        usernameField.SEND_KEYS("testuser")
        passwordField.SEND_KEYS("TestPass123!")
        loginButton.CLICK()
        
        // Assert
        WAIT_UNTIL(driver.URL CONTAINS "/chat")
        ASSERT driver.FIND_ELEMENT(By.Id("chatWindow")) IS VISIBLE
    END TEST
    
    TEST SendMessage_TypeAndSend_MessageAppears
        // Arrange
        LOGIN_AS("testuser")
        
        messageInput = driver.FIND_ELEMENT(By.Id("messageInput"))
        sendButton = driver.FIND_ELEMENT(By.Id("sendButton"))
        
        // Act
        messageInput.SEND_KEYS("Test message")
        sendButton.CLICK()
        
        // Assert
        WAIT_UNTIL_VISIBLE(By.Class("message-item"))
        messages = driver.FIND_ELEMENTS(By.Class("message-item"))
        ASSERT LENGTH(messages) >= 1
        ASSERT messages[LAST].TEXT CONTAINS "Test message"
    END TEST

END TEST_SUITE
```

> **Note**: UI Automation ist optional, da WPF Desktop-App. Fokus auf Unit + Integration Tests.

## 6. Security Tests

### 6.1 Penetration Testing Checklist

```pseudocode
TEST_SUITE SecurityTests

    TEST API_SQLInjection_IsProtected
        // Arrange
        maliciousInput = "'; DROP TABLE users; --"
        
        // Act
        response = API_POST("/api/users/search", {query: maliciousInput})
        
        // Assert
        ASSERT response.status == 400  // Bad Request
        ASSERT database.TABLE_EXISTS("users")  // Table still exists
    END TEST
    
    TEST API_XSS_IsSanitized
        // Arrange
        maliciousScript = "<script>alert('XSS')</script>"
        
        // Act
        response = API_POST("/api/messages", {content: maliciousScript})
        message = API_GET("/api/messages/{id}")
        
        // Assert
        ASSERT NOT CONTAINS(message.content, "<script>")
        ASSERT message.content == "&lt;script&gt;alert('XSS')&lt;/script&gt;"
    END TEST
    
    TEST Authentication_BruteForce_IsRateLimited
        // Arrange
        attempts = 0
        
        // Act
        FOR i = 1 TO 10 DO
            response = API_POST("/api/auth/login", {
                username: "test",
                password: "wrong"
            })
            
            IF response.status == 429 THEN  // Too Many Requests
                attempts = i
                BREAK
            END IF
        END FOR
        
        // Assert
        ASSERT attempts <= 5  // Rate limit after max 5 attempts
    END TEST

END TEST_SUITE
```

### 6.2 Crypto Security Tests

```pseudocode
TEST_SUITE CryptoSecurityTests

    TEST KeyGeneration_UsesSecureRNG
        // Arrange
        keys = []
        
        // Act
        FOR i = 1 TO 100 DO
            key = GENERATE_ENCRYPTION_KEY()
            keys.APPEND(key)
        END FOR
        
        // Assert - Check for randomness
        uniqueKeys = SET(keys)
        ASSERT LENGTH(uniqueKeys) == 100  // All keys unique
        
        // Statistical randomness test
        chiSquared = PERFORM_CHI_SQUARED_TEST(keys)
        ASSERT chiSquared < CRITICAL_VALUE  // Random distribution
    END TEST
    
    TEST PrivateKeys_NeverLoggedOrExposed
        // This is a CODE REVIEW checkpoint, not automated test
        
        // Manual checklist:
        // ✓ Check logs for key material
        // ✓ Check error messages for key exposure
        // ✓ Check API responses for private keys
        // ✓ Check database for plaintext private keys
        
        MANUAL_CHECK("Review code for key exposure")
    END TEST

END TEST_SUITE
```

## 7. Performance Tests

### 7.1 Crypto Performance

```pseudocode
TEST_SUITE CryptoPerformanceTests

    TEST EncryptionPerformance_CompletesWithin100ms
        // Arrange
        plaintext = "Test message for encryption"
        recipientPublicKey = LOAD_TEST_PUBLIC_KEY()
        
        // Act
        startTime = CURRENT_TIME_MS()
        encrypted = ENCRYPT_MESSAGE(plaintext, recipientPublicKey)
        endTime = CURRENT_TIME_MS()
        
        duration = endTime - startTime
        
        // Assert
        ASSERT duration < 100  // Less than 100ms
        ASSERT encrypted IS NOT NULL
    END TEST
    
    TEST Layer2Encryption_CompletesWithin10ms
        // Arrange
        plaintext = "Test message"
        masterKey = DERIVE_MASTER_KEY("TestPassword123!", userSalt)
        
        // Act
        startTime = CURRENT_TIME_MS()
        encrypted = LOCAL_STORAGE_ENCRYPT(plaintext, masterKey)
        endTime = CURRENT_TIME_MS()
        
        duration = endTime - startTime
        
        // Assert
        ASSERT duration < 10  // Less than 10ms (Layer 2 Password-based Encryption)
    END TEST
    
    TEST FullEncryptionStack_CompletesWithin10ms
        // Arrange
        plaintext = "Test message"
        sender = LOAD_TEST_USER("Alice")
        recipient = LOAD_TEST_USER("Bob")
        
        // Act
        startTime = CURRENT_TIME_MS()
        
        // Layer 1: E2E Transport Encryption
        encrypted = LAYER1_ENCRYPT(plaintext, recipient.publicKey)
        
        // Layer 2: Local Storage (nur bei Cache)
        cached = LOCAL_STORAGE_ENCRYPT(plaintext, sender.masterKey)
        
        endTime = CURRENT_TIME_MS()
        
        duration = endTime - startTime
        
        // Assert
        ASSERT duration < 10  // Less than 10ms total (Layer 1+2 combined)
    END TEST

END TEST_SUITE
```

### 7.2 Load Testing (K6 - Optional)

```pseudocode
// Load Testing Script Konzept
// Note: Echte Load Tests verwenden spezialisierte Tools (K6, JMeter, etc.)

CONFIGURE LoadTest AS
    stages = [
        {duration: 2 minutes, targetUsers: 100},   // Ramp up to 100 users
        {duration: 5 minutes, targetUsers: 100},   // Stay at 100 users
        {duration: 2 minutes, targetUsers: 0}      // Ramp down to 0
    ]
    
    thresholds = {
        http_request_duration_p95: 500ms,  // 95% of requests < 500ms
        http_request_failed_rate: 1%       // Error rate < 1%
    }
END CONFIGURE

FUNCTION LoadTestScenario()
    // Send message endpoint
    EXECUTE HTTP_POST(
        url: "http://localhost:5000/api/messages",
        body: {
            recipientId: TEST_USER_ID,
            content: ENCRYPTED_TEST_MESSAGE
        },
        headers: {
            "Content-Type": "application/json",
            "Authorization": "Bearer " + TEST_TOKEN
        }
    )
    
    // Verify response
    ASSERT response.status == 201
    ASSERT response.time < 500ms
END FUNCTION
```

> **📋 Vollständiges Load-Testing-Setup**: Siehe Repository `/tests/load-testing/`

## 8. Test Data & Fixtures

### 8.1 Test Data Builder Pattern

```pseudocode
CLASS TestDataBuilder

    STATIC FUNCTION BuildUser(username = "testuser")
        RETURN User {
            id: GENERATE_GUID(),
            username: username,
            email: username + "@example.com",
            passwordHash: HASH_PASSWORD("TestPass123!"),
            masterKeySalt: RANDOM_BYTES(32),
            createdAt: CURRENT_TIMESTAMP()
        }
    END FUNCTION
    
    STATIC FUNCTION BuildMessage(senderId, recipientId)
        RETURN Message {
            id: GENERATE_GUID(),
            senderId: senderId,
            recipientId: recipientId,
            encryptedContent: ENCRYPT("Test message"),
            timestamp: CURRENT_TIMESTAMP(),
            status: MessageStatus.Sent
        }
    END FUNCTION
    
    STATIC FUNCTION BuildPublicKey(userId, keyType = "x25519_e2e")
        RETURN PublicKey {
            id: GENERATE_GUID(),
            userId: userId,
            publicKeyBytes: GENERATE_PUBLIC_KEY(),
            keyType: keyType,
            createdAt: CURRENT_TIMESTAMP(),
            isActive: TRUE
        }
    END FUNCTION

END CLASS
```

**Verwendung in Tests**:
```pseudocode
TEST TestUserRegistration
    // Arrange
    testUser = TestDataBuilder.BuildUser("alice")
    
    // Act
    result = AuthService.RegisterUser(testUser)
    
    // Assert
    ASSERT result.success == TRUE
END TEST
```

## 9. Continuous Testing (CI/CD)

### 9.1 GitHub Actions Workflow

```pseudocode
// CI/CD Pipeline Configuration (Konzeptuell)

PIPELINE TestSuite
    TRIGGER on: [push, pull_request]
    
    JOB UnitTests
        RUN_ON: ubuntu-latest
        STEPS:
            1. CHECKOUT_CODE()
            2. SETUP_DOTNET(version: "8.0.x")
            3. RUN_TESTS(
                filter: "Category=Unit",
                collectCoverage: true
            )
            4. UPLOAD_COVERAGE_REPORT()
    END JOB
    
    JOB IntegrationTests
        RUN_ON: ubuntu-latest
        SERVICES:
            - PostgreSQL (image: postgres:16, password: test)
            - RabbitMQ (image: rabbitmq:3-management)
            - Redis (image: redis:7-alpine)
        
        STEPS:
            1. CHECKOUT_CODE()
            2. RUN_TESTS(filter: "Category=Integration")
    END JOB
    
    JOB SecurityTests
        RUN_ON: ubuntu-latest
        STEPS:
            1. CHECKOUT_CODE()
            2. RUN_SECURITY_SCAN()
            3. CHECK_VULNERABLE_DEPENDENCIES()
    END JOB
END PIPELINE
```

**CI/CD Best Practices**:
- ✅ Alle Tests müssen bestehen vor Merge
- ✅ Coverage darf nicht sinken
- ✅ Security Scans bei jedem Push
- ✅ Dependency Updates automatisch
