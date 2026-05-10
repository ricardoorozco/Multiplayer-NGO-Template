# 🎮 Unity Multiplayer Template — NGO + Relay + UI Toolkit

Una plantilla base, robusta y lista para producción para construir juegos multijugador en Unity. Resuelve la arquitectura, configuración de red y flujo de escenas inicial, para que puedas enfocarte en el juego en sí.

---

## 🛠️ Stack Tecnológico

| Área | Tecnología | Paquete (manifest.json) | Versión |
|---|---|---|---|
| Motor | Unity | N/A | 6 (LTS) / 6000.0+ |
| Networking Core | Netcode for GameObjects (NGO) | `com.unity.netcode.gameobjects` | 2.11.2 |
| Relay, Lobby & Auth | Unity Services Multiplayer | `com.unity.services.multiplayer` | 2.2.2 |
| Herramientas | Unity Multiplayer Center | `com.unity.multiplayer.center` | 1.0.1 |
| Render Pipeline | Universal Render Pipeline (URP) | `com.unity.render-pipelines.universal` | 17.3.0 |
| Interfaz de Usuario | UI Toolkit (`.uxml` + `.uss`) | `com.unity.modules.uielements` | 1.0.0 |
| Controles | New Input System | `com.unity.inputsystem` | 1.19.0 |

> **¿Por qué UI Toolkit?** Funciona con un modelo de estilado basado en web (similar a HTML/CSS), permite separar la lógica del juego de la presentación, y es el estándar moderno de Unity para interfaces en runtime.

> **¿Por qué Unity Relay?** Permite que jugadores se conecten a través de internet usando un simple **código de sala** (ej. `B3X7K9`), sin necesidad de abrir puertos en el router (port forwarding). Relay actúa como intermediario de red.

---

## 🗺️ Flujo de Escenas

```
[00_Splash] ──► [01_MainMenu] ──► [02_Lobby] ──► [03_Game]
      │                │
   Inicializa      Modal Opciones
   Servicios        (Prefab UI)
   Unity + Auth
```

Las escenas están numeradas con prefijo para garantizar el orden correcto en el `Build Settings`.

### `00_Splash` — Inicialización
- Muestra una barra de progreso mientras se realizan tareas críticas en segundo plano.
- **`Bootstrapper.cs`** inicializa `UnityServices` y ejecuta `SignInAnonymouslyAsync()`.
- La autenticación es **obligatoria** antes de poder usar Unity Relay.
- Al completarse, carga automáticamente `01_MainMenu`.

### `01_MainMenu` — Menú Principal
- Interfaz construida con UI Toolkit (`MainMenu.uxml` + `MainStyles.uss`).
- **Jugar** → carga la escena `02_Lobby`.
- **Opciones** → abre el `OptionsModal` (Prefab reutilizable).
- **Salir** → cierra la aplicación (`Application.Quit()`).

### `02_Lobby` — Sala de Espera y Conexión
La escena central donde se gestiona la sesión multijugador antes de entrar al juego.

| Rol | Acción |
|---|---|
| **Host** | Crea una sesión en Unity Relay → recibe un código alfanumérico corto. |
| **Cliente** | Introduce el código del Host → se une a la sesión de Relay. |

Una vez que todos los jugadores están listos, el **Host** inicia la partida. El `NetworkSceneManager` carga `03_Game` en todos los clientes **simultáneamente**, garantizando sincronía desde el primer frame.

### `03_Game` — Escena de Juego
- Escenario vacío configurado y listo para NGO.
- Instancia los `PlayerPrefab` de red al conectarse cada jugador.
- Menú de pausa que **reutiliza** el `OptionsModal` (mismo Prefab del menú principal).
- Lógica de manejo de desconexión: si el Host se desconecta, todos los clientes vuelven a `01_MainMenu`.

---

## 📂 Estructura del Proyecto

```
Assets/
│
├── Scenes/
│   ├── 00_Splash.unity
│   ├── 01_MainMenu.unity
│   ├── 02_Lobby.unity
│   └── 03_Game.unity
│
├── Scripts/
│   ├── Core/
│   │   └── Bootstrapper.cs          # Inicialización de Unity Services y Auth anónimo
│   ├── Networking/
│   │   ├── RelayManager.cs          # Crear/unirse a sesiones de Unity Relay
│   │   └── ConnectionManager.cs     # Manejar ciclo de vida de la conexión y desconexiones
│   └── UI/
│       ├── MainMenuController.cs    # Botones del menú principal
│       ├── LobbyController.cs       # UI de la sala de espera (código, lista de jugadores)
│       ├── OptionsModalController.cs# Opciones: volumen, gráficos, pantalla (con PlayerPrefs)
│       └── SplashController.cs      # Barra de progreso animada del Splash
│
├── UI/
│   ├── Documents/
│   │   ├── MainMenu.uxml            # Estructura del menú principal
│   │   ├── OptionsModal.uxml        # Modal de opciones (reutilizable)
│   │   ├── Lobby.uxml               # Sala de espera
│   │   └── Splash.uxml              # Pantalla de carga
│   └── Styles/
│       └── MainStyles.uss           # Design system global (colores, fuentes, botones, modales)
│
└── Prefabs/
    ├── Network/
    │   ├── NetworkManager.prefab    # Configurado con NGO + Unity Transport
    │   └── Player.prefab            # NetworkObject base del jugador
    └── UI/
        └── OptionsModal.prefab      # Modal de opciones instanciable desde cualquier escena
```

---

## ⚙️ Configuración en Unity Editor

### 1. Build Settings
Agrega las escenas en este orden exacto:
```
0 → Assets/Scenes/00_Splash.unity
1 → Assets/Scenes/01_MainMenu.unity
2 → Assets/Scenes/02_Lobby.unity
3 → Assets/Scenes/03_Game.unity
```

### 2. Project Settings → Input System
- Asegúrate de que `Active Input Handling` esté en **"New Input System Package"** (o ambos, si necesitas compatibilidad legacy).
- Ruta: `Edit → Project Settings → Player → Active Input Handling`.

### 3. Unity Services (en Unity Dashboard)
Para que Unity Relay funcione correctamente:
1. Ve a `Edit → Project Settings → Services` y vincula tu proyecto.
2. En el [Unity Dashboard](https://cloud.unity.com), habilita los servicios:
   - **Relay**
   - **Authentication**
   - **Lobby** (recomendado para gestionar jugadores en sala)

### 4. NetworkManager
- El `NetworkManager.prefab` debe estar configurado con `Unity Transport` como componente de transporte.
- Debe ser marcado como **DontDestroyOnLoad** para persistir entre escenas.
- El `PlayerPrefab` debe tener el componente `NetworkObject`.

---

## 🧩 Patrones y Convenciones

| Convención | Descripción |
|---|---|
| **Namespace** | `MultiplayerTemplate.Core`, `MultiplayerTemplate.Networking`, `MultiplayerTemplate.UI` |
| **Singleton** | Los managers globales (`NetworkManager`, `OptionsModalController`) usan el patrón Singleton. |
| **Persistencia de Opciones** | `PlayerPrefs` con claves tipadas: `MasterVol`, `MusicVol`, `SFXVol`, `FullScreen`, `Quality`. |
| **Nombres de Escenas** | Siempre referenciar por nombre (`string`), nunca por índice. |
| **Prefijos UXML** | Botones: `Btn`, Sliders: `Sld`, Toggles: `Tgl`, Paneles: `Pnl`. |

---

## 🚧 Roadmap de la Plantilla

- [x] Estructura de carpetas y archivos base
- [x] `Bootstrapper.cs` — Inicialización de Unity Services, Auth anónimo e integración con barra de progreso
- [x] `SplashController.cs` — Barra de progreso conectada al Bootstrapper
- [x] `MainMenu.uxml` + `MainStyles.uss` — Design system y menú principal
- [x] `OptionsModal.uxml` + `OptionsModalController.cs` — Modal de opciones con persistencia y aplicación real de volumen/gráficos
- [x] `MainMenuController.cs` — Lógica de botones del menú principal
- [x] `RelayManager.cs` — Crear y unirse a sesiones con Unity Relay
- [x] `ConnectionManager.cs` — Ciclo de vida de la conexión, desconexión limpia y manejo de pérdida de sesión
- [x] `LobbyController.cs` — UI de la sala de espera con validación y normalización de códigos
- [x] `Lobby.uxml` — Interfaz de sala de espera
- [ ] `NetworkManager.prefab` — Configuración base de Netcode for GameObjects *(manual en Unity Editor)*
- [ ] `Player.prefab` — NetworkObject base del jugador *(manual en Unity Editor)*
- [ ] Escenas Unity (`.unity`) — Creación y ensamblaje *(manual en Unity Editor)*

---

## ⚠️ Notas Técnicas Importantes

### Volumen de Audio
El `OptionsModalController` gestiona el volumen con sliders de `0` a `100`. Internamente esto se convierte a `0.0 – 1.0` y se aplica a `AudioListener.volume`, que es el control global de audio de Unity. Para música y SFX por separado, conecta los valores a los `AudioMixer` de tu juego desde el método `ApplySettings()`.

### Ciclo de Vida de la Conexión
El `ConnectionManager` usa `OnClientStopped` (no `OnClientDisconnectCallback`) para detectar cuándo el **cliente local** pierde su propia conexión. Esto evita el bug clásico donde el Host recibe un callback por cada cliente que se desconecta y termina siendo redirigido al menú incorrectamente.

### Cierre de Sesión NGO
Siempre usa `ConnectionManager.Disconnect()` para cerrar la sesión de red. Este método llama a `NetworkManager.Singleton.Shutdown()` antes de cargar el menú, garantizando que no queden conexiones activas ni handles de Relay sin liberar.

---

## 📄 Licencia

Este proyecto es una plantilla de uso libre. Úsala como punto de partida para tus propios proyectos multijugador con Unity.
