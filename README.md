# School Management System

Backend desarrollado en *ASP.NET Core Web API* para gestionar asistencia escolar.

El proyecto comenzó como una herramienta simple para registrar ausencias en dos cursos,
pero está diseñado para evolucionar hacia un sistema completo de gestión escolar.

Este proyecto está siendo desarrollado con foco en:

- Buenas prácticas
- Arquitectura escalable
- Código limpio
- Aprendizaje profundo de .NET

---

# Tech Stack

Tecnologías utilizadas actualmente:

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- InMemory Database (para desarrollo)
- Swagger / OpenAPI
- Git & GitHub

---

# Arquitectura actual

El sistema actualmente es un *monolito simple*, organizado en capas básicas:
Componentes principales:

*Controllers*

Contienen los endpoints HTTP de la API.

*Models*

Representan las entidades del dominio.

*Data*

Contiene el DbContext que conecta Entity Framework con la base de datos.

---

# Funcionalidades actuales

Actualmente el sistema permite:

- Crear estudiantes
- Listar estudiantes
- Probar la API mediante Swagger

Endpoints disponibles:
 
 GET /api/student
 POST /api/student

---

# Ejecución del proyecto

### Clonar el repositorio
git clone https://github.com/mpastorini9/SchoolManagementSystem
### Entrar al proyecto
cd SchoolManagementSystem
### Ejecutar la API
dotnet run --project SchoolManagementSystem.Api
La API estará disponible en:
https://localhost:5003/swagger

 ---

# Estado del proyecto

El sistema está en desarrollo activo.

Actualmente se encuentra implementado:

- Entidad Student
- DbContext con Entity Framework
- Persistencia en base de datos InMemory
- Endpoints básicos para estudiantes

---

# Roadmap

Próximos pasos planificados:

- Implementar entidad Course
- Implementar entidad Attendance
- Persistencia con SQL Server
- Migraciones con Entity Framework
- Arquitectura limpia (Clean Architecture)
- Sistema de autenticación
- Panel administrativo

---

# Objetivo del proyecto

Este proyecto tiene dos objetivos principales:

1. Construir un sistema real de gestión de asistencia escolar.
2. Servir como proyecto demostrable para desarrollo backend con .NET.

---

# Autor

Desarrollado por *Matias Pastorini*.