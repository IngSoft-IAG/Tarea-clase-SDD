# Spec-Driven Development – RoomBooking API

## Workflow elegido

**Spec-Kit lite (híbrido propio).** No usamos el CLI de GitHub Spec Kit ni el de OpenSpec, pero adoptamos las ideas centrales de ambos. Documentamos el workflow a continuación siguiendo el requisito del enunciado ("Si no usan una herramienta externa").

### Ideas tomadas de cada referencia

- **GitHub Spec Kit (`spec-driven.md`)** – Fases separadas `specify → plan → tasks → implement`. La spec es la fuente de verdad ejecutable, no un documento descartable. La spec describe *qué* y *por qué*; el *cómo* va en `plan.md`.
- **OpenSpec** – Una spec por *change* (por feature), no un documento monolítico. Cada scenario Given/When/Then mapea 1:1 a un test.
- **agent-skills (Addy Osmani)** – Cada fase produce un entregable concreto con criterio de "done" explícito antes de avanzar.

### Fases del workflow

| # | Fase | Entregable | Criterio de done |
|---|------|------------|------------------|
| 1 | Specify | `specs/<feature>/spec.md` | ≥2 scenarios G/W/T; sin detalles de C#/EF/firmas |
| 2 | Plan | `specs/<feature>/plan.md` | Archivos a tocar + firmas + mapeo scenario → comportamiento |
| 3 | Tasks | `specs/<feature>/tasks.md` | Lista ordenada (domain → DTO → service → controller → tests) |
| 4 | Implement | Código C# | `dotnet build` verde |
| 5 | Verify | Tests por scenario | `dotnet test` verde + tabla scenario → test |

### Reglas de la spec

1. La spec **no menciona** C#, EF Core, tipos específicos, nombres de métodos ni archivos. Esos detalles viven en `plan.md`.
2. Cada scenario tiene formato `GIVEN / WHEN / THEN` con resultado verificable (status HTTP + efecto en el estado).
3. Cambiar la spec después de implementar es válido pero requiere actualizar tests y código en el mismo commit.

### Cómo decidimos qué archivos modificar

El mapeo está en `plan.md` de cada feature. Regla general derivada del código existente:

- Entidad nueva/modificada → `Domain/`
- Contratos de request/response → `DTOs/`
- Lógica de negocio y validaciones → `Services/`
- Binding HTTP y mapeo de errores a status codes → `Controllers/`
- Un archivo de tests por service, con un `[TestMethod]` por scenario

### Cobertura: cómo comprobamos que cada scenario tiene test

Cada `spec.md` de feature termina con una tabla **Scenario → Test**. El nombre del test sigue la convención `Scenario_<N>_<NombreCorto>`. En la Fase 5 verificamos que la tabla está completa.

### Asistencia de IA

El workflow es compatible con el demo de `sequential-handoff-demo.md` del repo (Planner → Implementer → Tester). Las fases 1-3 corresponden al Planner, la fase 4 al Implementer, la fase 5 al Tester.

## Features en scope

| Feature | Spec | Estado |
|---------|------|--------|
| A – Crear Reserva | [`specs/create-reservation/spec.md`](specs/create-reservation/spec.md) | Pendiente |
| B – Cancelar Reserva | [`specs/cancel-reservation/spec.md`](specs/cancel-reservation/spec.md) | Pendiente |

## Modelo de datos compartido (Reservation)

Acordado transversalmente para ambos features. Vive acá porque las dos specs lo asumen.

| Campo | Tipo lógico | Obligatorio | Notas |
|-------|-------------|-------------|-------|
| Id | entero | sí | PK auto-generada |
| RoomId | entero | sí | FK a Room existente |
| UserId | entero | sí | FK a User existente |
| StartTime | fecha-hora UTC | sí | Inicio de la reserva |
| EndTime | fecha-hora UTC | sí | Fin de la reserva; debe ser posterior a StartTime |

**Decisiones de diseño globales (fijas para ambos features):**

1. **Cancelación = hard delete.** No hay campo `Status`. Cancelar una reserva la borra de la base.
2. **Solapamientos en la misma sala = bloqueados.** Dos reservas activas para la misma `RoomId` no pueden tener intervalos `[StartTime, EndTime)` que se superpongan.
3. **Validaciones estrictas al crear.** Ver scenarios del feature A.
4. **Tiempos en UTC.** La API no hace conversión de zona horaria; el cliente manda UTC.

## Non-goals (fuera de scope de esta tarea)

- Autenticación / autorización de usuarios.
- Modificar una reserva existente (no hay PUT/PATCH).
- Listar reservas (feature D no está elegido).
- Consultar disponibilidad (feature C no está elegido).
- Notificaciones, emails, recordatorios.
- Auditoría (`CreatedAt`, `CancelledBy`, etc.).
- Zonas horarias.