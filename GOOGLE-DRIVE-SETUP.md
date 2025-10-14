# Google Drive Setup for Build Agent

El Build Agent necesita permisos para subir archivos a la carpeta de Google Drive. Sigue estos pasos:

## Opción 1: Service Account (Recomendado para Producción)

### Paso 1: Crear Service Account en Google Cloud

1. Ve a [Google Cloud Console](https://console.cloud.google.com/)
2. Selecciona o crea un proyecto
3. Navega a **IAM & Admin > Service Accounts**
4. Click **Create Service Account**
5. Nombre: `eggscape-build-agent`
6. Click **Create and Continue**
7. **No agregues roles**, click **Continue**
8. Click **Done**

### Paso 2: Crear Key para Service Account

1. Click en el service account creado
2. Tab **Keys**
3. Click **Add Key > Create new key**
4. Selecciona **JSON**
5. Click **Create**
6. Se descarga un archivo JSON con las credenciales

### Paso 3: Habilitar Google Drive API

1. En Google Cloud Console, navega a **APIs & Services > Library**
2. Busca "Google Drive API"
3. Click **Enable**

### Paso 4: Compartir la Carpeta de Google Drive

1. Ve a [Google Drive](https://drive.google.com/)
2. Encuentra la carpeta: https://drive.google.com/drive/folders/127SFVubjMJrQfS965hm5Cl3V6h4yfcru
3. Click derecho > **Share**
4. En "Add people and groups", pega el **email del service account**
   - Lo encuentras en el JSON descargado, campo `client_email`
   - Ejemplo: `eggscape-build-agent@project-id.iam.gserviceaccount.com`
5. Selecciona **Editor** permissions
6. **Desactiva** "Notify people" (no es necesario notificar)
7. Click **Share**

### Paso 5: Configurar en Build Server

1. Ve a https://build-server-qwcy.onrender.com/settings
2. En la sección "Google Drive Configuration":
   - **Google Drive Folder ID**: `127SFVubjMJrQfS965hm5Cl3V6h4yfcru` (ya configurado)
   - **Google Drive Credentials (JSON)**: Pega el contenido completo del archivo JSON descargado
3. Click **Save Settings**

---

## Opción 2: OAuth (Para Testing Local)

⚠️ **No recomendado para producción** - Requiere intervención manual cada vez

### Configuración OAuth

1. Google Cloud Console > **APIs & Services > Credentials**
2. Click **Create Credentials > OAuth 2.0 Client ID**
3. Application type: **Desktop app**
4. Nombre: `Eggscape Build Agent - OAuth`
5. Download JSON
6. Coloca el JSON en `C:\build-agent\credentials\oauth-credentials.json`

### Primera Ejecución

El Build Agent abrirá un navegador para autorización:
1. Selecciona tu cuenta de Google
2. Click **Allow**
3. Se guarda el token en `C:\build-agent\credentials\token.json`

---

## Verificación

Después de configurar, verifica que funciona:

```bash
# Test upload (desde Build Agent)
# El agente intentará subir un archivo de prueba

# Ver carpeta en Drive:
https://drive.google.com/drive/folders/127SFVubjMJrQfS965hm5Cl3V6h4yfcru
```

## Estructura de Carpetas en Drive

Los builds se organizan así:

```
📁 Eggscape Builds (127SFVubjMJrQfS965hm5Cl3V6h4yfcru)
  ├── 📁 [commit-short-hash]_[bundle-code]/
  │   ├── 📄 build_info.txt
  │   ├── 📦 Eggscape_Android_[version]_[code].apk
  │   └── 📦 Eggscape_Android_[version]_[code].aab
  └── ...
```

Ejemplo:
```
📁 a7f3d82_1234/
  ├── 📄 build_info.txt
  ├── 📦 Eggscape_Android_1.0.123_1234.apk
  └── 📦 Eggscape_Android_1.0.123_1234.aab
```

## Troubleshooting

### Error: "insufficient permissions"
- Verifica que el service account tenga permisos de **Editor** en la carpeta
- Asegúrate de que la carpeta ID sea correcta

### Error: "API not enabled"
- Habilita Google Drive API en Google Cloud Console

### Error: "invalid credentials"
- Verifica que el JSON esté completo y sin errores de formato
- Regenera el key del service account si es necesario

### No se ven archivos en Drive
- Verifica que estés usando la cuenta correcta de Google
- El service account NO verá los archivos en "My Drive", solo la carpeta compartida aparece en "Shared with me"

## Security Notes

⚠️ **IMPORTANTE**:
- El JSON del service account contiene credenciales sensibles
- NO lo compartas públicamente
- NO lo commites a Git
- Guárdalo en variables de entorno o secretos seguros
- En Render, usa Environment Variables para almacenarlo

## Límites de Google Drive API

- **Free**: 1 billion queries/day
- **Uploads**: Sin límite de tamaño por archivo
- **Batch operations**: 100 requests por batch
- **Rate limiting**: 10 requests/segundo por usuario

Para Eggscape, estos límites son más que suficientes.
