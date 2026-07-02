## Justificación de las mejoras aplicadas

- Secretos hardcodeados en appsettings.json: se sustituyeron por placeholders y el servicio ahora requiere JWT_SECRET en el entorno o secreto gestionado; evita exposición de claves en VCS.
- RequireHttpsMetadata = false en la configuración JWT: se cambió a requerir HTTPS para no aceptar metadatos/token por canales inseguros.
- Validación de token demasiado permisiva: se activó ValidateIssuerSigningKey y RequireExpirationTime
- IdentityModelEventSource.ShowPII = true: se desactivó para evitar que información sensible se filtre en logs.
- Middleware de JWT que continuaba la pipeline tras token inválido: ahora corta la petición y retorna 401 para evitar procesamiento no autorizado.
- Orden incorrecto de middlewares (autenticación/autorización): UseAuthentication() se ejecuta antes de JwtMiddleware y UseAuthorization() después.
- Falta de validación de entrada en controladores: se añadieron DataAnnotations y comprobaciones (ModelState, longitudes, validaciones de ObjectId) para evitar entradas inválidas o malformadas.
- Swagger/UI expuesto en producción: ahora SwaggerUI está limitado a entorno Development.



---

## Informe de vulnerabilidades 

CRITICAL & HIGH

1) CVE-2023-45853

Paquete afectado y versión: zlib 1:1.2.13.dfsg-1 (Debian)

Severidad: CRITICAL

Componente: OS base

Vector de explotación: Un archivo ZIP dañado puede congelar el sistema o permitir que un atacante tome el control total desde afuera.

Estrategia de remediación: Actualizar el sistema operativo base del contenedor a una versión moderna y segura.

2) CVE-2026-42496

Paquete afectado y versión: perl-base / perl-Archive-Tar

Severidad: CRITICAL

Componente: OS base

Vector de explotación: Al abrir un archivo comprimido tramposo, este puede hackear carpetas prohibidas y alterar o borrar información del sistema.

Estrategia de remediación: Actualizar la versión de Linux de la base o eliminar herramientas que no se usen (como Perl).

3) CVE-2026-41992

Paquete afectado y versión: gzip 1.12-1 (Debian)

Severidad: HIGH

Componente: OS base

Vector de explotación: Un archivo comprimido falso puede saturar el programa y apagar la aplicación por completo.

Estrategia de remediación: Actualizar el sistema base y no aceptar archivos de páginas o usuarios desconocidos.

4) CVE-2025-69720

Paquete afectado y versión: libtinfo6 / ncurses

Severidad: HIGH

Componente: OS base

Vector de explotación: Enviar textos o códigos raros a la pantalla de comandos del sistema puede tumbarlo o dejar que un extraño lo maneje.

Estrategia de remediación: Actualizar la imagen base y configurar el contenedor para que no use pantallas de comandos innecesarias.

5) CVE-2026-54369

Paquete afectado y versión: libacl1

Severidad: HIGH

Componente: OS base

Vector de explotación: Si alguien ya logró entrar, puede usar un error de permisos para meterse en carpetas secretas y robar más accesos.

Estrategia de remediación: Actualizar la base y asegurarse de que la aplicación no corra con permisos de "Administrador" (root).

6) CVE-2026-44788

Paquete afectado y versión: SharpCompress (dependencia NuGet)

Severidad: HIGH

Componente: Dependencia del proyecto

Vector de explotación: Al desempaquetar un ZIP con la aplicación, este puede guardar archivos dañinos fuera del sitio permitido y romper la app.

Estrategia de remediación: Actualizar la librería interna (SharpCompress) desde el código a su última versión segura.

7) CVE-2026-44302

Paquete afectado y versión: Snappier (dependencia NuGet)

Severidad: HIGH

Componente: Dependencia del proyecto

Vector de explotación: Datos maliciosos pueden trabar la aplicación en un bucle eterno, gastando toda la memoria hasta colapsar el servicio.

Estrategia de remediación: Actualizar la librería a la versión fija y poner límites de tiempo para procesar archivos.

8) CVE-2025-9708

Paquete afectado y versión: KubernetesClient (NuGet)

Severidad: MEDIUM

Componente: Dependencia del proyecto

Vector de explotación: La aplicación confía a ciegas en conexiones externas sin revisar su seguridad, abriendo la puerta a que espíen los datos.

Estrategia de remediación: Actualizar el paquete en el proyecto y activar la verificación obligatoria de conexiones seguras.

9) CVE-2026-42497

Paquete afectado y versión: perl-Archive-Tar

Severidad: HIGH

Componente: OS base

Vector de explotación: Archivos comprimidos dañados pueden desconfigurar o borrar partes esenciales del sistema operativo interno.

Estrategia de remediación: Actualizar el sistema base y borrar los paquetes de Perl si la aplicación no los necesita para funcionar.

10) CVE-2026-54371

Paquete afectado y versión: libattr1

Severidad: HIGH

Componente: OS base

Vector de explotación: Un fallo al revisar las etiquetas de los archivos permite engañar al sistema para entrar a zonas prohibidas.

Estrategia de remediación: Actualizar la imagen base y limitar los privilegios generales del contenedor.

MEDIUM

11) CVE-2026-13595

Paquete afectado y versión: util-linux

Severidad: MEDIUM

Componente: OS base

Vector de explotación: Errores en las herramientas internas de Linux pueden hacer que un atacante manipule el disco duro virtual y falle el sistema.

Estrategia de remediación: Actualizar el sistema operativo base de la imagen a través del proceso automático de publicación.

12) CVE-2026-27171

Paquete afectado y versión: zlib

Severidad: MEDIUM

Componente: OS base

Vector de explotación: Archivos comprimidos alterados pueden agotar la potencia del procesador de golpe, haciendo que la app deje de responder.

Estrategia de remediación: Actualizar la base y limitar en código el tamaño máximo de los archivos que se pueden subir.

13) CVE-2026-5450

Paquete afectado y versión: glibc (libc6)

Severidad: MEDIUM

Componente: OS base

Vector de explotación: El envío de datos con formatos extraños confunde al motor del sistema, cerrando la aplicación de manera inesperada.

Estrategia de remediación: Actualizar la versión base de Linux y correr los procesos con accesos mínimos.

14) CVE-2025-30258

Paquete afectado y versión: gpgv / gnupg

Severidad: MEDIUM

Componente: OS base

Vector de explotación: Al validar firmas digitales de seguridad falsas, el sistema puede ignorar la protección o colgarse por completo.

Estrategia de remediación: Actualizar la base y evitar que la aplicación verifique de forma automática archivos en los que no confía.

15) CVE-2025-15649

Paquete afectado y versión: perl-IO-Compress

Severidad: MEDIUM

Componente: OS base

Vector de explotación: El procesamiento de archivos dañados provoca fallas que congelan las herramientas del sistema operativo.

Estrategia de remediación: Quitar los componentes de Perl de la imagen si no hacen falta, o actualizar la base del contenedor.
