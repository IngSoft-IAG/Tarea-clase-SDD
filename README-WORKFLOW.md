
Workflow utilizado (Spec-Driven Development propio)
En este proyecto no utilizamos una herramienta externa, sino que definimos un workflow propio basado en Spec-Driven Development, donde la spec actúa como fuente de verdad durante todo el proceso.

1. Escritura y revisión de la spec
Antes de implementar, definimos la spec del feature (Crear Reserva) en spec.md, incluyendo escenarios en formato Given/When/Then.

Nos aseguramos de cubrir al menos 2 escenarios:
Caso exitoso de creación de reserva
Caso inválido (por ejemplo, sala ocupada o datos incorrectos)
La spec fue revisada por el equipo antes de comenzar la implementación para validar que:
Los escenarios eran claros
Cubrían los casos relevantes del negocio
2. Decisión de archivos a modificar
A partir de la spec, identificamos las capas necesarias a implementar:

Domain: Reservation.cs (modelo de datos)
DTOs: ReservationDto.cs (entrada/salida de la API)
Service: ReservationService.cs (lógica de negocio)
Controller: ReservationsController.cs (exposición HTTP)
Esta estructura se definió siguiendo el patrón ya existente en el proyecto (Rooms y Users).

3. Implementación con asistencia de IA (Copilot)
Una vez definida la spec:

Utilizamos Copilot para generar código a partir de los escenarios definidos
La implementación se hizo respetando estrictamente la spec como contrato
El equipo validó manualmente el código sugerido por la IA, asegurando:
Coherencia con la lógica del negocio
Uso correcto del DbContext en memoria
Manejo adecuado de errores y validaciones
4. Derivación y cobertura de tests
Los tests fueron creados directamente a partir de los escenarios Given/When/Then de la spec:

Cada escenario tiene al menos un test asociado
Se validan tanto casos exitosos como casos de error
Se utilizó MSTest para implementar los tests en ReservationServiceTests.cs
Esto asegura trazabilidad directa entre spec y tests.

5. Validación del resultado final
Para validar que la implementación cumple con la spec:

Se ejecutaron todos los tests (dotnet test) y se verificó que pasen correctamente
Se probó manualmente el endpoint de reservas
Se revisó que el comportamiento del sistema coincida con lo definido en los escenarios
🧩 Nota final
Como mejora futura, agregaríamos más escenarios edge (por ejemplo, validación de fechas solapadas o múltiples reservas concurrentes) y una mayor formalización de reglas de negocio en la spec.

