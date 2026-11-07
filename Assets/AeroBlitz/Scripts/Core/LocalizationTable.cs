using System.Collections.Generic;

namespace AeroBlitz
{
    /// <summary>Todas las traducciones del juego: clave, Español, English, Français.</summary>
    public static class LocalizationTable
    {
        static Dictionary<string, string[]> _d;
        static void A(string key, string es, string en, string fr) => _d[key] = new[] { es, en, fr };

        public static Dictionary<string, string[]> Build()
        {
            _d = new Dictionary<string, string[]>();

            // ---------------- GENERAL ----------------
            A("close", "Cerrar", "Close", "Fermer");
            A("cancel", "Cancelar", "Cancel", "Annuler");
            A("lives_fmt", "Vidas {0}", "Lives {0}", "Vies {0}");
            A("online", "En línea", "Online", "En ligne");
            A("offline", "Sin conexión", "Offline", "Hors ligne");
            A("loading", "Cargando...", "Loading...", "Chargement...");
            A("lang.label", "Idioma", "Language", "Langue");
            A("lang.choose", "Elige un idioma", "Choose a language", "Choisis une langue");

            // ---------------- REGISTRO ----------------
            A("reg.subtitle", "Crea tu piloto", "Create your pilot", "Crée ton pilote");
            A("reg.username", "Nombre de usuario", "Username", "Nom d'utilisateur");
            A("reg.name_ph", "Ej: PilotoEstelar", "e.g. StarPilot", "Ex : PiloteStellaire");
            A("reg.country", "Selecciona tu país", "Select your country", "Choisis ton pays");
            A("reg.search", "Buscar país...", "Search country...", "Rechercher un pays...");
            A("reg.none", "Ningún país seleccionado", "No country selected", "Aucun pays sélectionné");
            A("reg.country_fmt", "País: {0}", "Country: {0}", "Pays : {0}");
            A("reg.start", "COMENZAR", "START", "COMMENCER");
            A("reg.err_short", "El nombre debe tener al menos 3 caracteres.", "Name must be at least 3 characters.", "Le nom doit contenir au moins 3 caractères.");
            A("reg.err_chars", "Usa solo letras, números, espacios o _.", "Use only letters, numbers, spaces or _.", "Utilise seulement des lettres, chiffres, espaces ou _.");
            A("reg.err_country", "Selecciona tu país.", "Select your country.", "Choisis ton pays.");
            A("reg.welcome_fmt", "¡Bienvenido, {0}!", "Welcome, {0}!", "Bienvenue, {0} !");

            // ---------------- MENÚ PRINCIPAL ----------------
            A("menu.vip", "PILOTO VIP", "VIP PILOT", "PILOTE VIP");
            A("menu.play_fmt", "JUGAR  ·  NIVEL {0}/{1}", "PLAY  ·  LEVEL {0}/{1}", "JOUER  ·  NIVEAU {0}/{1}");
            A("menu.no_lives", "SIN VIDAS  ·  CONSEGUIR", "NO LIVES  ·  GET MORE", "PLUS DE VIES  ·  EN OBTENIR");
            A("menu.hangar", "HANGAR", "HANGAR", "HANGAR");
            A("menu.store", "TIENDA", "STORE", "BOUTIQUE");
            A("menu.ranking", "RANKING", "RANKING", "CLASSEMENT");
            A("menu.top_world", "TOP MUNDIAL", "WORLD TOP", "TOP MONDIAL");
            A("menu.see_ranking", "Ver ranking completo / por país", "Full ranking / by country", "Classement complet / par pays");
            A("menu.daily", "Regalo diario", "Daily gift", "Cadeau quotidien");
            A("menu.daily_tomorrow", "Regalo: mañana", "Gift: tomorrow", "Cadeau : demain");
            A("menu.free_life", "Vida gratis (video)", "Free life (video)", "Vie gratuite (vidéo)");
            A("menu.settings", "Ajustes", "Settings", "Paramètres");
            A("menu.ship_fmt", "{0}  ·  Nivel de nave {1}  ·  {2} pts", "{0}  ·  Ship level {1}  ·  {2} pts", "{0}  ·  Niveau du vaisseau {1}  ·  {2} pts");
            A("menu.no_pilots", "Aún no hay pilotos. ¡Sé el primero!", "No pilots yet. Be the first!", "Aucun pilote pour l'instant. Sois le premier !");

            // ---------------- AJUSTES ----------------
            A("set.pilot_fmt", "Piloto: {0} ({1})", "Pilot: {0} ({1})", "Pilote : {0} ({1})");
            A("set.sound_on", "Sonido: SÍ", "Sound: ON", "Son : OUI");
            A("set.sound_off", "Sonido: NO", "Sound: OFF", "Son : NON");
            A("set.music_on", "Música: SÍ", "Music: ON", "Musique : OUI");
            A("set.music_off", "Música: NO", "Music: OFF", "Musique : NON");
            A("set.vib_on", "Vibración: SÍ", "Vibration: ON", "Vibration : OUI");
            A("set.vib_off", "Vibración: NO", "Vibration: OFF", "Vibration : NON");
            A("set.language_fmt", "Idioma: {0}", "Language: {0}", "Langue : {0}");
            A("set.restore", "Restaurar compras", "Restore purchases", "Restaurer les achats");
            A("set.privacy", "Política de privacidad", "Privacy policy", "Politique de confidentialité");
            A("set.reset", "Borrar progreso", "Delete progress", "Effacer la progression");
            A("set.reset_title", "¿Borrar todo?", "Delete everything?", "Tout effacer ?");
            A("set.reset_body", "Se perderá tu progreso local. Las compras se pueden restaurar.", "Your local progress will be lost. Purchases can be restored.", "Ta progression locale sera perdue. Les achats peuvent être restaurés.");
            A("set.reset_yes", "Borrar", "Delete", "Effacer");

            // ---------------- HUD ----------------
            A("hud.level_fmt", "NIVEL {0}  ·  {1}", "LEVEL {0}  ·  {1}", "NIVEAU {0}  ·  {1}");
            A("hud.health", "VIDA", "HEALTH", "VIE");
            A("hud.missiles_fmt", "MISILES\n{0}", "MISSILES\n{0}", "MISSILES\n{0}");
            A("hud.missiles_reload_fmt", "RECARGA\n{0}s", "RELOAD\n{0}s", "RECHARGE\n{0}s");
            A("hud.missiles_empty", "SIN\nMISILES", "NO\nMISSILES", "PLUS DE\nMISSILES");
            A("hud.trial_fmt", "{0} (prueba)", "{0} (trial)", "{0} (essai)");
            A("toast.trial_fmt", "¡{0} de prueba en este nivel!", "{0} trial for this level!", "{0} en essai pour ce niveau !");
            A("dlg.trial_buy_fmt", "¿Te gustó? {0}  {1}", "Liked it? {0}  {1}", "Ça t'a plu ? {0}  {1}");
            A("hud.run_coins_fmt", "Monedas +{0}", "Coins +{0}", "Pièces +{0}");
            A("hud.pu_time_fmt", "{0} {1}s", "{0} {1}s", "{0} {1}s");
            A("pu.shield", "Escudo", "Shield", "Bouclier");
            A("pu.overcharge", "Sobrecarga", "Overcharge", "Surcharge");
            A("pu.magnet", "Imán", "Magnet", "Aimant");
            A("pu.repair", "Reparación", "Repair", "Réparation");
            A("toast.pu_fmt", "¡{0}!", "{0}!", "{0} !");
            A("dlg.share", "COMPARTIR", "SHARE", "PARTAGER");
            A("share.chooser", "Compartir con", "Share with", "Partager avec");
            A("share.copied", "Texto copiado", "Text copied", "Texte copié");
            A("share.level_fmt", "¡Superé el nivel {0} de Aero Blitz con {1} puntos! ¿Puedes ganarme? {2}",
                "I just beat level {0} in Aero Blitz with {1} points! Can you beat me? {2}",
                "J'ai terminé le niveau {0} d'Aero Blitz avec {1} points ! Tu peux me battre ? {2}");
            A("dlg.pickup_coins_fmt", "Monedas recogidas: {0}", "Coins collected: {0}", "Pièces ramassées : {0}");
            A("hud.shield_fmt", "Escudo: {0} impacto(s)", "Shield: {0} hit(s)", "Bouclier : {0} impact(s)");
            A("hud.weapon", "CARGA DEL ARMA", "WEAPON CHARGE", "CHARGE DE L'ARME");
            A("hud.charging", "CARGANDO", "CHARGING", "CHARGE");
            A("hud.shots_per_fmt", "Disparos por carga: {0}", "Shots per charge: {0}", "Tirs par charge : {0}");
            A("hud.ready", "¡LISTA!", "READY!", "PRÊTE !");
            A("hud.fire", "¡FUEGO!", "FIRE!", "FEU !");
            A("hud.tap_fire_fmt", "Toca FUEGO: {0} disparos", "Tap FIRE: {0} shots", "Touche FEU : {0} tirs");
            A("hud.fire_n_fmt", "FUEGO\n{0}", "FIRE\n{0}", "FEU\n{0}");
            A("hud.hurry", "¡Dispara antes de que se vacíe!", "Shoot before it runs out!", "Tire avant qu'elle se vide !");

            // ---------------- DIÁLOGOS ----------------
            A("dlg.no_lives_title", "¡Sin vidas!", "Out of lives!", "Plus de vies !");
            A("dlg.no_lives_body", "Te quedaste sin vidas. Consigue más para seguir jugando.", "You ran out of lives. Get more to keep playing.", "Tu n'as plus de vies. Obtiens-en d'autres pour continuer.");
            A("dlg.video_life_fmt", "Ver video: +1 vida ({0} hoy)", "Watch video: +1 life ({0} today)", "Vidéo : +1 vie ({0} aujourd'hui)");
            A("dlg.lives_buy_fmt", "{0} vidas  —  {1}", "{0} lives  —  {1}", "{0} vies  —  {1}");
            A("dlg.go_store", "Ir a la tienda", "Go to store", "Aller à la boutique");
            A("dlg.complete_title_fmt", "¡Nivel {0} superado!", "Level {0} complete!", "Niveau {0} réussi !");
            A("dlg.complete_body_fmt", "<b>+{0}</b> puntos\n<color=#FFD24A><b>+{1}</b> monedas</color>\nObstáculos destruidos: {2}",
                                       "<b>+{0}</b> points\n<color=#FFD24A><b>+{1}</b> coins</color>\nObstacles destroyed: {2}",
                                       "<b>+{0}</b> points\n<color=#FFD24A><b>+{1}</b> pièces</color>\nObstacles détruits : {2}");
            A("dlg.campaign_done", "¡Completaste los 70 niveles! Nuevos sectores llegarán pronto.", "You beat all 70 levels! New sectors coming soon.", "Tu as terminé les 70 niveaux ! De nouveaux secteurs arrivent bientôt.");
            A("dlg.double_coins", "Ver video: monedas x2", "Watch video: coins x2", "Vidéo : pièces x2");
            A("dlg.next_fmt", "Siguiente: Nivel {0}", "Next: Level {0}", "Suivant : Niveau {0}");
            A("dlg.replay_last", "Rejugar nivel 70", "Replay level 70", "Rejouer le niveau 70");
            A("dlg.main_menu", "Menú principal", "Main menu", "Menu principal");
            A("dlg.failed_title", "Nave destruida", "Ship destroyed", "Vaisseau détruit");
            A("dlg.failed_body_fmt", "Perdiste una vida.\nVidas restantes: <b>{0}</b>\nReintentarás el nivel {1}.",
                                     "You lost a life.\nLives left: <b>{0}</b>\nYou will retry level {1}.",
                                     "Tu as perdu une vie.\nVies restantes : <b>{0}</b>\nTu vas rejouer le niveau {1}.");
            A("dlg.failed_last_fmt", "Perdiste tu última vida.\nTus 3 vidas se recargan en {0} horas, o gana vidas viendo videos.", "You lost your last life.\nYour 3 lives refill in {0} hours, or earn lives by watching videos.", "Tu as perdu ta dernière vie.\nTes 3 vies se rechargent dans {0} heures, ou gagne des vies avec des vidéos.");
            A("dlg.retry", "Reintentar", "Retry", "Réessayer");
            A("dlg.get_lives", "Conseguir vidas", "Get lives", "Obtenir des vies");
            A("dlg.revive_title", "¿Continuar?", "Continue?", "Continuer ?");
            A("dlg.revive_body", "Mira un video para revivir con 50% de vida y no perder una vida.", "Watch a video to revive with 50% health and keep your life.", "Regarde une vidéo pour revivre avec 50 % de vie sans perdre de vie.");
            A("dlg.revive_btn", "Revivir con video", "Revive with video", "Revivre avec une vidéo");
            A("dlg.no_thanks", "No, gracias", "No, thanks", "Non, merci");
            A("dlg.pause", "Pausa", "Pause", "Pause");
            A("dlg.level_fmt", "Nivel {0}", "Level {0}", "Niveau {0}");
            A("dlg.continue", "Continuar", "Continue", "Continuer");
            A("dlg.abandon", "Abandonar nivel", "Quit level", "Abandonner le niveau");
            A("dlg.abandon_title", "¿Abandonar?", "Quit?", "Abandonner ?");
            A("dlg.abandon_body", "Si abandonas el nivel perderás una vida.", "If you quit the level you will lose a life.", "Si tu abandonnes le niveau, tu perdras une vie.");
            A("dlg.abandon_yes", "Abandonar", "Quit", "Abandonner");

            // ---------------- ANUNCIOS ----------------
            A("ad.test_fmt", "ANUNCIO DE PRUEBA\n({0})\n\nConfigura tus IDs de LevelPlay en GameConfig.cs\n\n{1}",
                             "TEST AD\n({0})\n\nSet your LevelPlay IDs in GameConfig.cs\n\n{1}",
                             "PUBLICITÉ DE TEST\n({0})\n\nConfigure tes IDs LevelPlay dans GameConfig.cs\n\n{1}");
            A("ad.rewarded", "recompensado", "rewarded", "récompensée");
            A("ad.interstitial", "intersticial", "interstitial", "interstitielle");
            A("ad.unavailable", "Anuncio no disponible, intenta en un momento.", "Ad not available, try again shortly.", "Publicité indisponible, réessaie dans un instant.");

            // ---------------- AVISOS ----------------
            A("toast.coins_fmt", "+{0} monedas", "+{0} coins", "+{0} pièces");
            A("toast.equipment_bought_fmt", "¡{0} equipado!", "{0} equipped!", "{0} équipé !");
            A("toast.no_coins", "No tienes suficientes monedas", "Not enough coins", "Pas assez de pièces");
            A("toast.ship_unlocked_fmt", "¡{0} desbloqueada!", "{0} unlocked!", "{0} débloqué !");
            A("toast.life_limit", "Límite diario de vidas por video alcanzado", "Daily video life limit reached", "Limite quotidienne de vies par vidéo atteinte");
            A("toast.plus_life", "+1 vida", "+1 life", "+1 vie");
            A("toast.upgrade_limit", "Límite diario de mejoras por video alcanzado", "Daily video upgrade limit reached", "Limite quotidienne d'améliorations par vidéo atteinte");
            A("toast.upgraded_fmt", "¡{0} mejorado gratis!", "{0} upgraded for free!", "{0} amélioré gratuitement !");
            A("toast.daily_limit", "Límite diario alcanzado", "Daily limit reached", "Limite quotidienne atteinte");
            A("toast.daily_fmt", "Recompensa diaria: {0}", "Daily reward: {0}", "Récompense quotidienne : {0}");
            A("toast.vip_lives_fmt", "+{0} vidas VIP", "+{0} VIP lives", "+{0} vies VIP");

            // ---------------- COMPRAS ----------------
            A("iap.not_available", "Producto no disponible en la tienda.", "Product not available in the store.", "Produit indisponible dans la boutique.");
            A("iap.simulated", "Compra simulada (modo prueba).", "Simulated purchase (test mode).", "Achat simulé (mode test).");
            A("iap.store_down", "La tienda no está disponible. Revisa tu conexión.", "Store unavailable. Check your connection.", "Boutique indisponible. Vérifie ta connexion.");
            A("iap.restored", "Compras restauradas.", "Purchases restored.", "Achats restaurés.");
            A("iap.android_restore", "En Android las compras se restauran automáticamente.", "On Android, purchases are restored automatically.", "Sur Android, les achats sont restaurés automatiquement.");
            A("iap.done", "¡Compra completada!", "Purchase complete!", "Achat effectué !");
            A("iap.cancelled", "Compra cancelada.", "Purchase cancelled.", "Achat annulé.");
            A("iap.failed_fmt", "La compra falló: {0}", "Purchase failed: {0}", "L'achat a échoué : {0}");

            // ---------------- HANGAR ----------------
            A("hangar.premium", "PREMIUM", "PREMIUM", "PREMIUM");
            A("hangar.stats_fmt", "Nivel de nave {0}   ·   Vida {1}   ·   Velocidad {2:0.0}\nDisparos {3}   ·   Carga {4:0.0}s   ·   Ventana de disparo {5:0.0}s",
                                  "Ship level {0}   ·   Health {1}   ·   Speed {2:0.0}\nShots {3}   ·   Charge {4:0.0}s   ·   Fire window {5:0.0}s",
                                  "Niveau du vaisseau {0}   ·   Vie {1}   ·   Vitesse {2:0.0}\nTirs {3}   ·   Charge {4:0.0}s   ·   Fenêtre de tir {5:0.0}s");
            A("hangar.selected", "SELECCIONADA", "SELECTED", "SÉLECTIONNÉ");
            A("hangar.select", "SELECCIONAR", "SELECT", "SÉLECTIONNER");
            A("hangar.buy_fmt", "COMPRAR  {0}", "BUY  {0}", "ACHETER  {0}");
            A("hangar.buy_first", "Compra esta nave para mejorarla y personalizarla.", "Buy this ship to upgrade and customize it.", "Achète ce vaisseau pour l'améliorer et le personnaliser.");
            A("hangar.upgrades_fmt", "MEJORAS  ·  mejoras por video hoy: {0}", "UPGRADES  ·  video upgrades today: {0}", "AMÉLIORATIONS  ·  par vidéo aujourd'hui : {0}");
            A("hangar.lv_fmt", "Nv {0}/{1}", "Lv {0}/{1}", "Nv {0}/{1}");
            A("hangar.max", "MÁXIMO", "MAX", "MAX");
            A("hangar.free_video", "Gratis\n(video)", "Free\n(video)", "Gratuit\n(vidéo)");
            A("hangar.premium_eq", "EQUIPAMIENTO PREMIUM", "PREMIUM EQUIPMENT", "ÉQUIPEMENT PREMIUM");
            A("hangar.basic_eq", "EQUIPAMIENTO", "EQUIPMENT", "ÉQUIPEMENT");
            A("hangar.equip", "EQUIPAR", "EQUIP", "ÉQUIPER");
            A("hangar.unequip", "QUITAR", "REMOVE", "RETIRER");
            A("hangar.paint", "PINTURA", "PAINT", "PEINTURE");
            A("hangar.in_use", "EN USO", "IN USE", "UTILISÉE");

            // ---------------- TIENDA ----------------
            A("tab.offers", "Ofertas", "Offers", "Offres");
            A("tab.lives", "Vidas", "Lives", "Vies");
            A("tab.coins", "Monedas", "Coins", "Pièces");
            A("tab.free", "Gratis", "Free", "Gratuit");
            A("tab.ships", "Naves", "Ships", "Vaisseaux");
            A("tab.equipment", "Equipo", "Gear", "Équipement");
            A("tab.coinshop", "Con monedas", "With coins", "Avec pièces");
            A("store.owned", "ADQUIRIDO", "OWNED", "ACQUIS");
            A("store.sub_note", "Las suscripciones se renuevan automáticamente. Puedes cancelarlas desde tu tienda de aplicaciones.", "Subscriptions renew automatically. You can cancel them from your app store.", "Les abonnements se renouvellent automatiquement. Tu peux les annuler depuis ta boutique d'applications.");
            A("store.free_life", "+1 Vida", "+1 Life", "+1 Vie");
            A("store.watch_left_fmt", "Mira un video. Restantes hoy: {0}", "Watch a video. Left today: {0}", "Regarde une vidéo. Restantes aujourd'hui : {0}");
            A("store.watch", "VER VIDEO", "WATCH VIDEO", "VOIR LA VIDÉO");
            A("store.coins_fmt", "+{0} Monedas", "+{0} Coins", "+{0} Pièces");
            A("store.free_upgrade", "Mejora gratis", "Free upgrade", "Amélioration gratuite");
            A("store.free_upgrade_desc_fmt", "Cada mejora requiere {0} videos en el Hangar. Mejoras restantes hoy: {1}", "Each upgrade takes {0} videos in the Hangar. Upgrades left today: {1}", "Chaque amélioration demande {0} vidéos dans le Hangar. Restantes aujourd'hui : {1}");
            A("store.go_hangar", "IR AL HANGAR", "GO TO HANGAR", "ALLER AU HANGAR");
            A("store.daily_vip", "Monedas + 3 vidas VIP cada día.", "Coins + 3 VIP lives every day.", "Pièces + 3 vies VIP chaque jour.");
            A("store.daily_fmt", "{0} monedas cada día.", "{0} coins every day.", "{0} pièces chaque jour.");
            A("store.claim", "RECLAMAR", "CLAIM", "RÉCUPÉRER");
            A("store.tomorrow", "MAÑANA", "TOMORROW", "DEMAIN");
            A("store.coinshop_header", "Accesorios básicos con monedas del juego", "Basic items with in-game coins", "Accessoires de base avec les pièces du jeu");
            A("store.ship_fmt", "Nave {0}", "{0} Ship", "Vaisseau {0}");
            A("store.paint_fmt", "Pintura {0}", "{0} Paint", "Peinture {0}");
            A("store.paint_desc", "Aplícala a cualquier nave en el Hangar.", "Apply it to any ship in the Hangar.", "Applique-la à n'importe quel vaisseau dans le Hangar.");

            // ---------------- RANKING ----------------
            A("rank.world", "MUNDIAL", "WORLD", "MONDIAL");
            A("rank.by_country", "POR PAÍS", "BY COUNTRY", "PAR PAYS");
            A("rank.col_pilot", "PILOTO", "PILOT", "PILOTE");
            A("rank.col_country", "PAÍS", "COUNTRY", "PAYS");
            A("rank.col_points", "PUNTOS", "POINTS", "POINTS");
            A("rank.choose_country", "Elige un país", "Choose a country", "Choisis un pays");
            A("rank.country_btn_fmt", "País: {0}   (cambiar)", "Country: {0}   (change)", "Pays : {0}   (changer)");
            A("rank.loading", "Cargando ranking...", "Loading ranking...", "Chargement du classement...");
            A("rank.online_fmt", "{0} pilotos · en línea", "{0} pilots · online", "{0} pilotes · en ligne");
            A("rank.top_online_fmt", "Top {0} · {1} pilotos · en línea", "Top {0} · {1} pilots · online", "Top {0} · {1} pilotes · en ligne");
            A("rank.offline", "Sin conexión: se muestra solo tu puntaje", "Offline: only your score is shown", "Hors ligne : seul ton score est affiché");
            A("rank.empty_country", "No hay pilotos en este país todavía.", "No pilots in this country yet.", "Aucun pilote dans ce pays pour l'instant.");
            A("rank.mine_fmt", "Tu posición: #{0}  ·  {1} pts", "Your position: #{0}  ·  {1} pts", "Ta position : #{0}  ·  {1} pts");
            A("rank.mine_out_fmt", "Tus puntos: {0} (fuera del top)", "Your points: {0} (outside the top)", "Tes points : {0} (hors du top)");

            // ---------------- PANEL DE VIDAS ----------------
            A("lives.title", "Vidas", "Lives", "Vies");
            A("lives.count_fmt", "Tienes {0} de {1} vidas", "You have {0} of {1} lives", "Tu as {0} vies sur {1}");
            A("lives.refill_fmt", "Recarga completa en {0}", "Full refill in {0}", "Recharge complète dans {0}");
            A("lives.video_info_fmt", "Mira {0} videos para ganar 1 vida.\nVidas por video en este período: {1}/{2}", "Watch {0} videos to earn 1 life.\nVideo lives this period: {1}/{2}", "Regarde {0} vidéos pour gagner 1 vie.\nVies par vidéo sur cette période : {1}/{2}");
            A("lives.bar_fmt", "Video {0}/{1}  ·  {2:0.#}%", "Video {0}/{1}  ·  {2:0.#}%", "Vidéo {0}/{1}  ·  {2:0.#} %");
            A("lives.window_limit_fmt", "Límite alcanzado. Más vidas por video en {0}", "Limit reached. More video lives in {0}", "Limite atteinte. Plus de vies par vidéo dans {0}");
            A("lives.full", "Tienes todas tus vidas.", "Your lives are full.", "Tes vies sont au complet.");
            A("lives.watch_btn_fmt", "Ver video {0}/{1}", "Watch video {0}/{1}", "Voir la vidéo {0}/{1}");
            A("toast.life_video_fmt", "Video {0}/{1} completado", "Video {0}/{1} completed", "Vidéo {0}/{1} terminée");
            A("toast.lives_refilled", "¡Tus vidas se recargaron!", "Your lives have been refilled!", "Tes vies ont été rechargées !");
            A("toast.upgrade_video_fmt", "{0}: video {1}/{2}", "{0}: video {1}/{2}", "{0} : vidéo {1}/{2}");
            A("menu.lives_timer_fmt", "Vidas {0} · {1}", "Lives {0} · {1}", "Vies {0} · {1}");
            A("menu.no_lives_timer_fmt", "SIN VIDAS  ·  {0}", "NO LIVES  ·  {0}", "PLUS DE VIES  ·  {0}");
            A("hangar.video_fmt", "Video\n{0}/{1}", "Video\n{0}/{1}", "Vidéo\n{0}/{1}");
            A("store.free_life_desc_fmt", "3 videos = 1 vida. Progreso: {0}/{1} · Vidas por video: {2}/{3} cada {4} h", "3 videos = 1 life. Progress: {0}/{1} · Video lives: {2}/{3} every {4} h", "3 vidéos = 1 vie. Progression : {0}/{1} · Vies par vidéo : {2}/{3} toutes les {4} h");
            A("store.open", "VER", "OPEN", "OUVRIR");

            // ---------------- LEGAL ----------------
            A("legal.terms", "Términos y condiciones", "Terms and Conditions", "Conditions d'utilisation");
            A("legal.privacy", "Política de privacidad", "Privacy Policy", "Politique de confidentialité");
            A("legal.checkbox", "He leído y acepto los Términos y condiciones y la Política de privacidad", "I have read and accept the Terms and Conditions and the Privacy Policy", "J'ai lu et j'accepte les Conditions d'utilisation et la Politique de confidentialité");
            A("legal.read_terms", "Leer términos", "Read terms", "Lire les conditions");
            A("legal.read_privacy", "Leer privacidad", "Read privacy", "Lire la confidentialité");
            A("legal.must_accept", "Debes aceptar los términos y la política de privacidad para continuar.", "You must accept the terms and the privacy policy to continue.", "Tu dois accepter les conditions et la politique de confidentialité pour continuer.");
            A("legal.update_title", "Términos actualizados", "Updated terms", "Conditions mises à jour");
            A("legal.update_body", "Actualizamos los Términos y condiciones y la Política de privacidad. Léelos y acéptalos para seguir jugando.", "We updated the Terms and Conditions and the Privacy Policy. Please read and accept them to keep playing.", "Nous avons mis à jour les Conditions d'utilisation et la Politique de confidentialité. Lis-les et accepte-les pour continuer à jouer.");
            A("legal.accept", "Acepto", "I accept", "J'accepte");

            // ---------------- CUENTA / NUBE ----------------
            A("acc.google_btn", "Iniciar sesión con Google", "Sign in with Google", "Se connecter avec Google");
            A("acc.google_btn_backup", "Iniciar sesión con Google (respaldar progreso)", "Sign in with Google (back up progress)", "Se connecter avec Google (sauvegarder)");
            A("acc.recover_btn", "¿Ya jugaste antes? Iniciar sesión con Google", "Played before? Sign in with Google", "Déjà joué ? Se connecter avec Google");
            A("acc.linked_status", "Google conectado · progreso respaldado", "Google connected · progress backed up", "Google connecté · progression sauvegardée");
            A("acc.later", "Ahora no", "Not now", "Plus tard");
            A("acc.linked", "Cuenta conectada. Tu progreso se respalda en la nube.", "Account connected. Your progress is backed up to the cloud.", "Compte connecté. Ta progression est sauvegardée dans le cloud.");
            A("acc.restored", "¡Progreso recuperado!", "Progress restored!", "Progression récupérée !");
            A("acc.simulated", "Inicio de sesión simulado (modo prueba).", "Simulated sign-in (test mode).", "Connexion simulée (mode test).");
            A("acc.offline", "No hay conexión con el servicio en la nube. Revisa tu internet e inténtalo de nuevo.", "Can't reach the cloud service. Check your internet and try again.", "Service cloud injoignable. Vérifie ta connexion et réessaie.");
            A("acc.google_cancelled", "No se completó el inicio de sesión con Google.", "Google sign-in was not completed.", "La connexion Google n'a pas abouti.");
            A("acc.google_error", "Google no respondió. Inténtalo de nuevo.", "Google did not respond. Please try again.", "Google n'a pas répondu. Réessaie.");
            A("acc.unsupported", "El inicio de sesión con Google solo está disponible en Android.", "Google sign-in is only available on Android.", "La connexion Google n'est disponible que sur Android.");
            A("acc.error", "Ocurrió un error con la cuenta. Inténtalo de nuevo más tarde.", "Something went wrong with the account. Please try again later.", "Un problème est survenu avec le compte. Réessaie plus tard.");
            A("acc.conflict_title", "¿Qué progreso quieres usar?", "Which progress do you want to keep?", "Quelle progression veux-tu garder ?");
            A("acc.conflict_body_fmt", "Tu cuenta de Google ya tiene progreso guardado:\n<b>{0}</b>\n\nEn este teléfono:\n<b>{1}</b>\n\nEl que no elijas se reemplazará.", "Your Google account already has saved progress:\n<b>{0}</b>\n\nOn this phone:\n<b>{1}</b>\n\nThe one you don't choose will be replaced.", "Ton compte Google a déjà une progression :\n<b>{0}</b>\n\nSur ce téléphone :\n<b>{1}</b>\n\nCelle que tu ne choisis pas sera remplacée.");
            A("acc.summary_fmt", "Nivel {0} · {1} pts · {2} monedas", "Level {0} · {1} pts · {2} coins", "Niveau {0} · {1} pts · {2} pièces");
            A("acc.use_cloud", "Usar el de mi cuenta de Google", "Use my Google account's", "Utiliser celle du compte Google");
            A("acc.use_local", "Conservar el de este teléfono", "Keep this phone's", "Garder celle de ce téléphone");
            A("acc.protect_title", "Protege tus compras", "Protect your purchases", "Protège tes achats");
            A("acc.protect_body", "Inicia sesión con Google para respaldar tus monedas, naves y progreso. Si cambias de teléfono o reinstalas el juego, lo recuperarás todo.", "Sign in with Google to back up your coins, ships and progress. If you change phones or reinstall the game, you'll get everything back.", "Connecte-toi avec Google pour sauvegarder tes pièces, vaisseaux et progression. Si tu changes de téléphone ou réinstalles le jeu, tu récupéreras tout.");
            A("acc.delete_btn", "Eliminar mi cuenta y datos", "Delete my account and data", "Supprimer mon compte et mes données");
            A("acc.delete_title", "¿Eliminar todo?", "Delete everything?", "Tout supprimer ?");
            A("acc.delete_body_linked", "Se borrarán tu cuenta, tu progreso en la nube y en este teléfono, y tu puesto en el ranking. No se puede deshacer. Las compras de naves, equipo y VIP se pueden restaurar desde Google Play; las monedas no.", "Your account, your progress in the cloud and on this phone, and your ranking entry will be deleted. This cannot be undone. Ship, equipment and VIP purchases can be restored from Google Play; coins cannot.", "Ton compte, ta progression dans le cloud et sur ce téléphone, et ta place au classement seront supprimés. C'est irréversible. Les achats de vaisseaux, d'équipement et VIP peuvent être restaurés depuis Google Play ; les pièces non.");
            A("acc.delete_body_local", "Se borrará tu progreso en este teléfono y tu puesto en el ranking. No se puede deshacer. Las compras de naves, equipo y VIP se pueden restaurar desde Google Play; las monedas no.", "Your progress on this phone and your ranking entry will be deleted. This cannot be undone. Ship, equipment and VIP purchases can be restored from Google Play; coins cannot.", "Ta progression sur ce téléphone et ta place au classement seront supprimées. C'est irréversible. Les achats de vaisseaux, d'équipement et VIP peuvent être restaurés depuis Google Play ; les pièces non.");
            A("acc.delete_yes", "Eliminar", "Delete", "Supprimer");
            A("acc.deleted", "Tu cuenta y tus datos fueron eliminados.", "Your account and data were deleted.", "Ton compte et tes données ont été supprimés.");

            // ---------------- NIVELES ----------------
            A("lvl.training_fmt", "Entrenamiento {0}", "Training {0}", "Entraînement {0}");
            A("lvl.storm", "Tormenta de Asteroides", "Asteroid Storm", "Tempête d'Astéroïdes");
            A("lvl.w1", "Cinturón Exterior", "Outer Belt", "Ceinture Extérieure");
            A("lvl.w2", "Anillos de Hielo", "Ice Rings", "Anneaux de Glace");
            A("lvl.w3", "Nebulosa Carmesí", "Crimson Nebula", "Nébuleuse Pourpre");
            A("lvl.w4", "Campo Minado", "Minefield", "Champ de Mines");
            A("lvl.w5", "Llamarada Solar", "Solar Flare", "Éruption Solaire");
            A("lvl.w6", "Estación Abandonada", "Abandoned Station", "Station Abandonnée");
            A("lvl.w7", "Corredor de Láseres", "Laser Corridor", "Couloir Laser");
            A("lvl.w8", "Mar de Plasma", "Plasma Sea", "Mer de Plasma");
            A("lvl.w9", "Frontera Oscura", "Dark Frontier", "Frontière Obscure");
            A("lvl.w10", "Núcleo Galáctico", "Galactic Core", "Noyau Galactique");

            // ---------------- NAVES ----------------
            A("ship.falcon", "Halcón", "Falcon", "Faucon");
            A("ship.falcon.desc", "Nave inicial equilibrada.", "Balanced starter ship.", "Vaisseau de départ équilibré.");
            A("ship.wasp", "Avispa", "Wasp", "Guêpe");
            A("ship.wasp.desc", "Ligera y veloz, poco blindaje.", "Light and fast, little armor.", "Léger et rapide, peu blindé.");
            A("ship.guardian", "Guardián", "Guardian", "Gardien");
            A("ship.guardian.desc", "Resistente, ideal para niveles difíciles.", "Tough, ideal for hard levels.", "Résistant, idéal pour les niveaux difficiles.");
            A("ship.phoenix", "Fénix", "Phoenix", "Phénix");
            A("ship.phoenix.desc", "PREMIUM: carga rápida y más disparos.", "PREMIUM: fast charge and more shots.", "PREMIUM : charge rapide et plus de tirs.");
            A("ship.titan", "Titán", "Titan", "Titan");
            A("ship.titan.desc", "PREMIUM: el blindaje más pesado.", "PREMIUM: the heaviest armor.", "PREMIUM : le blindage le plus lourd.");
            A("ship.nova", "Nova", "Nova", "Nova");
            A("ship.nova.desc", "PREMIUM: la nave más rápida y letal.", "PREMIUM: the fastest, deadliest ship.", "PREMIUM : le vaisseau le plus rapide et le plus mortel.");

            // ---------------- MEJORAS ----------------
            A("up.Armor", "Blindaje", "Armor", "Blindage");
            A("up.Armor.desc", "+10% vida por nivel", "+10% health per level", "+10 % de vie par niveau");
            A("up.Engine", "Motor", "Engine", "Moteur");
            A("up.Engine.desc", "+6% velocidad por nivel", "+6% speed per level", "+6 % de vitesse par niveau");
            A("up.Magazine", "Cargador", "Magazine", "Chargeur");
            A("up.Magazine.desc", "+1 disparo por carga", "+1 shot per charge", "+1 tir par charge");
            A("up.Charger", "Condensador", "Capacitor", "Condensateur");
            A("up.Charger.desc", "-6% tiempo de carga", "-6% charge time", "-6 % de temps de charge");

            // ---------------- EQUIPO PREMIUM ----------------
            A("eq.eq_shield", "Escudo de Plasma", "Plasma Shield", "Bouclier Plasma");
            A("eq.eq_shield.desc", "Bloquea los 2 primeros impactos de cada nivel.", "Blocks the first 2 hits of each level.", "Bloque les 2 premiers impacts de chaque niveau.");
            A("eq.eq_twin", "Cañón Doble", "Twin Cannon", "Canon Double");
            A("eq.eq_twin.desc", "Dispara 2 proyectiles y +2 disparos por carga.", "Fires 2 projectiles and +2 shots per charge.", "Tire 2 projectiles et +2 tirs par charge.");
            A("eq.eq_quantum", "Núcleo Cuántico", "Quantum Core", "Noyau Quantique");
            A("eq.eq_quantum.desc", "-25% tiempo de carga y +1s de ventana de disparo.", "-25% charge time and +1s fire window.", "-25 % de temps de charge et +1 s de fenêtre de tir.");
            A("eq.eq_titanium", "Blindaje de Titanio", "Titanium Armor", "Blindage en Titane");
            A("eq.eq_titanium.desc", "+50% vida y -20% daño recibido.", "+50% health and -20% damage taken.", "+50 % de vie et -20 % de dégâts subis.");
            A("eq.eq_thrusters", "Propulsores Iónicos", "Ion Thrusters", "Propulseurs Ioniques");
            A("eq.eq_thrusters.desc", "+20% velocidad de maniobra.", "+20% maneuver speed.", "+20 % de vitesse de manœuvre.");
            A("eq.eq_triple", "Cañón Triple", "Triple Cannon", "Canon Triple");
            A("eq.eq_triple.desc", "Cada disparo sale en abanico de 3.", "Every shot fires a 3-way spread.", "Chaque tir part en éventail de 3.");
            A("eq.eq_plasma", "Cañón de Plasma", "Plasma Cannon", "Canon à Plasma");
            A("eq.eq_plasma.desc", "Abanico de 5 que atraviesa hasta 3 asteroides o drones.", "5-way spread that pierces up to 3 asteroids or drones.", "Éventail de 5 qui traverse jusqu'à 3 astéroïdes ou drones.");
            A("eq.eq_missiles", "Misiles", "Missiles", "Missiles");
            A("eq.eq_missiles.desc", "Botón de misiles: 4 por carga, 2 cargas por nivel. Destruyen muros y minas.", "Missile button: 4 per load, 2 loads per level. They destroy walls and mines.", "Bouton missiles : 4 par charge, 2 charges par niveau. Ils détruisent murs et mines.");
            A("eq.eq_plates", "Placas Reforzadas", "Reinforced Plating", "Plaques Renforcées");
            A("eq.eq_plates.desc", "+20% vida de la nave.", "+20% ship health.", "+20 % de vie du vaisseau.");
            A("eq.eq_magazine", "Cargador Ampliado", "Extended Magazine", "Chargeur Étendu");
            A("eq.eq_magazine.desc", "+2 disparos por carga.", "+2 shots per charge.", "+2 tirs par charge.");

            // ---------------- PINTURAS ----------------
            A("paint.default", "Estándar", "Standard", "Standard");
            A("paint.red", "Rojo", "Red", "Rouge");
            A("paint.green", "Verde", "Green", "Vert");
            A("paint.cyan", "Cian", "Cyan", "Cyan");
            A("paint.orange", "Naranja", "Orange", "Orange");
            A("paint.purple", "Púrpura", "Purple", "Violet");
            A("paint.gold", "Oro Legendario", "Legendary Gold", "Or Légendaire");
            A("paint.neon", "Neón Magenta", "Neon Magenta", "Magenta Néon");
            A("paint.toxic", "Lima Tóxica", "Toxic Lime", "Citron Toxique");
            A("paint.midnight", "Azul Medianoche", "Midnight Blue", "Bleu Minuit");
            A("paint.ruby", "Rubí", "Ruby", "Rubis");
            A("hangar.weekly_paint", "Misión semanal", "Weekly mission", "Mission hebdo");
            A("toast.weekly_paint_hint", "Esta pintura se gana con la misión semanal", "Earn this paint with the weekly mission", "Gagne cette peinture avec la mission hebdo");

            // ---------------- MISIONES ----------------
            A("menu.missions", "MISIONES", "MISSIONS", "MISSIONS");
            A("mis.title", "Misiones", "Missions", "Missions");
            A("mis.daily_fmt", "Diarias · se renuevan en {0}", "Daily · new in {0}", "Quotidiennes · nouvelles dans {0}");
            A("mis.weekly_fmt", "Semanal · termina en {0}", "Weekly · ends in {0}", "Hebdo · se termine dans {0}");
            A("mis.drones_fmt", "Destruye {0} drones", "Destroy {0} drones", "Détruis {0} drones");
            A("mis.nodamage_fmt", "Completa {0} niveles sin recibir daño", "Clear {0} levels without taking damage", "Termine {0} niveaux sans subir de dégâts");
            A("mis.coins_fmt", "Recoge {0} monedas en los niveles", "Collect {0} coins in levels", "Ramasse {0} pièces dans les niveaux");
            A("mis.asteroids_fmt", "Destruye {0} asteroides", "Destroy {0} asteroids", "Détruis {0} astéroïdes");
            A("mis.levels_fmt", "Completa {0} niveles", "Clear {0} levels", "Termine {0} niveaux");
            A("mis.reward_fmt", "Premio: {0}", "Reward: {0}", "Récompense : {0}");
            A("mis.rw_coins_fmt", "{0} monedas", "{0} coins", "{0} pièces");
            A("mis.rw_life", "1 vida", "1 life", "1 vie");
            A("mis.rw_chest", "Cofre sorpresa", "Mystery chest", "Coffre surprise");
            A("mis.rw_weekly_fmt", "Pintura exclusiva {0} + {1} monedas", "Exclusive {0} paint + {1} coins", "Peinture exclusive {0} + {1} pièces");
            A("mis.progress_fmt", "{0} / {1}", "{0} / {1}", "{0} / {1}");
            A("mis.claim", "RECLAMAR", "CLAIM", "RÉCUPÉRER");
            A("mis.claimed", "RECLAMADO", "CLAIMED", "RÉCUPÉRÉ");
            A("mis.in_progress", "EN CURSO", "IN PROGRESS", "EN COURS");
            A("mis.done_fmt", "¡Misión completada! {0}", "Mission complete! {0}", "Mission accomplie ! {0}");
            A("mis.level_done_fmt", "Misión completada: {0} (reclámala en Misiones)", "Mission complete: {0} (claim it in Missions)", "Mission accomplie : {0} (récupère-la dans Missions)");
            A("mis.weekly_done_title", "¡Misión semanal completada!", "Weekly mission complete!", "Mission hebdo accomplie !");
            A("mis.paint_unlocked_fmt", "Nueva pintura exclusiva: {0} (equípala en el Hangar)", "New exclusive paint: {0} (equip it in the Hangar)", "Nouvelle peinture exclusive : {0} (équipe-la au Hangar)");
            A("mis.great", "¡GENIAL!", "AWESOME!", "GÉNIAL !");
            A("chest.title", "¡Cofre abierto!", "Chest opened!", "Coffre ouvert !");
            A("chest.upgrade_fmt", "Mejora gratis: {0} ({1})", "Free upgrade: {0} ({1})", "Amélioration gratuite : {0} ({1})");
            A("time.days_fmt", "{0} d {1} h", "{0} d {1} h", "{0} j {1} h");

            // ---------------- PRODUCTOS (dinero real) ----------------
            A("prod.starter_pack", "Pack de Inicio", "Starter Pack", "Pack de Départ");
            A("prod.starter_pack.desc", "5 vidas + 3.000 monedas + Escudo de Plasma. ¡Solo una vez!", "5 lives + 3,000 coins + Plasma Shield. One time only!", "5 vies + 3 000 pièces + Bouclier Plasma. Une seule fois !");
            A("prod.vip_monthly", "Piloto VIP (mensual)", "VIP Pilot (monthly)", "Pilote VIP (mensuel)");
            A("prod.vip_monthly.desc", "Sin anuncios intersticiales, monedas x2 y +3 vidas cada día.", "No interstitial ads, x2 coins and +3 lives every day.", "Sans pubs interstitielles, pièces x2 et +3 vies chaque jour.");
            A("prod.remove_ads", "Quitar Anuncios", "Remove Ads", "Supprimer les pubs");
            A("prod.remove_ads.desc", "Elimina intersticiales y banners para siempre.", "Removes interstitials and banners forever.", "Supprime les interstitielles et bannières pour toujours.");
            A("prod.lives5", "5 Vidas", "5 Lives", "5 Vies");
            A("prod.lives5.desc", "Vuelve a la acción.", "Get back into action.", "Retourne dans l'action.");
            A("prod.lives15", "15 Vidas", "15 Lives", "15 Vies");
            A("prod.lives15.desc", "El mejor valor en vidas.", "Best value for lives.", "Le meilleur prix pour les vies.");
            A("prod.coins_small", "2.000 Monedas", "2,000 Coins", "2 000 Pièces");
            A("prod.coins_small.desc", "Bolsa de monedas.", "Bag of coins.", "Sac de pièces.");
            A("prod.coins_medium", "7.000 Monedas", "7,000 Coins", "7 000 Pièces");
            A("prod.coins_medium.desc", "Cofre de monedas (+15%).", "Chest of coins (+15%).", "Coffre de pièces (+15 %).");
            A("prod.coins_large", "15.000 Monedas", "15,000 Coins", "15 000 Pièces");
            A("prod.coins_large.desc", "Bóveda de monedas (+50%).", "Vault of coins (+50%).", "Chambre forte de pièces (+50 %).");
            A("prod.ship_phoenix", "Nave Fénix", "Phoenix Ship", "Vaisseau Phénix");
            A("prod.ship_phoenix.desc", "6 disparos, carga rápida.", "6 shots, fast charge.", "6 tirs, charge rapide.");
            A("prod.ship_titan", "Nave Titán", "Titan Ship", "Vaisseau Titan");
            A("prod.ship_titan.desc", "230 de vida, 7 disparos.", "230 health, 7 shots.", "230 de vie, 7 tirs.");
            A("prod.ship_nova", "Nave Nova", "Nova Ship", "Vaisseau Nova");
            A("prod.ship_nova.desc", "La más rápida, 8 disparos.", "The fastest, 8 shots.", "Le plus rapide, 8 tirs.");
            A("prod.eq_plasma_shield", "Escudo de Plasma", "Plasma Shield", "Bouclier Plasma");
            A("prod.eq_plasma_shield.desc", "Bloquea 2 impactos por nivel.", "Blocks 2 hits per level.", "Bloque 2 impacts par niveau.");
            A("prod.eq_twin_cannon", "Cañón Doble", "Twin Cannon", "Canon Double");
            A("prod.eq_twin_cannon.desc", "Doble proyectil y +2 disparos.", "Double projectile and +2 shots.", "Double projectile et +2 tirs.");
            A("prod.eq_quantum_core", "Núcleo Cuántico", "Quantum Core", "Noyau Quantique");
            A("prod.eq_quantum_core.desc", "-25% carga, +1s ventana.", "-25% charge, +1s window.", "-25 % charge, +1 s fenêtre.");
            A("prod.eq_titanium_armor", "Blindaje de Titanio", "Titanium Armor", "Blindage en Titane");
            A("prod.eq_titanium_armor.desc", "+50% vida, -20% daño.", "+50% health, -20% damage.", "+50 % vie, -20 % dégâts.");
            A("prod.eq_ion_thrusters", "Propulsores Iónicos", "Ion Thrusters", "Propulseurs Ioniques");
            A("prod.eq_ion_thrusters.desc", "+20% velocidad.", "+20% speed.", "+20 % de vitesse.");
            A("prod.eq_triple_cannon", "Cañón Triple", "Triple Cannon", "Canon Triple");
            A("prod.eq_triple_cannon.desc", "Disparos en abanico de 3.", "3-way spread shots.", "Tirs en éventail de 3.");
            A("prod.eq_plasma_cannon", "Cañón de Plasma", "Plasma Cannon", "Canon à Plasma");
            A("prod.eq_plasma_cannon.desc", "Abanico de 5 que atraviesa obstáculos.", "5-way spread that pierces obstacles.", "Éventail de 5 qui traverse les obstacles.");
            A("prod.eq_missiles", "Misiles", "Missiles", "Missiles");
            A("prod.eq_missiles.desc", "Teledirigidos: destruyen hasta lo indestructible.", "Homing: they destroy even the indestructible.", "Guidés : ils détruisent même l'indestructible.");
            A("prod.paint_gold", "Pintura Oro Legendario", "Legendary Gold Paint", "Peinture Or Légendaire");
            A("prod.paint_gold.desc", "Luce tu nave en oro.", "Show off your ship in gold.", "Fais briller ton vaisseau en or.");

            var result = _d;
            _d = null;
            return result;
        }
    }
}
