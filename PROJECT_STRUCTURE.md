# Secure Messenger - Projektstruktur

**Workspace:** I:\Just_for_fun\Messenger\
**Generiert:** 2026-01-27 08:35:11
**Version:** 9.2.2
**Branch:** master

---

## Solution-Uebersicht

**13 Projekte:**

### Backend Services (9)
- AuthService - Authentication, JWT, MFA (Port 5001)
- MessageService - Encrypted Messaging (Port 5002)
- UserService - User Profile Management (Port 5003)
- NotificationService - Push Notifications (Port 5004)
- CryptoService - Cryptographic Operations (Port 5005)
- KeyManagementService - Key Rotation (Port 5006)
- AuditLogService - DSGVO Audit Logs (Port 5007)
- FileTransferService - Secure File Sharing (Port 5008)
- GatewayService - API Gateway (Ocelot, Port 7001)

### Shared Libraries (2)
- MessengerContracts - DTOs & Interfaces
- MessengerCommon - Extensions & Helpers

### Frontend (1)
- MessengerClient - WPF Desktop App (MVVM, Material Design)

### Tests (1)
- MessengerTests - Unit & Integration Tests (173/178 passing)

---

## Projektstruktur

```
Messenger/
|-- [DIR] docs/
|   |-- [DIR] archive/
|   |   |-- [DIR] MessengerTests.E2E/
|   |   |   |-- [CS] LoginFlowTests.cs
|   |   |   |-- [CS] MessageFlowTests.cs
|   |   |   +-- [PROJ] MessengerTests.E2E.csproj
|   |   |-- [DIR] old-scripts/
|   |   |-- [MD] DEPLOYMENT_READY_v10.1.md
|   |   |-- [MD] FINAL_REPORT_v10.md
|   |   |-- [MD] FIXES_SUMMARY_v10.1.1.md
|   |   |-- [MD] README.md
|   |   |-- [MD] SUMMARY_v10.1.md
|   |   |-- [MD] VERSION_8.0_SUMMARY.md
|   |   +-- [MD] VERSION_8.1_PRODUCTION_READY.md
|   |-- [DIR] diagrams/
|   |   |-- [DIR] PNG/
|   |   +-- [MD] DIAGRAM_REFERENCE.md
|   |-- [DIR] fixes/
|   |   |-- [MD] CONFIGURATION_SYNC_REPORT.md
|   |   |-- [MD] MFA_CONTROLLER_SECURITY_FIXES.md
|   |   |-- [MD] MFA_FINAL_REPORT.md
|   |   +-- [MD] MFA_INTEGRATION_TESTS_REPORT.md
|   |-- [DIR] frontend/
|   |   |-- [MD] AGENT_PROMPT_XAML.md
|   |   |-- [MD] DTO_MAPPING.md
|   |   |-- [MD] QUICK_START.md
|   |   |-- [MD] README.md
|   |   +-- [MD] README_IMPLEMENTATION.md
|   |-- [DIR] guides/
|   |   |-- [MD] DEPLOYMENT_GUIDE.md
|   |   |-- [MD] PROJECT_STRUCTURE.md
|   |   +-- [MD] WORKSPACE_GUIDE.md
|   |-- [DIR] releases/
|   |   +-- [MD] VERSION_9.0_COMPLETE.md
|   |-- [DIR] reports/
|   |   +-- [MD] CODE_AUDIT_REPORT.md
|   |-- [MD] 00_INDEX.md
|   |-- [MD] 02_ARCHITECTURE.md
|   |-- [MD] 03_CRYPTOGRAPHY.md
|   |-- [MD] 04_USE_CASES.md
|   |-- [MD] 05_DATA_MODEL.md
|   |-- [MD] 06_MULTI_FACTOR_AUTHENTICATION.md
|   |-- [MD] 07_IMPLEMENTATION_PLAN.md
|   |-- [MD] 08_TESTING.md
|   |-- [MD] 09_API_REFERENCE.md
|   |-- [MD] COMMIT_TEMPLATE.md
|   |-- [MD] CRYPTO_API_REFERENCE.md
|   |-- [MD] DATABASE_MIGRATIONS.md
|   |-- [MD] DOCUMENTATION_CHANGELOG.md
|   |-- [MD] DOCUMENTATION_CLEANUP_PLAN.md
|   |-- [MD] GROUP_CHAT_API.md
|   |-- [MD] PRODUCTION_DEPLOYMENT.md
|   |-- [MD] RATE_LIMITING.md
|   |-- [MD] README.md
|   +-- [MD] WINDOWS_DEPLOYMENT.md
|-- [DIR] nginx/
|   +-- [DIR] logs/
|-- [DIR] ProjectProposal/
|   +-- [MD] 01_PROJECT_PROPOSAL.md
|-- [DIR] scripts/
|   |-- [DIR] archive/
|   |   |-- [PS1] check-docker.ps1
|   |   |-- [PS1] check-docker-nonadmin.ps1
|   |   |-- [PS1] docker-troubleshoot.ps1
|   |   |-- [PS1] master-setup.ps1
|   |   |-- [PS1] master-setup-fixed.ps1
|   |   |-- [PS1] setup-docker.ps1
|   |   |-- [PS1] setup-local-dev.ps1
|   |   |-- [PS1] simple-setup.ps1
|   |   +-- [PS1] test-docker.ps1
|   |-- [DIR] batch/
|   |   |-- [BAT] cleanup.bat
|   |   |-- [BAT] setup.bat
|   |   |-- [BAT] status.bat
|   |   +-- [BAT] test.bat
|   |-- [DIR] powershell/
|   |   |-- [PS1] Remove-DockerResources.ps1
|   |   |-- [PS1] Setup-Docker.ps1
|   |   |-- [PS1] Show-DockerStatus.ps1
|   |   +-- [PS1] Test-Docker.ps1
|   |-- [DIR] windows/
|   |   |-- [PS1] backup-database.ps1
|   |   |-- [PS1] Deploy-Production.ps1
|   |   |-- [PS1] generate-secrets.ps1
|   |   |-- [PS1] Live-Integration-Test.ps1
|   |   |-- [MD] README.md
|   |   |-- [PS1] Test-Deployment.ps1
|   |   +-- [PS1] Test-Production.ps1
|   |-- [SH] deploy-production.sh
|   |-- [MD] QUICK_REFERENCE.md
|   +-- [MD] README.md
|-- [DIR] src/
|   |-- [DIR] Backend/
|   |   |-- [DIR] AuditLogService/
|   |   |   |-- [DIR] Controllers/
|   |   |   |   +-- [CS] AuditController.cs
|   |   |   |-- [DIR] Data/
|   |   |   |   |-- [DIR] Entities/
|   |   |   |   |   +-- [CS] AuditLog.cs
|   |   |   |   +-- [CS] AuditDbContext.cs
|   |   |   |-- [DIR] Migrations/
|   |   |   |   |-- [CS] 20260109123625_InitialCreate.cs
|   |   |   |   |-- [CS] 20260109123625_InitialCreate.Designer.cs
|   |   |   |   +-- [CS] AuditDbContextModelSnapshot.cs
|   |   |   |-- [DIR] Properties/
|   |   |   |   +-- [JSON] launchSettings.json
|   |   |   |-- [DIR] Services/
|   |   |   |   +-- [CS] AuditLogService.cs
|   |   |   |-- [JSON] appsettings.json
|   |   |   |-- [PROJ] AuditLogService.csproj
|   |   |   +-- [CS] Program.cs
|   |   |-- [DIR] AuthService/
|   |   |   |-- [DIR] Controllers/
|   |   |   |   |-- [CS] AuthController.cs
|   |   |   |   +-- [CS] MFAController.cs
|   |   |   |-- [DIR] Data/
|   |   |   |   |-- [DIR] Entities/
|   |   |   |   |   |-- [CS] MfaMethod.cs
|   |   |   |   |   |-- [CS] RecoveryCode.cs
|   |   |   |   |   +-- [CS] User.cs
|   |   |   |   |-- [DIR] Migrations/
|   |   |   |   |   +-- [CS] 20260120000002_AddPerformanceIndexes.cs
|   |   |   |   +-- [CS] AuthDbContext.cs
|   |   |   |-- [DIR] DTOs/
|   |   |   |-- [DIR] Migrations/
|   |   |   |   |-- [CS] 20260108140721_InitialCreate.cs
|   |   |   |   |-- [CS] 20260108140721_InitialCreate.Designer.cs
|   |   |   |   +-- [CS] AuthDbContextModelSnapshot.cs
|   |   |   |-- [DIR] Properties/
|   |   |   |   +-- [JSON] launchSettings.json
|   |   |   |-- [DIR] Services/
|   |   |   |   |-- [CS] Argon2PasswordHasher.cs
|   |   |   |   |-- [CS] MFAService.cs
|   |   |   |   +-- [CS] TokenService.cs
|   |   |   |-- [DIR] Validators/
|   |   |   |   |-- [CS] LoginRequestValidator.cs
|   |   |   |   |-- [CS] RefreshTokenRequestValidator.cs
|   |   |   |   |-- [CS] RegisterRequestValidator.cs
|   |   |   |   |-- [CS] VerifyMfaRequestValidator.cs
|   |   |   |   +-- [CS] VerifyTotpSetupRequestValidator.cs
|   |   |   |-- [JSON] appsettings.Development.json
|   |   |   |-- [JSON] appsettings.json
|   |   |   |-- [JSON] appsettings.Production.json
|   |   |   |-- [PROJ] AuthService.csproj
|   |   |   |-- [CS] EnvironmentValidator.cs
|   |   |   |-- [CS] Program.cs
|   |   |   +-- [MD] README.md
|   |   |-- [DIR] CryptoService/
|   |   |   |-- [DIR] Controllers/
|   |   |   |   +-- [CS] CryptoController.cs
|   |   |   |-- [DIR] Layer1/
|   |   |   |   |-- [CS] ChaCha20Poly1305Encryption.cs
|   |   |   |   +-- [CS] TransportEncryptionService.cs
|   |   |   |-- [DIR] Layer2/
|   |   |   |   +-- [CS] LocalStorageEncryptionService.cs
|   |   |   |-- [DIR] Layer3/
|   |   |   |   +-- [CS] DisplayEncryptionService.cs
|   |   |   |-- [DIR] Services/
|   |   |   |   +-- [CS] GroupEncryptionService.cs
|   |   |   |-- [JSON] appsettings.json
|   |   |   |-- [PROJ] CryptoService.csproj
|   |   |   +-- [CS] Program.cs
|   |   |-- [DIR] FileTransferService/
|   |   |   |-- [DIR] Controllers/
|   |   |   |   +-- [CS] FilesController.cs
|   |   |   |-- [DIR] Data/
|   |   |   |   |-- [DIR] Entities/
|   |   |   |   |   +-- [CS] FileMetadata.cs
|   |   |   |   +-- [CS] FileDbContext.cs
|   |   |   |-- [DIR] Migrations/
|   |   |   |   |-- [CS] 20260109123604_InitialCreate.cs
|   |   |   |   |-- [CS] 20260109123604_InitialCreate.Designer.cs
|   |   |   |   +-- [CS] FileDbContextModelSnapshot.cs
|   |   |   |-- [DIR] Properties/
|   |   |   |   +-- [JSON] launchSettings.json
|   |   |   |-- [DIR] Services/
|   |   |   |   +-- [CS] EncryptedFileService.cs
|   |   |   |-- [JSON] appsettings.json
|   |   |   |-- [PROJ] FileTransferService.csproj
|   |   |   +-- [CS] Program.cs
|   |   |-- [DIR] GatewayService/
|   |   |   |-- [DIR] Properties/
|   |   |   |   +-- [JSON] launchSettings.json
|   |   |   |-- [JSON] appsettings.json
|   |   |   |-- [PROJ] GatewayService.csproj
|   |   |   |-- [JSON] ocelot.json
|   |   |   +-- [CS] Program.cs
|   |   |-- [DIR] KeyManagementService/
|   |   |   |-- [DIR] BackgroundServices/
|   |   |   |   +-- [CS] KeyRotationBackgroundService.cs
|   |   |   |-- [DIR] Controllers/
|   |   |   |   +-- [CS] KeyController.cs
|   |   |   |-- [DIR] Data/
|   |   |   |   |-- [DIR] Entities/
|   |   |   |   |   +-- [CS] PublicKey.cs
|   |   |   |   +-- [CS] KeyDbContext.cs
|   |   |   |-- [DIR] logs/
|   |   |   |-- [DIR] Migrations/
|   |   |   |   |-- [CS] 20260109085149_InitialCreate.cs
|   |   |   |   |-- [CS] 20260109085149_InitialCreate.Designer.cs
|   |   |   |   +-- [CS] KeyDbContextModelSnapshot.cs
|   |   |   |-- [DIR] Properties/
|   |   |   |   +-- [JSON] launchSettings.json
|   |   |   |-- [DIR] Services/
|   |   |   |   +-- [CS] KeyRotationService.cs
|   |   |   |-- [JSON] appsettings.json
|   |   |   |-- [PROJ] KeyManagementService.csproj
|   |   |   +-- [CS] Program.cs
|   |   |-- [DIR] MessageService/
|   |   |   |-- [DIR] Controllers/
|   |   |   |   |-- [CS] GroupsController.cs
|   |   |   |   +-- [CS] MessagesController.cs
|   |   |   |-- [DIR] Data/
|   |   |   |   |-- [DIR] Entities/
|   |   |   |   |   |-- [CS] Conversation.cs
|   |   |   |   |   |-- [CS] ConversationMember.cs
|   |   |   |   |   +-- [CS] Message.cs
|   |   |   |   +-- [CS] MessageDbContext.cs
|   |   |   |-- [DIR] Hubs/
|   |   |   |   +-- [CS] NotificationHub.cs
|   |   |   |-- [DIR] Migrations/
|   |   |   |   |-- [CS] 20260109072757_InitialCreate.cs
|   |   |   |   |-- [CS] 20260109072757_InitialCreate.Designer.cs
|   |   |   |   +-- [CS] MessageDbContextModelSnapshot.cs
|   |   |   |-- [DIR] Properties/
|   |   |   |   +-- [JSON] launchSettings.json
|   |   |   |-- [DIR] Services/
|   |   |   |   |-- [CS] IRabbitMQService.cs
|   |   |   |   +-- [CS] RabbitMQService.cs
|   |   |   |-- [JSON] appsettings.json
|   |   |   |-- [PROJ] MessageService.csproj
|   |   |   +-- [CS] Program.cs
|   |   |-- [DIR] NotificationService/
|   |   |   |-- [DIR] Hubs/
|   |   |   |   +-- [CS] NotificationHub.cs
|   |   |   |-- [DIR] logs/
|   |   |   |-- [DIR] Properties/
|   |   |   |   +-- [JSON] launchSettings.json
|   |   |   |-- [DIR] Services/
|   |   |   |   |-- [CS] PresenceService.cs
|   |   |   |   +-- [CS] RabbitMQConsumerService.cs
|   |   |   |-- [JSON] appsettings.json
|   |   |   |-- [PROJ] NotificationService.csproj
|   |   |   +-- [CS] Program.cs
|   |   +-- [DIR] UserService/
|   |       |-- [DIR] Controllers/
|   |       |   +-- [CS] UsersController.cs
|   |       |-- [DIR] Data/
|   |       |   |-- [DIR] Entities/
|   |       |   |   |-- [CS] Contact.cs
|   |       |   |   +-- [CS] UserProfile.cs
|   |       |   +-- [CS] UserDbContext.cs
|   |       |-- [DIR] Migrations/
|   |       |   |-- [CS] 20260109092409_InitialCreate.cs
|   |       |   |-- [CS] 20260109092409_InitialCreate.Designer.cs
|   |       |   +-- [CS] UserDbContextModelSnapshot.cs
|   |       |-- [DIR] Properties/
|   |       |   +-- [JSON] launchSettings.json
|   |       |-- [DIR] Services/
|   |       |   +-- [CS] ProfileService.cs
|   |       |-- [JSON] appsettings.json
|   |       |-- [CS] Program.cs
|   |       +-- [PROJ] UserService.csproj
|   |-- [DIR] Frontend/
|   |   +-- [DIR] MessengerClient/
|   |       |-- [DIR] Commands/
|   |       |   +-- [CS] RelayCommand.cs
|   |       |-- [DIR] Converters/
|   |       |   |-- [CS] AdditionalConverters.cs
|   |       |   |-- [CS] BoolToVisibilityConverter.cs
|   |       |   |-- [CS] InverseBoolToVisibilityConverter.cs
|   |       |   |-- [CS] MessageAlignmentConverter.cs
|   |       |   |-- [CS] MessageBackgroundConverter.cs
|   |       |   |-- [CS] StringToVisibilityConverter.cs
|   |       |   +-- [CS] TimestampConverter.cs
|   |       |-- [DIR] Data/
|   |       |   +-- [CS] LocalDbContext.cs
|   |       |-- [DIR] Services/
|   |       |   |-- [CS] ApiClient.cs
|   |       |   |-- [CS] AuthApiService.cs
|   |       |   |-- [CS] IAuthApiService.cs
|   |       |   |-- [CS] ICryptoApiService.cs
|   |       |   |-- [CS] IFileApiService.cs
|   |       |   |-- [CS] IMessageApiService.cs
|   |       |   |-- [CS] IUserApiService.cs
|   |       |   |-- [CS] LocalCryptoService.cs
|   |       |   |-- [CS] LocalStorageService.cs
|   |       |   +-- [CS] SignalRService.cs
|   |       |-- [DIR] ViewModels/
|   |       |   |-- [CS] ChatViewModel.cs
|   |       |   |-- [CS] ContactsViewModel.cs
|   |       |   |-- [CS] LoginViewModel.cs
|   |       |   |-- [CS] MainViewModel.cs
|   |       |   |-- [CS] RegisterViewModel.cs
|   |       |   |-- [CS] RelayCommand.cs
|   |       |   +-- [CS] SettingsViewModel.cs
|   |       |-- [DIR] Views/
|   |       |   |-- [XAML] ChatView.xaml
|   |       |   |-- [CS] ChatView.xaml.cs
|   |       |   |-- [XAML] ContactsView.xaml
|   |       |   |-- [CS] ContactsView.xaml.cs
|   |       |   |-- [XAML] LoginView.xaml
|   |       |   |-- [CS] LoginView.xaml.cs
|   |       |   |-- [XAML] MainWindow.xaml
|   |       |   |-- [CS] MainWindow.xaml.cs
|   |       |   |-- [XAML] MFASetupView.xaml
|   |       |   |-- [CS] MFASetupView.xaml.cs
|   |       |   |-- [XAML] RegisterView.xaml
|   |       |   |-- [CS] RegisterView.xaml.cs
|   |       |   |-- [XAML] SettingsView.xaml
|   |       |   +-- [CS] SettingsView.xaml.cs
|   |       |-- [XAML] App.xaml
|   |       |-- [CS] App.xaml.cs
|   |       |-- [XAML] MainWindow.xaml
|   |       |-- [CS] MainWindow.xaml.cs
|   |       +-- [PROJ] MessengerClient.csproj
|   +-- [DIR] Shared/
|       |-- [DIR] MessengerCommon/
|       |   |-- [DIR] Constants/
|       |   |   +-- [CS] Constants.cs
|       |   |-- [DIR] Extensions/
|       |   |   |-- [CS] Extensions.cs
|       |   |   +-- [CS] ServiceCollectionExtensions.cs
|       |   |-- [DIR] Helpers/
|       |   |   +-- [CS] Helpers.cs
|       |   +-- [PROJ] MessengerCommon.csproj
|       +-- [DIR] MessengerContracts/
|           |-- [DIR] DTOs/
|           |   |-- [CS] AuthDtos.cs
|           |   |-- [CS] ConversationDto.cs
|           |   |-- [CS] CryptoDtos.cs
|           |   |-- [CS] EventDtos.cs
|           |   |-- [CS] FileDto.cs
|           |   |-- [CS] MessageDto.cs
|           |   |-- [CS] MfaDto.cs
|           |   |-- [CS] UserDto.cs
|           |   +-- [CS] UserServiceDtos.cs
|           |-- [DIR] Interfaces/
|           |   |-- [CS] ICryptoService.cs
|           |   |-- [CS] ICryptoServices.cs
|           |   |-- [CS] IRepositories.cs
|           |   +-- [CS] IServices.cs
|           +-- [PROJ] MessengerContracts.csproj
|-- [DIR] ssl/
|   +-- [MD] README.md
|-- [DIR] tests/
|   |-- [DIR] MessengerTests/
|   |   |-- [DIR] CryptoTests/
|   |   |   |-- [CS] LocalStorageEncryptionTests.cs
|   |   |   +-- [CS] TransportEncryptionTests.cs
|   |   |-- [DIR] IntegrationTests/
|   |   |   |-- [CS] EndToEndEncryptionTests.cs
|   |   |   |-- [CS] MFAControllerIntegrationTests.cs
|   |   |   +-- [CS] RabbitMQIntegrationTests.cs
|   |   |-- [DIR] ServiceTests/
|   |   |   |-- [CS] AuditLogServiceTests.cs
|   |   |   |-- [CS] AuthServiceTests.cs
|   |   |   |-- [CS] FileTransferServiceTests.cs
|   |   |   |-- [CS] KeyManagementServiceTests.cs
|   |   |   |-- [CS] MessageServiceTests.cs
|   |   |   |-- [CS] NotificationServiceTests.cs
|   |   |   +-- [CS] UserServiceTests.cs
|   |   |-- [DIR] ValidatorTests/
|   |   |   |-- [CS] RegisterRequestValidatorTests.cs
|   |   |   +-- [CS] VerifyTotpSetupRequestValidatorTests.cs
|   |   +-- [PROJ] MessengerTests.csproj
|   +-- [DIR] MessengerTests.Performance/
|       |-- [CS] CryptoPerformanceTests.cs
|       +-- [PROJ] MessengerTests.Performance.csproj
|-- [ENV] .env
|-- [EX] .env.example
|-- [FILE] .gitattributes
|-- [FILE] .gitignore
|-- [BAT] build-client.bat
|-- [SH] build-client.sh
|-- [MD] CHANGELOG.md
|-- [MD] CHANGELOG_v10.2.md
|-- [BAT] cleanup.bat
|-- [PS1] cleanup.ps1
|-- [MD] CODE_OF_CONDUCT.md
|-- [MD] CONTRIBUTING.md
|-- [BAT] deploy.bat
|-- [PS1] deploy-complete.ps1
|-- [MD] DEPLOYMENT_READY.md
|-- [YML] docker-compose.lan.yml
|-- [YML] docker-compose.override.yml
|-- [YML] docker-compose.prod.yml
|-- [YML] docker-compose.yml
|-- [PS1] Export-ProjectStructure.ps1
|-- [SQL] init-db.sql
|-- [SLN] Messenger.sln
|-- [MD] MIGRATION_SUMMARY.md
|-- [MD] NEXT_STEPS.md
|-- [PS1] quick-start.ps1
|-- [MD] README.md
|-- [MD] SCRIPTS_GUIDE.md
|-- [FILE] secrets.txt
|-- [MD] SECURITY.md
|-- [BAT] setup.bat
|-- [MD] SETUP_CHECKLIST.md
|-- [BAT] status.bat
|-- [BAT] test.bat
|-- [BAT] test-live.bat
|-- [PS1] test-modules.ps1
|-- [BAT] test-production.bat
|-- [BAT] validate-deployment.bat
|-- [MD] WINDOWS_DEPLOYMENT_SUMMARY.md
+-- [MD] WORKSPACE_OVERVIEW.md
```

---

## Statistiken
| Kategorie | Anzahl |
|-----------|--------|
| **Projekte** | 13 |
| **Dateien gesamt** | 538 |
| **C# Dateien (.cs)** | 148 |
| **XAML Dateien** | 9 |
| **JSON Dateien** | 83 |
| **Markdown Dateien** | 60 |
| **Scripts (PS1/BAT/SH)** | 39 |

---

## Legende

| Marker | Bedeutung |
|--------|-----------|
| [DIR] | Verzeichnis |
| [CS] | C# Datei (.cs) |
| [PROJ] | Projekt-Datei (.csproj) |
| [SLN] | Solution-Datei (.sln) |
| [XAML] | XAML-Datei |
| [JSON] | JSON-Konfiguration |
| [YML] | Docker/YAML-Datei |
| [MD] | Markdown-Dokumentation |
| [PS1] | PowerShell-Script |
| [BAT] | Batch-Script |
| [SH] | Shell-Script (.sh) |
| [SQL] | SQL-Datei |
| [ENV] | Environment-Datei (.env) |
| [EX] | Example/Template-Datei |
| [FILE] | Sonstige Datei |

---

**Generiert mit:** Export-ProjectStructure.ps1
**Repository:** https://github.com/Krialder/Messenger-App
**Dokumentation:** WORKSPACE_OVERVIEW.md
