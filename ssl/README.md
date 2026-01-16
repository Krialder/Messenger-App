# 🔒 SSL/TLS Zertifikate für Secure Messenger

## Übersicht

Dieses Verzeichnis enthält die SSL/TLS-Zertifikate für HTTPS-Verschlüsselung.

⚠️ **WICHTIG**: Zertifikate und private Keys dürfen **NIEMALS** in Git committed werden!

---

## 📋 Erforderliche Dateien

```
ssl/
├── fullchain.pem   # Public Zertifikat + Certificate Chain
├── privkey.pem     # Private Key (GEHEIM! Niemals teilen!)
└── README.md       # Diese Datei
```

---

## 🚀 Option 1: Let's Encrypt (Empfohlen für Production)

### Schritt 1: Certbot installieren

```bash
# Ubuntu/Debian
sudo apt update
sudo apt install certbot

# CentOS/RHEL
sudo yum install certbot
```

### Schritt 2: Zertifikat generieren (Standalone)

```bash
# Domain ersetzen!
sudo certbot certonly --standalone -d messenger.yourdomain.com

# Bei Erfolg:
# Zertifikate werden gespeichert in:
# /etc/letsencrypt/live/messenger.yourdomain.com/
```

### Schritt 3: Zertifikate kopieren

```bash
# Ins Projekt-Verzeichnis kopieren
sudo cp /etc/letsencrypt/live/messenger.yourdomain.com/fullchain.pem ./ssl/
sudo cp /etc/letsencrypt/live/messenger.yourdomain.com/privkey.pem ./ssl/

# Permissions setzen
sudo chmod 644 ./ssl/fullchain.pem
sudo chmod 600 ./ssl/privkey.pem
sudo chown $USER:$USER ./ssl/*.pem
```

### Schritt 4: Auto-Renewal einrichten

```bash
# Testen
sudo certbot renew --dry-run

# Cronjob hinzufügen
sudo crontab -e

# Folgende Zeile hinzufügen (täglich um 2 Uhr):
0 2 * * * certbot renew --quiet && docker-compose -f docker-compose.yml -f docker-compose.prod.yml restart nginx
```

---

## 🛠️ Option 2: Certbot mit Nginx (Automatisch)

Falls Nginx bereits läuft:

```bash
sudo certbot --nginx -d messenger.yourdomain.com

# Certbot konfiguriert Nginx automatisch
```

---

## 🧪 Option 3: Selbst-signierte Zertifikate (NUR für Testing!)

⚠️ **NICHT für Production verwenden** (Browser-Warnungen, keine echte Sicherheit)

```bash
# Zertifikat generieren
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout ssl/privkey.pem \
  -out ssl/fullchain.pem \
  -subj "/C=DE/ST=State/L=City/O=Organization/CN=messenger.localhost"

# Permissions
chmod 644 ssl/fullchain.pem
chmod 600 ssl/privkey.pem
```

**Verwendung:**
- Nur für lokale Tests
- Browser zeigt Sicherheitswarnung (akzeptieren)
- Keine echte Verschlüsselung gegenüber Angreifern

---

## 🔍 Zertifikat prüfen

```bash
# Details anzeigen
openssl x509 -in ssl/fullchain.pem -text -noout

# Gültigkeit prüfen
openssl x509 -in ssl/fullchain.pem -noout -dates

# Private Key prüfen
openssl rsa -in ssl/privkey.pem -check
```

---

## 🔐 Sicherheits-Checklist

- [ ] `privkey.pem` hat Permissions `600` (nur Owner kann lesen)
- [ ] `fullchain.pem` hat Permissions `644` (alle können lesen, nur Owner kann schreiben)
- [ ] `.gitignore` enthält `ssl/*.pem` und `ssl/*.key`
- [ ] Zertifikate sind **NICHT** in Git committed
- [ ] Let's Encrypt Auto-Renewal ist konfiguriert
- [ ] Zertifikat ist gültig (nicht abgelaufen)

---

## 🔄 Zertifikat erneuern

### Manuell

```bash
# Zertifikat erneuern
sudo certbot renew

# Neue Zertifikate kopieren
sudo cp /etc/letsencrypt/live/messenger.yourdomain.com/fullchain.pem ./ssl/
sudo cp /etc/letsencrypt/live/messenger.yourdomain.com/privkey.pem ./ssl/

# Nginx neu laden
docker-compose -f docker-compose.yml -f docker-compose.prod.yml restart nginx
```

### Automatisch (Cronjob)

Siehe Option 1, Schritt 4

---

## 📚 Weitere Ressourcen

- **Let's Encrypt**: https://letsencrypt.org/
- **Certbot**: https://certbot.eff.org/
- **SSL Labs Test**: https://www.ssllabs.com/ssltest/

---

## ⚠️ Troubleshooting

### "Permission denied" beim Kopieren

```bash
# Als root ausführen
sudo -i
cp /etc/letsencrypt/live/.../fullchain.pem /pfad/zum/projekt/ssl/
chmod 644 /pfad/zum/projekt/ssl/fullchain.pem
chown user:user /pfad/zum/projekt/ssl/fullchain.pem
```

### Zertifikat nicht gültig

```bash
# Gültigkeit prüfen
openssl x509 -in ssl/fullchain.pem -noout -dates

# Wenn abgelaufen: Erneuern
sudo certbot renew --force-renewal
```

### Nginx startet nicht

```bash
# Config testen
docker-compose -f docker-compose.yml -f docker-compose.prod.yml exec nginx nginx -t

# Zertifikat-Pfad prüfen
ls -la ssl/
```

---

**Status**: ✅ Bereit für Let's Encrypt Zertifikate

**Nächste Schritte**:
1. Domain registrieren
2. DNS A-Record konfigurieren
3. Certbot ausführen
4. Zertifikate kopieren
5. Nginx starten
