# Parqueadero — Miniproyecto 2, Lenguajes de Programación

Sistema de gestión de parqueadero en C# / .NET. Proyecto académico hecho en pareja.

## Cómo trabajar (lo más importante)

- Haz **exactamente** lo que se pide en cada mensaje. Nada más.
- No agregues funcionalidades, pantallas, capas, patrones, paquetes ni archivos que no se hayan pedido.
- Nada de sobreingeniería: la solución más simple que cumpla el requisito. El proyecto se mejora poco a poco.
- Si algo no está claro o falta información, pregunta antes de escribir código.
- Antes de un cambio grande, explica brevemente qué vas a hacer y por qué.
- Explica las decisiones de forma sencilla: tenemos que sustentar el proyecto y entender todo el código.
- No crees migraciones de EF Core a menos que se pida explícitamente.
- En commits, Pull Requests e Issues no incluyas `Co-Authored-By`, "Generated with Claude Code" ni ninguna mención a Claude.

## Requisitos del enunciado (seguir al pie de la letra)

1. **Arquitectura compartida**:
   - Una biblioteca de clases base con la lógica de dominio y de negocio.
   - Aplicación web con el patrón **MVC**.
   - Aplicación de **escritorio**.
   - Almacenamiento en base de datos con **Entity Framework Core**.
2. **Estructuras de datos**: justificar cada estructura usada, con ejemplos comparativos de por qué se eligió.
3. **Autenticación y roles**: **ASP.NET Core Identity** para el inicio de sesión y al menos **2 roles** diferentes.
4. **Diseño responsive**: la web debe adaptarse a tablet y smartphone.
5. **Repositorio en GitHub**.

Usar los componentes de .NET y las características del lenguaje C# vistos en el curso.

## Decisiones tomadas

- **Proyectos de la solución**:
  - `Parqueadero.Core`: biblioteca de clases (entidades, lógica de negocio y `DbContext`).
  - `Parqueadero.Web`: ASP.NET Core MVC con Identity.
  - `Parqueadero.Escritorio`: WinForms (formularios escritos por código, porque usamos VS Code).
- `Web` y `Escritorio` referencian a `Core`. `Core` no referencia a ningún otro proyecto.
- **Base de datos**: SQLite. La web y el escritorio usan el mismo archivo `.db`. El `.db` no se sube a GitHub.
- **Roles**: Administrador y Operador (se puede agregar Cliente más adelante si se decide).
- **Editor**: VS Code.

## Git

- Ramas: `main`, `web`, `escritorio`. Los cambios llegan a `main` por Pull Request.
