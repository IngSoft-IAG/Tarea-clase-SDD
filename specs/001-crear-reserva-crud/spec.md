# Feature Specification: Gestión CRUD de Reservas

**Feature Branch**: `151282-crear-reserva-crud`  
**Created**: 2026-04-17  
**Status**: Draft  
**Input**: User description: "Crear la specificacion funcional para implementar la feature de crear reserva implementando el CRUD dentro del controller de ReservationsControllers.cs, implementar el modelo dentro del dominio reservations.cs y tambien su DTO dentro de ReservationsDto.cs y por ultimo el servicio dentro de ReservationsService.cs"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Crear una reserva válida (Priority: P1)

Como operador del sistema, quiero registrar una reserva con datos completos y válidos para asegurar que la ocupación de habitaciones quede correctamente planificada.

**Why this priority**: Es el flujo principal que aporta valor inmediato: sin alta de reservas, no existe gestión operativa.

**Independent Test**: Se puede probar creando una reserva con datos válidos y verificando que queda disponible para consulta posterior con toda su información.

**Acceptance Scenarios**:

1. **Given** un usuario autorizado y una habitación existente, **When** solicita crear una reserva con fechas válidas y datos obligatorios completos, **Then** la reserva se registra y se devuelve su identificador junto con el detalle guardado.
2. **Given** una solicitud de creación con datos incompletos o inválidos, **When** se intenta registrar la reserva, **Then** el sistema rechaza la operación con mensajes claros de validación.

---

### User Story 2 - Consultar reservas existentes (Priority: P2)

Como operador del sistema, quiero consultar una reserva específica o listar reservas para revisar disponibilidad, estado y planificación.

**Why this priority**: Permite explotar el dato creado y tomar decisiones operativas diarias.

**Independent Test**: Se puede probar recuperando reservas existentes por identificador y en listado, confirmando que la información coincide con lo registrado.

**Acceptance Scenarios**:

1. **Given** reservas previamente registradas, **When** el usuario solicita el listado, **Then** el sistema devuelve las reservas disponibles con información consistente.
2. **Given** un identificador de reserva existente, **When** el usuario consulta ese identificador, **Then** obtiene el detalle completo de la reserva solicitada.
3. **Given** un identificador inexistente, **When** el usuario consulta la reserva, **Then** el sistema informa que el recurso no existe.

---

### User Story 3 - Actualizar y cancelar reservas (Priority: P3)

Como operador del sistema, quiero modificar o cancelar una reserva para reflejar cambios de agenda y mantener información confiable.

**Why this priority**: Cierra el ciclo operativo de vida de la reserva y evita datos obsoletos.

**Independent Test**: Se puede probar editando una reserva existente y luego cancelándola, verificando respuestas y estado final esperado.

**Acceptance Scenarios**:

1. **Given** una reserva existente, **When** se actualizan campos permitidos con valores válidos, **Then** el sistema guarda los cambios y devuelve el estado actualizado.
2. **Given** una reserva existente, **When** el usuario solicita su cancelación/eliminación, **Then** la reserva deja de estar disponible para operaciones activas y el sistema confirma la acción.

---

### Edge Cases

- Intento de crear una reserva con rango de fechas inválido (fecha fin anterior o igual a fecha inicio).
- Intento de crear o actualizar una reserva para una habitación inexistente o inactiva.
- Intento de crear una reserva superpuesta con otra reserva activa para la misma habitación y período.
- Intento de actualizar o eliminar una reserva que no existe.
- Reintentos de eliminación sobre una reserva ya cancelada/eliminada.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: El sistema DEBE permitir crear una reserva con datos obligatorios: identificador de habitación, identificador de usuario solicitante, fecha/hora de inicio, fecha/hora de fin y estado inicial.
- **FR-002**: El sistema DEBE validar reglas de negocio antes de registrar o actualizar una reserva, incluyendo obligatoriedad de campos, coherencia de fechas y existencia de las entidades relacionadas.
- **FR-003**: El sistema DEBE impedir la creación o actualización de reservas que generen solapamiento temporal no permitido para la misma habitación.
- **FR-004**: El sistema DEBE permitir consultar reservas tanto en listado como por identificador único.
- **FR-005**: El sistema DEBE permitir actualizar una reserva existente en campos autorizados, manteniendo trazabilidad del estado resultante.
- **FR-006**: El sistema DEBE permitir cancelar/eliminar una reserva existente y devolver confirmación explícita del resultado.
- **FR-007**: El sistema DEBE devolver mensajes de error comprensibles cuando una operación no pueda completarse por validación o por inexistencia del recurso.
- **FR-008**: El sistema DEBE mantener la información de reservas persistida para su consulta posterior por los actores autorizados.

### Quality, UX, and Performance Requirements *(mandatory)*

- **QR-001 (Calidad funcional)**: Todas las operaciones CRUD de reservas DEBEN tener criterios de aceptación verificables y resultados consistentes entre operaciones equivalentes.
- **QR-002 (Validación y errores)**: El sistema DEBE responder con mensajes uniformes y accionables ante entradas inválidas y recursos inexistentes.
- **QR-003 (Tiempo de respuesta percibido)**: Las operaciones de consulta y registro de reservas DEBEN completar su respuesta en tiempos compatibles con uso operativo continuo (medidos en criterios de éxito).
- **QR-004 (Confiabilidad de datos)**: Las operaciones confirmadas DEBEN reflejarse de forma consistente en consultas posteriores sin discrepancias de estado.

### Key Entities *(include if feature involves data)*

- **Reserva**: Representa una asignación temporal de una habitación para un usuario; atributos clave: identificador, usuario, habitación, rango temporal, estado y marca de creación/actualización.
- **Habitación**: Recurso reservable sobre el que se aplica disponibilidad y posibles restricciones operativas.
- **Usuario**: Actor de negocio asociado a la reserva (quien solicita o para quien se agenda la reserva).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Al menos el 95% de solicitudes de creación de reserva con datos válidos se completan exitosamente en el primer intento.
- **SC-002**: El 100% de solicitudes con datos inválidos son rechazadas con mensaje de validación explícito indicando causa.
- **SC-003**: El 100% de operaciones de lectura sobre reservas existentes devuelve información consistente con la última operación confirmada.
- **SC-004**: El tiempo de respuesta percibido para crear, consultar, actualizar o cancelar una reserva es menor a 2 segundos en el 95% de los casos durante operación normal.
- **SC-005**: La cobertura de escenarios de aceptación definidos para CRUD de reservas alcanza al menos 90% de casos identificados (incluyendo casos borde críticos).

## Assumptions

- Se asume que los usuarios que operan reservas ya cuentan con permisos de acceso al sistema y autenticación vigente.
- Se asume que la gestión CRUD solicitada aplica únicamente a reservas y no modifica reglas de negocio existentes de usuarios o habitaciones fuera de su relación con la reserva.
- Se asume que la información mínima de usuario y habitación ya existe en el sistema para asociarla al crear una reserva.
- Se asume que cancelación y eliminación se consideran equivalentes desde perspectiva de negocio para esta versión.
