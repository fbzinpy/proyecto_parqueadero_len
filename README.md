# Parqueadero USC

Sistema de gestión de un parqueadero con **95 cupos** (75 para carros y 20 para motos), pensado para el campus de la Universidad Santiago de Cali. Incluye una **aplicación web** y una **aplicación de escritorio** que trabajan sobre la **misma base de datos**, además de una **página pública** para que los conductores consulten desde el celular si hay cupo, sin iniciar sesión.

> Proyecto académico de *Lenguajes de Programación · Miniproyecto 2*.

---

## Características

**Para el personal (Administrador y Operador)**
- **Panel en vivo:** cupos ocupados y libres, ocupación en porcentaje y plano ilustrado del parqueadero con cada cupo seleccionable.
- **Registro de entradas:** por placa y tipo de vehículo, con asignación automática del primer cupo libre o elección manual desde el plano.
- **Registro de salidas:** cálculo del cobro por hora o fracción (mínimo 1 hora) con la tarifa del tipo de vehículo.
- **Historial** de salidas con tiempo y total cobrado.
- **Cupo seleccionado:** código, tipo, estado, zona, distancia a la portería y, si está ocupado, placa y hora de entrada.

**Solo para el Administrador**
- Resumen del día: ingresos, vehículos atendidos, ocupación y operadores.
- Edición de tarifas por hora.
- Gestión de usuarios y roles.

**Para los conductores (sin cuenta)**
- Página `/Disponibilidad`: indica si hay cupo para carro y moto y muestra el plano con cupos libres y ocupados, actualizado solo cada 10 segundos.
- **Privacidad:** esta página nunca muestra placas, horas ni datos de usuarios. Solo qué cupos están libres u ocupados.

## Tecnologías

| Capa | Tecnología |
|---|---|
| Lenguaje y plataforma | C# 14 / .NET 10 |
| Web | ASP.NET Core MVC (Razor), CSS propio, JavaScript sin librerías |
| Escritorio | Windows Forms con dibujo del plano en GDI+ |
| Datos | Entity Framework Core 10 + SQLite |
| Autenticación | ASP.NET Core Identity (roles Administrador y Operador) |

## Arquitectura

La solución tiene tres proyectos. Toda la lógica del negocio vive en `Core`, de modo que la web y el escritorio aplican exactamente las mismas reglas.

```
Parqueadero.slnx
├── Parqueadero.Core        Entidades, reglas de negocio, acceso a datos y migraciones
├── Parqueadero.Web         Aplicación web ASP.NET Core MVC
└── Parqueadero.Escritorio  Aplicación de escritorio Windows Forms
```

**Parqueadero.Core**
- `Entidades/`: `Cupo`, `Ticket`, `Tarifa`, `TipoVehiculo`.
- `Servicios/`:
  - `ServicioParqueadero`: entradas, salidas, historial, tarifas y resumen del día.
  - `EstadoParqueadero`: estado en memoria de los cupos y vehículos.
  - `AsignadorCupos`: elige el primer cupo libre del tipo pedido.
  - `CalculadoraCobro`: horas cobradas y total a pagar.
  - `PlanoParqueadero`: posiciones del plano y distancia a la portería.
  - `ComparacionEstructuras`: mide con `Stopwatch` las estructuras elegidas contra alternativas.
- `Datos/`: `ParqueaderoDbContext`, datos iniciales (cupos, tarifas, roles y usuarios) y la ruta de la base de datos.

**Estructuras de datos.** `EstadoParqueadero` usa un `Dictionary` de placa a ticket para buscar un vehículo en O(1) al registrar una salida, y un `HashSet` con los cupos ocupados para saber si un cupo está libre también en O(1).

**Base de datos compartida.** Un único archivo `parqueadero.db` se guarda en la carpeta de la solución, junto a `Parqueadero.slnx`. La web y el escritorio lo encuentran subiendo desde su carpeta de ejecución, así que ven los mismos datos al instante.

## Requisitos

- [SDK de .NET 10](https://dotnet.microsoft.com/download)
- **Windows** para la aplicación de escritorio (la web corre en cualquier sistema compatible con .NET)
- La herramienta `dotnet-ef` solo si vas a crear migraciones: `dotnet tool install --global dotnet-ef`

## Cómo ejecutarlo

```bash
git clone https://github.com/fbzinpy/proyecto_parqueadero_len.git
cd proyecto_parqueadero_len
dotnet build
```

**Aplicación web**

```bash
dotnet run --project Parqueadero.Web
```

Abre la dirección que muestre la consola (por defecto `http://localhost:5233`).

**Aplicación de escritorio**

```bash
dotnet run --project Parqueadero.Escritorio
```

Al arrancar por primera vez, la web o el escritorio (el que se abra primero) aplican las migraciones y crean el archivo `parqueadero.db` con los 95 cupos, las tarifas, los roles y los usuarios de prueba.

### Usuarios iniciales

Al crear la base de datos se generan dos usuarios de demostración, uno con rol **Administrador** y otro con rol **Operador**. Sus datos están en [`DatosIniciales.cs`](Parqueadero.Core/Datos/DatosIniciales.cs). Cámbialos antes de usar el sistema fuera de un entorno de pruebas, y no los publiques si despliegas la aplicación.

### Tarifas iniciales

| Vehículo | Valor por hora |
|---|---|
| Carro | $3.000 |
| Moto | $1.500 |

El Administrador puede cambiarlas desde la pantalla de administración.

## Rutas principales de la web

| Ruta | Acceso | Descripción |
|---|---|---|
| `/Disponibilidad` | Público | ¿Hay cupo? en vivo, sin placas |
| `/Disponibilidad/Estado` | Público | JSON mínimo: `libresCarro`, `libresMoto`, `ocupados` |
| `/Cuenta/Login` | Público | Inicio de sesión del personal |
| `/` | Administrador y Operador | Panel con el plano de cupos |
| `/Movimientos/Entrada` | Administrador y Operador | Registrar entrada |
| `/Movimientos/Salida` | Administrador y Operador | Registrar salida y cobrar |
| `/Movimientos/Historial` | Administrador y Operador | Historial de salidas |
| `/Admin` | Administrador | Resumen, tarifas y usuarios |

## Base de datos y migraciones

Las migraciones están en `Parqueadero.Core/Migrations` y se aplican solas al iniciar la web. Para crear una nueva tras cambiar el modelo:

```bash
dotnet ef migrations add NombreDeLaMigracion --project Parqueadero.Core --startup-project Parqueadero.Web
```

Si cambia el plano o los datos iniciales (por ejemplo, el número de cupos), borra tu `parqueadero.db*` local para que se recree con los datos nuevos. El archivo no se versiona.

## Diseño

La interfaz usa el estilo **"Noche + campus USC"**, el mismo en la web y en el escritorio:

- Fondo `#0e0f12`, tarjetas `#17191e` con borde `#262930`, esquinas rectas y sin sombras.
- Un único color de acento, amarillo `#ffc000`.
- Tipografías **Barlow Condensed** (títulos y números, en mayúsculas) y **Barlow** (texto).
- Diseño adaptable: la web se ve en computador, tablet y celular.
