# Contributing to Secure Messenger

Vielen Dank für Ihr Interesse, zu diesem Projekt beizutragen! 🎉

## Code of Conduct

Dieses Projekt und alle Beteiligten verpflichten sich, eine belästigungsfreie Umgebung für alle zu schaffen. Bitte seien Sie respektvoll und professionell in allen Interaktionen.

## Wie kann ich beitragen?

### Fehler melden

Wenn Sie einen Fehler gefunden haben:

1. **Prüfen Sie**, ob der Fehler bereits gemeldet wurde
2. **Erstellen Sie ein Issue** mit folgenden Informationen:
   - Klare Beschreibung des Problems
   - Schritte zur Reproduktion
   - Erwartetes vs. tatsächliches Verhalten
   - Systemumgebung (.NET Version, OS, etc.)
   - Screenshots (falls relevant)

### Feature Requests

Feature-Vorschläge sind willkommen! Bitte erstellen Sie ein Issue mit:
- Detaillierter Beschreibung des Features
- Use Case / Business Value
- Mögliche Implementierungsideen

### Pull Requests

#### Vorbereitung

1. **Fork** das Repository
2. **Clone** Ihren Fork:
   ```bash
   git clone https://github.com/your-username/Messenger.git
   cd Messenger
   ```
3. **Erstellen Sie einen Branch**:
   ```bash
   git checkout -b feature/your-feature-name
   # oder
   git checkout -b fix/issue-description
   ```

#### Development

1. **Installieren Sie Dependencies**:
   ```bash
   dotnet restore
   ```

2. **Implementieren Sie Ihre Änderungen**
   - Folgen Sie den bestehenden Code-Konventionen
   - Schreiben Sie aussagekräftige Commit-Messages
   - Fügen Sie Tests hinzu/aktualisieren Sie Tests

3. **Testen Sie Ihre Änderungen**:
   ```bash
   # Unit Tests
   dotnet test --filter "Category=Unit"
   
   # Integration Tests
   dotnet test --filter "Category=Integration"
   
   # Code Coverage
   dotnet test --collect:"XPlat Code Coverage"
   ```

4. **Code-Formatierung**:
   ```bash
   dotnet format
   ```

#### Pull Request erstellen

1. **Push** Ihren Branch:
   ```bash
   git push origin feature/your-feature-name
   ```

2. **Erstellen Sie einen Pull Request** auf GitHub mit:
   - Klarer Beschreibung der Änderungen
   - Referenz zu zugehörigen Issues (#123)
   - Screenshots (falls UI-Änderungen)

3. **PR-Checkliste**:
   - [ ] Tests sind vorhanden und bestehen
   - [ ] Code Coverage >= 80%
   - [ ] Dokumentation aktualisiert
   - [ ] Keine Merge-Konflikte
   - [ ] CI/CD Pipeline erfolgreich

## Coding Guidelines

### C# / .NET

- **Naming Conventions**: PascalCase für Klassen/Methoden, camelCase für Variablen
- **Async/Await**: Verwenden Sie async/await für I/O-Operationen
- **Dependency Injection**: Nutzen Sie Constructor Injection
- **SOLID Principles**: Befolgen Sie Clean Code-Prinzipien

### Testing

- **Unit Tests**: Mind. 80% Coverage
- **Crypto Tests**: Mind. 90% Coverage
- **Test-Naming**: `MethodName_Scenario_ExpectedResult`
- **AAA Pattern**: Arrange, Act, Assert

### Dokumentation

- **Code-Kommentare**: Nur für komplexe Logik
- **XML-Dokumentation**: Für öffentliche APIs
- **README**: Aktualisieren bei neuen Features
- **Diagramme**: PlantUML für Architektur-Änderungen

## Security

### Sicherheitslücken melden

**NICHT** über öffentliche Issues melden!

Senden Sie stattdessen eine E-Mail an: security@secure-messenger.local

Wir werden:
- Innerhalb von 48 Stunden antworten
- Das Problem vertraulich behandeln
- Sie über den Fix informieren
- Sie im Security AdvisoryMention (falls gewünscht)

### Security-Best Practices

- Keine Hardcoded Secrets
- Input Validation immer durchführen
- Kryptographie-Code nur nach Review
- Dependency Updates regelmäßig

## Review Process

1. **Automated Checks**: CI/CD Pipeline muss erfolgreich sein
2. **Code Review**: Mind. 1 Approval von Maintainer
3. **Security Review**: Bei Crypto/Auth-Änderungen obligatorisch
4. **Testing**: Alle Tests müssen bestehen

## Branch Strategy

- `main`: Produktions-Branch (geschützt)
- `develop`: Development-Branch
- `feature/*`: Feature-Branches
- `fix/*`: Bugfix-Branches
- `hotfix/*`: Kritische Fixes für Production

## Release Process

1. Feature-Freeze auf `develop`
2. Release-Branch erstellen: `release/v1.0.0`
3. Testing & Bug-Fixes
4. Merge in `main` + Tag
5. Deploy
6. Merge zurück in `develop`

## Lizenz

Durch Ihren Beitrag stimmen Sie zu, dass Ihre Änderungen unter der MIT-Lizenz lizenziert werden.

## Fragen?

Bei Fragen können Sie:
- Ein Issue erstellen
- Eine Diskussion starten
- Das Team kontaktieren

---

**Vielen Dank für Ihren Beitrag!** 🙏
