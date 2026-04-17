# Tarea practica - Spec-Driven Development

## Objetivo
Aplicar Spec-Driven Development sobre un codebase real.

## Stack del proyecto
- .NET 10 Web API
- EF Core InMemory
- MSTest

## Estructura entregada
Este repositorio ya incluye:
- CRUD funcional de salas en `RoomBookingApi/Controllers/RoomsController.cs`
- CRUD funcional de usuarios en `RoomBookingApi/Controllers/UsersController.cs`
- Entidades base en `RoomBookingApi/Domain`
- DbContext en memoria y datos semilla en `RoomBookingApi/Data`

Este repositorio se entrega con esqueleto para completar:
- `RoomBookingApi/Domain/Reservation.cs`
- `RoomBookingApi/Services/ReservationService.cs`
- `RoomBookingApi/DTOs/ReservationDto.cs`
- `RoomBookingApi/Controllers/ReservationsController.cs`
- `RoomBookingApi.Tests/ReservationServiceTests.cs`
- `spec.md`

## Regla de oro
Antes de escribir codigo o pedirle algo a Copilot, el grupo debe tener una spec escrita con al menos 2 scenarios Given/When/Then.

Ademas, todo cambio debe cumplir la constitucion del proyecto en `.specify/memory/constitution.md`, en particular:
- calidad de codigo verificable en review,
- tests automatizados trazables a escenarios,
- consistencia de experiencia de usuario en validaciones/errores,
- y requisitos de performance medibles por feature.

## Features a elegir (minimo 2)
1. Feature A - Crear Reserva
2. Feature B - Cancelar Reserva
3. Feature C - Consultar Disponibilidad
4. Feature D - Listar Reservas del Usuario

## Workflow de trabajo
No es obligatorio seguir un workflow unico.

Cada grupo debe elegir una de estas opciones:
1. Usar una herramienta vista en clase, por ejemplo GitHub Spec Kit, OpenSpec u otra similar, y seguir el workflow que propone esa herramienta.
2. Trabajar sin herramienta externa, definiendo su propio workflow de Spec-Driven Development antes de implementar.

## Requisitos del workflow elegido
Independientemente de la herramienta o enfoque, el grupo debe:
1. Definir explicitamente que workflow va a seguir.
2. Escribir una spec antes de implementar.
3. Asegurarse de que la spec incluya al menos 2 scenarios Given/When/Then.
4. Implementar el feature elegido tomando la spec como fuente de verdad.
5. Derivar los tests a partir de los scenarios de la spec.
6. Validar que el codigo final cumple lo especificado.
7. Verificar cumplimiento de la constitucion (calidad, testing, UX consistente y performance) antes de merge.

## Si usan una herramienta externa
El entregable debe dejar claro:
1. Que herramienta eligieron.
2. Que workflow propone esa herramienta.
3. Como aplicaron ese workflow en este proyecto.

## Si no usan una herramienta externa
Deben definir y documentar su propio workflow.

Como minimo, ese workflow propio deberia responder:
1. Como van a escribir y revisar la spec.
2. Como van a decidir que archivos modificar.
3. Como van a implementar con Copilot u otra asistencia.
4. Como van a comprobar que cada scenario tiene cobertura.
5. Como van a validar el resultado final.

## Comandos utiles
```powershell
# Restaurar dependencias y compilar
dotnet build RoomBooking.sln

# Correr tests
dotnet test RoomBooking.sln

# Levantar API
dotnet run --project RoomBookingApi
```

## Ejemplo para clase: Multi-Perspective Code Review
Este repo incluye un ejemplo reusable de code review con subagentes de Copilot en VS Code.

Archivos incluidos:
- `.github/agents/thorough-reviewer.agent.md`: agente coordinador.
- `.github/agents/correctness-reviewer.agent.md`: revisa logica y edge cases.
- `.github/agents/code-quality-reviewer.agent.md`: revisa mantenibilidad.
- `.github/agents/security-reviewer.agent.md`: revisa validacion y exposicion de datos.
- `.github/agents/architecture-reviewer.agent.md`: revisa consistencia estructural.
- `.github/prompts/multi-perspective-review.prompt.md`: slash command para ejecutar la review.

Como mostrarlo en clase:
1. Abrir el repo en VS Code con Copilot Chat habilitado.
2. Ir a Agent mode en el chat.
3. Ejecutar el prompt `/multi-perspective-review`.
4. Pedir algo como: `review the pending changes in the reservation feature` o `review RoomBookingApi/Controllers/ReservationsController.cs and RoomBookingApi/Services/ReservationService.cs`.
5. Mostrar que el agente coordinador lanza subagentes en paralelo, cada uno con una perspectiva distinta.

La idea del ejemplo es mostrar un patron de orquestacion: un agente principal sintetiza hallazgos, mientras agentes mas chicos trabajan con contexto aislado para reducir sesgo entre perspectivas.

## Entregable esperado
Cada grupo debe crear una PR desde una branch con sus numeros de alumnos e indicar lo siguiente en la descripcion de la PR:
1. Una breve explicacion del workflow usado, indicando si siguieron una herramienta o uno propio.
2. Implementacion del feature elegido en API + service + DTO(s).
3. Tests derivados de los scenarios.
4. Breve nota final (3-5 lineas) con ajustes que harian a la spec.
