## Proyecto elegido

https://github.com/aelassas/microservices



## Resumen Ejecutivo

* **Vulnerabilidades Iniciales:** 72 en total (69 en el Sistema Operativo base Debian y 3 en dependencias de NuGet).
* **Vulnerabilidades Corregidas / Mitigadas:** 72 eliminadas (69 removidas del sistema operativo base y 3 solucionadas en los paquetes NuGet del proyecto).
* **Estado Actual:** **0 Vulnerabilidades detectadas** (Escaneo 100% limpio en todo el contenedor).


##  Tabla de Vulnerabilidades Encontradas y Remediadas

| CVE | Severidad | Componente | Riesgo de Explotación | Remediación Aplicada |
| :--- | :--- | :--- | :--- | :--- |
| **CVE-2023-45853** | **CRITICAL** | OS Base (`zlib`) | Un archivo ZIP dañado puede romper la memoria del sistema y tomar el control remoto. | Migración a Imagen Base *Alpine* (elimina la librería vulnerable). |
| **CVE-2026-42496** | **CRITICAL** | OS Base (`perl`) | Un archivo comprimido tramposo puede hackear carpetas prohibidas y alterar datos. | Uso de base ultra ligera sin utilidades ni entornos *Perl*. |
| **CVE-2026-41992** | **HIGH** | OS Base (`gzip`) | Archivos comprimidos falsos pueden saturar el programa y apagar la app. | Eliminación de herramientas de descompresión heredadas de Debian. |
| **CVE-2025-69720** | **HIGH** | OS Base (`ncurses`) | Códigos raros enviados a la consola pueden colgar el sistema o dar control externo. | Transición a Alpine (reduce la superficie de comandos expuesta). |
| **CVE-2026-54369** | **HIGH** | OS Base (`libacl1`) | Un atacante interno puede saltar carpetas usando un error de permisos. | Reemplazo del sistema operativo por una distribución minimalista. |
| **CVE-2026-44788** | **HIGH** | NuGet (`SharpCompress`) | Ataque *Zip Slip*: guardar archivos maliciosos fuera del sitio permitido al descomprimir. | Actualización del paquete NuGet a su última versión estable segura. |
| **CVE-2026-44302** | **HIGH** | NuGet (`Snappier`) | Datos maliciosos congelan la app en un bucle eterno, tumbando el servicio. | Actualización de la librería NuGet e implementación de límites de tiempo. |
| **CVE-2025-9708** | **MEDIUM** | NuGet (`KubernetesClient`)| Conexiones a ciegas sin verificar seguridad, permitiendo espionaje de datos. | Actualización del paquete NuGet y activación de validación obligatoria. |



## 3. Decisiones de Hardening en el Dockerfile

El archivo de configuración de Docker se transformó utilizando una estrategia optimizada para seguridad activa:

**Adopción de Alpine Linux (`8.0-alpine`):** Se reemplazó la imagen base tradicional de Linux (Debian) por una versión minimalista orientada a la seguridad. Alpine utiliza una librería de sistema reducida (`musl`) y **carece por completo de paquetes innecesarios, compiladores secundarios o utilidades obsoletas (como Perl)**. Esto redujo el reporte de fallos del sistema operativo a cero de forma inmediata.



## Explicación de cada corrección aplicada al código de la aplicación

Para solucionar los fallos detectados dentro de las librerías de .NET, se modificó el archivo de configuración del proyecto (`CatalogMicroservice.csproj`):

* **Riesgo Mitigado:** Escritura arbitraria de archivos en el disco duro (*Zip Slip*), denegaciones de servicio por congelamiento de CPU y fugas de datos en tránsito por falta de validación en certificados SSL.
* **Acción Realizada:** Se incrementaron las versiones de las dependencias directas en las referencias del proyecto:
  ```xml
  <PackageReference Include="KubernetesClient" Version="17.0.14" />
  <PackageReference Include="Snappier" Version="1.3.1" />
  <PackageReference Include="SharpCompress" Version="0.37.2" />


- Secretos hardcodeados en appsettings.json: se sustituyeron por placeholders y el servicio ahora requiere JWT_SECRET en el entorno o secreto gestionado; evita exposición de claves en VCS.
- RequireHttpsMetadata = false en la configuración JWT: se cambió a requerir HTTPS para no aceptar metadatos/token por canales inseguros.
- Validación de token demasiado permisiva: se activó ValidateIssuerSigningKey y RequireExpirationTime
- IdentityModelEventSource.ShowPII = true: se desactivó para evitar que información sensible se filtre en logs.
- Middleware de JWT que continuaba la pipeline tras token inválido: ahora corta la petición y retorna 401 para evitar procesamiento no autorizado.
- Orden incorrecto de middlewares (autenticación/autorización): UseAuthentication() se ejecuta antes de JwtMiddleware y UseAuthorization() después.
- Falta de validación de entrada en controladores: se añadieron DataAnnotations y comprobaciones (ModelState, longitudes, validaciones de ObjectId) para evitar entradas inválidas o malformadas.
- Swagger/UI expuesto en producción: ahora SwaggerUI está limitado a entorno Development.



## Comando exacto usado para construir y escanear ambas imágenes

* Creacion de la imagen vulnerable original:
    docker build -t catalog-microservice:vulnerable -f src/microservices/CatalogMicroservice/Dockerfile .

* Escaneo con trivy imagen orinigal vulnerable:
    trivy image --format json -o trivy-report-before.txt catalog-microservice:vulnerable

* Creacion de la imagen Hardened:
    docker build -t catalog-microservice:hardened -f src/microservices/CatalogMicroservice/Dockerfile.hardened .

* Escaneo con trivy imagen Hardened:
    trivy image --format json -o trivy-report-after.txt catalog-microservice:hardened



## Captura o output de Trivy mostrando **0 vulnerabilidades**


![0 vulnerabilidades](image.png)

Report Summary

┌──────────────────────────────────────────────────────────────────────────────────┬─────────────┬─────────────────┬─────────┐
│                                      Target                                      │    Type     │ Vulnerabilities │ Secrets │
├──────────────────────────────────────────────────────────────────────────────────┼─────────────┼─────────────────┼─────────┤
│ catalog-microservice:hardened (alpine 3.23.5)                                    │   alpine    │        0        │    -    │
├──────────────────────────────────────────────────────────────────────────────────┼─────────────┼─────────────────┼─────────┤
│ app/CatalogMicroservice.deps.json                                                │ dotnet-core │        0        │    -    │
├──────────────────────────────────────────────────────────────────────────────────┼─────────────┼─────────────────┼─────────┤
│ usr/share/dotnet/shared/Microsoft.AspNetCore.App/8.0.28/Microsoft.AspNetCore.Ap- │ dotnet-core │        0        │    -    │
│ p.deps.json                                                                      │             │                 │         │
├──────────────────────────────────────────────────────────────────────────────────┼─────────────┼─────────────────┼─────────┤
│ usr/share/dotnet/shared/Microsoft.NETCore.App/8.0.28/Microsoft.NETCore.App.deps- │ dotnet-core │        0        │    -    │
│ .json                                                                            │             │                 │         │
└──────────────────────────────────────────────────────────────────────────────────┴─────────────┴─────────────────┴─────────┘
Legend:
- '-': Not scanned
- '0': Clean (no security findings detected)



## Lecciones Aprendidas y Recomendaciones para CI/CD

* Evitar Tags de Imágenes Genéricos: Usar FROM dotnet/aspnet:8.0 arrastra componentes masivos y desactualizados del sistema operativo. Es vital acotar el entorno usando variantes declarativas y ligeras como -alpine.

* Automatizar Escaneos en el Pipeline: Integrar Trivy como un paso obligatorio inmediatamente después de la compilación de la imagen en tu pipeline de CI/CD (GitHub Actions, GitLab CI o Azure Pipelines).

* Política de Rebuild Periódico: Las vulnerabilidades en sistemas operativos son descubiertas a diario. Se recomienda programar tareas de compilación semanales (cron-jobs) para asegurar que las imágenes en producción absorban automáticamente los últimos parches de seguridad liberados por el proveedor de la imagen base.
