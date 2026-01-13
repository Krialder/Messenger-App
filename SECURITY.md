# Security Policy

## Supported Versions

The following versions of Secure Messenger are currently supported with security updates:

| Version | Supported          | Status |
| ------- | ------------------ | ------ |
| 9.0.x   | :white_check_mark: | Current stable release |
| < 9.0   | :x:                | No longer supported |

---

## Reporting a Vulnerability

**⚠️ Please DO NOT report security vulnerabilities through public GitHub issues.**

We take security seriously. If you discover a security vulnerability, please follow responsible disclosure practices:

### Contact Information

**Email**: security@example.com *(Replace with your actual security contact email)*  
**Response Time**: We aim to acknowledge reports within **48 hours**

### What to Include

When reporting a vulnerability, please include:

1. **Description** - Detailed description of the vulnerability
2. **Steps to Reproduce** - Step-by-step instructions to reproduce the issue
3. **Impact** - Potential impact and attack scenarios
4. **Affected Versions** - Which versions are affected
5. **Suggested Fix** (optional) - If you have recommendations

### What to Expect

1. **Acknowledgment** - Within 48 hours
2. **Initial Assessment** - Within 5 business days
3. **Regular Updates** - Every 5-7 days on progress
4. **Fix Timeline** - Depends on severity:
   - **Critical**: 7-14 days
   - **High**: 14-30 days
   - **Medium**: 30-60 days
   - **Low**: 60-90 days

### Disclosure Policy

We follow a **coordinated disclosure** process:

1. You report the vulnerability privately
2. We confirm and investigate the issue
3. We develop and test a fix
4. We release a security patch
5. We publicly disclose the vulnerability (with your credit, if desired)

**Embargo Period**: Typically 90 days from initial report, or until patch is released (whichever comes first).

---

## Security Best Practices

### For Users

1. **Use Strong Passwords**
   - Minimum 12 characters
   - Mix of uppercase, lowercase, numbers, and symbols
   - Avoid common words or personal information

2. **Enable Multi-Factor Authentication (MFA)**
   - Set up TOTP in Settings
   - Keep backup codes secure

3. **Keep Software Updated**
   - Use the latest version of Messenger
   - Enable automatic updates

4. **Verify Contacts**
   - Verify identity before sharing sensitive information
   - Use key fingerprint verification

5. **Secure Your Device**
   - Use full-disk encryption
   - Keep your operating system updated
   - Use antivirus software

### For Developers/Deployers

1. **Secure Configuration**
   - Change all default passwords in `.env`
   - Use strong, random secrets (min. 32 characters)
   - Enable HTTPS/TLS in production
   - Configure firewall rules

2. **Dependency Management**
   - Regularly update NuGet packages
   - Run `dotnet list package --vulnerable`
   - Use Dependabot for automated updates

3. **Database Security**
   - Use strong database passwords
   - Limit network access to databases
   - Enable database encryption at rest
   - Regular automated backups

4. **Secret Management**
   - Never commit secrets to version control
   - Use environment variables or secret managers
   - Rotate secrets regularly (every 90 days)
   - Use `.env` file for local development (add to `.gitignore`)

5. **Monitoring & Logging**
   - Enable audit logging
   - Monitor for suspicious activity
   - Set up alerts for security events
   - Regularly review logs

6. **Network Security**
   - Use HTTPS for all API communication
   - Configure CORS properly
   - Implement rate limiting
   - Use a WAF (Web Application Firewall) in production

---

## Security Features

### Implemented Security Measures

- ✅ **3-Layer End-to-End Encryption**
  - Layer 1: Transport (X25519 + ChaCha20-Poly1305)
  - Layer 2: Local Storage (Argon2id + AES-256-GCM)
  - Layer 3: Group Chat (Signal Protocol)

- ✅ **Password Security**
  - Argon2id hashing (industry best practice)
  - Minimum 8 characters, complexity requirements
  - Secure password reset flow

- ✅ **Multi-Factor Authentication**
  - TOTP (Time-based One-Time Password)
  - Compatible with Google Authenticator, Authy, etc.

- ✅ **JWT Authentication**
  - Short-lived access tokens (60 minutes)
  - Refresh token rotation
  - Token invalidation on logout

- ✅ **Automatic Key Rotation**
  - Encryption keys rotated every 30 days
  - Seamless key migration

- ✅ **Audit Logging**
  - All critical actions logged
  - JSONB storage for flexible querying
  - User activity tracking

- ✅ **File Transfer Security**
  - Encrypted uploads/downloads
  - AES-256-GCM encryption
  - Virus scanning (optional integration)

- ✅ **API Security**
  - Input validation
  - SQL injection prevention (EF Core)
  - XSS prevention
  - CSRF protection

---

## Known Limitations

### Current Limitations (v9.0)

1. **No Perfect Forward Secrecy (PFS)** in Layer 1
   - Mitigation: Automatic key rotation every 30 days
   - Future: Implement Double Ratchet Algorithm

2. **No Voice/Video Encryption**
   - Voice/Video calls not yet implemented
   - Planned for future version

3. **Limited Rate Limiting**
   - Basic rate limiting in API Gateway
   - Production deployments should use additional WAF

### Out of Scope

The following are explicitly out of scope for this project:

- **Mobile Applications** - Desktop-only (WPF)
- **Web Client** - No browser-based client
- **Metadata Protection** - Sender/recipient metadata is logged
- **Quantum Resistance** - Uses classical cryptography (X25519, AES)

---

## Security Audit History

| Date | Auditor | Scope | Results |
|------|---------|-------|---------|
| 2025-01-10 | Internal | Code Review | 151 tests passing, ~97% coverage |
| TBD | External | Full Security Audit | Pending |

---

## Compliance

### Standards & Regulations

This project implements security controls aligned with:

- **OWASP Top 10** - Application Security Risks (2021)
- **NIST Cybersecurity Framework** - Best practices
- **CWE/SANS Top 25** - Most dangerous software weaknesses

**Note**: This is a learning project and has not been certified for compliance with:
- HIPAA (Health Insurance Portability and Accountability Act)
- GDPR (General Data Protection Regulation)
- SOC 2 (Service Organization Control 2)

---

## Security Resources

### For Developers

- **[OWASP Application Security](https://owasp.org/)**
- **[.NET Security Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/DotNet_Security_Cheat_Sheet.html)**
- **[Cryptography Best Practices](https://docs.microsoft.com/en-us/dotnet/standard/security/cryptography-model)**

### For Users

- **[Password Security Tips](https://www.cisa.gov/secure-our-world/use-strong-passwords)**
- **[Multi-Factor Authentication Guide](https://www.cisa.gov/mfa)**

---

## Acknowledgments

We would like to thank the security researchers and contributors who have helped improve the security of this project.

---

**Last Updated**: 2025-01-10  
**Version**: 9.0

**For security concerns, contact**: security@example.com *(Update with your email)*
