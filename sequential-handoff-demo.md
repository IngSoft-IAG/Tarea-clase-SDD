# Demo: Flujo secuencial con handoffs (Feature Planner → Implementer → Tester)

Ejemplo de flujo secuencial entre agentes usando handoffs de Copilot en VS Code.
Cada agente tiene un rol acotado y al terminar propone el siguiente paso con un botón.

```
Feature Planner  ──[Start Implementation]──►  Feature Implementer  ──[Write Tests]──►  Feature Tester
   (solo lectura,                                 (escribe código,                        (escribe tests,
    genera el plan)                               no escribe tests)                        no toca prod)
```

## Archivos del ejemplo

- `.github/agents/feature-planner.agent.md` — analiza el codebase y produce un plan escrito. Sin `edit`.
- `.github/agents/feature-implementer.agent.md` — implementa DTOs, service y controller. Sin tests.
- `.github/agents/feature-tester.agent.md` — escribe unit tests sobre lo implementado. Sin tocar prod.

## Cómo ejecutarlo

1. Abrir el repo en VS Code con Copilot Chat en **Agent mode**.
2. Seleccionar el agente **Feature Planner** en el selector de agentes.
3. Escribir: `Plan the POST /api/reservations endpoint to create a booking`.
4. El agente lee el codebase y devuelve un plan estructurado con endpoint, scenarios y pasos de implementación.
5. Al terminar aparece el botón **Start Implementation** — hacer clic para cambiar al Implementer con el contexto del plan ya cargado.
6. El Implementer escribe DTOs, service y controller. Al terminar aparece el botón **Write Tests**.
7. Hacer clic en **Write Tests** para pasar al Tester. El Tester escribe los casos de test, corre `dotnet test` y reporta.

## Por qué cada agente tiene tools distintos

| Agente | Tiene `edit` | Tiene `run/terminal` | Por qué |
|---|---|---|---|
| Feature Planner | No | No | Fuerza pensar antes de actuar. No puede modificar nada. |
| Feature Implementer | Sí | Sí (`dotnet build`) | Escribe código de producción y verifica que compila. |
| Feature Tester | Sí | Sí (`dotnet test`) | Escribe tests y verifica que pasan. No toca prod. |

La restricción de tools no es cosmética: si el Planner pudiera editar archivos, tendería a implementar directo en lugar de planear. El handoff pasa el contexto entre agentes sin copy-paste manual.
