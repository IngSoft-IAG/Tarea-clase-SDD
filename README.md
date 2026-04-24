# Tarea practica - SDLC con AI Skills

## Objetivo
Aplicar un ciclo de desarrollo completo asistido por AI skills, desde la ideacion hasta la implementacion, sobre un codebase real de reservas de salas.

## Stack del proyecto
- .NET 8 Web API
- EF Core InMemory
- MSTest

## Estructura del proyecto

El repositorio ya incluye funcionalidad base:
- CRUD completo de **Rooms** (`RoomsController`, `RoomService`)
- CRUD completo de **Users** (`UsersController`, `UserService`)
- Entidad `Reservation` definida pero con logica minima
- Creacion basica de reservas sin validaciones
- Consulta de reserva por ID
- Seed data con 3 salas y 2 usuarios
- Tests esqueleto con solo happy path

## Dinamica de trabajo

Los alumnos se dividen en equipos. Cada equipo elige **una feature** de la lista y la desarrolla siguiendo las 4 fases del workflow obligatorio.

## Features disponibles

Cada equipo elige **una** feature. No se pueden repetir features entre equipos.

### Feature 1 - Validacion completa al crear reserva
La creacion de reservas existe pero no valida nada. Implementar todas las validaciones necesarias: que el usuario exista, que la sala exista, que la sala este activa, que el horario sea valido (inicio < fin), y que no haya conflicto con otras reservas en la misma sala.

### Feature 2 - Cancelar reserva
Permitir cancelar una reserva existente. Decidir si es un DELETE fisico o un cambio de estado (requiere agregar un campo `Status` a la entidad). Considerar que no se deberia poder cancelar una reserva que ya paso.

### Feature 3 - Consultar disponibilidad de una sala
Dado un rango de fecha/hora, devolver si una sala esta disponible o listar los slots libres. Util para que un usuario pueda ver antes de reservar cuando hay lugar.

### Feature 4 - Listar reservas de un usuario
Endpoint para obtener todas las reservas de un usuario especifico. Incluir filtros opcionales por rango de fechas y/o estado. Considerar paginacion si el equipo lo ve necesario.

### Feature 5 - Reservas recurrentes
Permitir crear una reserva que se repita (diaria, semanal) durante un rango de fechas. Validar conflictos en cada ocurrencia. Decidir si se almacenan como reservas individuales o como una entidad padre con ocurrencias.

### Feature 6 - Notificaciones de conflicto
Cuando se intenta crear una reserva que tiene conflicto, en lugar de solo devolver un error, devolver informacion util: que reserva genera el conflicto, quien la tiene, y sugerir los slots libres mas cercanos en esa misma sala.

## Workflow obligatorio - 4 Fases

Cada equipo debe seguir estas 4 fases en orden. Las primeras 3 fases son de definicion y planificacion; recien en la fase 4 se escribe codigo.

---

### Fase 1 - Ideacion y stress-test con `/grill-me`

**Skill:** `grill-me`

El equipo presenta su feature y la skill les hace preguntas una por una para forzarlos a pensar en todos los aspectos: edge cases, decisiones de diseno, impacto en el modelo de datos, que pasa si falla, etc.

**Como ejecutarlo:**
1. Escribir `/grill-me` y presentar la feature elegida.
2. Responder cada pregunta hasta que el grilling termine.
3. El resultado es una comprension profunda de la feature antes de escribir una sola linea.

---

### Fase 2 - PRD (Product Requirements Document) con `/to-prd`

**Skill:** `to-prd`

Con el contexto ganado en la Fase 1, generar un PRD formal que quede como GitHub Issue. El PRD incluye: problema, solucion, user stories, decisiones de implementacion, testing, y que queda fuera de scope.

**Como ejecutarlo:**
1. Despues de terminar el grill-me (o en una nueva sesion explicando el contexto), ejecutar `/to-prd`.
2. La skill analiza el codebase y genera el PRD basado en la conversacion.
3. Se crea un GitHub Issue con el PRD completo.

---

### Fase 3 - Plan de implementacion con `/to-issues`

**Skill:** `to-issues`

Tomar el PRD y descomponerlo en issues de implementacion independientes, organizados como slices verticales (cada issue atraviesa todas las capas: dominio, servicio, controller, tests).

**Como ejecutarlo:**
1. Ejecutar `/to-issues` referenciando el PRD creado en la Fase 2.
2. La skill explora el codebase, propone los slices, y pregunta sobre granularidad y dependencias.
3. Se crean los GitHub Issues con criterios de aceptacion y orden de dependencia.

---

### Fase 4 - Implementacion con `/implement-issue` y `@feature-tester`

**Skill:** `implement-issue` | **Agente:** `feature-tester`

Implementar cada issue usando la skill y el agente disponibles en el repositorio. La skill trae el issue de GitHub con `gh` y lo implementa siguiendo los patrones del codebase. Luego, el agente tester escribe los tests unitarios correspondientes.

**Como ejecutarlo:**
1. Ejecutar `/implement-issue` seguido del numero de issue (ej: `/implement-issue 5`). La skill usa `gh issue view` para leer los criterios de aceptacion y luego implementa.
2. Revisar el codigo generado con el equipo.
3. Usar `@feature-tester` para escribir los tests del codigo implementado.
4. Verificar que todo compila y los tests pasan.

**Entregable de la fase:** Codigo implementado y tests pasando.

---

## Comandos utiles

```bash
# Compilar
dotnet build RoomBooking.sln

# Correr tests
dotnet test RoomBooking.sln

# Levantar API
dotnet run --project RoomBookingApi
```

## Entregable final

Cada equipo debe crear una PR desde una branch con los nombres/numeros de los integrantes. La PR debe incluir:

1. **En la descripcion de la PR:**
   - Feature elegida.
   - Link al Issue del PRD (Fase 2).
   - Links a los Issues de implementacion (Fase 3).
   - Breve reflexion (5-10 lineas): que descubrieron en el grill-me que no habian considerado, como cambio su plan original despues de pasar por las fases de definicion, y que harian diferente la proxima vez.

2. **En el codigo:**
   - Implementacion completa de la feature.
   - Tests derivados de los scenarios del PRD.
