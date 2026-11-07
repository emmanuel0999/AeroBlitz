# Aero Blitz

Juego 2D vertical para Android/iOS hecho en Unity 6 (6000.0.57f1). Todo (UI, sprites, sonidos) se genera por código, así que funciona sin assets externos. Puedes reemplazar el arte cuando quieras.

## Cómo abrirlo

1. En Unity Hub: **Add > D:\Unity\AeroBlitz**.
2. Abre `Assets/AeroBlitz/Scenes/Main.unity` y presiona **Play**.
3. En la vista Game elige una resolución vertical (por ejemplo 1080x1920).
4. Controles en PC: flechas/WASD para mover, **Espacio** para disparar y **Esc** para pausar. En móvil: arrastra el dedo y toca **FUEGO**.
5. Menú **Aero Blitz > Borrar partida guardada** para volver a probar el registro.

## Estructura

```
Assets/AeroBlitz/Scripts/
  Core/         GameConfig (TODO lo configurable), GameManager, PlayerData, SaveSystem, Countries, AudioFX, SpriteFactory
  Data/         ShipCatalog (naves), UpgradeCatalog (mejoras/equipo/pinturas), ProductCatalog (compras reales), LevelDatabase (70 niveles), ShipStats
  Gameplay/     GameplayController (partida, obstáculos, colisiones), WeaponSystem (barra de carga)
  Monetization/ AdsManager (Unity LevelPlay), IAPManager (Unity IAP 5), LeaderboardService (Unity Leaderboards)
  UI/           Registro, Menú principal (con ranking), HUD, Hangar, Tienda, Ranking
```

## Idiomas

El juego está en **Español, English y Français**.
- La primera vez se usa el idioma del teléfono: español, francés o, para cualquier otro idioma, inglés.
- Se puede cambiar en la pantalla de registro (botones de arriba) o en **Ajustes > Idioma**. La elección se guarda.
- Todos los textos están en `Core/LocalizationTable.cs`, con una línea por texto: `A("clave", "español", "english", "français")`.
- Los nombres de países están en `Core/Countries.cs`, en los 3 idiomas.
- Para añadir otro idioma: agrega un valor al enum `Language` en `Localization.cs`, una columna más en cada línea de la tabla y en los países.
- En Google Play y App Store crea también la ficha de la tienda en inglés y francés, y los nombres de los productos IAP en esos idiomas.

## Reglas del juego

| Regla | Dónde se cambia |
|---|---|
| 70 niveles. Del 1 al 5 son fáciles y a partir del 6 sube la dificultad, con un nivel "tormenta" cada 10 | `LevelDatabase.cs` |
| Obstáculos **rompibles** (asteroides, drones) e **indestructibles** (muros, minas, compuertas láser). Cada tipo quita distinta vida | `LevelDatabase.BaseDamage` |
| Arma: la barra se carga durante *chargeTime*. Llena, da *fireWindow* segundos y N disparos. La carga siempre dura más que la ventana | `WeaponSystem.cs`, `ShipStats.cs` |
| Disparos por carga según la nave, las mejoras de cargador y el equipo | `ShipCatalog`, `UpgradeCatalog` |
| 3 vidas iniciales. Pierdes una al morir o al abandonar un nivel. Sin vidas no puedes jugar | `GameConfig.DefaultLives` |
| Al quedarte en 0 vidas empieza una espera de **8 horas**, al final de la cual se recargan las 3 vidas automáticamente (cuenta aunque el juego esté cerrado) | `GameConfig.LifeRefillHours` |
| **3 videos = 1 vida** (barra de progreso de 33,3% por video). Máximo **2 vidas por video cada 5 horas** | `VideosPerLife`, `MaxVideoLivesPerWindow` |
| **3 videos = 1 nivel de mejora**. Máximo **2 mejoras por video al día**. El progreso se guarda por nave y por mejora | `VideosPerUpgrade`, `MaxRewardedUpgradesPerDay` |
| Al pasar un nivel ganas puntos (ranking) y monedas, y se guarda el progreso. Si sales sin pasarlo, repites ese nivel | `GameManager.OnLevelCompleted` |
| Las mejoras con monedas son básicas (niveles 1-3). El mejor equipo y las mejores naves solo se compran con dinero real | `UpgradeCatalog`, `ShipCatalog` |

## Monetización (lo que tienes que configurar)

### 1. Anuncios: Unity LevelPlay (ya integrado)

**Datos de la red Unity Ads** (Unity Cloud > Monetización). Se introducen en LevelPlay > SDK Networks > Unity Ads:

| Dato | Valor |
|---|---|
| Game ID (Android) | `800383517` |
| Organization core ID | `4674259427047` |
| Placement Rewarded | `BP_Rewarded_Android` |
| Placement Interstitial | `BP_Interstitial_Android` |
| Placement Banner | `BP_Banner_Android` |

El **App Key** y los **Ad Unit IDs** que usa el juego (`GameConfig.cs`) vienen del panel de LevelPlay (https://platform.ironsrc.com). No son los datos de la tabla anterior. Ya configurados para Android:

| Dato LevelPlay (Android) | Valor |
|---|---|
| App Key | `2863b9aa5` |
| Rewarded_Android | `ivda4ufacc4abczy` |
| Interstitial_Android | `x6wpyrqxq2oh3ved` |
| Banner_Android | `89fe6vto996k6ctu` |

LevelPlay es el sistema de anuncios actual de Unity (reemplaza a "Advertisement Legacy"). Es **mediación**: Unity Ads, AdMob, Meta, AppLovin y otras redes compiten por cada anuncio, y eso suele subir los ingresos por anuncio.
1. En https://cloud.unity.com ve a **Monetization (LevelPlay)** y crea la app de Android y la de iOS.
2. Copia el **App Key** de cada una en `GameConfig.cs` (`AndroidAppKey`, `IosAppKey`).
3. Crea 3 **Ad Units** por plataforma (Rewarded, Interstitial y Banner) y copia sus IDs en `GameConfig.cs`.
4. Para probar, registra tu teléfono como **dispositivo de prueba** en el dashboard. Nunca hagas clic en tus propios anuncios reales, porque pueden suspender tu cuenta.
5. En **Mediation** activa las redes que quieras (empieza con Unity Ads y AdMob).

Dónde aparecen los anuncios:

| Formato | Uso |
|---|---|
| **Recompensado** | +1 vida, mejora gratis de la nave, monedas x2 al ganar, revivir al morir, +150 monedas |
| **Intersticial** | Cada 3 partidas, a partir del nivel 4 y con al menos 90 segundos entre uno y otro |
| **Banner** | Solo en el menú principal, nunca durante el juego |

Los límites de videos (3 videos por vida, máximo 2 vidas cada 5 h, 3 videos por mejora, máximo 2 mejoras al día, 5 de monedas) protegen las ventas de la tienda y se ajustan en `GameConfig`.

> Si no configuras los IDs, el Editor y los builds de desarrollo muestran un **anuncio simulado** para que puedas probar el flujo. En los builds de producción nunca se simula nada.

### 2. Compras con dinero real: Unity IAP 5 (ya integrado)
Crea estos productos **con los mismos IDs** en Google Play Console y en App Store Connect:

| ID | Tipo | Precio sugerido |
|---|---|---|
| aeroblitz.starter_pack | No consumible | 1.99 |
| aeroblitz.vip_monthly | Suscripción | 4.99/mes |
| aeroblitz.remove_ads | No consumible | 2.99 |
| aeroblitz.lives5 / lives15 | Consumible | 0.99 / 1.99 |
| aeroblitz.coins_small / medium / large | Consumible | 0.99 / 2.99 / 4.99 |
| aeroblitz.ship_phoenix / ship_titan / ship_nova | No consumible | 2.99 / 4.99 / 6.99 |
| aeroblitz.eq_plasma_shield / eq_twin_cannon / eq_ion_thrusters | No consumible | 1.99 / 1.99 / 1.49 |
| aeroblitz.eq_quantum_core / eq_titanium_armor | No consumible | 2.99 |
| aeroblitz.paint_gold | No consumible | 0.99 |

El juego muestra automáticamente el precio localizado de cada país. El botón **Restaurar compras** es obligatorio en iOS y ya está incluido.

**Importante para producción:** valida los recibos para evitar compras falsas. Hazlo en `IAPManager.OnPurchasePending`, antes de entregar el producto: valida en tu propio servidor, o usa la validación local de Google/Apple (hay ejemplos en *Package Manager > In-App Purchasing > Samples*).

### 3. Ranking mundial y por país: Unity Leaderboards (ya integrado)
1. En Unity: **Edit > Project Settings > Services** y enlaza el proyecto de Unity Cloud.
2. En el Dashboard, en **Leaderboards**, crea uno con ID `aeroblitz_global`, orden **descendente** y estrategia **Keep best**.
3. Activa **Authentication > Anonymous**.

La pantalla de ranking muestra el **top 50** con scroll, y abajo tu posición aunque estés fuera del top (se cambia en `GameConfig.RankingTopCount`). El país se guarda como metadata de cada puntaje. El filtro "Por país" busca entre el top 1000 mundial. Si en el futuro tienes cientos de miles de jugadores, conviene crear un leaderboard por país. Sin conexión, el ranking muestra solo al jugador local.

### 4. Otras fuentes de ingreso incluidas
- **Suscripción VIP**: es el ingreso recurrente más valioso. Da monedas x2, quita los intersticiales y regala 3 vidas diarias.
- **Pack de inicio**: oferta de una sola vez con mucho valor. Convierte a jugadores que nunca han pagado.
- **Quitar anuncios**: los jugadores que odian los anuncios también pagan.
- **Cosméticos** (Pintura Oro): no afectan el equilibrio del juego y los jugadores los compran por gusto.
- **Regalo diario**: hace que los jugadores vuelvan cada día, y así ven más anuncios.
- **Revivir con video**: es el anuncio con mejor tasa de visualización.

### 5. Ideas para siguientes versiones
- Pase de batalla por temporadas (el formato de mayor ingreso en juegos casuales).
- Ofertas por tiempo limitado después de perder 3 veces seguidas.
- Firebase Analytics para medir en qué nivel abandonan los jugadores y ajustar la dificultad.
- Torneos semanales con premios en el ranking.

## Cuenta de Google y guardado en la nube

- **Opcional para el jugador:** sin iniciar sesión, el progreso se guarda solo en el teléfono.
- **Respaldo automático:** con sesión iniciada (Google Play Juegos), **todo el progreso** se sube a Unity Cloud Save unos segundos después de cada cambio.
- **Recuperar el progreso:** al reinstalar o cambiar de teléfono, el jugador pulsa "¿Ya jugaste antes? Iniciar sesión con Google" y lo recupera.
- **Si hay dos progresos distintos:** si el teléfono y la cuenta tienen avances diferentes, el jugador elige cuál conservar.
- **Aviso después de comprar:** tras una compra real, si el jugador no inició sesión, se le invita a hacerlo ("Protege tus compras").
- **Borrado de cuenta:** en Ajustes está "Eliminar mi cuenta y datos", que Google Play exige a las apps con cuentas.

Configuración (cuando tu cuenta de Google Play Console esté aprobada):
1. En Play Console, entra en tu app y ve a **Play Games Services > Configuración**. Crea el proyecto de juegos y dos credenciales:
   - **Android**, con el nombre de paquete y el SHA-1 de la clave de firma.
   - **Servidor de juegos**, que te da el ID de cliente web y su secreto.
2. En Unity, abre **Window > Google Play Games > Setup > Android setup**. Pega los recursos que te da Play Console y el ID de cliente web.
3. En https://cloud.unity.com ve a **Authentication > Identity providers > Google Play Games** y pon el ID de cliente web y el secreto.
4. Prueba en un teléfono real: el inicio de sesión con Google no funciona en el Editor ni en WebGL, donde se simula.

## Términos y condiciones y privacidad

- **Aceptación obligatoria:** el jugador marca una casilla antes de empezar. Se guardan la versión aceptada y la fecha.
- **Dónde están los textos:** en `Assets/AeroBlitz/Resources/Legal/` (`terms_*.txt` y `privacy_*.txt`, en español, inglés y francés). Se pueden leer dentro del juego.
- **Datos por completar:** busca todos los `[COMPLETAR: ...]` (nombre legal, país, correo y fecha). El menú **Aero Blitz > Revisar documentos legales** te los lista.
- **Protección al publicar:** un build de producción **no se genera** mientras quede algún `[COMPLETAR`. Los builds de desarrollo sí se generan.
- **Si cambias los textos:** sube `GameConfig.TermsVersion`. Los jugadores existentes tendrán que aceptarlos de nuevo.
- **Publícalos también en una web** (por ejemplo, en Google Sites, que es gratis) y pon las URLs en `GameConfig.TermsUrl` y `GameConfig.PrivacyUrl`. Google Play exige la URL de la política de privacidad.
- **Son una plantilla:** no sustituyen la revisión de un abogado de tu país.

## Antes de publicar (checklist)
- [ ] Completa los `[COMPLETAR: ...]` de los documentos legales y haz que un abogado los revise.
- [ ] Configura Google Play Games Services y el proveedor de Google Play Games en Unity Authentication.
- [x] Identificador del juego: `com.frailescitystudios.aeroblitz` (empresa: Frailes City Studios). **No cambiarlo nunca después de publicar.**
- [ ] Justo antes de lanzar, reinicia el leaderboard `aeroblitz_global` desde el dashboard de Unity para borrar los puntajes de prueba.
- [ ] Configura el App Key y los Ad Unit IDs de LevelPlay en `GameConfig.cs`.
- [ ] Crea los productos IAP en las dos tiendas.
- [ ] Enlaza Unity Cloud y crea el leaderboard.
- [ ] Publica tu **política de privacidad** y pon su URL en `MainMenuScreen.ShowSettings`. Es obligatoria cuando hay anuncios y compras.
- [ ] Configura el consentimiento GDPR/CCPA para los anuncios. LevelPlay lo gestiona desde el dashboard con su integración de consentimiento (UMP/CMP).
- [ ] Cambia la sal (`Salt`) en `SaveSystem.cs`.
- [ ] Android: crea un keystore, compila un AAB y súbelo a Google Play.
