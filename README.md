# Proyecto de Programación 1 y EFV

## Descripción
Juego cooperativo 2D desarrollado en Unity con sistema de oleadas de enemigos, recolección de recursos e inventario compartido.

## Características
- **Sistema Cooperativo**: Soporte para 2 jugadores
- **Sistema de Oleadas**: Enemigos aparecen en oleadas progresivas
- **Sistema de Recursos**: Recolección de madera y otros materiales
- **Sistema de Inventario**: Inventario compartido entre jugadores
- **Sistema de Interacciones**: Interacción con objetos del mundo

## Estructura del Proyecto

### Scripts Principales
- `GameManager.cs` - Controlador principal del juego
- `SceneSetup.cs` - Configuración automática de la escena
- `SystemTester.cs` - Pruebas de sistemas

### Sistemas
- **Movimiento**: `PlayerMovement.cs`
- **Cooperativo**: `CooperativeManager.cs`
- **Inventario**: `InventorySystem.cs`, `InventoryUI.cs`
- **Recursos**: `ResourceManager.cs`, `ResourceNode.cs`, `TreeResource.cs`
- **Enemigos**: `EnemyController.cs`, `WaveManager.cs`, `EnemySpawner.cs`
- **Interacciones**: `InteractionSystem.cs`
- **UI**: `CombatUI.cs`, `InteractionPrompt.cs`

### Prefabs Incluidos
- `Player.prefab` - Prefab de jugador
- `BasicEnemy.prefab` - Enemigo básico
- `TreeResource.prefab` - Recurso de árbol
- `ResourceNode.prefab` - Nodo de recurso genérico

## Configuración

1. Abrir el proyecto en Unity 2022.3 o superior
2. Cargar la escena `SampleScene`
3. Agregar el script `SceneSetup` a un GameObject en la escena
4. Configurar los prefabs y puntos de spawn en el inspector
5. Ejecutar el juego

## Controles

### Jugador 1
- **Movimiento**: WASD
- **Interacción**: E
- **Inventario**: Tab

### Jugador 2
- **Movimiento**: Flechas
- **Interacción**: Enter
- **Inventario**: Tab (compartido)

### Debug (Modo Debug activado)
- **R**: Reiniciar juego
- **P**: Pausar/Reanudar
- **C**: Limpiar inventario

## Sistemas de Juego

### Sistema Cooperativo
- Soporte para 2 jugadores simultáneos
- Inventario compartido
- Progreso conjunto

### Sistema de Oleadas
- Enemigos aparecen en oleadas
- Dificultad progresiva
- Spawns automáticos

### Sistema de Recursos
- Recolección de madera de árboles
- Recursos renovables
- Sistema de regeneración

### Sistema de Inventario
- Inventario compartido entre jugadores
- UI intuitiva
- Gestión automática de items

## Desarrollo

### Arquitectura
- Patrón Singleton para GameManager
- Sistemas modulares independientes
- Eventos para comunicación entre sistemas
- Configuración automática de escena

### Testing
- Script `SystemTester` para verificar funcionamiento
- Logs detallados en consola
- Modo debug integrado

## Notas Técnicas
- Unity 2022.3+
- Input System (legacy)
- 2D Renderer
- Prefabs configurados automáticamente