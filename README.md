# ApiProyectoFinalAYSW

Este proyecto es una solución integral que combina una **API REST Genérica** con un **Frontend en Blazor**, diseñada para gestionar proyectos, entregables, responsables y más, con soporte para múltiples motores de base de datos.

## Tabla de Contenidos
- [Acerca del Proyecto](#acerca-del-proyecto)
- [Características Principales](#características-principales)
- [Tecnologías Utilizadas](#tecnologías-utilizadas)
- [Arquitectura](#arquitectura)
- [Requisitos Previos](#requisitos-previos)
- [Configuración y Ejecución](#configuración-y-ejecución)
- [Capturas de Pantalla](#capturas-de-pantalla)

---

## Acerca del Proyecto
**ApiProyectoFinalAYSW** nace de la necesidad de tener una herramienta flexible y extensible para la administración de datos. La API es capaz de realizar operaciones CRUD (Crear, Leer, Actualizar, Borrar) de forma dinámica sobre cualquier tabla de la base de datos configurada, mientras que el frontend proporciona una interfaz intuitiva para interactuar con estos datos.

## Características Principales
- **CRUD Genérico:** Capacidad de operar sobre tablas de forma dinámica sin necesidad de controladores específicos para cada entidad.
- **Multi-Base de Datos:** Soporte nativo para SQL Server, PostgreSQL, MySQL y MariaDB mediante un patrón de repositorio abstracto.
- **Autenticación Segura:** Implementación de JWT (JSON Web Tokens) para proteger los endpoints y gestionar sesiones de usuario.
- **Gestión de Entidades:** Pantallas específicas en Blazor para:
  - Proyectos y Tipos de Proyecto.
  - Actividades y Entregables.
  - Responsables y Usuarios.
  - Archivos y Variables Estratégicas.
- **Políticas de Seguridad:** Configuración de tablas prohibidas mediante JSON para restringir el acceso a datos sensibles.
- **Documentación Interactiva:** Integración con Swagger para probar y explorar la API fácilmente.

## Tecnologías Utilizadas
### Backend
- **ASP.NET Core 9.0**
- **JWT Authentication**
- **Swagger / OpenAPI**
- **BCrypt.Net** (para encriptación de contraseñas)

### Frontend
- **Blazor Web (NET 9.0)**
- **Bootstrap** (para el diseño responsivo)
- **Servicios API Genéricos**

### Bases de Datos Soportadas
- SQL Server
- PostgreSQL
- MySQL / MariaDB

## Arquitectura
El proyecto sigue una arquitectura desacoplada:
1. **Capa de Controladores:** Maneja las peticiones HTTP y delega la lógica a los servicios.
2. **Capa de Servicios:** Contiene la lógica de negocio y coordina las operaciones entre los repositorios.
3. **Capa de Repositorios:** Abstrae la comunicación con el motor de base de datos específico.
4. **Frontend:** Una aplicación Blazor que consume la API mediante un cliente HTTP genérico.

## Requisitos Previos
- **.NET 9.0 SDK** instalado.
- Un motor de base de datos compatible (SQL Server por defecto).
- Visual Studio 2022 o Visual Studio Code.

## Configuración y Ejecución
1. **Base de Datos:** Ejecute el script `QueryBaseDeDatos.sql` en su servidor de base de datos.
2. **Configuración de la API:** Ajuste las cadenas de conexión y el proveedor de base de datos en `ApiBack/appsettings.json`.
3. **Ejecución del Backend:**
   ```bash
   cd ApiBack
   dotnet run
   ```
4. **Ejecución del Frontend:**
   ```bash
   cd FrontendBlazorApi
   dotnet run
   ```

## Capturas de Pantalla
*Aquí puedes añadir imágenes del funcionamiento de la aplicación:*

### Inicio de Sesión
![Login](https://via.placeholder.com/800x400?text=Pantalla+de+Login)

### Panel de Control / Proyectos
![Dashboard](https://via.placeholder.com/800x400?text=Lista+de+Proyectos)

### Gestión de Actividades
![Actividades](https://via.placeholder.com/800x400?text=Gestión+de+Actividades)

---
*Desarrollado para el curso de Aplicación y Servicios Web.*
