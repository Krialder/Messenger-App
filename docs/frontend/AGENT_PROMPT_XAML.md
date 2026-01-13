# 🤖 Agent Prompt Template - Phase 13.2 (XAML Implementation)

## 📋 **Context für AI Agent**

**Projekt**: Secure Messenger - Ende-zu-Ende verschlüsselte Messaging-App  
**Location**: `I:\Just_for_fun\Messenger\`  
**Repository**: https://github.com/Krialder/Messenger-App  
**Branch**: master (feature/frontend-xaml-views für XAML work)  
**Framework**: .NET 8.0 + WPF + ReactiveUI + MaterialDesign

---

## ✅ **Aktueller Status (Version 8.0)**

**Backend**: 100% Complete ✅
- 9 Microservices production-ready
- 151 Tests passing (100% pass rate)
- ~97% Code Coverage
- API Gateway (Ocelot) ready

**Frontend Backend-Logik**: 100% Complete ✅
- ✅ Refit API Clients (5 services)
- ✅ SignalRService (Real-time)
- ✅ LocalCryptoService (Layer 2 Encryption)
- ✅ LocalStorageService (SQLite)
- ✅ 6 ViewModels (ReactiveUI MVVM)
- ✅ LocalDbContext (EF Core)
- ✅ App.xaml.cs (Dependency Injection)

**Frontend XAML UI**: 20% Complete ⏳
- ⏳ XAML Views (0/7)
- ⏳ Value Converters (0/10)
- ⏳ Resource Dictionaries (0/3)

**Overall Progress**: **80% Complete**

---

## 🎯 **AUFGABE: XAML Views implementieren**

### **Ziel**: Vollständige UI-Implementierung mit MaterialDesign

### **Priorität**: HIGH (Blocker für 100% Completion)

### **Geschätzte Zeit**: 8-12 Stunden

---

## 📁 **Dateien zu erstellen/bearbeiten**

### **1. XAML Views** (7 Files)

```
src/Frontend/MessengerClient/Views/
├── LoginView.xaml ⏳ (PRIORITY 1)
├── LoginView.xaml.cs ⏳
├── MainWindow.xaml ⏳ (PRIORITY 2)
├── MainWindow.xaml.cs ⏳
├── ChatView.xaml ⏳ (PRIORITY 3 - Most Complex)
├── ChatView.xaml.cs ⏳
├── RegisterView.xaml ⏳
├── RegisterView.xaml.cs ⏳
├── ContactsView.xaml ⏳
├── ContactsView.xaml.cs ⏳
├── SettingsView.xaml ⏳
├── SettingsView.xaml.cs ⏳
├── MFASetupView.xaml ⏳
└── MFASetupView.xaml.cs ⏳
```

### **2. Value Converters** (10 Files)

```
src/Frontend/MessengerClient/Converters/
├── BoolToVisibilityConverter.cs ⏳
├── InverseBoolToVisibilityConverter.cs ⏳
├── StringToVisibilityConverter.cs ⏳
├── MessageAlignmentConverter.cs ⏳
├── MessageBackgroundConverter.cs ⏳
├── TimestampConverter.cs ⏳
├── UnreadCountConverter.cs ⏳
├── StringToBitmapImageConverter.cs ⏳
├── MessageStatusConverter.cs ⏳
└── OnlineStatusConverter.cs ⏳
```

### **3. Resource Dictionaries** (3 Files)

```
src/Frontend/MessengerClient/Resources/
├── Colors.xaml ⏳
├── Styles.xaml ⏳
└── MaterialDesignTheme.xaml ⏳
```

### **4. App.xaml aktualisieren** (1 File)

```
src/Frontend/MessengerClient/App.xaml ⏳
- Resource Dictionaries registrieren
- Converters registrieren
```

---

## 🛠️ **Implementierungs-Anforderungen**

### **Technology Stack** (bereits installiert ✅)

```xml
<!-- MessengerClient.csproj -->
<PackageReference Include="MaterialDesignThemes" Version="4.9.0" />
<PackageReference Include="MaterialDesignColors" Version="2.1.4" />
<PackageReference Include="ReactiveUI.WPF" Version="19.5.31" />
```

### **Design Guidelines**

**MaterialDesign Components zu verwenden**:
```xml
- materialDesign:PackIcon (Icons)
- MaterialDesignOutlinedTextBox (Input Fields)
- MaterialDesignRaisedButton (Primary Buttons)
- MaterialDesignFlatButton (Secondary Buttons)
- MaterialDesignCard (Cards)
- BundledTheme (BaseTheme="Dark", PrimaryColor="Blue", SecondaryColor="Pink")
```

**MVVM Binding Requirements**:
```
- Alle ViewModels sind bereits komplett implementiert
- Data Binding MUSS funktionieren (TwoWay für Input, OneWay für Display)
- Commands MUSS an Buttons gebunden werden
- IObservable<bool> canExecute MUSS respektiert werden
```

**Coding Standards** (aus .editorconfig):
```
- Indentation: 4 spaces
- Encoding: UTF-8
- Line endings: CRLF
- No trailing whitespace
- var NUR für built-in types
- Braces immer required
- Accessibility modifiers explicit
- this. prefix NICHT verwenden
```

---

## 📚 **Verfügbare ViewModels** (bereits komplett ✅)

### **LoginViewModel** (Binding-Properties)

```csharp
Properties:
- string Email { get; set; }
- string Password { get; set; }
- string ErrorMessage { get; set; }
- bool IsLoading { get; set; }
- bool MfaRequired { get; set; }
- string MfaCode { get; set; }

Commands:
- ReactiveCommand<Unit, Unit> LoginCommand
- ReactiveCommand<Unit, Unit> VerifyMfaCommand
- ReactiveCommand<Unit, Unit> NavigateToRegisterCommand

Events:
- EventHandler LoginSuccessful
- EventHandler NavigateToRegister
```

### **ChatViewModel** (Binding-Properties)

```csharp
Properties:
- ObservableCollection<MessageViewModel> Messages
- ObservableCollection<ConversationViewModel> Conversations
- ConversationViewModel? SelectedConversation
- string MessageText { get; set; }
- bool IsLoading { get; set; }

Commands:
- ReactiveCommand<Unit, Unit> SendMessageCommand
- ReactiveCommand<Unit, Unit> SendFileCommand
- ReactiveCommand<Unit, Unit> RefreshConversationsCommand

Models:
- MessageViewModel { Id, Content, SenderId, Timestamp, IsSent, IsDelivered, IsRead }
- ConversationViewModel { Id, Name, LastMessage, LastMessageTime, UnreadCount }
```

### **ContactsViewModel, SettingsViewModel, etc.**
Siehe: `src/Frontend/MessengerClient/README_IMPLEMENTATION.md`

---

## 🎯 **Prompt für AI Agent**

```
# AUFGABE: Implementiere XAML Views für MessengerClient (WPF)

## Context
- Projekt: I:\Just_for_fun\Messenger\src\Frontend\MessengerClient
- Framework: .NET 8.0 + WPF + ReactiveUI + MaterialDesign
- Status: Backend Logic 100% complete, XAML UI fehlt
- ViewModels: Alle komplett implementiert (siehe README_IMPLEMENTATION.md)

## Ziel
Implementiere alle XAML Views mit MaterialDesign UI Components.

## Priorität
1. LoginView.xaml (START HERE)
2. MainWindow.xaml (Navigation Shell)
3. ChatView.xaml (Chat UI mit Message Bubbles)
4. RegisterView.xaml
5. ContactsView.xaml
6. SettingsView.xaml
7. MFASetupView.xaml

## Anforderungen
- MaterialDesign Components verwenden (siehe README_IMPLEMENTATION.md)
- Data Binding zu existierenden ViewModels
- Command Binding für alle Buttons
- Value Converters für Visibility, Alignment, etc.
- Resource Dictionaries (Colors, Styles)
- Code-Behind minimal halten (nur PasswordBox binding)

## Coding Standards
- Aus .editorconfig: 4 spaces, UTF-8, CRLF, explicit modifiers
- KEIN var (außer built-in types)
- Braces immer required
- KEIN this. prefix

## Deliverables
- ✅ 7 XAML Views komplett
- ✅ 10 Value Converters
- ✅ 3 Resource Dictionaries
- ✅ App.xaml aktualisiert
- ✅ Alle Bindings funktionieren
- ✅ Kompiliert ohne Fehler

## Referenzen
- WORKSPACE_GUIDE.md (Projekt-Struktur)
- README_IMPLEMENTATION.md (Complete ViewModels Reference)
- QUICK_START.md (Step-by-step Guide für LoginView)

## Nächster Schritt
Starte mit LoginView.xaml (siehe QUICK_START.md für vollständiges Template)
```

---

## 🧪 **Validierungs-Checkliste**

Nach Implementierung MUSS folgendes funktionieren:

### **LoginView**
- [x] Email TextBox Binding funktioniert
- [x] PasswordBox Binding funktioniert (via Code-Behind)
- [x] ErrorMessage wird angezeigt bei Fehler
- [x] IsLoading zeigt ProgressBar
- [x] MfaRequired zeigt MFA Input
- [x] LoginCommand wird ausgeführt bei Button-Click
- [x] Navigation zu RegisterView funktioniert
- [x] Navigation zu MainWindow nach erfolgreichem Login

### **ChatView**
- [x] Conversation List wird angezeigt (ItemsSource: Conversations)
- [x] SelectedConversation binding funktioniert (TwoWay)
- [x] Messages werden angezeigt (ItemsSource: Messages)
- [x] Message Bubbles sind links/rechts aligned (IsSent converter)
- [x] MessageText TextBox binding funktioniert
- [x] SendMessageCommand wird ausgeführt
- [x] SignalR OnMessageReceived fügt neue Messages hinzu

### **Alle Views**
- [x] MaterialDesign Theme wird angewendet
- [x] Keine Compile Errors
- [x] Data Binding funktioniert
- [x] Commands werden ausgeführt
- [x] Value Converters funktionieren
- [x] Navigation funktioniert

---

## 📦 **Build & Test**

```sh
# 1. Build MessengerClient
cd src/Frontend/MessengerClient
dotnet build

# 2. Run (mit Backend Services)
# Terminal 1: Backend Services (docker-compose up -d)
# Terminal 2: Frontend
dotnet run

# 3. Validierung
# - LoginView wird angezeigt
# - Email/Password eingeben
# - Login funktioniert
# - Navigate to MainWindow
# - ChatView wird angezeigt
```

---

## 🎯 **Success Criteria**

**Phase 13.2 ist complete wenn**:
- ✅ Alle 7 XAML Views implementiert
- ✅ Alle 10 Value Converters implementiert
- ✅ Resource Dictionaries konfiguriert
- ✅ App kompiliert ohne Fehler
- ✅ Login Flow funktioniert (Register → Login → JWT → MainWindow)
- ✅ Chat UI funktioniert (Send → Encrypt → SignalR → Receive → Decrypt)
- ✅ Alle Bindings funktionieren
- ✅ Frontend 100% Complete!

**Dann**: Phase 14 (Deployment) ODER Phase 13.3 (E2E Tests)

---

**Version**: 8.0  
**Last Updated**: 2025-01-10  
**Status**: ⏳ **Ready for XAML Implementation**

**Start Here**: `LoginView.xaml` (siehe QUICK_START.md)
